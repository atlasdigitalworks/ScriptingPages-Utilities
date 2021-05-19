using System;
using System.Net.NetworkInformation;

namespace RobvanderWoude
{
	class FastPing
	{
		static int Main( string[] args )
		{
			try
			{
				string hostname = string.Empty;
				char[] test = { '/', '?' };

				#region Command Line Parsing

				if ( args.Length == 1 )
				{
					hostname = args[0];
				}
				else
				{
					return WriteError( );
				}
				if ( hostname.IndexOfAny( test ) != -1 )
				{
					return WriteError( );
				}
				#endregion Command Line Parsing

				try
				{
					Ping ping = new Ping( );
					PingReply reply = ping.Send( hostname );
					Console.WriteLine( reply.Address );
					if ( reply.Status == IPStatus.Success )
					{
						return 0;
					}
					else
					{
						return 1;
					}
				}
				catch ( PingException e )
				{
					Console.Error.WriteLine( "ERROR: {0} ({1})", e.Message, e.InnerException.Message );
					return 1;
				}

			}
			catch ( Exception e )
			{
				Console.Error.WriteLine( "ERROR: {0}", e.Message );
				return 1;
			}
		}

	
		#region Error Handling

		public static int WriteError( Exception e = null )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			Console.OpenStandardError( );
			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR: " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}

			/*
			FastPing,  Version 1.00
			Faster PING alternative

			Usage:   FASTPING    hostname
			or:      FASTPING    ipaddress

			Where:   hostname    is the host name to be pinged
			         ipaddress   is the IP address to be pinged

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "FastPing,  Version 1.00" );
			Console.Error.WriteLine( "Faster PING alternative" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "FASTPING  hostname" );
			Console.ResetColor( );
			Console.Error.Write( "or:      " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "FASTPING  ipaddress" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "hostname" );
			Console.ResetColor( );
			Console.Error.WriteLine( "      is the host name to be pinged" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         ipaddress" );
			Console.ResetColor( );
			Console.Error.WriteLine( "     is the IP address to be pinged" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.Write( "http://www.robvanderwoude.com" );
			Console.OpenStandardOutput( );
			return 1;
		}

		#endregion Error Handling
	}
}
