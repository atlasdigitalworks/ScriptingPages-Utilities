using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.IO;

using System.Text;

using System.Text.RegularExpressions;

using System.Windows.Forms;

 

 

 

namespace RobvanderWoude

{

	class ListIntCmd

	{

		public static string progver = "1.05";

 

 

		[STAThreadAttribute]

		static int Main( string[] args )

		{

			#region Initialize variables

 

			int rc = 0;

			string separartor = "\n";

			bool separatorset = false;

			bool copy = false;

			bool copyset = false;

			bool logging = false;

			bool logset = false;

			string logdir = Directory.GetParent( System.Reflection.Assembly.GetEntryAssembly( ).Location ).ToString( ); // This program's parent folder

			string logfile = Path.Combine( logdir, "ListIntCmd.log" );

			bool logfileset = false;

			string logtext = String.Empty;

			DateTime logbegin;

			DateTime logend;

 

			#endregion Initialize variables

 

 

			#region Command Line Parsing

 

			if ( args.Length > 0 )

			{

				foreach ( string arg in args )

				{

					switch ( arg.Substring( 0, Math.Min( 2, arg.Length ) ).ToUpper( ) )

					{

 

						case "/?":

							return WriteError( );

						case "/C":

							if ( copyset )

							{

								return WriteError( "Duplicate command line switch /C" );

							}

							copy = true;

							copyset = true;

							break;

						case "/L":

							if ( logset )

							{

								return WriteError( "Duplicate command line switch /L" );

							}

							logging = true;

							logset = true;

							if ( arg.Length > 4 && arg[2] == ':' )

							{

								logfile = Environment.ExpandEnvironmentVariables( arg.Substring( 3 ).Trim( "\" ".ToCharArray( ) ) );

								logdir = Directory.GetParent( logfile ).ToString( );

								logfileset = true;

							}

							break;

						default:

							if ( separatorset )

							{

								return WriteError( );

							}

							// Translate:            \n to linefeed,          \t to tab,            \/ to slash,        \\ to backslash, escaped \n to literal \n, escaped \t to literal \t

							separartor = arg.Replace( "\\n", "\n" ).Replace( "\\t", "\t" ).Replace( "\\/", "/" ).Replace( "\\\\", "\\" ).Replace( "\\\n", "\\n" ).Replace( "\\\t", "\\t" );

							separatorset = true;

							break;

					}

				}

			}

 

			if ( logfileset && !String.IsNullOrEmpty( logfile ) )

			{

				if ( !Directory.Exists( logdir ) )

				{

					return WriteError( String.Format( "Invalid log path \"{0}\"", logdir ) );

				}

			}

 

			#endregion Command Line Parsing

 

 

			try

			{

				string comspec = Environment.GetEnvironmentVariable( "COMSPEC" );

 

				logbegin = DateTime.Now;

				logtext += String.Format( "ListIntCmd.exe,  Version {0}\n", progver );

				logtext += String.Format( "{0}\n", Environment.OSVersion.VersionString );

				logtext += String.Format( "Search started at {0}\n\n", logbegin.ToString( "yyyy-MM-dd, HH:mm:ss.fff" ) );

				logtext += String.Format( "COMSPEC=\"{0}, ProductVersion={1}\"\n\n", comspec, FileVersionInfo.GetVersionInfo( comspec ).ProductVersion );

 

				StreamReader file = new StreamReader( comspec, Encoding.ASCII );

				string content = file.ReadToEnd( );

				file.Close( );

 

				List<string> intcmds = new List<string>( );

				//string excludestr = "ABOVENORMAL,AFFINITY,APPICON,BAT,BELOWNORMAL,CMD,CMDCMDLINE,CMDEXTVERSION,COM,COMSPEC,COPYCMD,COPYRIGHT,CRLF,CSVFS,DEFINED,DIRCMD,DISABLEDELAYEDEXPANSION,DISABLEEXTENSIONS,DLL,DO,ENABLEDELAYEDEXPANSION,ENABLEEXTENSIONS,ELSE,EOF,EQU,ERRORLEVEL,EXE,EXIST,FAT,FH,GEQ,GTR,HIGH,HIGHESTNUMANODENUMBER,HH,IDI,IN,INFO,JS,KERNEL,LEQ,LIST,LOW,LSS,MAX,MIN,MM,MSC,MUI,NEQ,NODE,NORMAL,NOT,NTDLL,NTFS,NY,OFF,ON,PATHEXT,RANDOM,REALTIME,REFS,SCRIPT,SEPARATE,SHARED,UNCC,US,VBS,VERSION,VS,WAIT,WA,WC,WD,WINDOWS,WP,WS,XCOPY";

				// extended exclusion string for Windows 2000 and XP; the longer the exclusion string, the higher the risk that some new future internal command will be omited in the output

				string excludestr = "ABOVENORMAL,AFFINITY,ANSI,APPICON,ASCII,AZ,BAT,BELOWNORMAL,BOTH,CMD,CMDCMDLINE,CMDEXTVERSION,COM,COMSPEC,CONFIG,COPYCMD,COPYRIGHT,CRLF,CSVFS,CTRL,CURRENT,DEFINED,DIRCMD,DISABLEDELAYEDEXPANSION,DISABLEEXTENSIONS,DLL,DO,DOC,DOS,DWORD,ENABLEDELAYEDEXPANSION,ENABLEEXTENSIONS,ELSE,ENTER,EOF,EQU,ERROR,ERRORLEVEL,EXE,EXIST,EXISTS,EXPAND,FALSE,FAT,FH,GEQ,GTR,GUI,HIGH,HIGHESTNUMANODENUMBER,HH,HKEY,HSM,IDI,IDLE,IN,INFO,IS,JS,KERNEL,LEQ,LIST,LNK,LOCAL,LOW,LSS,MACHINE,MAX,MIN,MM,MSC,MUI,NEQ,NODE,NORMAL,NOT,NT,NTDLL,NTFS,NY,NYA,OFF,ON,OTHER,PATHEXT,PROCESSING,RANDOM,REALTIME,REFS,REG,REGEDT,SCRIPT,SEPARATE,SHARED,STACK,SYS,SZ,TEMP,TWO,UNC,UNCC,UNKNOWN,US,USER,VAR,VBS,VERSION,VS,WAIT,WA,WC,WD,WINDOWS,WKERNEL,WORD,WP,WS,WV,XCOPY,XP";

				string[] excludearr = excludestr.Split( ",".ToCharArray( ) );

				List<string> exclude = new List<string>( excludearr ); // Optimized for .NET Framework 2.0; in .NET Framework 3.5+ we might have used List<string> exclude = excludestr.Split( ",".ToCharArray( ) ).ToList<string>( );

 

				string pattern = @"([A-Z]\0){2,}";

				Regex regex = new Regex( pattern );

				if ( regex.IsMatch( content ) )

				{

					logtext += "List of regex matches:";

					foreach ( Match match in regex.Matches( content ) )

					{

						string line = String.Empty;

						string intcmd = match.ToString( ).Replace( "\0", String.Empty );

						line += String.Format( "\n{0,-24}", intcmd );

						if ( exclude.Contains( intcmd ) )

						{

							line += String.Format( "\texcluded by exlusion list", intcmd );

						}

						else if ( intcmds.Contains( intcmd ) )

						{

							line += String.Format( "\tskipped duplicate", intcmd );

						}

						else

						{

							intcmds.Add( intcmd );

							line += String.Format( "\tadded to the list of internal commands", intcmd );

						}

						logtext += line.TrimEnd( );

					}

					logtext += String.Format( "\n\nResults so far:\n\n{0}\n\n", String.Join( "\n", intcmds.ToArray( ) ) );

					intcmds.Sort( );

					logtext += String.Format( "\nResults after sorting:\n\n{0}\n\n", String.Join( "\n", intcmds.ToArray( ) ) );

				}

 

				// Return a default list if we could not find the internal commands in %COMSPEC%

				if ( intcmds.Count == 0 )

				{

					logtext += "\nUsing default, hard-coded list\n\n";

					string defintcmdsstr = "ASSOC,BREAK,CALL,CD,CHDIR,CLS,COLOR,COPY,DATE,DEL,DIR,DPATH,ECHO,ENDLOCAL,ERASE,EXIT,FOR,FTYPE,GOTO,IF,KEYS,MD,MKDIR,MKLINK,MOVE,PATH,PAUSE,POPD,PROMPT,PUSHD,RD,REM,REN,RENAME,RMDIR,SET,SETLOCAL,SHIFT,START,TIME,TITLE,TYPE,VER,VERIFY,VOL";

					string[] defintcmdsarr = defintcmdsstr.Split( ",".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );

					intcmds = new List<string>( defintcmdsarr ); // Optimized for .NET Framework 2.0; in .NET Framework 3.5+ we might have used List<string> intcmds = defintcmdsstr.Split( ",".ToCharArray( ) ).ToList<string>( );

					rc = 2;

				}

 

				string result = String.Join( separartor, intcmds.ToArray( ) );

 

				Console.Write( result );

 

				if ( copy )

				{

					logtext += "Writing result to clipboard . . .\n\n";

					Clipboard.SetText( result );

					logtext += String.Format( "Text written to clipboard:\n\n{0}\n\n", Clipboard.GetText( ) );

				}

 

				if ( logging )

				{

					logend = DateTime.Now;

					TimeSpan duration = logend - logbegin;

					logtext += String.Format( "Search ended at {0} ({1:N0} milliseconds)\n", logend.ToString( "yyyy-MM-dd, HH:mm:ss.fff" ), duration.TotalMilliseconds );

					StreamWriter logstream = new StreamWriter( logfile );

					logstream.Write( logtext );

					logstream.Close( );

				}

 

				return rc;

			}

			catch ( Exception e )

			{

				return WriteError( e.Message );

			}

		}

 

 

		#region Error Handling

 

		/*

		ListIntCmd.exe,  Version 1.05

		List all available internal commands

 

		Usage:  LISTINTCMD  [ "separator" ]  [ /C ]  [ /L[:logfile] ]

 

		Where:  separator     is the character or string used to separate the

		                      command names in the output (default: linefeed)

		        /C            Copies output to clipboard

		        /L[:logfile]  Logs the entire search process to logfile

		                      (default log file name and location:

		                      ListIntCmd.log in program's parent folder)

 

		Notes:  Use doublequotes if separator contains spaces or "special" characters.

		        separator accepts \n for linefeeds, \t for tabs, \/ for slashes,

		        \\ for backslashes, and """" for doublequotes.

		        Return code 2 if commands could not be retrieved and default list

		        had to be returned instead, 1 on other errors, 0 if all is well.

 

		Written by Rob van der Woude

		http://www.robvanderwoude.com

		*/

 

		public static int WriteError( string errormsg = null )

		{

			if ( !String.IsNullOrEmpty( errormsg ) )

			{

				Console.Error.WriteLine( );

				Console.ForegroundColor = ConsoleColor.Red;

				Console.Error.Write( "ERROR: " );

				Console.ForegroundColor = ConsoleColor.White;

				Console.Error.WriteLine( errormsg );

				Console.ResetColor( );

			}

 

			Console.Error.WriteLine( );

 

			Console.Error.WriteLine( "ListIntCmd.exe,  Version {0}", progver );

 

			Console.Error.WriteLine( "List all available internal commands" );

 

			Console.Error.WriteLine( );

 

			Console.Error.Write( "Usage:  " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.WriteLine( "LISTINTCMD  [ \"separator\" ]  [ /C ]  [ /L[:logfile] ]" );

			Console.ResetColor( );

 

			Console.Error.WriteLine( );

 

			Console.Error.Write( "Where:  " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "separator" );

			Console.ResetColor( );

			Console.Error.WriteLine( "     is the character or string used to separate the" );

 

			Console.Error.WriteLine( "                      command names in the output (default: linefeed)" );

 

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "        /C            C" );

			Console.ResetColor( );

			Console.Error.WriteLine( "opies output to clipboard" );

 

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "        /L[:logfile]  L" );

			Console.ResetColor( );

			Console.Error.Write( "ogs the entire search process to " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.WriteLine( "logfile" );

			Console.ResetColor( );

 

			Console.Error.WriteLine( "                      (default log file name and location:" );

 

			Console.Error.WriteLine( "                      ListIntCmd.log in program's parent folder)" );

 

			Console.Error.WriteLine( );

 

			Console.Error.Write( "Notes:  Use doublequotes if " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "separator" );

			Console.ResetColor( );

			Console.Error.WriteLine( " contains spaces or \"special\" characters." );

 

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "        separator" );

			Console.ResetColor( );

			Console.Error.Write( " accepts " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "\\n" );

			Console.ResetColor( );

			Console.Error.Write( " for linefeeds, " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "\\t" );

			Console.ResetColor( );

			Console.Error.Write( " for tabs, " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "\\/" );

			Console.ResetColor( );

			Console.Error.WriteLine( " for slashes," );

 

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "        \\\\" );

			Console.ResetColor( );

			Console.Error.Write( " for backslashes, and " );

			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.Write( "\"\"\"\"" );

			Console.ResetColor( );

			Console.Error.WriteLine( " for doublequotes." );

 

			Console.Error.WriteLine( "        Return code 2 if commands could not be retrieved and default list" );

 

			Console.Error.WriteLine( "        had to be returned instead, 1 on other errors, 0 if all is well." );

 

			Console.Error.WriteLine( );

 

			Console.Error.WriteLine( "Written by Rob van der Woude" );

 

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;

		}

 

		#endregion Error Handling

	}

}