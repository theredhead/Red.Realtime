using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Red.Web.Realtime
{
	public enum ClientState
	{
		Invalid,
		Opening,
		Open,
		Closing,
		Closed
	}

	public class Client
	{
		const int BUFFER_BYTE_SIZE = 1204 * 128;
		private List<Frame> frames = new List<Frame>();
		private bool isRunning = false;
		public string ClientId { get; set; }
		public ClientState State { get; private set; } = ClientState.Invalid;
		private byte[] buffer;

		public Server Server { get; set; }
		public Socket Socket { get; set; }

		public void Open()
		{
			if (!isRunning)
			{
				isRunning = true;
				State = ClientState.Opening;
				buffer = new byte[BUFFER_BYTE_SIZE];
				Socket.BeginReceive(buffer, 0, BUFFER_BYTE_SIZE, SocketFlags.None, new AsyncCallback(BeginPerformHandshake), this);
			}
		}

		private void Listen()
		{ 
			Socket.BeginReceive(buffer, 0, BUFFER_BYTE_SIZE, SocketFlags.None, new AsyncCallback(BeginReceiveFrame), this);
		}

		public void Close()
		{
			isRunning = false;
			Socket.Close();
		}


		private void BeginPerformHandshake(IAsyncResult result)
		{
			int numberOfBytesReceived = Socket.EndReceive(result);
			byte[] requestData = new byte[numberOfBytesReceived];  
			Array.Copy(buffer, requestData, numberOfBytesReceived);
			HandshakeRequest request = new HandshakeRequest(requestData);
			ClientId = request.WebSocketKey;
			SendBytes(request.FormulateResponse());
			State = ClientState.Open;

			Listen();
		}

		private void BeginReceiveFrame(IAsyncResult result)
		{ 
			int numberOfBytesReceived = Socket.EndReceive(result);
			byte[] frameData = new byte[numberOfBytesReceived];
			Array.Copy(buffer, frameData, numberOfBytesReceived);
			Frame frame = new Frame(frameData);
			FrameReceived(frame);
			Listen();
		}

		protected virtual void FrameReceived(Frame frame)
		{
			frames.Add(frame);
			if (frame.IsFinalFrame)
			{
				FrameSequenceReceived(frames);
				frames.Clear();
			}
		}

		public virtual void SendBytes(byte[] bytes)
		{
			Socket.Send(bytes);
		}

		public virtual void Send(string message)
		{
			SendBytes(Server.FrameMessage(Encoding.UTF8.GetBytes(message)));
		}

		protected virtual void FrameSequenceReceived(List<Frame> frames)
		{
			int numberOfFramesOnStack = frames.Count;
			string message;
			if (numberOfFramesOnStack == 1)
			{
				message = frames[0].MessageText;
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				foreach (Frame frame in frames)
					builder.Append(frame.MessageText);

				message = builder.ToString();
			}

			MessageReceived(message);
		}

		protected virtual void MessageReceived(string message)
		{
			Console.WriteLine($"{ClientId}> {message}");
		}
	}
}
