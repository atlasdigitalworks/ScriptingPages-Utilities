using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class Which
	{
		public static string progver = "1.48";

		public static char switchchar = '/';

		[STAThread]
		static int Main( string[] args )
		{
			#region Initialize Variables

			string[] path = String.Format( "{0};{1}", Environment.CurrentDirectory, Environment.GetEnvironmentVariable( "PATH" ) ).Split( ";".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );
			string[] pathext = ( ";" + Environment.GetEnvironmentVariable( "PATHEXT" ).ToLower( ) ).Split( ';' ); // unlike PATH, do NOT remove empty entries, we REQUIRE the first one to be empty
			string prog = string.Empty;
			string result = String.Empty;
			bool all = false;
			bool copy = false;
			bool extonly = false;
			bool filever = false;
			bool openexp = false;
			bool prodver = false;
			bool set_all = false;
			bool set_copy = false;
			bool set_exp = false;
			bool set_ext = false;
			bool set_fver = false;
			bool set_prog = false;
			bool set_pver = false;
			bool found = false;

			#endregion Initialize Variables

			#region Command Line Parsing

			if ( args.Length == 0 )
			{
				return WriteError( );
			}

			int scd = 0;
			int scu = 0;

			foreach ( string arg in args )
			{
				if ( arg[0] == '-' )
				{
					scu += 1;
				}
				if ( arg[0] == '/' )
				{
					scd += 1;
				}
				if ( arg == "/?" )
				{
					return WriteError( );
				}
				if ( arg == "-?" || arg == "-h" || arg == "--help" )
				{
					switchchar = '-';
					return WriteError( );
				}
			}

			if ( scu > scd )
			{
				switchchar = '-';
			}

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' || arg[0] == '-' )
				{
					if ( arg.Length != 2 )
					{
						return WriteError( "Invalid command line switch {0}", ( switchchar == '/' ? arg.ToUpper( ) : arg.ToLower( ) ) );
					}
					switch ( arg[1].ToString( ).ToUpper( ) )
					{
						case "A":
							if ( set_all )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( arg[1] ) );
							}
							if ( set_exp )
							{
								return WriteError( "Command line switches {0} and {1} are mutually exclusive", SwitchString( arg[1] ), SwitchString( "E" ) );
							}
							all = true;
							set_all = true;
							break;
						case "C":
							if ( set_copy )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( arg[1] ) );
							}
							copy = true;
							set_copy = true;
							break;
						case "E":
							if ( set_exp )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( arg[1] ) );
							}
							if ( set_all )
							{
								return WriteError( "Command line switches {0} and {1} are mutually exclusive", SwitchString( "A" ), SwitchString( arg[1] ) );
							}
							openexp = true;
							set_exp = true;
							break;
						case "F":
							if ( set_fver )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( arg[1] ) );
							}
							if ( set_pver )
							{
								return WriteError( "Command line switches {0} and {1} are mutually exclusive", SwitchString( arg[1] ), SwitchString( "P" ) );
							}
							filever = true;
							set_fver = true;
							break;
						case "P":
							if ( set_pver )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( "P" ) );
							}
							if ( set_fver )
							{
								return WriteError( "Command line switches {0} and {1} are mutually exclusive", SwitchString( "F" ), SwitchString( arg[1] ) );
							}
							prodver = true;
							set_pver = true;
							break;
						case "X":
							if ( set_ext )
							{
								return WriteError( "Duplicate command line switch {0}", SwitchString( arg[1] ) );
							}
							extonly = true;
							set_ext = true;
							break;
						case "?":
							return WriteError( );
						default:
							return WriteError( "Invalid command line switch {0}", ( switchchar == '/' ? arg.ToUpper( ) : arg.ToLower( ) ) );
					}
				}
				else
				{
					if ( set_prog )
					{
						return WriteError( "Invalid or duplicate command line argument: \"{0}\"", arg );
					}
					else
					{
						char[] forbidden = { '\\', '?', '*', ':', ';', '/' };
						if ( arg.IndexOfAny( forbidden ) == -1 )
						{
							prog = arg;
							set_prog = true;
						}
						else
						{
							return WriteError( "Invalid characters in specified program name: \"{0}\"", arg );
						}
					}
				}
			}

			#endregion Command Line Parsing

			try
			{
				if ( !extonly )
				{
					#region DOSKEY macros

					// Try DOSKEY macros first
					Process doskey = new Process( );
					doskey.StartInfo.Arguments = "/macros";
					doskey.StartInfo.CreateNoWindow = false;
					doskey.StartInfo.FileName = Environment.GetFolderPath( Environment.SpecialFolder.System ) + "\\doskey.exe";
					doskey.StartInfo.LoadUserProfile = false;
					doskey.StartInfo.RedirectStandardError = false;
					doskey.StartInfo.RedirectStandardInput = false;
					doskey.StartInfo.RedirectStandardOutput = true;
					doskey.StartInfo.UseShellExecute = false;
					doskey.Start( );
					doskey.WaitForExit( 1000 );
					do
					{
						string line = doskey.StandardOutput.ReadLine( );
						if ( !found || all )
						{
							if ( !String.IsNullOrEmpty( line ) )
							{
								if ( line.IndexOf( '=' ) > 0 )
								{
									string pattern = "^" + prog.ToUpper( ).Replace( ".", "\\." ) + "=";
									if ( Regex.IsMatch( line.ToUpper( ), pattern ) )
									{
										Console.ForegroundColor = ConsoleColor.White;
										Console.Write( "[{0}]::", doskey.StartInfo.FileName.ToUpper( ) );
										Console.ResetColor( );
										Console.WriteLine( line );
										result += String.Format( "[{0}]::{1}\n", doskey.StartInfo.FileName.ToUpper( ), line );
										found = true;
									}
								}
							}
						}
					} while ( doskey.StandardOutput.Peek( ) != -1 );
					doskey.Close( );

					#endregion DOSKEY macros

					#region Internal commands

					// Next try internal commands
					if ( !found || all )
					{
						if ( prog.IndexOf( '.' ) == -1 )
						{
							if ( ListInternalCommands( ).Contains( prog.ToUpper( ) ) )
							{
								Console.ForegroundColor = ConsoleColor.White;
								Console.Write( "[{0}]::", Environment.GetEnvironmentVariable( "COMSPEC" ).ToUpper( ) );
								Console.ResetColor( );
								Console.WriteLine( prog.ToUpper( ) );
								result += String.Format( "[{0}]::{1}\n", Environment.GetEnvironmentVariable( "COMSPEC" ).ToUpper( ), prog.ToUpper( ) );
								found = true;
							}
						}
					}

					#endregion Internal commands
				}

				#region External commands

				// Finally try external commands
				if ( !found || all )
				{
					foreach ( string folder in path )
					{
						if ( !found || all )
						{
							string dir = ( folder + @"\" ).Replace( @"\\", @"\" );
							foreach ( string ext in pathext )
							{
								if ( !found || all )
								{
									// The EXTERNAL program FILE to be searched MUST have an extension, either
									// specified on the command line or one of the extensions listed in PATHEXT.
									if ( ( prog + ext ).IndexOf( '.' ) > -1 )
									{
										if ( File.Exists( dir + prog + ext ) )
										{
											string ver = String.Empty;
											if ( filever )
											{
												string fileversion = FileVersionInfo.GetVersionInfo( dir + prog + ext ).FileVersion;
												if ( String.IsNullOrEmpty( fileversion ) )
												{
													ver = String.Empty;
												}
												else
												{
													ver = String.Format( " (file version {0})", fileversion );
												}
											}
											else if ( prodver )
											{
												string productversion = FileVersionInfo.GetVersionInfo( dir + prog + ext ).ProductVersion;
												if ( String.IsNullOrEmpty( productversion ) )
												{
													ver = String.Empty;
												}
												else
												{
													ver = String.Format( " (product version {0})", productversion );
												}
											}
											Console.WriteLine( dir + prog + ext + ver );
											result += String.Format( "{0}{1}{2}{3}\n", dir, prog, ext, ver );
											found = true;
										}
									}
								}
							}
						}
					}
				}

				#endregion External commands

				if ( found )
				{
					#region Copy to clipboard

					if ( copy )
					{
						if ( !all )
						{
							result = result.TrimEnd( "\n".ToCharArray( ) );
						}
						Clipboard.SetText( result );
					}

					#endregion Copy to clipboard

					#region Open In Explorer

					if ( openexp )
					{
						string file = result.TrimEnd( "\n".ToCharArray( ) );
						string sel = String.Format( "/Select, {0}", file );
						ProcessStartInfo expl = new ProcessStartInfo( "Explorer.exe", sel );
						System.Diagnostics.Process.Start( expl );
					}

					#endregion Open In Explorer

					return 0;
				}
				else
				{
					return 1;
				}
			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}
		}


		public static List<string> ListInternalCommands( )
		{
			string comspec = Environment.GetEnvironmentVariable( "COMSPEC" );
			StreamReader file = new StreamReader( comspec, Encoding.ASCII );
			string content = file.ReadToEnd( );
			file.Close( );

			bool include = false;
			List<string> intcmds = new List<string>( );
			string excludestr = "ABOVENORMAL,AFFINITY,ANSI,APPICON,ASCII,AZ,BAT,BELOWNORMAL,BOTH,CMD,CMDCMDLINE,CMDEXTVERSION,COM,COMSPEC,CONFIG,COPYCMD,COPYRIGHT,CRLF,CSVFS,CTRL,CURRENT,DEFINED,DIRCMD,DISABLEDELAYEDEXPANSION,DISABLEEXTENSIONS,DLL,DO,DOC,DOS,DWORD,ENABLEDELAYEDEXPANSION,ENABLEEXTENSIONS,ELSE,ENTER,EOF,EQU,ERROR,ERRORLEVEL,EXE,EXIST,EXISTS,EXPAND,FALSE,FAT,FH,GEQ,GTR,GUI,HIGH,HIGHESTNUMANODENUMBER,HH,HKEY,HSM,IDI,IDLE,IN,INFO,IS,JS,KERNEL,LEQ,LIST,LNK,LOCAL,LOW,LSS,MACHINE,MAX,MIN,MM,MSC,MUI,NEQ,NODE,NORMAL,NOT,NT,NTDLL,NTFS,NY,NYA,OFF,ON,OTHER,PATHEXT,PROCESSING,RANDOM,REALTIME,REFS,REG,REGEDT,SCRIPT,SEPARATE,SHARED,STACK,SYS,SZ,TEMP,TWO,UNC,UNCC,UNKNOWN,US,USER,VAR,VBS,VERSION,VS,WAIT,WA,WC,WD,WINDOWS,WKERNEL,WORD,WP,WS,WV,XCOPY,XP";
			string[] excludearr = excludestr.Split( ",".ToCharArray( ) );
			List<string> exclude = new List<string>( excludearr ); // Optimized for .NET Framework 2.0; in .NET Framework 3.5+ we might have used List<string> exclude = excludestr.Split( ",".ToCharArray( ) ).ToList<string>( );

			string pattern = @"([A-Z]\0){2,}";
			Regex regex = new Regex( pattern );
			if ( regex.IsMatch( content ) )
			{
				foreach ( Match match in regex.Matches( content ) )
				{
					string intcmd = match.ToString( ).Replace( "\0", String.Empty );
					if ( intcmd == "CD" ) // The start of the commands list, as found in Windows 7 SP1, EN-GB
					{
						include = true;
					}
					if ( intcmd == "ERRORLEVEL" ) // The end of the commands list, as found in Windows 7 SP1, EN-GB
					{
						include = false;
					}
					if ( include && !exclude.Contains( intcmd ) && !intcmds.Contains( intcmd ) )
					{
						intcmds.Add( intcmd );
					}
				}
				intcmds.Sort( );
			}
			if ( intcmds.Count == 0 )
			{
				// Return a default list if we could not find the internal commands in %COMSPEC%
				string defaultinternalcommands = @"ASSOC,BREAK,CALL,CD,CHDIR,CLS,COLOR,COPY,DATE,DEL,DIR,DPATH,ECHO,ENDLOCAL,ERASE,EXIT,FOR,FTYPE,GOTO,IF,KEYS,MD,MKDIR,MKLINK,MOVE,PATH,PAUSE,POPD,PROMPT,PUSHD,RD,REM,REN,RENAME,RMDIR,SET,SETLOCAL,SHIFT,START,TIME,TITLE,TYPE,VER,VERIFY,VOL";
				string[] defaultintcmdarr = defaultinternalcommands.Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );
				intcmds = new List<string>( defaultintcmdarr );
			}
			return intcmds;
		}


		public static string SwitchString( char sw )
		{
			return SwitchString( sw.ToString( ) );
		}


		public static string SwitchString( string sw )
		{
			if ( switchchar == '-' )
			{
				return String.Format( "{0}{1}", switchchar, sw.ToLower( ) );
			}
			else
			{
				return String.Format( "{0}{1}", switchchar, sw.ToUpper( ) );
			}
		}


		public static int WriteError( params string[] errmsg )
		{
			/*
			Which,  Version 1.47
			Port of the UNIX command to Windows

			Usage:   WHICH  progname  [ /A | /E ]  [ /C ]  [ /F | /P ]  [ /X ]

			Where:   progname   the program name or internal command to be searched for
			         /A         returns All matches (default: stop at first match)
			         /C         Copies result to clipboard
			         /E         opens Explorer with result selected, if it is a file
			         /F         returns name and File version for external commands
			         /P         returns name and Product version for external commands
			         /X         returns eXternal commands only, no DOSKEY macros or
			                    internal commands   (default: all types)

			Note:    By default, this program first compares the specified name against
			         a list of active DOSKEY macros, next against a list of CMD's
			         internal commands, and finally it tries to find a matching program
			         file in the current directory and the PATH, using the extensions
			         listed in PATHEXT.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			if ( errmsg.Length > 0 )
			{
				List<string> errargs = new List<string>( errmsg );
				errargs.RemoveAt( 0 );
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR: " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errmsg[0], errargs.ToArray( ) );
				Console.ResetColor( );
			}

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Which,  Version {0}", progver );
			Console.Error.WriteLine( "Port of the UNIX command to Windows" );
			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			if ( switchchar == '/' )
			{
				Console.Error.WriteLine( "WHICH  progname  [ /A | /E ]  [ /C ]  [ /F | /P ]  [ /X ]" );
			}
			else
			{
				Console.Error.WriteLine( "which  progname  [ -a | -e ]  [ -c ]  [ -f | -p ]  [ -x ]" );
			}
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "progname" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   the program name or internal command to be searched for" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}", SwitchString( "A" ) );
			Console.ResetColor( );
			Console.Error.Write( "         returns " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ll matches (default: stop at first match)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}         C", SwitchString( "C" ) );
			Console.ResetColor( );
			Console.Error.WriteLine( "opies result to clipboard" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}", SwitchString( "E" ) );
			Console.ResetColor( );
			Console.Error.Write( "         opens " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "E" );
			Console.ResetColor( );
			Console.Error.Write( "xplorer with result selected, " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "if" );
			Console.ResetColor( );
			Console.Error.WriteLine( " it is a file" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}", SwitchString( "F" ) );
			Console.ResetColor( );
			Console.Error.Write( "         returns name and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "F" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ile version for external commands" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}", SwitchString( "P" ) );
			Console.ResetColor( );
			Console.Error.Write( "         returns name and " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "P" );
			Console.ResetColor( );
			Console.Error.WriteLine( "roduct version for external commands" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         {0}", SwitchString( "X" ) );
			Console.ResetColor( );
			Console.Error.Write( "         returns e" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "X" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ternal commands only, no DOSKEY macros or" );

			Console.Error.WriteLine( "                    internal commands   (default: all types)" );

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Note:    By default, this program first compares the specified name against" );
			Console.Error.WriteLine( "         a list of active DOSKEY macros, next against a list of CMD's" );
			Console.Error.WriteLine( "         internal commands, and finally it tries to find a matching program" );
			Console.Error.WriteLine( "         file in the current directory and the PATH, using the extensions" );
			Console.Error.WriteLine( "         listed in PATHEXT." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}
