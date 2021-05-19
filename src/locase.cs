using System;
using System.Collections.Generic;
using System.IO;


namespace RobvanderWoude
{
	class LoCase
	{
		public static string progver = "2.02";


		static int Main( string[] args )
		{
			bool verbose = false;
			char[] upcaseletters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( );
			List<string> files = new List<string>( );

			#region Command Line Parsing

			if ( Console.IsInputRedirected )
			{
				// No command line arguments in case of redirected input
				if ( args.Length > 0 )
				{
					return ShowHelp( );
				}
			}
			else
			{
				if ( args.Length == 0 || ( args.Length == 1 && args[0].ToUpper( ).Substring( 0, 2 ) == "/V" ) )
				{
					args = new string[] { "*.*" };
				}
				foreach ( string arg in args )
				{
					if ( arg[0] == '/' )
					{
						if ( arg == "/?" )
						{
							return ShowHelp( );
						}
						if ( arg.Length > 1 && arg.Substring( 0, 2 ).ToUpper( ) == "/V" )
						{
							if ( verbose )
							{
								return ShowHelp( "Duplicate command line switch /V" );
							}
							verbose = true;
						}
						else
						{
							return ShowHelp( "Invalid command line switch \"{0}\"", arg );
						}
					}
					else
					{
						string parentfolder = Directory.GetParent( arg ).FullName;
						if ( !Directory.Exists( parentfolder ) )
						{
							return ShowHelp( "Invalid parent folder \"{0}\"", parentfolder );
						}
						string filespec = Path.GetFileName( arg );
						foreach ( string file in Directory.GetFiles( parentfolder, filespec ) )
						{
							if ( Path.GetFileName( file ).IndexOfAny( upcaseletters ) > -1 )
							{
								if ( !files.Contains( file ) )
								{
									files.Add( file );
								}
							}
						}
					}
				}
			}

			#endregion Command Line Parsing


			if ( Console.IsInputRedirected )
			{
				#region Convert redirected input to lower case

				Console.OpenStandardInput( );
				string input = Console.In.ReadToEnd( );
				Console.OpenStandardOutput( );
				string output = input.ToLower( );
				Console.Out.Write( output );
				return 0;

				#endregion Convert redirected input to lower case
			}
			else
			{
				#region Rename files to lower case

				foreach ( string file in files )
				{
					if ( File.Exists( file ) )
					{
						string parentfolder = Directory.GetParent( file ).FullName;
						string filename = Path.GetFileName( file );
						if ( filename.IndexOfAny( upcaseletters ) > -1 )
						{
							string newfilename = Path.Combine( parentfolder, filename.ToLowerInvariant( ) );
							try
							{
								if ( verbose )
								{
									Console.WriteLine( "\"{0}\"  =>  \"{1}\"", file, newfilename );
								}
								File.Move( file, newfilename );
							}
							catch ( Exception e )
							{
								Console.Error.WriteLine( "Error renaming \"{0}\": {1}", filename, e.Message );
							}
						}
					}
				}
				if ( verbose )
				{
					Console.WriteLine( "{0} matching file{1} renamed", ( files.Count == 0 ? "No" : files.Count.ToString( ) ), ( files.Count == 1 ? String.Empty : "s" ) );
				}
				return files.Count;

				#endregion Rename files to lower case
			}
		}


		#region Error handling

		public static int ShowHelp( params string[] errmsg )
		{
			#region Help text

			/*
			LoCase.exe,  Version 2.02
			Either rename specified files or render redirected input to all lower case

			Usage:    LoCase.exe  [ filespec  [ filespec  [ ... ] ] ]  [ /V ]

			or:       somecommand  |  LoCase.exe

			Where:    filespec     file(s) to be renamed (wildcards allowed, default: *.*)
					  somecommand  command whose output will be rendered to lower case
					  /V           Verbose mode: displays file renaming and files count

			Notes:    Use doublequotes if filespec contains non-alphnumeric characters.
			          If no folder is specified in filespec, current directory is assumed.
					  Return code (\"ErrorLevel\") equals the number of renamed files,
					  or -1 in case of errors.
			          This program requires .NET Framework 4.5; use an older version of
			          of this program if you don't have .NET Framework 4.5 installed.
			
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

			Console.Error.WriteLine( "LoCase.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Either rename specified files or render redirected input to all lower case" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "LoCase.exe  [ filespec  [ filespec  [ ... ] ] ]  [ /V ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "or:       " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "somecommand  |  LoCase.exe" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "filespec" );
			Console.ResetColor( );
			Console.Error.WriteLine( "     file(s) to be renamed (wildcards allowed, default: *.*)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          somecommand" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  command whose output will be rendered to lower case" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /V           V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "erbose mode: displays file renaming and files count" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    Use doublequotes if filespec contains non-alphnumeric characters." );

			Console.Error.WriteLine( "          If no folder is specified in filespec, current directory is assumed." );

			Console.Error.WriteLine( "          Return code (\"ErrorLevel\") equals the number of renamed files," );

			Console.Error.WriteLine( "          or -1 in case of errors." );

			Console.Error.WriteLine( "          This program requires .NET Framework 4.5; use an older version of" );

			Console.Error.WriteLine( "          of this program if you don't have .NET Framework 4.5 installed." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help text

			return -1;
		}

		#endregion Error handling
	}
}
