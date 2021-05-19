using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class DeDup
	{
		static string progver = "1.02";


		static int Main( string[] args )
		{
			#region Initialize Variables

			bool ignorecase = false;
			bool ignorewhitespace = false;
			bool isredirected = Console.IsInputRedirected; // Requires .NET Framework 4.5
			bool returnduplicates = false;
			bool sortoutput = false;
			bool trimoutput = false;
			int redirectnum = ( isredirected ? 1 : 0 );
			int arguments = args.Length + redirectnum;
			int rc = 0;
			string filename = string.Empty;
			string input = String.Empty;

			#endregion Initialize Variables


			#region Command Line Parsing

			if ( arguments == 0 )
			{
				return ShowHelp( );
			}
			if ( arguments > 1 )
			{
				for ( int i = 1 - redirectnum; i < args.Length; i++ )
				{
					if ( args[i][0] != '/' || args[i].Length < 2 )
					{
						return ShowHelp( "Invalid command line argument \"{0}\"", args[i] );
					}
					switch ( args[i][1].ToString( ).ToUpper( ) )
					{
						case "C":
							if ( ignorecase )
							{
								return ShowHelp( "Duplicate command line switch /C" );
							}
							ignorecase = true;
							break;
						case "/R":
							if ( returnduplicates )
							{
								return ShowHelp( "Duplicate command line switch /R" );
							}
							returnduplicates = true;
							break;
						case "S":
							if ( sortoutput )
							{
								return ShowHelp( "Duplicate command line switch /S" );
							}
							sortoutput = true;
							break;
						case "T":
							if ( trimoutput )
							{
								return ShowHelp( "Duplicate command line switch /T" );
							}
							trimoutput = true;
							break;
						case "W":
							if ( ignorewhitespace )
							{
								return ShowHelp( "Duplicate command line switch /W" );
							}
							ignorewhitespace = true;
							break;
						default:
							return ShowHelp( "Invalid command line switch {0}", args[i] );
					}
				}
			}
			if ( isredirected )
			{
				// Read the redirected Standard Input
				input = Console.In.ReadToEnd( );
			}
			else
			{
				filename = args[0];
				// Check if the file name is valid
				if ( filename.IndexOf( "/" ) > -1 )
				{
					return ShowHelp( );
				}

				if ( filename.IndexOfAny( "?*".ToCharArray( ) ) > -1 )
				{
					return ShowHelp( "Wildcards not allowed" );
				}

				// Check if the file exists
				if ( File.Exists( filename ) )
				{
					// Read the file content
					using ( StreamReader file = new StreamReader( filename ) )
					{
						input = file.ReadToEnd( );
					}
				}
				else
				{
					return ShowHelp( "File not found: \"" + filename + "\"" );
				}
			}

			#endregion Command Line Parsing


			#region Check Each Line

			List<string> deduplines = new List<string>( );
			Regex regex = new Regex( @"[\t ]+" );
			foreach ( string line in input.Split( "\n\r".ToCharArray( ) ) )
			{
				string checkline = line;
				if ( ignorewhitespace )
				{
					checkline = regex.Replace( checkline, " " );
				}
				if ( trimoutput )
				{
					checkline = checkline.Trim( );
				}
				if ( !String.IsNullOrWhiteSpace( checkline ) || !ignorewhitespace )
				{
					if ( ignorecase )
					{
						bool found = false;
						foreach ( string storedline in deduplines )
						{
							if ( storedline.ToLower( ) == checkline.ToLower( ) )
							{
								found = true;
								rc++;
							}
						}
						if ( !found )
						{
							deduplines.Add( checkline );
						}
					}
					else
					{
						if ( !deduplines.Contains( checkline ) )
						{
							deduplines.Add( checkline );
							rc++;
						}
					}
				}
			}

			#endregion Check Each Line


			#region Display Results

			if ( sortoutput )
			{
				deduplines.Sort( );
			}

			foreach ( string line in deduplines )
			{
				Console.WriteLine( line );
			}

			#endregion Display Results


			return rc;
		}


		// Displays help text
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
			DeDup.exe,  Version 1.02
			Remove duplicate lines from a text file or from redirected input
			
			Usage:   DeDup.exe  filename  [ options ]
			   or:   some_command  |  DeDup.exe  [ options ]
			
			Where:   filename      file to be investigated
			         some_command  command whose Standard Output is to be investigated
			
			Options: /C            ignore Case
			         /R            Return code equals number of duplicates removed
			         /S            Sort results
			         /T            Trim leading and trailing whitespace from output
			         /W            ignore Whitespace (any combination of tabs and/or
			                       spaces will be replaced by a single space in output,
			                       empty lines or lines containing only whitespace will
			                       be removed from output)
			
			Notes:   The filtered output is sent to the screen (Standard Output).
			         In case of duplicate lines, only the first match is returned.
			         Return code ("errorlevel") equals the number of removed
			         duplicates if /R is used, -1 in case of errors, or 0 otherwise.
			         This version of the program requires .NET Framework 4.5.
			
			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "DeDup,  Version {0}", progver );

			Console.Error.WriteLine( "Remove duplicate lines from a text file or from redirected input" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "DeDup.exe  filename  [ options ]" );
			Console.ResetColor( );

			Console.Error.Write( "   or:   some_command  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "|  DeDup.exe  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( "      file to be investigated" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         some_command" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  command whose Standard Output is to be investigated" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/C" );
			Console.ResetColor( );
			Console.Error.Write( "            ignore " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "C" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ase" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /R            R" );
			Console.ResetColor( );
			Console.Error.WriteLine( "eturn code equals number of duplicates removed" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /S            S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ort results" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /T            T" );
			Console.ResetColor( );
			Console.ResetColor( );
			Console.Error.WriteLine( "rim leading and trailing whitespace from output" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /W" );
			Console.ResetColor( );
			Console.Error.Write( "            ignore " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "hitespace (any combination of tabs and/or" );

			Console.Error.WriteLine( "                       spaces will be replaced by a single space in output," );

			Console.Error.WriteLine( "                       empty lines or lines containing only whitespace will" );

			Console.Error.WriteLine( "                       be removed from output)" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:   The filtered output is sent to the screen (Standard Output)." );

			Console.Error.WriteLine( "         In case of duplicate lines, only the first match is returned." );

			Console.Error.WriteLine( "         Return code (\"errorlevel\") equals the number of removed" );

			Console.Error.Write( "         duplicates if " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/R" );
			Console.ResetColor( );
			Console.Error.WriteLine( " is used, -1 in case of errors, or 0 otherwise." );

			Console.Error.WriteLine( "         This version of the program requires .NET Framework 4.5." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help Text

			return -1;
		}
	}
}
