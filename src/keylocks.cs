using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class KeyLocks
	{
		static string progver = "1.01";

		#region Global Variables

		static string keylocks = String.Empty;
		static int keystatus = -1;
		static bool capslock = Control.IsKeyLocked( Keys.CapsLock );
		static bool numlock = Control.IsKeyLocked( Keys.NumLock );
		static bool scrolllock = Control.IsKeyLocked( Keys.Scroll );
		static bool insert = Control.IsKeyLocked( Keys.Insert );
		static bool returncapslockonly = false;
		static bool returnnumlockonly = false;
		static bool returnscrolllockonly = false;
		static bool returninsertonly = false;

		#endregion Global Variables


		static int Main( string[] args )
		{
			int interval = 1;
			bool loop = false;
			bool returnansi = false;
			bool settitle = false;

			#region Parse Command Line

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' && arg.Length > 1 )
				{
					switch ( arg[1] )
					{
						case '?':
							return ShowHelp( );
						case 'a':
						case 'A':
							if ( returnansi )
							{
								return ShowHelp( "Duplicate command line switch /A" );
							}
							returnansi = true;
							break;
						case 'c':
						case 'C':
							if ( returncapslockonly )
							{
								return ShowHelp( "Duplicate command line switch /C" );
							}
							returncapslockonly = true;
							break;
						case 'i':
						case 'I':
							if ( returninsertonly )
							{
								return ShowHelp( "Duplicate command line switch /I" );
							}
							returninsertonly = true;
							break;
						case 'l':
						case 'L':
							if ( loop )
							{
								return ShowHelp( "Duplicate command line switch /L" );
							}
							loop = true;
							if ( arg.Length > 3 && arg[2] == ':' )
							{
								if ( !Int32.TryParse( arg.Substring( 3 ), out interval ) )
								{
									return ShowHelp( "Invalid loop interval " + arg );
								}
								if ( interval < 1 || interval > 10 )
								{
									return ShowHelp( "Loop interval " + interval + " outside allowed range (1..10)" );
								}
							}
							break;
						case 'n':
						case 'N':
							if ( returnnumlockonly )
							{
								return ShowHelp( "Duplicate command line switch /I" );
							}
							returnnumlockonly = true;
							break;
						case 's':
						case 'S':
							if ( returnscrolllockonly )
							{
								return ShowHelp( "Duplicate command line switch /S" );
							}
							returnscrolllockonly = true;
							break;
						case 't':
						case 'T':
							if ( settitle )
							{
								return ShowHelp( "Duplicate command line switch /S" );
							}
							settitle = true;
							break;
						default:
							return ShowHelp( "Invalid command line argument \"{0}\"", arg );
					}
				}
				else
				{
					return ShowHelp( "Invalid command line argument \"{0}\"", arg );
				}
			}

			#endregion Parse Command Line

			if ( loop )
			{
				while ( true )
				{
					GetStatus( );
					if ( settitle )
					{
						SetTitle( );
					}
					Thread.Sleep( interval * 1000 );
				}
			}
			else
			{
				GetStatus( );
				if ( settitle )
				{
					SetTitle( );
				}
				if ( returnansi )
				{
					#region Build ANSI String

					int statusx = Console.BufferWidth - 4;
					int cursorx = Console.CursorLeft;
					int cursory = Console.CursorTop;
					string cursorsave = "\x1B[s";
					string cursorrestore = "\x1B[u";
					string clearline = "\x1B[K";
					string cursormove = "\x1B[1D ";
					string boldgreen = "\x1B[1;32m";
					string resetcolors = "\x1B[0m";
					bool bold = false;
					if ( cursorx < statusx )
					{
						cursormove = String.Format( "\x1B[{0}C", statusx - cursorx );
					}
					else if ( cursorx > statusx )
					{
						cursormove = String.Format( "\x1B[{0}D ", 1 + cursorx - statusx );
					}
					string status = String.Empty;
					if ( capslock )
					{
						bold = true;
						status += boldgreen + "C";
					}
					else
					{
						bold = false;
						status = "c";
					}
					if ( numlock )
					{
						if ( !bold )
						{
							bold = true;
							status += boldgreen;
						}
						status += "N";
					}
					else
					{
						if ( bold )
						{
							status += resetcolors;
							bold = false;
						}
						status += "n";
					}
					if ( scrolllock )
					{
						if ( !bold )
						{
							bold = true;
							status += boldgreen;
						}
						status += "S";
					}
					else
					{
						if ( bold )
						{
							status += resetcolors;
							bold = false;
						}
						status += "s";
					}
					if ( insert )
					{
						if ( !bold )
						{
							bold = true;
							status += boldgreen;
						}
						status += "I";
					}
					else
					{
						if ( bold )
						{
							status += resetcolors;
							bold = false;
						}
						status += "i";
					}
					if ( bold )
					{
						bold = false;
						status += resetcolors;
					}

					#endregion Build ANSI String

					Console.Write( "{0}{1}{2}{3}{4}", cursorsave, clearline, cursormove, status, cursorrestore );
				}
				else
				{
					Console.WriteLine( keylocks );
				}
			}

			return keystatus;
		}


		static void GetStatus( )
		{
			capslock = Control.IsKeyLocked( Keys.CapsLock );
			numlock = Control.IsKeyLocked( Keys.NumLock );
			scrolllock = Control.IsKeyLocked( Keys.Scroll );
			insert = Control.IsKeyLocked( Keys.Insert );
			if ( returncapslockonly )
			{
				keystatus = ( capslock ? 1 : 0 );
			}
			else if ( returninsertonly )
			{
				keystatus = ( insert ? 1 : 0 );
			}
			else if ( returnnumlockonly )
			{
				keystatus = ( numlock ? 1 : 0 );
			}
			else if ( returnscrolllockonly )
			{
				keystatus = ( scrolllock ? 1 : 0 );
			}
			else
			{
				keystatus = ( capslock ? 1 : 0 ) + ( numlock ? 2 : 0 ) + ( scrolllock ? 4 : 0 ) + ( insert ? 8 : 0 );
			}
			keylocks = ( capslock ? "C" : "c" ) + ( numlock ? "N" : "n" ) + ( scrolllock ? "S" : "s" ) + ( insert ? "I" : "i" );
		}


		static void SetTitle( )
		{
			string title = Console.Title;
			Console.Title = ( title + new String( ' ', 100 ) ).Substring( 0, 100 ) + keylocks;
		}


		static int ShowHelp( params string[] errmsg )

		{

			#region Help Text

			/*
			KeyLocks.exe,  Version 1.01
			Return status for CapsLock, NumLock, ScrollLock and Insert keys
 
			Usage:  KEYLOCKS   [ /A ]  [ /C | /I | /N | /S ]  [ /L[:sec] ]  [ /T ]
 
			Where:  /A         generate ANSI sequence of status
			        /C         return code 1 if CapsLock is on, otherwise 0
			        /I         return code 1 if Insert is on, otherwise 0
			        /L[:sec]   continuous Loop with interval in seconds (1..10; default: 1)
			        /N         return code 1 if NumLock is on, otherwise 0
			        /S         return code 1 if ScrollLock is on, otherwise 0
			        /T         show status in window Title

			Notes:  /L excludes all other switches except /T.
			        /T is of little use without /L as the title is restored to its
			        previous state (without status) as soon as this program terminates.
			        Switches /C, /I, /N and /S are all mutually exclusive.
					With /T status is shown in the console window title bar as CNSI where
			        C is for CapsLock, N for NumLock, etcetera; a capital character means
			        the key lock is on, a lower case character means it is off.
			        In case of (command line) errors, the return code ("errorlevel") is -1.
			        With /C, /I, /N and /S retun code is 0 for key lock off or 1 if on.
			        With /L return code is 0.
			        Otherwise the returncode represents the key locks status:
			        0 if all key locks are off, +1 if capsLock is on,
			        +2 if NumLock is on, +4 if ScrollLock is on, +8 if Insert is on.
 
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

			Console.Error.WriteLine( "KeyLocks.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Return status for CapsLock, NumLock, ScrollLock and Insert keys" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "KEYLOCKS   [ /A ]  [ /C | /I | /N | /S ]  [ /L[:sec] ]  [ /T ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/A" );
			Console.ResetColor( );
			Console.Error.Write( "         generate " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.WriteLine( "NSI sequence of status" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /C" );
			Console.ResetColor( );
			Console.Error.Write( "         return code 1 if " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "C" );
			Console.ResetColor( );
			Console.Error.WriteLine( "apsLock is on, otherwise 0" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /I" );
			Console.ResetColor( );
			Console.Error.Write( "         return code 1 if " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "I" );
			Console.ResetColor( );
			Console.Error.WriteLine( "nsert is on, otherwise 0" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /L[:sec]" );
			Console.ResetColor( );
			Console.Error.Write( "   continuous " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.Write( "oop with interval in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "sec" );
			Console.ResetColor( );
			Console.Error.WriteLine( "onds (1..10; default: 1)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /N" );
			Console.ResetColor( );
			Console.Error.Write( "         return code 1 if " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "N" );
			Console.ResetColor( );
			Console.Error.WriteLine( "umLock is on, otherwise 0" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /S" );
			Console.ResetColor( );
			Console.Error.Write( "         return code 1 if " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "crollLock is on, otherwise 0" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /T" );
			Console.ResetColor( );
			Console.Error.Write( "         show status in window " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "itle" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/L" );
			Console.ResetColor( );
			Console.Error.Write( " excludes all other switches except " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /T" );
			Console.ResetColor( );
			Console.Error.Write( " is of little use without " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/L" );
			Console.ResetColor( );
			Console.Error.WriteLine( " as the title is restored to its" );

			Console.Error.WriteLine( "        previous state (without status) as soon as this program terminates." );

			Console.Error.Write( "        Switches " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/C" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/I" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/N" );
			Console.ResetColor( );
			Console.Error.Write( " and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " are all mutually exclusive." );

			Console.Error.Write( "        With " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.Write( " status is shown in the console window title bar as " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "CNSI" );
			Console.ResetColor( );
			Console.Error.WriteLine( " where" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        C" );
			Console.ResetColor( );
			Console.Error.Write( " is for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "C" );
			Console.ResetColor( );
			Console.Error.Write( "apsLock, " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "N" );
			Console.ResetColor( );
			Console.Error.Write( " for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "N" );
			Console.ResetColor( );
			Console.Error.WriteLine( "umLock, etcetera; a capital character means" );

			Console.Error.WriteLine( "        the key lock is on, a lower case character means it is off." );

			Console.Error.WriteLine( "        In case of (command line) errors, the return code (\"errorlevel\") is -1." );

			Console.Error.Write( "        With " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/C" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/I" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/N" );
			Console.ResetColor( );
			Console.Error.Write( " and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " retun code is 0 for key lock off or 1 if on." );

			Console.Error.Write( "        With " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/L" );
			Console.ResetColor( );
			Console.Error.WriteLine( " return code is 0." );

			Console.Error.WriteLine( "        Otherwise the returncode represents the key locks status:" );

			Console.Error.WriteLine( "        0 if all key locks are off, +1 if capsLock is on," );

			Console.Error.WriteLine( "        +2 if NumLock is on, +4 if ScrollLock is on, +8 if Insert is on." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Display Help Text


			return -1;
		}
	}
}