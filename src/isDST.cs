using System;
using System.Globalization;


namespace RobvanderWoude
{
	class isDST
	{
		static string progver = "1.02";


		static int Main( string[] args )
		{
			try
			{
				// Defaults
				bool verbose = false;
				DateTime checkDate = DateTime.Now;

				#region command line parsing

				if ( args.Length > 2 )
				{
					return WriteError( "Invalid command line argument(s)." );
				}

				foreach ( string arg in args )
				{
					switch ( arg.ToLower( ) )
					{
						case "/?":
						case "/h":
						case "-h":
						case "--h":
						case "/help":
						case "-help":
						case "--help":
							return WriteError( string.Empty );
						case "/v":
						case "-v":
						case "--v":
						case "/verbose":
						case "-verbose":
						case "--verbose":
							verbose = true;
							break;
						default:
							// Check if the argument is a valid date/time
							try
							{
								checkDate = System.Convert.ToDateTime( arg );
							}
							catch ( Exception e )
							{
								return WriteError( e );
							}
							break;
					}
				}

				// Two command line arguments are allowed, but only if one of these is the /Verbose switch
				if ( args.Length == 2 && !verbose )
				{
					return WriteError( "Invalid command line argument(s)." );
				}

				#endregion command line parsing

				// Display result on screen
				if ( verbose )
				{
					Console.WriteLine( "{0} {1} in Daylight Saving Time", checkDate.ToLongDateString( ).ToString( ), ( TimeZone.CurrentTimeZone.IsDaylightSavingTime( checkDate ) ? "IS" : "is NOT " ) );
				}
				
				// Return result as 'errorlevel'
				if ( TimeZone.CurrentTimeZone.IsDaylightSavingTime( checkDate ) )
				{
					return 0;
				}
				else
				{
					return 2;
				}
			}
			catch ( Exception e )
			{
				return WriteError( e );
			}
		}

		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			/*
			isDST,  Version 1.02
			Check if a date is in Daylight Saving Time

			Usage:  ISDST  [ date ]  /Verbose

			Where:  date       is an optional date/time to check (default: today/now)
					/Verbose   tells the program to display the result on screen

			Notes:  An "errorlevel" 0 is returned if the date is in DST, 2 if the date
					is not in DST, or 1 in case of (command line) errors.
					This program uses local date/time formats and timezone settings.
			        /Verbose switch may be abbreviated to /V.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
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

			Console.Error.WriteLine( "isDST,  Version {0}", progver );

			Console.Error.WriteLine( "Check if a date is in Daylight Saving Time" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ISDST  [ date ]  /Verbose" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "date" );
			Console.ResetColor( );
			Console.Error.WriteLine( "       is an optional date/time to check (default: today/now)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose   tells the program to display the result on screen" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:  An \"errorlevel\" 0 is returned if the date is in DST, 2 if the date" );

			Console.Error.WriteLine( "        is not in DST, or 1 in case of (command line) errors." );

			Console.Error.WriteLine( "        This program uses local date/time formats and timezone settings." );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /Verbose" );
			Console.ResetColor( );
			Console.Error.Write( " switch may be abbreviated to " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}

	}
}