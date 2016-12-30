using System;
using System.Net.Sockets;
using System.Text;
using Red.Web.Realtime;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace RealtimeTestApp
{
	internal class TestClient : Client
	{
		private JsonSerializer serializer = new JsonSerializer();

		public TestClient()
		{

		}

		protected override void MessageReceived(string message)
		{
			base.MessageReceived(message);

			Dictionary<string, string> options = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
			HandleRequest(options);
		}

		void HandleRequest(Dictionary<string, string> options)
		{
			switch (options["action"])
			{ 
				case "speak" : Speak(options); break;
				default:

				break;
			}
		}

		void Speak(Dictionary<string, string> options)
		{
			options["sender"] = ClientId;
			options["when"] = DateTime.Now.ToString();

			using (MemoryStream stream = new MemoryStream())
			{
				using (TextWriter writer = new StreamWriter(stream))
				{
					serializer.Serialize(writer, options);
				}

				Server.Broadcast(Server.FrameMessage(stream.ToArray()));
			}
		}
	}
}
