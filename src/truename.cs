using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class TrueName
	{
		#region Global Variables

		static string progver = "1.02";
		static SortedList<string, string> subst = new SortedList<string, string>( );
		static SortedList<string, string> mapped = new SortedList<string, string>( );

		#endregion Global Variables


		static int Main( string[] args )
		{
			#region Parse Command Line

			bool verbose = false;

			if ( args.Length < 1 || args.Length > 2 )
			{
				return ShowHelp( );
			}

			if ( args.Length == 2 )
			{
				if ( args[1].ToUpper( ) == "/V" )
				{
					verbose = true;
				}
				else if ( args[1] == "/?" )
				{
					return ShowHelp( );
				}
				else
				{
					//return ShowHelp( "Invalid command line argument \"{0}\"", args[1] );
					return ShowHelp( "Invalid command line argument \"" + args[1] + "\"" );
				}
			}

			string inputpath = args[0];

			#endregion Parse Command Line

			#region Skip UNCs

			if ( inputpath.StartsWith( @"\\" ) )
			{
				if ( verbose )
				{
					Console.WriteLine( "\"{0}\" => \"{1}\"", inputpath, inputpath );
				}
				else
				{
					Console.WriteLine( inputpath );
				}
				return 0;
			}

			#endregion Skip UNCs

			#region Abort On Invalid First Character

			if ( "/?*<>:;".IndexOf( inputpath[0] ) > -1 )
			{
				return ShowHelp( );
			}

			#endregion Abort On Invalid First Character

			// Convert relative path to full path first
			string fullpath = inputpath;
			try
			{
				if ( File.Exists( inputpath ) || Directory.Exists( inputpath ) || Directory.Exists( Directory.GetParent( inputpath ).FullName ) )
				{
					fullpath = Path.GetFullPath( inputpath );
				}
				else
				{
					return ShowHelp( String.Format( "File or folder not found \"{0}\"", inputpath ) );
				}
			}
			catch ( Exception e )
			{
				return ShowHelp( e );
			}

			// List all SUBSTituted drives
			ListSUBST( );

			// List all mapped network drives
			ListMapped( );

			string outputpath = fullpath;
			// Use a loop to detect combinations of SUBSTituted and mapped drives too
			do
			{
				fullpath = outputpath;
				string driveletter = Path.GetPathRoot( outputpath )[0].ToString( ).ToUpper( );
				if ( mapped.ContainsKey( driveletter ) )
				{
					outputpath = Regex.Replace( fullpath, "^" + driveletter + ":", mapped[driveletter], RegexOptions.IgnoreCase );
				}
				else if ( subst.ContainsKey( driveletter ) )
				{
					outputpath = Regex.Replace( fullpath, "^" + driveletter + ":", subst[driveletter], RegexOptions.IgnoreCase );
				}
				else
				{
					outputpath = fullpath;
				}
			}
			while ( outputpath != fullpath && !outputpath.StartsWith( @"\\" ) );

			// Check for reparse points
			outputpath = ReparsePoints( outputpath );

			// Display the result
			if ( verbose )
			{
				Console.WriteLine( "\"{0}\" => \"{1}\"", inputpath, outputpath );
			}
			else
			{
				Console.WriteLine( outputpath );
			}

			return 0;
		}


		static string GetReparseTarget( string folder )
		{
			// This function uses CMD.EXE's internal DIR command with its
			// /ADL switch to find the true location of folder reparse points.
			string parentfolder = Directory.GetParent( folder ).FullName;
			string testsubfolder = Path.GetFileName( folder );
			ProcessStartInfo psi = new ProcessStartInfo( );
			psi.FileName = "CMD.EXE";
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;
			psi.RedirectStandardOutput = true;
			Process process = new Process( );
			psi.Arguments = String.Format( "/C DIR /ADL \"{0}\"", parentfolder );
			process.StartInfo = psi;
			process.Start( );
			process.WaitForExit( );
			string result = process.StandardOutput.ReadToEnd( );
			string[] lines = result.Split( Environment.NewLine.ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );
			foreach ( string line in lines )
			{
				string pattern = String.Format( @"\s+{0}\s+\[([^\]]+)\]", testsubfolder );
				Regex regex = new Regex( pattern, RegexOptions.IgnoreCase );
				if ( regex.IsMatch( line ) )
				{
					MatchCollection matches = regex.Matches( line );
					return matches[0].Groups[1].ToString( );
				}
			}
			return folder;
		}


		static bool IsReparsePoint( string folder )
		{
			bool isreparsepoint = false;
			DirectoryInfo dirinfo = new DirectoryInfo( folder );
			isreparsepoint = ( ( dirinfo.Attributes & FileAttributes.ReparsePoint ) != 0 );
			return isreparsepoint;
		}


		static void ListMapped( )
		{
			// List all mapped network drives
			string pattern = @"^OK\s+([A-Z]):\s+(\\\\[^\\]+\\.+)\s{2,}[^\s]+"; // This regex will fail if the UNC path contains multiple consecutive spaces
			Regex regex = new Regex( pattern, RegexOptions.IgnoreCase );
			ProcessStartInfo psi = new ProcessStartInfo( "NET.EXE", "Use" );
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			Process process = new Process( );
			process.StartInfo = psi;
			process.Start( );
			process.WaitForExit( );
			foreach ( string line in process.StandardOutput.ReadToEnd( ).Split( Environment.NewLine.ToCharArray( ) ) )
			{
				if ( regex.IsMatch( line ) )
				{
					MatchCollection matches = regex.Matches( line );
					if ( matches[0].Groups.Count == 3 )
					{
						string drive = matches[0].Groups[1].ToString( ).ToUpper( );
						string path = matches[0].Groups[2].ToString( ).Trim( );
						mapped[drive] = path;
					}
				}
			}
		}


		static void ListSUBST( )
		{
			// List all SUBSTituted drives
			ProcessStartInfo psi = new ProcessStartInfo( "SUBST.EXE" );
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			Process process = new Process( );
			process.StartInfo = psi;
			process.Start( );
			process.WaitForExit( );
			foreach ( string line in process.StandardOutput.ReadToEnd( ).Split( Environment.NewLine.ToCharArray( ) ) )
			{
				if ( !String.IsNullOrWhiteSpace( line ) )
				{
					string drive = line.Trim( )[0].ToString( ).ToUpper( );
					string path = line.Split( " \t".ToCharArray( ) )[2];
					path = Regex.Replace( path, @"^UNC\\", @"\\" );
					subst[drive] = path;
				}
			}
		}


		static string ReparsePoints( string path )
		{
			// This function iterates throught the folder and its parent folder(s) until it finds a reparse point for the specified path;
			// if found, it replaces the specified path with the true path, and starts all over again to check for nested reparse points.
			string folder = Path.GetFullPath( path );
			string file = String.Empty;
			if ( File.Exists( path ) )
			{
				// Check folder reparse points only
				folder = Directory.GetParent( folder ).ToString( );
				file = Path.GetFileName( path );
			}

			string testfolder = folder;
			string remainder = String.Empty;
			while ( testfolder != Path.GetPathRoot( testfolder ) )
			{
				if ( IsReparsePoint( testfolder ) )
				{
					testfolder = Path.Combine( GetReparseTarget( testfolder ), remainder );
					remainder = String.Empty;
				}
				remainder = Path.Combine( Path.GetFileName( testfolder ), remainder );
				string dummy = Path.GetPathRoot( testfolder );
				testfolder = Directory.GetParent( testfolder ).FullName;
			}
			folder = Path.Combine( testfolder, remainder );
			path = Path.Combine( folder, file );
			return path;
		}


		static int ShowHelp( Exception e )
		{
			if ( e.GetType( ) == typeof( IOException ) || e.GetType( ) == typeof( AccessViolationException ) )
			{
				ShowHelp( e.Message );
				return -2;
			}
			else
			{
				return ShowHelp( e.Message );
			}
		}


		static int ShowHelp( string errmsg = "" )
		{
			#region Error Message

			if ( errmsg.Length > 0 )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR:\t" );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errmsg );
				Console.ResetColor( );
			}

			#endregion Error Message

			#region Help Text

			/*
			TrueName,  Version 1.02
			Displays the true path of a file or directory, like COMMAND.COM's internal
			TRUENAME command; this program handles SUBSTituted drives as well as mapped
			network drives and directory junctions, and even some combinations.

			Usage:  TRUENAME  "path"  [ /V ]

			Where:  "path"    is the drive, or path to the folder or file to be checked
			        /V        displays Verbose output, i.e. "specified path" => "true path"
			                  (default: display true path only, without doublequotes)

			Notes:  When specifying a file for "path", it doesn't necessarily have to
			        exist, as long as its parent folder does.
			        If "path" is an UNC path, it will be returned without testing.
			        File reparse points are ignored by this program.
			        This program uses DIR's /ADL switch to find the reparse target.
			        As always, enclose paths and file names in doublequotes if necessary.
			        This program's return code ("errorlevel") will be -1 in case of
			        (command line) errors, -2 if the specified folder is invalid or if
			        access is denied, or 0 otherwise.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "TrueName,  Version {0}", progver );

			Console.Error.WriteLine( "Displays the true path of a file or directory, like COMMAND.COM's internal" );

			Console.Error.WriteLine( "TRUENAME command; this program handles SUBSTituted drives as well as mapped" );

			Console.Error.WriteLine( "network drives and directory junctions, and even some combinations." );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "TRUENAME  \"path\"  [ /V ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"path\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    is the drive, or path to the folder or file to be checked" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /V" );
			Console.ResetColor( );
			Console.Error.Write( "        displays " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "V" );
			Console.ResetColor( );
			Console.Error.Write( "erbose output, i.e. " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "\"specified path\" => \"true path\"" );
			Console.ResetColor( );

			Console.Error.WriteLine( "                  (default: display true path only, without doublequotes)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:  When specifying a file for " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"path\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( ", it doesn't necessarily have to" );

			Console.Error.WriteLine( "        exist, as long as its parent folder does." );

			Console.Error.Write( "        If " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"path\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( " is an UNC path, it will be returned without testing." );

			Console.Error.WriteLine( "        File reparse points are ignored by this program." );

			Console.Error.Write( "        This program uses " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.Write( "DIR" );
			Console.ResetColor( );
			Console.Error.Write( "'s " );
			Console.ForegroundColor = ConsoleColor.DarkGray; ;
			Console.Error.Write( "/ADL" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch to find the reparse target." );

			Console.Error.WriteLine( "        As always, enclose paths and file names in doublequotes if necessary." );

			Console.Error.WriteLine( "        This program's return code (\"errorlevel\") will be -1 in case of" );

			Console.Error.WriteLine( "        (command line) errors, -2 if the specified folder is invalid or if" );

			Console.Error.WriteLine( "        access is denied, or 0 otherwise." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help Text

			return -1;
		}
	}
}
