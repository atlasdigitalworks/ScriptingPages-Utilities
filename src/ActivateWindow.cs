using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace RobvanderWoude
{
	class ActivateWindow
	{
		static readonly string progver = "1.00";


		static bool debug = false;
		static string matchingtitle = string.Empty;


		static int Main( string[] args )
		{
			string windowtitle = string.Empty;
			string exact = "partial ";
			bool exacttitle = false;


			#region Parse and Validate Command Line

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' )
				{
					switch ( arg.ToUpper( ) )
					{
						case "/?":
							return ShowHelp( );
						case "/D":
						case "/DEBUG":
							if ( debug )
							{
								return ShowHelp( "Duplicate switch /D" );
							}
							debug = true;
							break;
						case "/X":
						case "/EXACT":
							if ( exacttitle )
							{
								return ShowHelp( "Duplicate switch /X" );
							}
							exacttitle = true;
							exact = "exact ";
							break;
						default:
								return ShowHelp( "Invalid switch \"{0}\"", arg );
					}
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( windowtitle ) )
					{
						// First unnamed argument is title
						windowtitle = arg;
					}
					else
					{
						// No second unnamed argument allowed
						return ShowHelp( "Invalid unnamed argument \"{0}\"", arg );
					}
				}
			}


#if DEBUG
			debug = true;
#endif


			#endregion Parse and validate Command Line


			// Find the window with the specified title
			IntPtr handle = FindWindow( windowtitle, exacttitle );
			if ( handle == IntPtr.Zero )
			{
				return ShowHelp( "No window was found with {0}title \"{1}\"", exact, windowtitle );
			}
			else
			{
				if ( debug && !exacttitle )
				{
					Console.WriteLine( "Specified title : \"{0}\"\nMatching title  : \"{1}\"\n", windowtitle, matchingtitle );
				}
				// If found, make it the foreground window
				if ( !SetForegroundWindow( handle ) )
				{
					return ShowHelp( "Unable to move the specified window to the foreground" );
				}
			}

			return 0;
		}


		static IntPtr FindWindow( string title, bool exacttitlematch = false )
		{
			foreach ( Process process in Process.GetProcesses( ) )
			{
				if ( process.MainWindowTitle.Equals( title ) )
				{
					return process.MainWindowHandle; // Return the FIRST matching window
				}
				else if ( !exacttitlematch )
				{
					if ( process.MainWindowTitle.Contains( title ) )
					{
						matchingtitle = process.MainWindowTitle;
						return process.MainWindowHandle; // Return the FIRST matching window
					}
				}
			}
			return IntPtr.Zero; // In case no matching title was found
		}


		#region DLL Imports

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool SetForegroundWindow( IntPtr hWnd );

		#endregion DLL Imports


		#region Error handling

		static int ShowHelp( params string[] errmsg )
		{
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


			#region Help Text

			/*
			ActivateWindow.exe,  Version 1.00
			Activate the specified window

			Usage:   ActivateWindow.exe  title  [ options ]

			Where:   title   is the window title

			Options: /D      Debug mode: show the screen coordinates used
			         /X      window title and specified title must match eXactly
			                 (default: window title contains specified title)

			Note:    Return code -1 in case of errors, otherwise 0.

			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "ActivateWindow.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Activate the specified window" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ActivateWindow.exe  title  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write( "title   " );
			Console.ResetColor( );
			Console.WriteLine( "is the window title" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/D      D" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ebug mode: show the screen coordinates used" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /X      " );
			Console.ResetColor( );
			Console.Error.Write( "window title and specified title must match e" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "X" );
			Console.ResetColor( );
			Console.Error.WriteLine( "actly" );

			Console.Error.Write( "                 (default: window title " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "contains " );
			Console.ResetColor( );
			Console.Error.WriteLine( "specified title)" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Note:    Return code -1 in case of errors, otherwise 0." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Display Help Text


			return -1;
		}

		#endregion Error handling
	}
}
