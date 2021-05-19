using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RobvanderWoude
{
	class ReadLine
	{
		static int Main( string[] args )
		{
			#region Command Line Parsing

			string filename = string.Empty;
			int linestart = 1;
			int lineend = 2;
			bool concat = false;
			bool addspaces = false;
			string concatchar = string.Empty;
			bool skipempty = false;
			bool trimlines = false;
			bool numlines = false;
			bool redirected;
			bool set_c = false;
			bool set_l = false;
			bool set_s = false;
			bool set_t = false;
			bool set_input = false;

			if ( ConsoleEx.InputRedirected )
			{
				set_input = true;
				redirected = true;
			}
			else
			{
				if ( args.Length == 0 )
				{
					return WriteError( );
				}
				redirected = false;
			}

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' )
				{
					try
					{
						switch ( arg.ToUpper( )[1] )
						{
							case '?':
								return WriteError( );
							case 'C':
								if ( arg.ToUpper( ) != "/C" && arg.ToUpper( ) != "/CS" )
								{
									return WriteError( "Invalid command line switch " + arg );
								}
								concat = true;
								if ( arg.ToUpper( ) == "/CS" )
								{
									addspaces = true;
								}
								if ( set_c )
								{
									return WriteError( "Duplicate command line argument /C*" );
								}
								set_c = true;
								break;
							case 'L':
								if ( arg.ToUpper( ).StartsWith( "/L:" ) && arg.Length > 3 )
								{
									if ( arg[2] == ':' )
									{
										string linessel = arg.Substring( 3 );
										string pattern = @"^(\-?\d+)$";
										Match match = Regex.Match( linessel, pattern );
										if ( match.Success )
										{
											linestart = Convert.ToInt32( match.Groups[1].Value );
											lineend = linestart + 1;
										}
										else
										{
											pattern = @"^(\-?\d+)\.\.(\-?\d+)$";
											match = Regex.Match( linessel, pattern );
											if ( match.Success )
											{
												linestart = Convert.ToInt32( match.Groups[1].Value );
												lineend = Convert.ToInt32( match.Groups[2].Value ) + 1;
											}
											else
											{
												pattern = @"^(\-?\d+),(\-?\d+)$";
												match = Regex.Match( linessel, pattern );
												if ( match.Success )
												{
													// numlines is true if the second number specifies the number of lines instead of a line number
													numlines = true;
													linestart = Convert.ToInt32( match.Groups[1].Value );
													lineend = Convert.ToInt32( match.Groups[2].Value );
													if ( lineend < 1 )
													{
														return WriteError( "Invalid number of lines (" + lineend.ToString( ) + "), must be 1 or higher" );
													}
												}
											}
										}
									}
									else
									{
										return WriteError( "Invalid command line switch " + arg );
									}
								}
								else
								{
									return WriteError( "Invalid command line switch " + arg );
								}
								if ( set_l )
								{
									return WriteError( "Duplicate command line argument /L" );
								}
								set_l = true;
								break;
							case 'S':
								if ( arg.ToUpper( ) != "/SE" )
								{
									return WriteError( "Invalid command line switch " + arg );
								}
								skipempty = true;
								if ( set_s )
								{
									return WriteError( "Duplicate command line argument /SE" );
								}
								set_s = true;
								break;
							case 'T':
								if ( arg.ToUpper( ) != "/T" )
								{
									return WriteError( "Invalid command line switch " + arg );
								}
								trimlines = true;
								if ( set_t )
								{
									return WriteError( "Duplicate command line argument /T" );
								}
								set_t = true;
								break;
							default:
								return WriteError( "Invalid command line switch " + arg );
						}
					}
					catch
					{
						return WriteError( "Invalid command line switch " + arg );
					}
				}
				else
				{
					if ( set_input )
					{
						return WriteError( "Multiple inputs specified (file + redirection or multiple files)" );
					}
					if ( redirected )
					{
						return WriteError( "Do not specify a file name when using redirected input" );
					}
					else
					{
						filename = arg;
					}
				}
			}

			#endregion

			try
			{
				int count = 0;
				bool output = false;
				string[] lines;
				List<string> alllines = new List<string>( );

				if ( redirected )
				{
					// Read standard input and store the lines in a list
					int peek = 0;
					do
					{
						alllines.Add( Console.In.ReadLine( ) );
					} while ( peek != -1 );
					// Convert the list to an array
					lines = alllines.ToArray( );
				}
				else
				{
					// Read the file and store the lines in a list
					lines = File.ReadAllLines( filename );
				}

				// Check if negative numbers were used, and if so, calculate the resulting line numbers
				if ( linestart < 0 )
				{
					linestart += lines.Length + 1;
				}
				if ( lineend < 0 )
				{
					lineend += lines.Length + 1;
				}
				if ( numlines )
				{
					lineend += linestart;
				}
				
				// Iterate through the array of lines and display the ones matching the command line switches
				foreach ( string line in lines )
				{
					string myline = line;
					if ( trimlines )
					{
						myline = myline.Trim( );
					}
					bool skip = skipempty && ( myline.Trim( ) == string.Empty );
					if ( !skip )
					{
						count += 1;
						if ( count >= linestart && count < lineend )
						{
							if ( concat )
							{
								Console.Write( "{0}{1}", concatchar, myline );
							}
							else
							{
								Console.WriteLine( myline );
							}
							if ( addspaces )
							{
								concatchar = " ";
							}
						}
					}
				}
			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}

			return 0;
		}

		#region Redirection Detection

		// Code to detect redirection by Hans Passant on StackOverflow.com
		// http://stackoverflow.com/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected
		public static class ConsoleEx
		{
			public static bool OutputRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stdout ) );
				}
			}

			public static bool InputRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stdin ) );
				}
			}

			public static bool ErrorRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stderr ) );
				}
			}

			// P/Invoke:
			private enum FileType { Unknown, Disk, Char, Pipe };
			private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };

			[DllImport( "kernel32.dll" )]
			private static extern FileType GetFileType( IntPtr hdl );

			[DllImport( "kernel32.dll" )]
			private static extern IntPtr GetStdHandle( StdHandle std );
		}

		#endregion

		#region Error Handling

		public static int WriteError( Exception e = null )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR: " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}

			/*
			ReadLine,  Version 0.30 beta
			Return the specified line(s) from a file or Standard Input

			Usage:   READLINE  filename  [ options ]
			   or:   READLINE  [ options ]  <  filename
			   or:   command  |  READLINE  [ options ]

			Where:   filename   is the optional file to be read
					 command    is the optional command whose output is to be read

			Options: /C         Concatenate lines
					 /CS        Concatenate lines with Spaces in between
					 /L:n       read line n
					 /L:n..m    read lines n through m
					 /L:n,m     read m lines starting at line n
								(negative numbers start counting from the end backwards)
					 /SE        Skip Empty lines
			         /T         Trim leading and trailing whitespace from lines

			Examples:
			READLINE  file                        read the first non-empty line (default)
			READLINE  file  /L:2  /SE             read the second non-empty line of file
			READLINE  file  /L:5..7               read lines 5..7 of file
			READLINE  file  /L:-1                 read the last line of file
			READLINE  file  /L:-2..-1             read the last 2 lines of file
			READLINE  file  /L:-2,2               read the last 2 lines of file

			Check for redirection by Hans Passant on StackOverflow.com
			/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "ReadLine,  Version 0.30 beta" );
			Console.Error.WriteLine( "Return the specified line(s) from a file or Standard Input" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "READLINE  filename  [ options ]" );
			Console.ResetColor( );
			Console.Error.Write( "   or:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "READLINE  [ options ]  <  filename" );
			Console.ResetColor( );
			Console.Error.Write( "   or:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "command  |  READLINE  [ options ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the optional file to be read" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         command" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    is the optional command whose output is to be read" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/C         C" );
			Console.ResetColor( );
			Console.Error.WriteLine( "oncatenate lines" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /CS        C" );
			Console.ResetColor( );
			Console.Error.Write( "oncatenate lines with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "paces in between" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /L:n" );
			Console.ResetColor( );
			Console.Error.Write( "       read line " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "n" );
			Console.ResetColor( );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /L:n..m" );
			Console.ResetColor( );
			Console.Error.Write( "    read lines " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "n" );
			Console.ResetColor( );
			Console.Error.Write( " through " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "m" );
			Console.Error.Write( "         /L:n,m" );
			Console.ResetColor( );
			Console.Error.Write( "     read " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "m" );
			Console.ResetColor( );
			Console.Error.Write( " lines starting at line " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "n" );
			Console.ResetColor( );
			Console.Error.WriteLine( "                    (negative numbers start counting from the end backwards)" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /SE        S" );
			Console.ResetColor( );
			Console.Error.Write( "kip " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "E" );
			Console.ResetColor( );
			Console.Error.WriteLine( "mpty lines" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /T         T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "rim leading and trailing whitespace from lines" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Examples:" );
			Console.Error.WriteLine( "READLINE  file                        read the first non-empty line (default)" );
			Console.Error.WriteLine( "READLINE  file  /L:2  /SE             read the second non-empty line of file" );
			Console.Error.WriteLine( "READLINE  file  /L:5..7               read lines 5..7 of file" );
			Console.Error.WriteLine( "READLINE  file  /L:-1                 read the last line of file" );
			Console.Error.WriteLine( "READLINE  file  /L:-2..-1             read the last 2 lines of file" );
			Console.Error.WriteLine( "READLINE  file  /L:-2,2               read the last 2 lines of file" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Check for redirection by Hans Passant on " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "StackOverflow.com" );
			Console.Error.WriteLine( "/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}

		#endregion

	}
}
