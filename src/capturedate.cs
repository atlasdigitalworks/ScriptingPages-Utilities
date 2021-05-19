using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class CaptureDate
	{
		static readonly string progver = "1.03";


		#region Global Variables

		static bool confirmsettimestamp = true;
		static bool readabletimestamp = true;
		static bool recursive = false;
		static bool settimestamp = false;
		static bool wildcards = false;
		static double timediffthreshold = 3600;
		static int renamecount = 0;
		static string searchpattern = "*.*";

		#endregion Global Variables


		static int Main( string[] args )
		{
			#region Initialize Variables

			string startdir = Environment.CurrentDirectory; // in case no directory is specified
			string[] filespec;

			#endregion Initialize Variables


			#region Parse Command Line

			if ( args.Length == 0 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg == "/?" || arg.Length < 2 )
				{
					return ShowHelp( );
				}
				if ( arg[0] == '/' )
				{
					switch ( arg.Substring( 0, 2 ).ToUpper( ) )
					{
						case "/D":
							if ( arg.Length > 3 && arg[2] == ':' )
							{
								if ( timediffthreshold != 3600 )
								{
									return ShowHelp( "Duplicate switch: /D" );
								}
								try
								{
									timediffthreshold = Convert.ToDouble( arg.Substring( 3 ) );
								}
								catch
								{
									return ShowHelp( "Invalid value: \"{0}\"", arg );
								}
							}
							else
							{
								return ShowHelp( "Invalid value: \"{0}\"", arg );
							}
							break;
						case "/R":
							if ( recursive )
							{
								return ShowHelp( "Duplicate switch: /R" );
							}
							recursive = true;
							break;
						case "/S":
							if ( settimestamp )
							{
								return ShowHelp( "Duplicate switch: /S" );
							}
							settimestamp = true;
							break;
						case "/T":
							if ( !readabletimestamp )
							{
								return ShowHelp( "Duplicate switch: /T" );
							}
							readabletimestamp = false;
							break;
						case "/Y":
							if ( !confirmsettimestamp )
							{
								return ShowHelp( "Duplicate switch: /Y" );
							}
							confirmsettimestamp = false;
							break;
						default:
							return ShowHelp( "Invalid switch: \"{0}\"", arg );
					}
				}
				else
				{
					searchpattern = Path.GetFileName( arg );
					wildcards = ( searchpattern.IndexOfAny( "*?".ToCharArray( ) ) > -1 );
					startdir = Path.GetDirectoryName( arg );
					if ( !String.IsNullOrEmpty( startdir ) && !Directory.Exists( startdir ) )
					{
						return ShowHelp( "Folder not found: \"{0}\"", startdir );
					}
					filespec = Directory.GetFiles( startdir, searchpattern );
				}
			}
			if ( !confirmsettimestamp && !settimestamp )
			{
				return ShowHelp( "/Y switch is valid only when combined with /S switch." );
			}
			if ( timediffthreshold != 3600 && !settimestamp )
			{
				return ShowHelp( "/D switch is valid only when combined with /S switch." );
			}

			#endregion Parse Command Line


			ProcessFolder( startdir );
			
			return renamecount;
		}


		static void ProcessFolder( string folder )
		{
			string[] filespec = Directory.GetFiles( folder, searchpattern );
			foreach ( string filename in filespec )
			{
				ProcessImage( filename );
			}
			if ( recursive )
			{
				string[] subdirs = Directory.GetDirectories( folder );
				foreach ( string subdir in subdirs )
				{
					ProcessFolder( subdir );
				}
			}
		}


		static void ProcessImage( string filename )
		{
			#region Read First MB of File

			long filesize = new FileInfo( filename ).Length;
			int buffersize = Convert.ToInt32( Math.Min( filesize, 1048576 ) ); // Buffer size is 1 MB or file size, whichever is the smallest
			StreamReader file = new StreamReader( filename );
			char[] buffer = new Char[buffersize];
			_ = file.Read( buffer, 0, buffersize - 1 ); // Read only the first 1 MB of the file (or the entire file if smaller than 1 MB)
			file.Close( );
			string header = new String( buffer );

			if ( String.IsNullOrEmpty( header ) )
			{
				ShowHelp( "Could not open file \"{0}\"", filename );
				return;
			}

			#endregion Read First MB of File


			#region Find Earliest Date String

			string pattern = "\\b[12]\\d{3}:[01]\\d:[0-3]\\d [0-2]\\d:[0-5]\\d:[0-5]\\d\\b";
			Regex regexp = new Regex( pattern, RegexOptions.None );
			MatchCollection matches = regexp.Matches( header );
			if ( matches.Count == 0 )
			{
				ShowHelp( "No capture date found in file header" );
				return;
			}
			string photodatedelimited = String.Empty;
			foreach ( Match match in matches )
			{
				if ( String.IsNullOrEmpty( photodatedelimited ) || String.Compare( photodatedelimited, match.Value, StringComparison.Ordinal ) > 0 )
				{
					photodatedelimited = match.Value;
				}
			}
			photodatedelimited = photodatedelimited.Substring( 0, 10 ).Replace( ":", "-" ) + photodatedelimited.Substring( 10 );
			string photodatenodelims = photodatedelimited.Replace( ":", "" ).Replace( "-", "" ).Replace( " ", "T" );

			#endregion Find Earliest Date String


			if ( settimestamp )
			{
				string timeformat;
				if ( readabletimestamp )
				{
					timeformat = "{0:yyyy}-{0:MM}-{0:dd} {0:HH}:{0:mm}:{0:ss}";
				}
				else
				{
					timeformat = "{0:yyyy}{0:MM}{0:dd}T{0:HH}{0:mm}{0:ss}";
				}
				DateTime currenttimestamp = File.GetLastWriteTime( filename );
				if ( DateTime.TryParse( photodatedelimited, out DateTime newtimestamp ) ) // Try parsing the new file timestamp using the capture timestamp string
				{
					if ( DateTime.Compare( currenttimestamp, newtimestamp ) == 0 ) // File and capture timestamps are equal
					{
						string photodate = photodatenodelims;
						if ( readabletimestamp )
						{
							photodate = photodatedelimited;
						}
						if ( wildcards )
						{
							Console.WriteLine( "{0}\t\"{1}\"", photodate, filename );
						}
						else
						{
							Console.WriteLine( photodate );
						}
					}
					else
					{
						double timediff = Math.Abs( ( currenttimestamp - newtimestamp ).TotalSeconds ); // Calculate absolute value of timestamps' difference in seconds
						if ( timediff > timediffthreshold ) // Ignore time differences up to threshold (default 1 hour, or value set with /D switch)
						{
							string blanks = "\n" + new String( ' ', 70 ) + new String( '\b', 70 ); // String to erase the first 70 characters on the next line
							Console.WriteLine( "Image file name        : {0}", filename );
							Console.WriteLine( "Current file timestamp : " + timeformat, currenttimestamp );
							Console.WriteLine( "Capture timestamp      : " + timeformat, newtimestamp );
							if ( confirmsettimestamp )
							{
								Console.Write( "Do you want to change the file's timestamp to the capture time? [yN] " );
								string answer = Console.ReadKey( ).KeyChar.ToString( ).ToUpper( );
								if ( answer == "Y" )
								{
									File.SetLastWriteTime( filename, newtimestamp );
									Console.CursorTop -= 1; // Move the cursor 1 line up
									Console.WriteLine( blanks + "New file timestamp     : " + timeformat, File.GetLastWriteTime( filename ) ); // Overwrite prompt with new timestamp
									renamecount += 1;
								}
								else
								{
									Console.WriteLine( "\nskipping . . ." );
								}
							}
							else
							{
								File.SetLastWriteTime( filename, newtimestamp );
								Console.CursorTop -= 1; // Move the cursor 1 line up
								Console.WriteLine( blanks + "New file timestamp     : " + timeformat, File.GetLastWriteTime( filename ) ); // Overwrite prompt with new timestamp
								renamecount += 1;
							}
						}
						else
						{
							Console.WriteLine( photodatedelimited ); // Timespans' difference is not above threshold (default 1 hour, or value set with /D switch)
						}
					}
				}
				else
				{
					ShowHelp( "Could not determine timestamp of \"{0}\"", filename );
					return;
				}
			}
			else
			{
				Console.WriteLine( "{0}\t\"{1}\"", photodatedelimited, filename );
			}
		}


		static string Today( bool usedelims = true )
		{
			string dateformat;
			if ( usedelims )
			{
				dateformat = "{0:yyyy}-{0:MM}-{0:dd} {0:HH}:{0:mm}:{0:ss}";
			}
			else
			{
				dateformat = "{0:yyyy}{0:MM}{0:dd}T{0:HH}{0:mm}{0:ss}";
			}
			return String.Format( dateformat, DateTime.Now );
		}


		#region Error handling

		public static int ShowHelp( params string[] errmsg )
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
			CaptureDate,  Version 1.03
			Return the capture date and time for the specified image file

			Usage:   CAPTUREDATE  image  [ options ]
			
			Where:   image        specifies the image file(s) (wildcards allowed)

			Options: /D:seconds   minimum Difference in seconds between current file
			                      timestamp and capture date/time; if the difference
			                      exceeds the specified number of seconds, the file
			                      timestamp will be set to the capture date/time
			                      (default: 3600 seconds = 1 hour; requires /S switch)
			         /R           Recursive (include subdirectories); you probably want
			                      to use wildcards for image with this option
			         /S           Set the image file timestamp to the capture date/time
			         /T           return the timestamp without "-" and ":" delimiters,
			                      e.g. 20171114T135628 instead of 2017-11-14 13:56:28
			         /Y           do not ask for confirmation before changing the image
			                      file's timestamp (requires /S switch)
			
			Notes:   Result will be displayed on screen, e.g. 2017-11-14 13:56:28.
			         The date/time is extracted by searching for the earliest date/time
			         string in the first 1048576 bytes (1 MB) of the image file.
			         With /S switch used, the timestamp is changed only if the difference
			         between the current timestamp and the capture time exceeds 1 hour or
			         the threshold set with the /D switch.
			         The program will ask for confirmation before changing the file's
			         timestamp, unless the /Y switch is used.
			         Return code ("errorlevel") is -1 in case of errors, or with /S it
			         equals the number of files renamed, or 0 otherwise.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Display Help

			string timestampDelimited = Today( true );
			string timestampNodelims = Today( false );
			Console.Error.WriteLine( );

			Console.Error.WriteLine( "CaptureDate,  Version {0}", progver );

			Console.Error.WriteLine( "Return the capture date and time for the specified image file" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CAPTUREDATE  image  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "image" );
			Console.ResetColor( );
			Console.Error.WriteLine( "        specifies the image file(s) (wildcards allowed)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/D:seconds" );
			Console.ResetColor( );
			Console.Error.Write( "   minimum " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "D" );
			Console.ResetColor( );
			Console.Error.Write( "ifference in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "seconds" );
			Console.ResetColor( );
			Console.Error.WriteLine( " between current file" );

			Console.Error.WriteLine( "                      timestamp and capture date/time; if the difference" );

			Console.Error.WriteLine( "                      exceeds the specified number of seconds, the file" );

			Console.Error.WriteLine( "                      timestamp will be set to the capture date/time" );

			Console.Error.Write( "                      (default: 3600 seconds = 1 hour; requires " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /R           R" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ecursive (include subdirectories); you probably want" );

			Console.Error.Write( "                      to use wildcards for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "image" );
			Console.ResetColor( );
			Console.Error.WriteLine( " with this option" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /S           S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "et the image file's timestamp to the capture date/time" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "           return the timestamp without \"-\" and \":\" delimiters," );

			Console.Error.Write( "                      e.g. " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( timestampNodelims );
			Console.ResetColor( );
			Console.Error.Write( " instead of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( timestampDelimited );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /Y" );
			Console.ResetColor( );
			Console.Error.WriteLine( "           do not ask for confirmation before changing the image" );

			Console.Error.Write( "                      file's timestamp (requires " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:   Result will be displayed on screen, e.g. " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( timestampDelimited );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.Error.WriteLine( "         The date/time is extracted by searching for the earliest date/time" );

			Console.Error.WriteLine( "         in the first 1048576 bytes (1 MB) of the image file." );

			Console.Error.Write( "         With " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch used, the timestamp is changed only if the difference" );

			Console.Error.WriteLine( "         between the current timestamp and the capture time exceeds 1 hour or" );

			Console.Error.Write( "         the threshold set with the " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/D" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch." );

			Console.Error.WriteLine( "         The program will ask for confirmation before changing the file's" );

			Console.Error.Write( "         timestamp, unless the " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/Y" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch is used." );

			Console.Error.Write( "         Return code (\"errorlevel\") is -1 in case of errors, or with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.WriteLine( " it" );

			Console.Error.WriteLine( "         equals the number of files renamed, or 0 otherwise." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Display Help


			return -1;
		}

		#endregion Error handling

	}
}
