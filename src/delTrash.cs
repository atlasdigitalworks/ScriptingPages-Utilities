using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;


namespace RobvanderWoude
{
	class DelTrash
	{
		static string progver = "1.00";


		static int Main( string[] args )
		{
			bool confirm = false;
			bool progress = false;
			RecycleFlags flags = RecycleFlags.RecycleNoSound;
			string drive = null; // default: All drives
			string windrive = Path.GetPathRoot( Environment.SystemDirectory );

			
			#region Parse Command Line

			if ( args.Length > 3 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				switch ( arg.ToUpper( ) )
				{
					case "/?":
						return ShowHelp( );
					case "/C":
						if ( confirm )
						{
							return ShowHelp( "Duplicate command line switch /C" );
						}
						confirm = true;
						break;
					case "/P":
						if ( progress )
						{
							return ShowHelp( "Duplicate command line switch /P" );
						}
						progress = true;
						break;
					case "/W":
						if ( drive != null )
						{
							if ( drive == windrive )
							{
								return ShowHelp( "Either specify a drive letter or use the /W switch, not both" );
							}
							else
							{
								return ShowHelp( "Duplicate drive specification {0} and {1}", drive, arg.ToUpper( ) );
							}
						}
						drive = windrive;
						break;
					default:
						bool validdrive = false;
						DriveInfo[] alldrives = DriveInfo.GetDrives( );
						foreach ( DriveInfo drvinf in alldrives )
						{
							if ( drvinf.Name == arg.ToUpper( ) + "\\" )
							{
								validdrive = true;
								drive = arg.ToUpper( );
							}
						}
						if ( !validdrive )
						{
							return ShowHelp( "Invalid command line argument \"{0}\"", arg );
						}
						break;
				}
			}

			if ( !confirm )
			{
				flags |= RecycleFlags.RecycleNoConfirmation;
			}
			if ( !progress )
			{
				flags |= RecycleFlags.RecycleNoProgressUI;
			}

			#endregion Parse Command Line


			uint result = SHEmptyRecycleBin( IntPtr.Zero, drive, flags );
			if ( result == 0 )
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}


		static int ShowHelp( params string[] errmsg )
		{
			#region Help Text

			/*
			DelTrash.exe,  Version 1.00
			Empty the Recycle Bin
			
			Usage:   DelTrash.exe  [ drive: | /W ]  [ /C ]  [ /P ]
			
			Where:   drive:  empty recycle bin on this drive only (default: all drives)
			         /W      empty recycle bin on Windows' drive only (default: all drives)
			         /C      prompt for Confirmation
			         /P      show Progress
			
			Note:    Return code is 0 if the Recycle Bin was emptied successfully, or 1
			         if there was nothing to delete or in case of (command line) errors.
			
			Credits: Based on code by Vinoth Kumar
			         www.codeproject.com/Articles/20172/Empty-the-Recycle-Bin-using-C

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

			Console.Error.WriteLine( "DelTrash.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Empty the Recycle Bin" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "DelTrash.exe  [ drive: | /W ]  [ /C ]  [ /P ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "drive:" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  empty recycle bin on this drive only (default: all drives)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /W" );
			Console.ResetColor( );
			Console.Error.Write( "      empty recycle bin on " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "indows' drive only (default: all drives)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /C" );
			Console.ResetColor( );
			Console.Error.Write( "      prompt for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "C" );
			Console.ResetColor( );
			Console.Error.WriteLine( "onfirmation" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /P" );
			Console.ResetColor( );
			Console.Error.Write( "      show " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "P" );
			Console.ResetColor( );
			Console.Error.WriteLine( "rogress" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Note:    Return code is 0 if the Recycle Bin was emptied successfully, or 1" );

			Console.Error.WriteLine( "         if there was nothing to delete or in case of (command line) errors." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits: Based on code by Vinoth Kumar" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         www.codeproject.com/Articles/20172/Empty-the-Recycle-Bin-using-C" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help Text

			return 1;
		}


		public enum RecycleFlags : uint
		{
			RecycleNoConfirmation = 0x00000001,
			RecycleNoProgressUI = 0x00000002,
			RecycleNoSound = 0x00000004
		}


		[DllImport( "Shell32.dll", CharSet = CharSet.Unicode )]
		static extern uint SHEmptyRecycleBin( IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags );
	}
}
