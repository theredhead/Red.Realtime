using System;
using System.Net.Sockets;
using System.Text;
using Red.Web.Realtime;

namespace RealtimeTestApp
{
	internal class TestClient : Client
	{
		protected override void MessageReceived(string message)
		{
			base.MessageReceived(message);
			Server.Broadcast(message);
		}
	}	
}
