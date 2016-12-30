using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Red.Web.Realtime
{
	public class Server
	{
		private Socket mainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		private List<Client> clients = new List<Client>();

		public IPAddress IP { get; set; } = IPAddress.Any;
		public int port { get; set; } = 6502;

		private bool isRunning = false;

		public void Start()
		{
			if (!isRunning)
			{
				isRunning = true;
				mainSocket.Bind(new IPEndPoint(IP, port));
				mainSocket.Listen(10);
				WaitForNewConnection();
			}
		}

		void WaitForNewConnection()
		{
			mainSocket.BeginAccept(new AsyncCallback(BeginConnectClient), this);
		}

		private void BeginConnectClient(IAsyncResult result)
		{
			Socket clientSocket = mainSocket.EndAccept(result);
			Client client = CreateClient();
			client.Server = this;
			client.Socket = clientSocket;
			clients.Add(client);
			client.Open();

			WaitForNewConnection();
		}

		protected virtual Client CreateClient()
		{
			Client client = new Client();
			return client;
		}

		public void Stop()
		{
			isRunning = false;
		}

		public byte[] FrameMessage(byte[] bytes)
		{
			int headerLength = 0;
			byte[] header = new byte[10];

			header[0] = (byte)129;

			if (bytes.Length <= 125)
			{
				header[1] = (byte)bytes.Length;
				headerLength = 2;
			}
			else if (bytes.Length >= 126 && bytes.Length <= 65535)
			{
				header[1] = (byte)126;
				int len = bytes.Length;
				header[2] = (byte)((len >> 8) & (byte)255);
				header[3] = (byte)(len & (byte)255);
				headerLength = 4;
			}
			else
			{
				header[1] = (byte)127;
				int len = bytes.Length;
				header[2] = (byte)((len >> 56) & (byte)255);
				header[3] = (byte)((len >> 48) & (byte)255);
				header[4] = (byte)((len >> 40) & (byte)255);
				header[5] = (byte)((len >> 32) & (byte)255);
				header[6] = (byte)((len >> 24) & (byte)255);
				header[7] = (byte)((len >> 16) & (byte)255);
				header[8] = (byte)((len >> 8) & (byte)255);
				header[9] = (byte)(len & (byte)255);
				headerLength = 10;
			}

			int frameLength = headerLength + bytes.Length;

			byte[] framedMessage = new byte[frameLength];

			int ix = 0;
			for (int i = 0; i < headerLength; i++)
			{
				framedMessage[ix] = header[i];
				ix++;
			}
			for (int i = 0; i < bytes.Length; i++)
			{
				framedMessage[ix] = bytes[i];
				ix++;
			}

			return framedMessage;
		}

		public void Broadcast(byte[] bytes)
		{
			foreach (Client client in clients)
			{
				client.SendBytes(bytes);
			}
		}

		public void Broadcast(String message)
		{
			byte[] bytes = FrameMessage(Encoding.UTF8.GetBytes(message));
			Broadcast(bytes);
		}
	}
}
