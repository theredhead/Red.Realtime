using System;
using Red.Web.Realtime;

namespace RealtimeTestApp
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			TestServer server = new TestServer();
			server.Start();


			Console.ReadLine();
			server.Stop();
		}
	}
}
