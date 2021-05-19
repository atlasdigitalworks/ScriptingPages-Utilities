using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RobvanderWoude
{
	class CloneDate
	{
		static string progver = "1.00";


		static int Main( string[] args )
		{
			bool debug = false;

			#region Parse command line

			switch ( args.Length )
			{
				case 0:
					return ShowHelp( string.Empty );
				case 2:
					break;
				case 3:
					if ( args[2].Substring( 0, 2 ).ToUpper( ) == "/D" )
					{
						debug = true;
					}
					else
					{
						return ShowHelp( "Invalid command line argument(s)" );
					}
					break;
				default:
					return ShowHelp( "Invalid number of command line arguments" );
			}

			#endregion Parse command line

			try
			{
				#region Validate command line arguments

				string sourcefile = args[0];
				// Check if a source file was specified
				if ( string.IsNullOrWhiteSpace( sourcefile ) )
				{
					return ShowHelp( "Invalid source file specification" );
				}
				// Check if the source file name is valid, and make sure to use its full path
				try
				{
					sourcefile = Path.GetFullPath( sourcefile ).Trim( '"' );
				}
				catch ( ArgumentException )
				{
					return ShowHelp( "No wildcards allowed in source file" );
				}
				// Check if the source file exists
				if ( !File.Exists( sourcefile ) )
				{
					return ShowHelp( "File not found: \"{0}\"", sourcefile );
				}

				string targetspec = args[1];
				if ( string.IsNullOrWhiteSpace( targetspec ) )
				{
					return ShowHelp( "Invalid target file specification" );
				}
				// Check if the target directory exists
				string targetdir = string.Empty;
				try
				{
					targetdir = Path.GetDirectoryName( targetspec );
					if ( string.IsNullOrWhiteSpace( targetdir ) )
					{
						targetdir = Path.GetFullPath( "." );
					}
				}
				catch ( ArgumentException )
				{
					return ShowHelp( "Target folder not found: \"{0}\"", targetspec );
				}

				#endregion Validate command line arguments

				// Extract the FILE specification (removing the path)
				string targetfilespec = targetspec.Substring( targetspec.LastIndexOf( "\\" ) + 1 );
				string[] targetfiles = Directory.EnumerateFiles( targetdir, targetfilespec ).ToArray<string>( );
				DateTime timestamp = File.GetLastWriteTime( sourcefile );
				int count = 0;
				int rc = 0;
				foreach ( string targetfile in targetfiles )
				{
					if ( targetfile.ToUpper( ) != sourcefile.ToUpper( ) )
					{
						count++;
						if ( debug )
						{
							Console.WriteLine( "File   : {0}", targetfile );
							Console.WriteLine( "Before : {0}", File.GetLastWriteTime( targetfile ) );
						}
						try
						{
							File.SetLastWriteTime( targetfile, timestamp );
						}
						catch ( Exception e )
						{
							rc = 1;
							if ( debug )
							{
								Console.WriteLine( "Error  : {0}", e.Message );
							}
							else
							{
								Console.Error.WriteLine( "File   : {0}", targetfile );
								Console.Error.WriteLine( "Error  : {0}", e.Message );
							}
						}
						if ( debug )
						{
							Console.WriteLine( "After  : {0}", File.GetLastWriteTime( targetfile ) );
							Console.WriteLine( );
						}
					}
				}

				if ( debug )
				{
					Console.WriteLine( "{0} matching file{1}", count, ( count == 1 ? "" : "s" ) );
				}

				if ( count == 0 )
				{
					return ShowHelp( "No matching target files: \"{0}\"", targetspec );
				}

				return rc;
			}
			catch ( Exception e )
			{
				return ShowHelp( e.Message );
			}
		}


		public static int ShowHelp( params string[] errmsg )
		{
			/*
			CloneDate.exe,  Version 1.00
			Modify the LastModified date (timestamp) of the target file(s) to
			match the specified source file's timestamp

			Usage:    CloneDate.exe  sourcefile  targetfiles  [ /Debug ]

			Where:    sourcefile     is the file whose timestamp is to be cloned
			          targetfiles    are the files whose timestamp are to be modified
			                         (single filespec, wildcards * and ? are allowed)
			          /Debug         displays file name and timestamps before and
			                         after modification for each matching file

			Example:  CloneDate.exe C:\boot.ini C:\test.log
			          will change C:\test.log's timestamp to match C:\boot.ini's

			Notes:    Target filespec may include sourcefile (sourcefile will be skipped).
			          Always be careful when using wildcards; they may also return matching
			          "short" (8.3) file names (for backwards compatibility with FAT16).

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

			Console.Error.WriteLine( "CloneDate.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Modify the LastModified date (timestamp) of the target" );

			Console.Error.WriteLine( "file(s) to match the specified source file's timestamp" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CloneDate.exe  sourcefile  targetfiles" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "sourcefile" );
			Console.ResetColor( );
			Console.Error.WriteLine( "     is the file whose timestamp is to be cloned" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          targetfiles" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    are the files whose timestamp are to be modified" );

			Console.Error.WriteLine( "                         (single filespec, wildcards * and ? are allowed)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /D" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ebug         displays file name and timestamps before and" );

			Console.Error.WriteLine( "                         after modification for each matching file" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Example:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CloneDate.exe C:\\boot.ini C:\\test.log" );
			Console.ResetColor( );

			Console.Error.WriteLine( "          will change C:\\test.log's timestamp to match C:\\boot.ini's" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    Target filespec may include sourcefile (sourcefile will be skipped)." );

			Console.Error.WriteLine( "          Always be careful when using wildcards; they may also return matching" );

			Console.Error.WriteLine( "          \"short\" (8.3) file names (for backwards compatibility with FAT16)." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}

	}
}
