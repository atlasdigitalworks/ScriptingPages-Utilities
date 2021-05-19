using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace RobvanderWoude
{
	class WGetLite
	{
		static string progver = "1.01";

		private static ManualResetEvent allDone = new ManualResetEvent( false );


		static int Main( string[] args )
		{
			string url = string.Empty;
			string enc = "ASCII";
			string outputFile = string.Empty;
			int statusCode = 0;
			bool save = false;
			bool overwrite = false;
			bool show = true;

			#region Command Line Parsing

			// Custom error message
			string msgInvalid = "Invalid command line argument" + ( ( args.Length == 1 ) ? string.Empty : "(s)" );

			// No command line arguments? Display help
			if ( args.Length == 0 )
			{
				return ShowHelp( string.Empty );
			}

			foreach ( string arg in args )
			{
				// Check switches first
				switch ( arg.ToLower( ).Substring( 0, 2 ) )
				{
					case "/?":
					case "-?":
					case "/h":
					case "-h":
						// Display help
						return ShowHelp( string.Empty );
					case "--":
						if ( arg.ToLower( ) == "--help" )
						{
							// Display help
							return ShowHelp( string.Empty );
						}
						else
						{
							// Display error message
							return ShowHelp( msgInvalid );
						}
					case "/e":
						if ( arg.ToLower( ).StartsWith( "/e:" ) || arg.ToLower( ).StartsWith( "/encoding:" ) )
						{
							enc = arg.Substring( arg.IndexOf( ":" ) + 1 );
						}
						else
						{
							return ShowHelp( msgInvalid );
						}
						break;
					case "/o":
						overwrite = true;
						break;
					case "/r":
						show = false;
						break;
					case "/s":
						show = false;
						save = true;
						if ( arg.ToLower( ).StartsWith( "/s:" ) || arg.ToLower( ).StartsWith( "/save:" ) )
						{
							outputFile = arg.Substring( arg.IndexOf( ":" ) + 1 );
						}
						else
						{
							return ShowHelp( msgInvalid );
						}
						break;
					default:
						url = arg;
						break;
				}
			}

			if ( string.IsNullOrEmpty( url ) )
			{
				return ShowHelp( "A URL must be specified" );
			}

			if ( overwrite && !save )
			{
				return ShowHelp( "/Overwrite requires /Save." );
			}

			if ( save )
			{
				if ( File.Exists( @outputFile ) )
				{
					if ( !overwrite )
					{
						return ShowHelp( "File already exists." );
					}
				}
			}

			#endregion Command Line Parsing

			try
			{
				statusCode = (int) GetResponse( url );

				if ( save )
				{
					using ( StreamWriter outputStream = new StreamWriter( @outputFile ) )
					{
						if ( overwrite )
						{
							outputStream.Flush( );
						}
						outputStream.Write( Get( url, enc ) );
						outputStream.Close( );
					}
				}
				else if ( show )
				{
					Console.Write( Get( url, enc ) );
				}

				return ( statusCode == 200 ? 0 : statusCode );
			}
			catch ( Exception e )
			{
				return ShowHelp( e.Message );
			}
		}


		public static HttpStatusCode GetResponse( string url )
		{
			// Returns the HTTP Status Code for the specified URL, or 0 on errors
			try
			{
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create( url );
				req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
				req.AllowAutoRedirect = false;
				HttpWebResponse resp = (HttpWebResponse) req.GetResponse( );
				HttpStatusCode stat = resp.StatusCode;
				req.Abort( );
				return stat;
			}
			catch ( WebException e )
			{
				// Handle exceptions caused by the server response, e.g. 404
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse) e.Response;
					return httpResponse.StatusCode;
				}
				// Handle the "real" exceptions
				catch
				{
					ShowHelp( e.Message );
					return 0;
				}
			}
			catch ( Exception e )
			{
				ShowHelp( e.Message );
				return 0;
			}
		}


		// Get( ) based on blog post by Sugree Phatanapherom
		// http://www.howforge.com/how-to-implement-simple-wget-in-c
		private static string Get( string url, string encoding )
		{
			try
			{
				// The next couple of ServicePointManager lines are required for secure connections only

				System.Net.ServicePointManager.Expect100Continue = true;
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Ssl3;
				WebClient webClient = new WebClient( );
				byte[] response = webClient.DownloadData( url );
				switch ( encoding.ToUpper( ) )
				{
					case "A":
					case "ANSI":
					case "ASCII":
						return Encoding.ASCII.GetString( response );
					case "U":
					case "ULE":
					case "UNICODE":
					case "UNICODELE":
					case "UNICODE LE":
					case "UNICODE (LE)":
						return Encoding.Unicode.GetString( response );
					case "UB":
					case "UBE":
					case "UNICODEBE":
					case "UNICODE BE":
					case "UNICODE (BE)":
						return Encoding.BigEndianUnicode.GetString( response );
					case "U7":
					case "UTF7":
					case "UTF-7":
						return Encoding.UTF7.GetString( response );
					case "U8":
					case "UTF8":
					case "UTF-8":
						return Encoding.UTF8.GetString( response );
					case "U32":
					case "UTF32":
					case "UTF-32":
						return Encoding.UTF32.GetString( response );
					default:
						ShowHelp( "Invalid encoding" );
						return string.Empty;
				}
			}
			catch ( Exception e )
			{
				ShowHelp( e.Message );
				return string.Empty;
			}
		}

		#region Error Handling

		public static int ShowHelp( Exception e )
		{
			return ShowHelp( e == null ? null : e.Message );
		}

		public static int ShowHelp( string errorMessage )
		{
			/*
			WGetLite,  Version 1.01
			Get web content (text only)

			Usage:  WGETLITE  url  [ /Response | /Save:filename ]  [ /Encoding:encoding ]

			Where:  /Response returns server response code only
					/Save     saves downloaded content to specified file
					/Encoding specifies encoding of url; accepted values are:
			                  ANSI, Unicode, Unicode (BE), UTF-7, UTF-8, UTF-32

			Notes:  On errors the actual HTTP response code is returned as 'errorlevel'.
			        Usually specifying the encoding won't be necessary.
			        Switches may be abbreviated, e.g. /E:U8 instead of /Encoding:UTF-8.
			        Based on a blog post by Sugree Phatanapherom:
			        http://www.howforge.com/how-to-implement-simple-wget-in-c

			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR:  " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "WGetLite,  Version {0}", progver );

			Console.Error.WriteLine( "Get web content (text only)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "WGETLITE  url  [ /Response | /Save:filename ]  [ /Encoding:encoding ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/R" );
			Console.ResetColor( );
			Console.Error.WriteLine( "esponse returns server response code only" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ave     saves downloaded content to specified file" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /E" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ncoding specifies encoding of url; accepted values are:" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                  A" );
			Console.ResetColor( );
			Console.Error.Write( "SCII, " );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "nicode, " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "nicode (" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "BE" );
			Console.ResetColor( );
			Console.Error.Write( "), " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "TF-" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "7" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "TF-" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "8" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "TF-" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "32" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:  On errors the actual HTTP response code is returned as 'errorlevel'." );

			Console.Error.WriteLine( "        Usually specifying the encoding won't be necessary." );

			Console.Error.Write( "        Switches may be abbreviated, e.g. " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/E" );
			Console.ResetColor( );
			Console.Error.Write( ":" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U8" );
			Console.ResetColor( );
			Console.Error.Write( " instead of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/E" );
			Console.ResetColor( );
			Console.Error.Write( "ncoding:" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "UTF-8" );
			Console.Error.WriteLine( "." );
			Console.ResetColor( );

			Console.Error.WriteLine( "        Based on a blog post by Sugree Phatanapherom:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "        http://www.howforge.com/how-to-implement-simple-wget-in-c" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			return 1;
		}

		#endregion Error Handling
	}
}
