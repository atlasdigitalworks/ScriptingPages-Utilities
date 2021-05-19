using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class CountDown
	{
		static readonly string progver = "2.01";


		static int Main( string[] args )
		{
			string prefix = string.Empty;
			string suffix = string.Empty;
			bool returnremaining = false;
			int seconds = 0;


			#region Parse Command Line

			if ( args.Length == 0 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg == "/?" )
				{
					return ShowHelp( );
				}
				else if ( arg.ToUpper( ) == "/R" )
				{
					if ( returnremaining )
					{
						return ShowHelp( "Duplicate command line switch /R" );
					}
					returnremaining = true;
				}
				else if ( int.TryParse( arg, out int test ) )
				{
					if ( seconds == 0 )
					{
						seconds = test;
					}
					else
					{
						return ShowHelp( "Duplicate timeout argument" );
					}
				}
				else if ( string.IsNullOrWhiteSpace( prefix ) )
				{
					prefix = arg;
				}
				else if ( string.IsNullOrWhiteSpace( suffix ) )
				{
					suffix = arg;
				}
				else
				{
					return ShowHelp( "Invalid or too many command line arguments", arg );
				}
			}

			// Enable single command line argument for prefix seconds suffix
			if ( seconds == 0 && !string.IsNullOrWhiteSpace( prefix ) && string.IsNullOrWhiteSpace( suffix ) )
			{
				string pattern = @"^([^\d]*)(\d+)(.*)$";
				Regex regex = new Regex( pattern );
				if ( regex.IsMatch( prefix ) )
				{
					MatchCollection matches = regex.Matches( prefix );
					if ( matches.Count == 1 )
					{
						if ( matches[0].Groups.Count == 4 )
						{
							if ( int.TryParse( matches[0].Groups[2].ToString( ), out seconds ) )
							{
								prefix = matches[0].Groups[1].ToString( );
								suffix = matches[0].Groups[3].ToString( );
							}
						}
					}
				}
			}

			if ( seconds < 1 || seconds > 86400 )
			{
				return ShowHelp( "Specified timeout ({0}) outside allowed range", seconds.ToString( ) );
			}

			#endregion Parse Command Line


			// Temporarily change console properties
			Console.CursorVisible = false;
			Console.TreatControlCAsInput = true;

			// Determine the string length for number of digits
			int counterlength = seconds.ToString( ).Length;

			// Determine the prefix string length
			int prefixlength = prefix.Length;

			// Determine the suffix string length
			int suffixlength = suffix.Length;

			// Display optional message
			Console.Write( prefix );

			// Clear space on screen for countdown counter value
			Console.Write( new String( ' ', counterlength + suffixlength ) );

			// Start countdown
			int countdown = seconds;
			while ( !Console.KeyAvailable && countdown > 0 )
			{
				// Wipe previous countdown counter value from screen
				Console.Write( new String( '\b', counterlength + suffixlength ) );
				Console.Write( new String( ' ', counterlength + suffixlength ) );
				Console.Write( new String( '\b', counterlength + suffixlength ) );
				Console.Write( "{0," + counterlength + "}", countdown );
				Console.Write( suffix );
				countdown -= 1; // decrement countdown counter
				Thread.Sleep( 1000 ); // wait 1 second
			}

			// Wipe entire countdown counter line from screen
			Console.Write( new String( '\b', counterlength + suffixlength + prefixlength ) );
			Console.Write( new String( ' ', counterlength + suffixlength + prefixlength ) );
			Console.Write( new String( '\b', counterlength + suffixlength + prefixlength ) );

			// Restore console properties
			Console.CursorVisible = true;
			Console.TreatControlCAsInput = false;

			// If a key is pressed to interrupt countdown, remove it from keyboard buffer
			if ( countdown > 0 && Console.KeyAvailable )
			{
				Console.ReadKey( true );
			}

			// Return code depends on command line switch and on countdown completion
			if ( returnremaining )
			{
				return countdown; // remaining number of seconds (requires /R)
			}
			else if ( countdown == 0 )
			{
				return 0; // countdown completed
			}
			else
			{
				return 2; // countdown interrupted (not with /R)
			}
		}


		static int ShowHelp( params string[] errmsg )
		{
			#region Help Text

			/*
			CountDown,  Version 2.01
			Count down for the specified number of seconds or until a key is pressed
 
			Usage:  COUNTDOWN  [ prefix ]  seconds  [ suffix ]  [ /R ]
 
			   or:  COUNTDOWN  "prefix seconds suffix"  [ /R ]
 
			Where:  prefix     optional prefix message for the counter, e.g. "Waiting "
			        seconds    delay in seconds (1 .. 86400 = 1 second .. 24 hours)
			        suffix     optional suffix message for the counter, e.g. " seconds"
			        /R         return code equals the number of Remaining seconds
			                   (default: return code 0 = OK, 1 = errors, 2 = key pressed)
 
			Note:   COUNTDOWN "Wait " 20 " seconds" is equal to COUNTDOWN "Wait 20 seconds"
			        In the latter notation the first number on the command line will be
			        used for seconds, any following number will be included in suffix.

			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Error Message

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

			#endregion Error Message


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "CountDown,  Version {0}", progver );

			Console.Error.WriteLine( "Count down for the specified number of seconds or until a key is pressed" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "COUNTDOWN  [ prefix ]  seconds  [ suffix ]  [ /R ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "   or:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "COUNTDOWN  \"prefix seconds suffix\"  [ /R ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "prefix     " );
			Console.ResetColor( );
			Console.Error.WriteLine( "optional prefix message for the counter, e.g. \"Waiting \"" );


			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        seconds    " );
			Console.ResetColor( );
			Console.Error.WriteLine( "delay in seconds (1 .. 86400 = 1 second .. 24 hours)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        suffix     " );
			Console.ResetColor( );
			Console.Error.WriteLine( "optional suffix message for the counter, e.g. \" seconds\"" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /R" );
			Console.ResetColor( );
			Console.Error.Write( "         return code equals the number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "R" );
			Console.ResetColor( );
			Console.Error.WriteLine( "emaining seconds" );

			Console.Error.WriteLine( "                   (default: return code 0 = OK, 1 = errors, 2 = key pressed)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Note:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "COUNTDOWN \"Wait \" 20 \" seconds\" " );
			Console.ResetColor( );
			Console.Error.Write( "is equal to " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "COUNTDOWN \"Wait 20 seconds\"" );
			Console.ResetColor( );

			Console.Error.WriteLine( "        In the latter notation the first number on the command line will be" );

			Console.Error.Write( "        used for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "seconds" );
			Console.ResetColor( );
			Console.Error.Write( ", any following number will be included in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "suffix" );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Display Help Text

			return 1;
		}
	}
}
