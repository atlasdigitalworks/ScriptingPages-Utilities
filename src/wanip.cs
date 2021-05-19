namespace RobvanderWoude
{
	class WANIP
	{
		static string progver = "1.03";


		static int Main( string[] args )
		{
			// These are the URLs the program uses to try and get the computer's WAN IP address; if
			// a site fails, the next one is tried; if all fail, their error messages are displayed
			string[] urls = { "https://www.robvanderwoude.com/wanip.php", "https://www.robvanderwoude.net/wanip.php", "http://automation.whatismyip.com/n09230945.asp" };

			if ( args.Length == 0 )
			{
				bool found = false;
				string errormessage = System.String.Empty;
				string ipPattern = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
				string wanip = System.String.Empty;
				// The next couple of ServicePointManager lines are required for secure connections only
				System.Net.ServicePointManager.Expect100Continue = true;
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Ssl3;
				System.Net.WebClient myWebClient = new System.Net.WebClient( );
				foreach ( string url in urls )
				{
					if ( !found )
					{
						try
						{
							System.IO.Stream myStream = myWebClient.OpenRead( url );
							System.IO.StreamReader myStreamReader = new System.IO.StreamReader( myStream );
							wanip = myStreamReader.ReadToEnd( );
							if ( System.Text.RegularExpressions.Regex.IsMatch( wanip, ipPattern ) )
							{
								System.Console.WriteLine( wanip );
								found = true;
							}
							else
							{
								errormessage += System.String.Format( "\n\nThe URL did not return a valid IP address: {0}", url );
							}
						}
						catch ( System.Exception e )
						{
							errormessage += System.String.Format( "\n\n{0} ({1})", e.Message, url );
						}
					}
				}
				if ( found )
				{
					return 0;
				}
				else
				{
					if ( !System.String.IsNullOrEmpty( errormessage ) )
					{
						System.Console.Error.WriteLine( errormessage );
					}
					return 1;
				}
			}
			else
			{
				System.Console.Error.WriteLine( );

				System.Console.Error.WriteLine( "WANIP.exe,  Version {0}", progver );

				System.Console.Error.WriteLine( "Return the computer's WAN IP address" );

				System.Console.Error.WriteLine( );

				System.Console.Error.WriteLine( "Usage:  WANIP" );

				System.Console.Error.WriteLine( );

				System.Console.Error.WriteLine( "Note:   The program tries {0} different URLs to get the WAN IP address", urls.Length );

				System.Console.Error.WriteLine( );

				System.Console.Error.WriteLine( "Written by Rob van der Woude" );

				System.Console.Error.WriteLine( "https://www.robvanderwoude.com" );

				return 1;
			}
		}
	}
}
