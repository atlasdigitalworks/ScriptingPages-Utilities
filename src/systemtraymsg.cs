using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class SystemTrayMessage
	{
		static string progver = "1.06";

		static int rc = 0;
		static NotifyIcon notifyicon;


		static int Main( string[] args )
		{
			#region Initialize Variables

			bool escapemessage = true;
			bool tooltip = false;
			bool wait = false;
			int timeout = 10000;
			int iconindex = 277;
			string[] searchpath = Environment.GetEnvironmentVariable( "PATH" ).Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
			string iconfile = "shell32.dll";
			string iconext = ".dll";
			Icon systrayicon = IconExtractor.Extract( iconfile, iconindex, true );
			ToolTipIcon tooltipicon = ToolTipIcon.Info;
			string message = String.Empty;
			string title = String.Empty;

			#endregion Initialize Variables


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
				if ( arg.ToUpper( ) == "/W" )
				{
					if ( wait )
					{
						return ShowHelp( "Duplicate command line switch /W" );
					}
					wait = true;
				}
				else if ( arg.ToUpper( ) == "/L" )
				{
					if ( !escapemessage )
					{
						return ShowHelp( "Duplicate command line switch /NE" );
					}
					escapemessage = false;
				}
				else if ( arg[0] == '/' )
				{
					if ( arg.Length > 3 && arg[2] == ':' )
					{
						switch ( arg[1].ToString( ).ToUpper( ) )
						{
							case "I":
								if ( tooltipicon != ToolTipIcon.Info )
								{
									return ShowHelp( "Duplicate command line switch /I" );
								}
								switch ( arg.Substring( 3 ).ToUpper( ) )
								{
									case "ERROR":
										tooltipicon = ToolTipIcon.Error;
										break;
									case "INFO":
										tooltipicon = ToolTipIcon.Info;
										break;
									case "NONE":
										tooltipicon = ToolTipIcon.None;
										break;
									case "WARNING":
										tooltipicon = ToolTipIcon.Warning;
										break;
									default:
										return ShowHelp( "Invalid tooltip icon type \"{0}\"", arg.Substring( 3 ) );
								}
								break;
							case "P":
								if ( iconfile.ToUpper( ) != "SHELL32.DLL" )
								{
									return ShowHelp( "Duplicate command line switch /P" );
								}
								iconext = Path.GetExtension( arg.Substring( 3 ) ).ToLower( );
								if ( File.Exists( arg.Substring( 3 ) ) )
								{
									iconfile = Path.GetFullPath( arg.Substring( 3 ) );
								}
								else if ( iconext == ".dll" || iconext == ".exe" )
								{
									foreach ( string folder in searchpath )
									{
										if ( File.Exists( Path.Combine( folder, arg.Substring( 3 ) ) ) )
										{
											iconfile = Path.Combine( folder, arg.Substring( 3 ) );
											break;
										}
									}
									if ( !File.Exists( iconfile ) )
									{
										return ShowHelp( "Invalid path to icon file or library \"{0}\"", arg.Substring( 3 ) );
									}
								}
								break;
							case "S":
								if ( iconindex != 277 )
								{
									return ShowHelp( "Duplicate command line switch /S" );
								}
								try
								{
									iconindex = Convert.ToInt32( arg.Substring( 3 ) );
									if ( iconindex < 0 )
									{
										return ShowHelp( "Invalid system tray icon index \"{0}\"", arg.Substring( 3 ) );
									}
								}
								catch ( Exception )
								{
									return ShowHelp( "Invalid system tray icon argument \"{0}\"", arg );
								}
								break;
							case "T":
								if ( String.IsNullOrWhiteSpace( title ) )
								{
									title = arg.Substring( 3 ).Trim( "\" \t".ToCharArray( ) );
								}
								else
								{
									return ShowHelp( "Duplicate command line switch /T" );
								}
								break;
							case "V":
								if ( timeout != 10000 )
								{
									return ShowHelp( "Duplicate command line switch /V" );
								}
								try
								{
									timeout = 1000 * Convert.ToInt32( arg.Substring( 3 ) );
									if ( timeout < 1000 )
									{
										return ShowHelp( "Invalid time (\"{0}\"), must be greater than zero", arg.Substring( 3 ) );
									}
								}
								catch ( Exception )
								{
									return ShowHelp( "Invalid time \"{0}\"", arg.Substring( 3 ) );
								}
								break;
							default:
								return ShowHelp( "Invalid command line switch \"{0}\"", arg );
						}
					}
					else
					{
						return ShowHelp( "Invalid command line switch \"{0}\"", arg );
					}
				}
				else
				{
					if ( String.IsNullOrWhiteSpace( message ) )
					{
						message = arg;
						tooltip = true;
					}
					else
					{
						return ShowHelp( "Duplicate message on command line" );
					}
				}
			}
			
			if ( String.IsNullOrWhiteSpace( message ) )
			{
				tooltip = false;
			}
			
			if ( tooltip && escapemessage )
			{
				message = Regex.Replace( message, @"(?<!\\)\\n", "\n" );
				message = Regex.Replace( message, @"(?<!\\)\\t", "\t" );
				message = Regex.Replace( message, @"\\", @"\" );
			}

			if ( iconext == ".dll" )
			{
				systrayicon = IconExtractor.Extract( iconfile, iconindex, true );
			}
			else
			{
				systrayicon = new Icon( iconfile );
			}

			#endregion Parse Command Line


			notifyicon = new NotifyIcon( );
			notifyicon.Icon = systrayicon;
			notifyicon.Visible = true;

			if ( tooltip )
			{
				if ( wait )
				{
					// Wait for the icon/message to be clicked
					notifyicon.BalloonTipClicked += ( object sender, EventArgs e ) => { rc = 2; };
					notifyicon.BalloonTipClosed += ( object sender, EventArgs e ) => { rc = 2; };
				}
				notifyicon.ShowBalloonTip( timeout, title, message, tooltipicon );
			}

			if ( wait )
			{
				rc = 3;
				while ( timeout > 0 && rc == 3 )
				{
					Thread.Sleep( 1 );
					Application.DoEvents( );
					timeout--;
				}
				notifyicon.Visible = false;
				notifyicon.Dispose( );
			}

			return rc;
		}


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
			SystemTrayMessage.exe,  Version 1.06
			Display an icon in the system tray's notification area
			
			Usage:    SystemTrayMessage.exe  [ message ]  [ options ]
			
			Where:    message     is the message text in the optional tooltip balloon
			
			Options:  /I:icon     tooltip Icon (Error, Info, None, Warning; default: Info)
			          /L          treat message as Literal text without interpreting
			                      escaped characters, e.g. show "\n" as literal "\n"
			                      instead of interpreting it as a newline character
			          /P:path     Path to an icon file or library (default: Shell32.dll)
			          /S:index    System tray icon index in icon library (default: 277)
			          /T:title    specifies the optional Title in the tooltip balloon
			          /V:seconds  specifies the number of seconds the tooltip balloon
			                      will remain Visible (default: 10)
			          /W          Wait for the timeout to elapse or for the user to click
			                      the message, then remove the icon from the notification
			                      area (default: exit without waiting)
			
			Notes:    If no tooltip message is specified, switches /I, /L and /T will be
			          ignored and no tooltip will be shown.
			          By default, \n is interpreted as newline and \t as tab in message;
			          in some cases this may lead to misinterpretations, e.g. when showing
			          a path like "c:\temp"; either escape backslashes in paths or use /L
			          to treat all message text as literal text.
			          Command line switch /S will be ignored if switch /P specifies
			          anything but an icon library.
			          Use my Shell32Icons.exe to select a Shell32 icon and get its index.
			          Return code ("ErrorLevel") is -1 in case of errors; with /W switch,
			          return code is 2 if message balloon is clicked, or 3 if the timeout
			          expired without clicking; otherwise return code is 0.
			
			Credits:  Code to extract icons from Shell32.dll by Thomas Levesque
			          http://stackoverflow.com/questions/6873026
			
			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "SystemTrayMessage.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Display an icon in the system tray's notification area" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "SystemTrayMessage.exe  [ message ]  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "message" );
			Console.ResetColor( );
			Console.Error.Write( "     is the " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "message" );
			Console.ResetColor( );
			Console.Error.WriteLine( " text in the optional tooltip balloon" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/I:icon" );
			Console.ResetColor( );
			Console.Error.Write( "     tooltip " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "I" );
			Console.ResetColor( );
			Console.Error.WriteLine( "con (Error, Info, None, Warning; default: Info)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /L" );
			Console.ResetColor( );
			Console.Error.Write( "          treat " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "message" );
			Console.ResetColor( );
			Console.Error.Write( " as " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "iteral text without interpreting" );

			Console.Error.WriteLine( "                      escaped characters, e.g. show \"\\n\" as literal \"\\n\"" );

			Console.Error.WriteLine( "                      instead of interpreting it as a newline character" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /P:path     P" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ath to an icon file or library (default: Shell32.dll)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /S:index    S" );
			Console.ResetColor( );
			Console.Error.Write( "ystem tray icon " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "index" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  in icon library (default: 277)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /T:title" );
			Console.ResetColor( );
			Console.Error.Write( "    optional " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "itle in the tooltip balloon" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /V:seconds" );
			Console.ResetColor( );
			Console.Error.Write( "  number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "seconds" );
			Console.ResetColor( );
			Console.Error.WriteLine( " the tooltip balloon will remain" );

			Console.Error.Write( "" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                      V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "isible (default: 10)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /W          W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ait for the timeout to elapse or for the user to click" );

			Console.Error.WriteLine( "                      the message, then remove the icon from the notification" );

			Console.Error.WriteLine( "                      area (default: exit without waiting)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:    If no tooltip " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "message" );
			Console.ResetColor( );
			Console.Error.Write( " is specified, switches " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/I" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/L" );
			Console.ResetColor( );
			Console.Error.Write( " and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.WriteLine( " will be" );

			Console.Error.WriteLine( "          ignored and no tooltip will be shown." );

			Console.Error.Write( "          By default, " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\\n" );
			Console.ResetColor( );
			Console.Error.Write( " is interpreted as newline and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\\t" );
			Console.ResetColor( );
			Console.Error.Write( " as tab in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "message" );
			Console.ResetColor( );
			Console.Error.WriteLine( ";" );

			Console.Error.WriteLine( "          in some cases this may lead to misinterpretations, e.g. when showing" );

			Console.Error.Write( "          a path like \"c:\\temp\"; either escape backslashes in paths or use " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "/L" );
			Console.ResetColor( );

			Console.Error.WriteLine( "          to treat all message text as literal text." );

			Console.Error.Write( "          Command line switch " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.Write( " will be ignored if switch " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/P" );
			Console.ResetColor( );
			Console.Error.WriteLine( " specifies" );

			Console.Error.WriteLine( "          anything but an icon library." );

			Console.Error.WriteLine( "          Use my Shell32Icons.exe to select a Shell32 icon and get its index." );

			Console.Error.Write( "          Return code (\"ErrorLevel\") is -1 in case of errors; with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch," );

			Console.Error.WriteLine( "          return code is 2 if message balloon is clicked, or 3 if the timeout" );

			Console.Error.WriteLine( "          expired without clicking; otherwise return code is 0." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits:  Code to extract icons from Shell32.dll by Thomas Levesque" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "          http://stackoverflow.com/questions/6873026" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Help Text


			return -1;
		}


		// Code to extract icons from Shell32.dll by Thomas Levesque
		// http://stackoverflow.com/questions/6873026
		public class IconExtractor
		{
			public static Icon Extract( string file, int number, bool largeIcon )
			{
				IntPtr large;
				IntPtr small;
				ExtractIconEx( file, number, out large, out small, 1 );
				try
				{
					return Icon.FromHandle( largeIcon ? large : small );
				}
				catch
				{
					return null;
				}
			}

			[DllImport( "Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall )]
			private static extern int ExtractIconEx( string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons );
		}
	}
}
