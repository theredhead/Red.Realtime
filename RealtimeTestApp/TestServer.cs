using System;
using Red.Web.Realtime;

namespace RealtimeTestApp
{
	class TestServer : Server
	{ 
		protected override Client CreateClient()
		{
			return new TestClient();
		}
	}
	
}
