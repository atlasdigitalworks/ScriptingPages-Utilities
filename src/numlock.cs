using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RobvanderWoude
{
	public class NumLock
	{
		[DllImport( "user32.dll" )]
		static extern void keybd_event( byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo );
		const int KEYEVENTF_EXTENDEDKEY = 0x1;
		const int KEYEVENTF_KEYUP = 0x2;

		public static int Main( string[] args )
		{
			try
			{
				bool numLock;
				bool newState = false;
				bool setState = false;
				bool setVerbose = false;
				bool verbose = true;

				if ( args.Length > 2 )
				{
					return WriteError( "Invalid command line argument(s)." );
				}
				else
				{
					foreach ( string arg in args )
					{
						switch ( arg.ToUpper( ) )
						{
							case "/?":
							case "-H":
							case "/HELP":
							case "-HELP":
							case "--HELP":
								return WriteError( string.Empty );
							case "/Q":
							case "-Q":
							case "/QUIET":
							case "-QUIET":
							case "--QUIET":
								if ( setVerbose )
								{
									return WriteError( "Duplicate switch not allowed." );
								}
								setVerbose = true;
								verbose = false;
								break;
							case "/V":
							case "-V":
							case "/VERBOSE":
							case "-VERBOSE":
							case "--VERBOSE":
								if ( setVerbose )
								{
									return WriteError( "Duplicate switch not allowed." );
								}
								setVerbose = true;
								verbose = true;
								break;
							case "0":
							case "OFF":
								if ( setState )
								{
									return WriteError( "Duplicate argument not allowed." );
								}
								setState = true;
								newState = false;
								break;
							case "1":
							case "ON":
								if ( setState )
								{
									return WriteError( "Duplicate argument not allowed." );
								}
								setState = true;
								newState = true;
								break;
							default:
								return WriteError( "Invalid command line argument " + arg );
						}
					}
				}

				if ( Control.IsKeyLocked( Keys.NumLock ) )
				{
					numLock = true;
					if ( setState && !newState )
					{
						keybd_event( 0x90, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr) 0 );
						keybd_event( 0x90, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0 );
					}
				}
				else
				{
					numLock = false;
					if ( setState && newState )
					{
						keybd_event( 0x90, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr) 1 );
						keybd_event( 0x90, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 1 );
					}
				}

				numLock = Control.IsKeyLocked( Keys.NumLock );
				if ( verbose )
				{
					Console.Write( "NumLock = " );
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine( ( numLock ? "ON" : "OFF" ) );
					Console.ResetColor( );
				}
				return ( numLock ? 2 : 0 );
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
			NumLock,  Version 1.01
			Read or set NumLock key state

			Usage:  NUMLOCK  [ 0 | 1 | ON | OFF ]  [ /Quiet | /Verbose ]

			Where:  0 or OFF   sets NumLock off
					1 or ON    sets NumLock on
			        /Quiet     won't display anything on screen
			        /Verbose   displays the (new) NumLock key state on screen (default)

			Notes:  An \"errorlevel\" 0 is returned if NumLock is off, 2 if
					NumLock is on, or 1 in case of (command line) errors.

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
			Console.Error.WriteLine( "NumLock,  Version 1.01" );
			Console.Error.WriteLine( "Read or set NumLock key state" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "NUMLOCK  [ 0 | 1 | ON | OFF ]  [ /Quiet | /Verbose ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "0" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "OFF" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   sets NumLock off" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        1" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "ON" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    sets NumLock on" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /Q" );
			Console.ResetColor( );
			Console.Error.WriteLine( "uiet     won't display anything on screen" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose   displays the (new) NumLock key state on screen (default)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Notes:  An \"errorlevel\" 0 is returned if NumLock is off, 2 if" );
			Console.Error.WriteLine( "        NumLock is on, or 1 in case of (command line) errors." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}