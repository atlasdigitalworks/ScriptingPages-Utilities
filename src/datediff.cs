using System;
using System.Collections.Generic;


namespace RobvanderWoude
{
	class DateDiff
	{
		public static string progver = "1.02";
		static int rc = 1;


		static int Main( string[] args )
		{
			#region Initialize Variables

			bool swdays = false;
			bool swhours = false;
			bool swmils = false;
			bool swmins = false;
			bool swraw = false;
			bool swsign = false;
			bool swsecs = false;
			bool swticks = false;
			bool swverbose = false;
			bool swweeks = false;
			bool swyears = false;
			DateTime dt1;
			DateTime dt2;
			int years = 0;
			string sign = String.Empty;

			#endregion Initialize Variables


			#region Command Line Parsing

			if ( args.Length < 2 || args.Length > 3 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg == "/?" )
				{
					return ShowHelp( );
				}
			}

			try
			{
				if ( args[0].ToLower( ) == "now" )
				{
					dt1 = DateTime.Now;
				}
				else
				{
					dt1 = Convert.ToDateTime( args[0] );
				}
			}
			catch ( FormatException )
			{
				return ShowHelp( "Invalid date/time format \"{0}\"", args[0] );
			}

			try
			{
				if ( args[1].ToLower( ) == "now" )
				{
					dt2 = DateTime.Now;
				}
				else
				{
					dt2 = Convert.ToDateTime( args[1] );
				}
			}
			catch ( FormatException )
			{
				return ShowHelp( "Invalid date/time format \"{0}\"", args[1] );
			}

			if ( args.Length == 3 )
			{
				switch ( args[2].ToUpper( ) )
				{
					case "/A":
						rc = 1;
						swsign = true;
						break;
					case "/D":
						rc = 0;
						swdays = true;
						break;
					case "/H":
						rc = 0;
						swhours = true;
						break;
					case "/I":
						rc = 1;
						swmils = true;
						break;
					case "/M":
						rc = 0;
						swmins = true;
						break;
					case "/R":
						rc = 1;
						swraw = true;
						break;
					case "/S":
						rc = 0;
						swsecs = true;
						break;
					case "/T":
						rc = 1;
						swticks = true;
						break;
					case "/V":
						rc = 1;
						swverbose = true;
						break;
					case "/W":
						rc = 0;
						swweeks = true;
						break;
					case "/Y":
						rc = 0;
						swyears = true;
						break;
					default:
						rc = 1;
						return ShowHelp( "Invalid command line argument \"{0}\"", args[2] );
				}
			}

			#endregion Command Line Parsing


			try
			{
				TimeSpan diff = dt1 - dt2;

				if ( swraw )
				{
					Console.WriteLine( diff );
					return 0;
				}

				if ( swticks )
				{
					Console.WriteLine( diff.Ticks );
					return 0;
				}

				if ( swsign )
				{
					Console.WriteLine( ( diff.Ticks < 0 ? "-" : "+" ) );
					return 0;
				}

				if ( diff.Ticks < 0 )
				{
					// swap command line arguments, required to separate the years from the days
					dt1 = Convert.ToDateTime( args[1] );
					dt2 = Convert.ToDateTime( args[0] );
					diff = dt1 - dt2;
					sign = "-";
				}

				if ( args.Length == 3 && !swverbose && !swyears )
				{
					if ( swdays )
					{
						rc = diff.Days;
					}
					if ( swweeks )
					{
						rc = Convert.ToInt32( diff.Days / 7 );
					}
					if ( swhours )
					{
						rc = diff.Days * 24 + diff.Hours;
					}
					if ( swmins )
					{
						rc = ( diff.Days * 24 + diff.Hours ) * 60 + diff.Minutes;
					}
					if ( swsecs )
					{
						rc = Convert.ToInt32( diff.Ticks / 10000000 );
					}
					if ( swmils )
					{
						rc = Convert.ToInt32( diff.Ticks / 10000 );
					}
					Console.WriteLine( "{0}{1}", sign, rc );
					return rc;
				}

				years = dt1.Year - dt2.Year;
				if ( years != 0 )
				{
					dt2 = dt2.AddYears( years );
					diff = dt1 - dt2;
				}
				if ( diff.Days < 0 )
				{
					dt2 = dt2.AddYears( -1 );
					diff = dt1 - dt2;
					years -= 1;
				}

				if ( swyears )
				{
					Console.WriteLine( "{0}{1}", sign, years );
					return years;
				}

				string result = String.Empty;
				if ( years != 0 || swverbose )
				{
					result += String.Format( "{0}{1} year{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), years, ( Math.Abs( years ) == 1 ? String.Empty : "s" ) );
				}
				if ( diff.Days != 0 || !String.IsNullOrEmpty( result ) || swverbose )
				{
					result += String.Format( "{0}{1} day{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), diff.Days, ( Math.Abs( diff.Days ) == 1 ? String.Empty : "s" ) );
				}
				if ( diff.Hours != 0 || !String.IsNullOrEmpty( result ) || swverbose )
				{
					result += String.Format( "{0}{1} hour{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), diff.Hours, ( Math.Abs( diff.Hours ) == 1 ? String.Empty : "s" ) );
				}
				if ( diff.Minutes != 0 || !String.IsNullOrEmpty( result ) || swverbose )
				{
					result += String.Format( "{0}{1} minute{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), diff.Minutes, ( Math.Abs( diff.Minutes ) == 1 ? String.Empty : "s" ) );
				}
				if ( diff.Seconds != 0 || !String.IsNullOrEmpty( result ) || swverbose )
				{
					result += String.Format( "{0}{1} second{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), diff.Seconds, ( Math.Abs( diff.Seconds ) == 1 ? String.Empty : "s" ) );
				}
				if ( diff.Milliseconds != 0 || swverbose )
				{
					result += String.Format( "{0}{1} millisecond{2}", ( String.IsNullOrEmpty( result ) ? String.Empty : ", " ), diff.Milliseconds, ( Math.Abs( diff.Milliseconds ) == 1 ? String.Empty : "s" ) );
				}
				Console.WriteLine( "{0}{1}{2}", sign, ( String.IsNullOrEmpty( sign ) ? String.Empty : " " ), result );
				return rc;
			}
			catch ( Exception e )
			{
				return ShowHelp( e.Message );
			}
		}


		#region Error handling

		static int ShowHelp( params string[] errmsg )
		{
			/*
			DateDiff,  Version 1.02
			Batch tool calculate the timespan between two specified dates

			Usage:    DATEDIFF  date  date  [ option ]

			Where:    "date"    is a date/time in local or ISO date/time format, or "Now"
			          "option"  can be one of the following switches:
			                    /A    return only the sign of the timespan (+ or -)
			                    /D    return total number of entire Days in timespan
			                    /H    return total number of entire Hours in timespan
			                    /I    return total number of mIlliseconds in timespan
			                    /M    return total number of entire Minutes in timespan
			                    /R    return Raw timespan string (+/-dd.hh:mm:ss.iii000000)
			                    /S    return total number of entire Seconds in timespan
			                    /T    return total number of Ticks in timespan
			                    /V    Verbose output string, including leading and trailing
			                          zeroes (e.g. 0 years, or 0 milliseconds)
			                    /W    return total number of entire Weeks in timespan
			                    /Y    return number of entire Years in timespan

			Notes:    Dates must be entered either in YYYY-MM-DD [hh:mm[:ss[.iii]]] format,
			          or in local system's date/time format with DOT for decimal delimiter,
			          or as "Now", in which case the current date and time will be used.
			          In both dates, date and time components are optional, but at least
			          one of these must be specified: if no date is specified, today is
			          assumed, if no time is specified, 00:00:00.000000000 is assumed.
			          Defaults output: years, days, hours, minutes, seconds, milliseconds;
			          first value shown will be first non-zero value, unless /V specified;
			          milliseconds will not be shown if zero, unless /V or /I specified.
			          Return code ("ErrorLevel") equals absolute value of the result for
			          /D or /H or /M or /S or /W or /Y switches; otherwise 0 if result is
			          valid or 1 in case of (command line) errors.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			if ( errmsg.Length > 0 )
			{
				List<string> errargs = new List<string>( errmsg );
				errargs.RemoveAt( 0 );
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR:\t" );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errmsg[0], errargs.ToArray( ) );
				Console.ResetColor( );

			}

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "DateDiff,  Version {0}", progver );
			Console.Error.WriteLine( "Batch tool calculate the timespan between two specified dates" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "DATEDIFF  date  date  [ option ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"date\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    is a date/time in local or ISO date/time format, or \"Now\"" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          \"option\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  can be one of the following switches:" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /A" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    return only the sign of the timespan (+ or -)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /D" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "D" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ays in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /H" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "H" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ours in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /I" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of m" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "I" );
			Console.ResetColor( );
			Console.Error.WriteLine( "lliseconds in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /M" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "M" );
			Console.ResetColor( );
			Console.Error.WriteLine( "inutes in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /R" );
			Console.ResetColor( );
			Console.Error.Write( "    return " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "R" );
			Console.ResetColor( );
			Console.Error.WriteLine( "aw timespan string (+/-dd.hh:mm:ss.iii000000)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /S" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "econds in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /T" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "icks in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /V    V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose output string, including leading and trailing" );

			Console.Error.WriteLine( "                          zeroes (e.g. 0 years, or 0 milliseconds)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /W" );
			Console.ResetColor( );
			Console.Error.Write( "    return total number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "eeks in timespan" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                    /Y" );
			Console.ResetColor( );
			Console.Error.Write( "    return number of entire " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "Y" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ears in timespan" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    Dates must be entered either in YYYY-MM-DD [hh:mm[:ss[.iii]]] format," );

			Console.Error.Write( "          or in local system's date/time format with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "DOT" );
			Console.ResetColor( );
			Console.Error.WriteLine( " for decimal delimiter," );

			Console.Error.Write( "          or as " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"Now\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( ", in which case the current date and time will be used." );

			Console.Error.WriteLine( "          In both dates, date and time components are optional, but at least" );

			Console.Error.WriteLine( "          one of these must be specified: if no date is specified, today is" );

			Console.Error.WriteLine( "          assumed, if no time is specified, 00:00:00.000000000 is assumed." );

			Console.Error.WriteLine( "          Default output: years, days, hours, minutes, seconds, milliseconds;" );

			Console.Error.Write( "          first value shown will be first non-zero value, unless " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/V" );
			Console.ResetColor( );
			Console.Error.WriteLine( " specified;" );

			Console.Error.Write( "          milliseconds will not be shown if zero, unless " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/V" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/I" );
			Console.ResetColor( );
			Console.Error.WriteLine( " is specified." );

			Console.Error.WriteLine( "          Return code (\"ErrorLevel\") equals absolute value of the result for" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /D" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/H" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/M" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/Y" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switches; otherwise 0 if result is" );

			Console.Error.WriteLine( "          valid or 1 in case of (command line) errors." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return rc;
		}

		#endregion Error handling
	}
}
