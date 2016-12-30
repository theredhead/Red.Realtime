using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Red.Web.Realtime
{
	public class HandshakeRequest
	{
		internal const string CRLF = "\r\n";
		const string WebSocketProtocolGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		public string WebSocketKey { get; private set; }
		public string WebSocketVersion { get; private set; }
		public string WebSocketExtensions { get; private set; }

		class RequestHeaders : Dictionary<string, string>
		{
			public string FirstLine { get; set; }

			public RequestHeaders(byte[] bytes) : base()
			{
				string body = Encoding.UTF8.GetString(bytes);
				string[] lines = body.Split(CRLF.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				FirstLine = lines[0];
				for (int ix = 1; ix < lines.Length; ix++)
				{
					string label = lines[ix].Substring(0, lines[ix].IndexOf(':'));
					string value = lines[ix].Substring(lines[ix].IndexOf(':') + 1).Trim();
					base[label] = value;
				}
			}
		}

		public HandshakeRequest(byte[] bytes)
		{
			RequestHeaders headers = new RequestHeaders(bytes);
			WebSocketKey = headers["Sec-WebSocket-Key"];
			WebSocketVersion = headers["Sec-WebSocket-Version"];
			WebSocketExtensions = headers["Sec-WebSocket-Extensions"];
		}

		internal byte[] FormulateResponse()
		{
			string handshakeResponse = Convert.ToBase64String(
				SHA1.Create().ComputeHash(
					Encoding.UTF8.GetBytes(
						WebSocketKey + WebSocketProtocolGuid
					)));
			
			Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + CRLF
				+ "Connection: Upgrade" + CRLF
				+ "Upgrade: websocket" + CRLF
				+ "Sec-WebSocket-Accept: " + handshakeResponse + CRLF
				+ CRLF);

			return response;
		}
	}
}