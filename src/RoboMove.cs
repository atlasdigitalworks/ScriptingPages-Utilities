using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace RobvanderWoude
{
	class RoboMove
	{
		static string progver = "1.00";
		static string logfile = Path.Combine( Directory.GetCurrentDirectory( ), "RoboMove.log" );
		static List<string> logtext = new List<string>( );
		static bool debugmode = false;
		static bool logging = false;
		static bool overwritelog = false;
		static bool testmode = false;
		static bool verbose = false;
		static int duplicates = 0;
		static int newfiles = 0;
		static int movedfiles = 0;
		static int matchingfiles = 0;


		static int Main( string[] args )
		{
			#region Parse Command Line

			if ( args.Length < 2 )
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
			if ( args.Length > 2 )
			{
				foreach ( string arg in args.Skip<string>( 2 ) )
				{
					if ( arg.Length > 1 && arg[0] == '/' )
					{
						switch ( arg.ToUpper( ).Substring( 0, 2 ) )
						{
							case "/D":
								if ( debugmode )
								{
									return ShowHelp( "Duplicate command line switch /D" );
								}
								debugmode = true;
								break;
							case "/L":
								if ( logging )
								{
									return ShowHelp( "Duplicate command line switch /L" );
								}
								if ( arg.Length > 4 && arg[2] == ':' )
								{
									if ( Directory.Exists( Directory.GetParent( arg.Substring( 3 ) ).FullName ) )
									{
										logfile = arg.Substring( 3 );
									}
									else
									{
										return ShowHelp( "Invalid directory for log file: \"{0}\"", Directory.GetParent( arg.Substring( 3 ) ).FullName );
									}
								}
								else
								{
									return ShowHelp( "Invalid logfile specified: {0}", arg );
								}
								logging = true;
								break;
							case "/O":
								if ( overwritelog )
								{
									return ShowHelp( "Duplicate command line switch /O" );
								}
								overwritelog = true;
								break;
							case "/T":
								if ( testmode )
								{
									return ShowHelp( "Duplicate command line switch /T" );
								}
								testmode = true;
								break;
							case "/V":
								if ( verbose )
								{
									return ShowHelp( "Duplicate command line switch /V" );
								}
								verbose = true;
								break;
							default:
								return ShowHelp( "Invalid command line switch \"{0}\"", args[2] );
						}
					}
					else
					{
						return ShowHelp( "Invalid command line argument \"{0}\"", args[2] );
					}
				}
			}

			// Debug Mode implies Logging to file
			logging = ( logging || debugmode );

			// Overwrite can only be used if Logging is enabled
			if ( overwritelog && !logging )
			{
				return ShowHelp( "/O switch can only be used with /D or /L" );
			}

			if ( debugmode )
			{
				Display( "\nRoboMove {0}, started on {1}, at {2}\n", progver, DateTime.Now.Date.ToShortDateString( ), DateTime.Now.ToShortTimeString( ) );
				Display( "#arguments:\t{0}", args.Length.ToString( ) );
				int i = 0;
				foreach ( string arg in args )
				{
					Display( "argument #{0}:\t\"{1}\"", i.ToString( ), arg );
					i += 1;
				}
				Display( "\nSource Root:\t\"{0}\"", args[0] );
				Display( "Source exists:\t{0}", Directory.Exists( args[0] ).ToString( ) );
				Display( "Target Root:\t\"{0}\"", args[1] );
				Display( "Target exists:\t{0}", Directory.Exists( args[1] ).ToString( ) );
				Display( "Debug Mode:\t{0}", debugmode.ToString( ) );
				Display( "Log to File:\t{0}", logging.ToString( ) );
				Display( "Log File:\t\"{0}\"", logfile );
				Display( "Log exists:\t{0}", File.Exists( logfile ).ToString( ) );
				Display( "Overwrite Log:\t{0}", overwritelog.ToString( ) );
				Display( "Test Mode:\t{0}", testmode.ToString( ) );
				Display( "Verbose:\t{0}\n", verbose.ToString( ) );
			}

			// Validate paths of source and target directories
			string sourcerootdir;
			string targetrootdir;
			if ( Directory.Exists( args[0] ) )
			{
				try
				{
					sourcerootdir = Path.GetFullPath( args[0] );
				}
				catch ( Exception e )
				{
					return ShowHelp( "Invalid source root directory \"{0}\": {1}", args[0], e.Message );
				}
			}
			else
			{
				return ShowHelp( "Source directory \"{0}\" not found", args[0] );
			}
			if ( Directory.Exists( args[1] ) )
			{
				try
				{
					targetrootdir = Path.GetFullPath( args[1] );
				}
				catch ( Exception e )
				{
					return ShowHelp( "Invalid target root directory \"{0}\": {1}", args[0], e.Message );
				}
			}
			else
			{
				return ShowHelp( "Target directory \"{0}\" not found", args[1] );
			}

			#endregion Parse Command Line

			// List directory trees
			string[] sourcetreeabs = Directory.GetDirectories( sourcerootdir, "*.*", SearchOption.AllDirectories );
			string[] targettreeabs = Directory.GetDirectories( targetrootdir, "*.*", SearchOption.AllDirectories );
			List<string> sourcetree = new List<string>( );
			List<string> targettree = new List<string>( );

			// Remove root folders from paths
			int srclen = sourcerootdir.Length;
			foreach ( string dir in sourcetreeabs )
			{
				if ( dir.ToLower( ).IndexOf( sourcerootdir.ToLower( ) ) == 0 )
				{
					sourcetree.Add( dir.Substring( srclen ) );
				}
				else
				{
					Display( "ABORTING: error while building source directory tree list" );
					return 1;
				}
			}
			int tgtlen = targetrootdir.Length;
			foreach ( string dir in targettreeabs )
			{
				if ( dir.ToLower( ).IndexOf( targetrootdir.ToLower( ) ) == 0 )
				{
					targettree.Add( dir.Substring( tgtlen ) );
				}
				else
				{
					Display( "ABORTING: error while building target directory tree list" );
					return 1;
				}
			}

			// Add missing folders to target tree
			foreach ( string dir in sourcetree )
			{
				if ( !targettree.Contains( dir ) )
				{
					targettree.Add( dir );
					string newdir = Path.Combine( targetrootdir, dir.TrimStart( '\\' ) );
					if ( MKDIR( newdir ) == 1 && ( debugmode || testmode ) )
					{
						return 1;
					}
				}
			}

			// List file trees
			string[] sourcelistabs = Directory.GetFiles( sourcerootdir, "*.*", SearchOption.AllDirectories );
			string[] targetlistabs = Directory.GetFiles( targetrootdir, "*.*", SearchOption.AllDirectories );
			List<string> sourcelist = new List<string>( );
			List<string> targetlist = new List<string>( );

			// List unique file names
			SortedList<string, bool> sourcefilenames = new SortedList<string, bool>( ); // string filename, bool unique name
			foreach ( string file in sourcelistabs )
			{
				string name = Path.GetFileName( file );
				if ( sourcefilenames.ContainsKey( name ) )
				{
					sourcefilenames[name] = false;
				}
				else
				{
					sourcefilenames.Add( name, true );
				}
			}
			SortedList<string, bool> targetfilenames = new SortedList<string, bool>( ); // string filename, bool unique name
			foreach ( string file in targetlistabs )
			{
				string name = Path.GetFileName( file );
				if ( targetfilenames.ContainsKey( name ) )
				{
					targetfilenames[name] = false;
				}
				else
				{
					targetfilenames.Add( name, true );
				}
			}

			// Remove root folders from paths
			foreach ( string file in sourcelistabs )
			{
				if ( file.ToLower( ).IndexOf( sourcerootdir.ToLower( ) ) == 0 )
				{
					sourcelist.Add( file.Substring( srclen ) );
				}
				else
				{
					Display( "ABORTING: error while removing root folder from source directory tree list" );
					return 1;
				}
			}
			foreach ( string file in targetlistabs )
			{
				if ( file.ToLower( ).IndexOf( targetrootdir.ToLower( ) ) == 0 )
				{
					targetlist.Add( file.Substring( tgtlen ) );
				}
				else
				{
					Display( "ABORTING: error while removing root folder from target directory tree list" );
					return 1;
				}
			}

			// Compare directory trees
			foreach ( string file in targetlist )
			{
				string name = Path.GetFileName( file );
				if ( sourcelist.Contains( file ) )
				{
					if ( debugmode || verbose )
					{
						Display( "OK: locations of file \"{0}\" in source and target trees match", name );
						matchingfiles += 1;
					}
				}
				else
				{
					if ( sourcefilenames.ContainsKey( name ) && targetfilenames.ContainsKey( name ) )
					{
						if ( sourcefilenames[name] && targetfilenames[name] )
						{
							string currentlocation = Path.Combine( targetrootdir, file.TrimStart( '\\' ) );
							string sourcelocation = sourcelist.FirstOrDefault( stringToCheck => stringToCheck.Contains( name ) );
							string movetolocation = Path.Combine( targetrootdir, sourcelocation.TrimStart( '\\' ) );
							if ( MOVE( currentlocation, movetolocation ) == 1 && ( debugmode || testmode ) )
							{
								return 1;
							}
						}
						else
						{
							if ( debugmode || testmode || verbose )
							{
								Display( "SKIPPED: duplicate file name \"{0}\"", name );
							}
							duplicates += 1;
						}
					}
					else
					{
						if ( debugmode || testmode || verbose )
						{
							Display( "SKIPPED: file \"{0}\" not found in source tree", name );
						}
						// New file in target: SKIP
						newfiles += 1;
					}
				}
			}

			// Cleanup moved folders from target tree
			targettree.Sort( );
			targettree.Reverse( ); // Reversed sort guarantees that empty subfolders will be deleted first
			foreach ( string dir in targettreeabs )
			{
				if ( !sourcetree.Contains( dir ) )
				{
					if ( Directory.GetFiles( dir, "*.*", SearchOption.TopDirectoryOnly ).Length == 0 )
					{
						if ( Directory.GetDirectories( dir, "*.*", SearchOption.AllDirectories ).Length == 0 )
						{
							if ( RMDIR( dir ) == 1 )
							{
								return 2;
							}
						}
					}
				}
			}

			// End of Main program: show results
			if ( debugmode || verbose )
			{
				Display( "{0} file location{1} in source and target tree matched", matchingfiles.ToString( ), ( matchingfiles == 1 ? String.Empty : "s" ) );
				Display( "{0} file{1} moved", movedfiles.ToString( ), ( movedfiles == 1 ? " was" : "s were" ) );
				Display( "{0} duplicate file name{1} found", duplicates.ToString( ), ( duplicates == 1 ? " was" : "s were" ) );
				Display( "{0} file{1} found in the target tree that did not exist in the source tree", newfiles.ToString( ), ( newfiles == 1 ? " was" : "s were" ) );
				Display( "\nRoboMove {0}, finished on {1}, at {2}\n", progver, DateTime.Now.Date.ToShortDateString( ), DateTime.Now.ToShortTimeString( ) );
			}
			if ( duplicates + newfiles == 0 )
			{
				return WriteLog( );
			}
			else
			{
				if ( debugmode || verbose )
				{
					int skipped = duplicates + newfiles;
					Display( "{0} file{1} skipped, use the following command to finish synchronization:", skipped.ToString( ), ( skipped == 1 ? " was" : "s were" ) );
					Display( "ROBOCOPY /MIR \"{0}\" \"{1}\"", sourcerootdir, targetrootdir );
				}
				if ( WriteLog( ) == 1 )
				{
					return 1;
				}
				else
				{
					return 2;
				}
			}
		}


		static void Display( string text, params string[] values )
		{
			string newtext = String.Format( text, values );
			if ( verbose )
			{
				Console.WriteLine( newtext );
			}
			if ( logging )
			{
				logtext.Add( newtext );
			}
		}


		static int MKDIR( string dir )
		{
			if ( testmode )
			{
				Display( "Creating directory \"{0}\" skipped in Test Mode", dir );
				return 0;
			}
			if ( debugmode || verbose )
			{
				Display( "Creating directory \"{0}\"", dir );
			}
			try
			{
				Directory.CreateDirectory( dir );
				if ( debugmode || verbose )
				{
					Display( "Directory \"{0}\" created successfully", dir );
				}
				return 0;
			}
			catch ( DirectoryNotFoundException e )
			{
				Display( "Directory not found while trying to create directory \"{0}\": {1}", dir, e.Message );
				return 1;
			}
			catch ( IOException e )
			{
				Display( "I/O error while trying to create directory \"{0}\": {1}", dir, e.Message );
				return 1;
			}
		}


		static int MOVE( string currentlocation, string newlocation )
		{
			if ( testmode )
			{
				Display( "Moving file \"{0}\" to \"{1}\" skipped in Test Mode", currentlocation, newlocation );
				return 0;
			}
			if ( debugmode || verbose )
			{
				Display( "Moving file \"{0}\" to \"{1}\"", currentlocation, newlocation );
			}
			try
			{
				File.Move( currentlocation, newlocation );
				movedfiles += 1;
				return 0;
			}
			catch ( DirectoryNotFoundException e )
			{
				Console.Error.WriteLine( "Directory not found while trying to move \"{0}\" to \"{1}\": {2}", currentlocation, newlocation, e.Message );
				return 1;
			}
			catch ( IOException e )
			{
				Display( "I/O error while trying to move \"{0}\" to \"{1}\": {2}", currentlocation, newlocation, e.Message );
				return 1;
			}
		}


		static int RMDIR( string dir )
		{
			if ( testmode )
			{
				Display( "Removing directory \"{0}\" skipped in Test Mode", dir );
				return 0;
			}
			if ( debugmode || verbose )
			{
				Display( "Removing directory \"{0}\"", dir );
			}
			try
			{
				Directory.Delete( dir );
				return 0;
			}
			catch ( DirectoryNotFoundException e )
			{
				Display( "Directory not found while trying to remove \"{0}\": {1}", dir, e.Message );
				return 1;
			}
			catch ( IOException e )
			{
				Display( "I/O error while trying to remove directory \"{0}\": {1}", dir, e.Message );
				return 1;
			}
		}


		static int ShowHelp( params string[] errmsg )
		{

			/*
			RoboMove,  Version 1.00
			Move files in target directory tree to make it match the source directory tree
 
			Usage:    ROBOMOVE  sourcedir  targetdir  [ options ]
 
			Where:    "sourcedir"   is the source tree root directory
			          "targetdir"   is the target tree root directory
			Options:  /D            Debugging mode: more screen output than Verbose
			          /L[:logfile]  enable Logging
			                        (default logfile: RoboMove.log in current directory)
			          /O            Overwrite existing logfile (default: append)
			          /T            Test mode: shows the changes that would be made,
			                        but doesn't really make any changes
			          /V            Verbose mode: display every action for every file
 
			Notes:    ROBOMOVE is a ROBOCOPY like utility, but instead of copying or
			          deleting files, it only moves them. It is intended to be used when
			          large numbers of files have been moved and have to be synchronized
			          with a slow network drive (moving files is faster then copying).
			          ROBOCOPY may be required afterwards to finish the synchronization.
			          Duplicate file names within the directory trees will not be moved.
			          Return code is 0 if all files were moved and no further action is
			          required (i.e. no duplicate or new files), 1 in case of command line
			          errors, 2 if duplicate or new files require ROBOCOPY to be run.
 
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

			Console.Error.WriteLine( "RoboMove,  Version {0}", progver );

			Console.Error.WriteLine( "Move files in target directory tree to make it match the source directory tree" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ROBOMOVE  sourcedir  targetdir  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"sourcedir\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the source tree root directory" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          \"targetdir\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the target tree root directory" );

			Console.Error.Write( "Options:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/D            D" );
			Console.ResetColor( );
			Console.Error.Write( "ebugging mode: more screen output than " );
			Console.Error.Write( "Options:  " );
			Console.Error.Write( "V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /L[:logfile]" );
			Console.ResetColor( );
			Console.Error.Write( "  enable " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ogging" );

			Console.Error.WriteLine( "                        (default logfile: RoboMove.log in current directory)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /O            O" );
			Console.ResetColor( );
			Console.Error.WriteLine( "verwrite existing logfile (default: append)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /T            T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "est mode: shows the changes that would be made" );

			Console.Error.WriteLine( "                        but doesn't really make any changes" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /V            V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose mode: display every action for every file" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    ROBOMOVE is a ROBOCOPY like utility, but instead of copying or" );

			Console.Error.WriteLine( "          deleting files, it only moves them. It is intended to be used when" );

			Console.Error.WriteLine( "          large numbers of files have been moved and have to be synchronized" );

			Console.Error.WriteLine( "          with a slow network drive (moving files is faster then copying)." );

			Console.Error.WriteLine( "          ROBOCOPY may be required afterwards to finish the synchronization." );

			Console.Error.WriteLine( "          Duplicate file names within the directory trees will not be moved." );

			Console.Error.WriteLine( "          Return code is 0 if all files were moved and no further action is" );

			Console.Error.WriteLine( "          required (i.e. no duplicate or new files), 1 in case of command line" );

			Console.Error.WriteLine( "          errors, 2 if duplicate or new files require ROBOCOPY to be run." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}


		static int WriteLog( )
		{
			if ( logging )
			{
				try
				{
					StreamWriter file = new StreamWriter( logfile, !overwritelog, Encoding.Default );
					foreach ( string line in logtext )
					{
						file.WriteLine( line );
					}
					file.Close( );
				}
				catch ( Exception e )
				{
					return ShowHelp( "Error writing logfile: {0}", e.Message );
				}
			}
			return 0;
		}
	}
}

