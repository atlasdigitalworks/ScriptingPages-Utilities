using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RobvanderWoude
{
	class BatHL
	{
		static int Main( string[] args )
		{
			#region Command Line Parsing

			bool dispnums = true;
			bool logging = false;
			bool silent = false;
			string logfile = String.Empty;
			string source = String.Empty;

			if ( args.Length == 0 || args.Length > 3 )
			{
				return WriteError( );
			}
			foreach ( string arg in args )
			{
				if ( arg.Substring( 0, 1 ) == "/" )
				{
					if ( arg.ToUpper( ) == "/N" )
					{
						dispnums = false;
					}
					else if ( arg.ToUpper( ) == "/S" )
					{
						silent = true;
						dispnums = false;
					}
					else if ( arg.ToUpper( ).Substring( 0, 3 ) == "/L:" )
					{
						logging = true;
						logfile = arg.Substring( 3 );
					}
					else
					{
						return WriteError( "Invalid command line argument \"" + arg + "\"" );
					}
				}
				else
				{
					if ( String.IsNullOrEmpty( source ) )
					{
						source = arg;
					}
					else
					{
						return WriteError( "Invalid command line argument \"" + arg + "\"" );
					}
				}
			}

			if ( String.IsNullOrEmpty( source ) )
			{
				return WriteError( "Please specify a source file" );
			}
			if ( !File.Exists( source ) )
			{
				return WriteError( "Source file not found" );
			}
			if ( !String.IsNullOrEmpty( logfile ) )
			{
				if ( !Directory.Exists( Directory.GetParent( logfile ).FullName ) )
				{
					return WriteError( "Invalid path to logfile" );
				}
			}

			#endregion Command Line Parsing

			int parenthesis = 0;
			int rc = 1;
			string errors = String.Empty;

			// testing...
			//Console.ResetColor( );

			ConsoleColor bgcolor = Console.BackgroundColor;
			ConsoleColor fgcolor = Console.ForegroundColor;
			try
			{
				source = args[0];
				if ( source.IndexOfAny( "/?*".ToCharArray( ) ) > -1 )
				{
					return WriteError( );
				}
				if ( File.Exists( source ) )
				{
					rc = 0;
					int linenum = 0;
					StreamReader src = new StreamReader( source );
					while ( !src.EndOfStream )
					{
						linenum += 1;
						if ( dispnums )
						{
							// display line numbers
							Console.BackgroundColor = ConsoleColor.Gray;
							Console.ForegroundColor = ConsoleColor.DarkMagenta;
							Console.Write( "{0,4}", linenum );
							Console.BackgroundColor = bgcolor;
							Console.ForegroundColor = fgcolor;
							Console.Write( " " );
						}
						RegexOptions options = RegexOptions.IgnoreCase;
						string line = src.ReadLine( );
						string pattern = @"\(";
						int paropen = Regex.Matches( line, pattern ).Count;
						pattern = @"\)";
						int parclose = Regex.Matches( line, pattern ).Count;
						parenthesis = parenthesis + paropen - parclose;
						if ( parenthesis < 0 )
						{
							string temperr = "\nToo many closing parenthesis, starting at line ";
							if ( errors.IndexOf( temperr ) == -1 )
							{
								errors += temperr + linenum;
							}
						}
						// check for orphaned doublequotes
						pattern = "\"";
						if ( Regex.Matches( line, pattern ).Count % 2 != 0 )
						{
							errors += "\nOdd number of doublequotes in line " + linenum;
						}
						// check for orphaned singlequotes
						pattern = "'";
						if ( Regex.Matches( line, pattern ).Count % 2 != 0 )
						{
							errors += "\nOdd number of singlequotes in line " + linenum;
						}
						// check for orphaned percent signs
						pattern = "%";
						int perc = Regex.Matches( line, pattern ).Count;
						// ignore %~1 etc.
						pattern = "[^%]%~";
						perc -= Regex.Matches( line, pattern ).Count;
						if ( perc % 2 != 0 )
						{
							errors += "\nPossibly unterminated percent signs in line " + linenum;
						}
						// check for orphaned exclamation marks
						pattern = "!";
						if ( Regex.Matches( line, pattern ).Count % 2 != 0 )
						{
							errors += "\nOdd number of exclamation marks in line " + linenum;
						}

						if ( !silent )
						{
							// highlight remarks
							pattern = @"^\s*(REM(\s|$)|::)";
							Regex regex = new Regex( pattern, options );
							if ( regex.IsMatch( line ) )
							{
								Console.ForegroundColor = ConsoleColor.DarkGreen;
								Console.WriteLine( line );
								Console.ResetColor( );
								if ( parenthesis != 0 )
								{
									pattern = @"^\s*::";
									regex = new Regex( pattern, options );
									if ( regex.IsMatch( line ) )
									{
										errors += "\nNever use \"::\" inside code blocks, use \"REM\" instead! Potential error detected in line " + linenum;
									}
								}
							}
							else
							{
								// highlight ECHOed text - may err on escaped redirection marks
								pattern = @"^(.*\b@?ECHO(?:\.|\s))(.+?)((?:&|>|\|).+)?$";
								regex = new Regex( pattern, options );
								if ( regex.IsMatch( line ) )
								{
									MatchCollection matches = Regex.Matches( line, pattern, options );
									foreach ( Match match in matches )
									{
										Console.Write( match.Groups[1].Value );
										string test = match.Groups[2].Value.Trim( ).ToLower( );
										// ignore ECHO OFF or ECHO ON, highlight any other ECHOed text
										if ( test != "off" && test != "on" )
										{
											Console.ForegroundColor = ConsoleColor.Cyan;
										}
										Console.Write( match.Groups[2].Value );
										Console.ResetColor( );
										Console.WriteLine( match.Groups[3].Value );
									}
								}
								else
								{
									// highlight doublequoted strings
									if ( line.IndexOf( '"' ) > -1 )
									{
										string part;
										int len = line.Length;
										int idx = line.IndexOf( '"' );
										// split the line into quoted and non-quoted parts
										while ( !String.IsNullOrWhiteSpace( line ) )
										{
											len = line.Length;
											idx = line.IndexOf( '"' );
											if ( idx == 0 )
											{
												if ( line.IndexOf( '"', 1 ) == -1 )
												{
													Console.ForegroundColor = ConsoleColor.Red;
													part = line;
													line = String.Empty;
													len = 0;
													idx = -1;
													errors += "\nUnterminated doublequotes in line " + linenum;
													rc = linenum;
												}
												else
												{
													Console.ForegroundColor = ConsoleColor.Yellow;
													part = line.Substring( 0, Math.Min( line.IndexOf( '"', 1 ) + 1, len ) );
													line = line.Substring( Math.Min( line.IndexOf( '"', 1 ) + 1, len ) );
													len = line.Length;
													idx = line.IndexOf( '"' );
												}
											}
											else
											{
												Console.ResetColor( );
												if ( line.IndexOf( '"', 1 ) == -1 )
												{
													part = line;
													line = String.Empty;
													len = 0;
													idx = -1;
												}
												else
												{
													part = line.Substring( 0, Math.Min( line.IndexOf( '"' ), len ) );
													line = line.Substring( Math.Min( line.IndexOf( '"' ), len ) );
													len = line.Length;
													idx = line.IndexOf( '"' );
												}
											}
											Console.Write( part );
										}
										Console.WriteLine( );
										Console.ResetColor( );
									}
									else
									{
										if ( line.ToUpper( ).IndexOf( "IF EXISTS" ) > -1 )
										{
											errors += "\n\"IF EXISTS\" (with trailing \"S\") found in line " + linenum + ", correct syntax for file existance check is \"IF EXIST\" without trailing \"S\"";
										}
										// highlight some special characters in "ordinary" code
										Console.ResetColor( );
										string[] test = new string[] { "X", "Y", "Z" };
										while ( !String.IsNullOrWhiteSpace( test[2] ) )
										{
											test = FindChars( "()'%!".ToCharArray( ), line );
											Console.Write( test[0] );
											HighlightChar( test[1] );
											line = test[2];
										}
										Console.WriteLine( line );

									}
								}
								Console.ResetColor( );
							}
						}
					}
					src.Close( );
					// display warnings if applicable
					if ( parenthesis > 0 )
					{
						errors += "\nMissing " + parenthesis + " closing parenthesis";
					}
					if ( !String.IsNullOrEmpty( errors ) )
					{
						if ( silent )
						{
							Console.ResetColor( );
							Console.WriteLine( "File name: \"{0}\"", source );
							Console.WriteLine( );
						}
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine( errors );
						Console.ResetColor( );
						Console.WriteLine( );
						rc = Math.Max( 1, rc );
						if ( logging )
						{
							StreamWriter log = new StreamWriter( logfile, true );
							log.WriteLine( "File name: \"{0}\"", source );
							log.WriteLine( errors.Replace( "\n", "\n\t" ) + "\n\n" );
							log.Close( );
						}
					}
				}
				Console.ForegroundColor = fgcolor;
				Console.BackgroundColor = bgcolor;
				return rc;
			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}
		}


		public static string[] FindChars( char[] chrArr, string strIn )
		{
			int chrPos = strIn.IndexOfAny( chrArr );
			if ( chrPos > -1 )
			{
				string part1 = strIn.Substring( 0, Math.Max( 0, chrPos ) );
				string part2 = strIn.Substring( chrPos, 1 );
				string part3 = strIn.Substring( Math.Min( chrPos + 1, strIn.Length ) );
				return new string[] { part1, part2, part3 };
			}
			else
			{
				return new string[] { strIn, String.Empty, String.Empty };
			}
		}

		// write a single character in a different color
		public static void HighlightChar( string chr )
		{
			if ( !String.IsNullOrWhiteSpace( chr ) )
			{
				ConsoleColor foregroundcolor = Console.ForegroundColor;
				ConsoleColor highlightcolor;
				switch ( chr )
				{
					case "(":
					case ")":
					case "'":
						highlightcolor = ConsoleColor.Yellow;
						break;
					case "%":
					case "!":
						highlightcolor = ConsoleColor.Green;
						break;
					default:
						highlightcolor = foregroundcolor;
						break;
				}
				Console.ForegroundColor = highlightcolor;
				Console.Write( chr );
				Console.ForegroundColor = foregroundcolor;
			}
		}


		#region Error Handling

		public static int WriteError( Exception e = null )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			Console.OpenStandardError( );
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
			BatHL,  Version 0.51 beta
			Search batch source code for unterminated quotes, parenthesis and percent signs

			Usage:   BATHL  batchfile  [ /L:"logfile" ]  [ /N | /S ]

			Where:   batchfile    is the source code to check and highlight
			         /L:"logfile" tells the program to log the results to a file
			         /N           tells the program Not to display line numbers
			         /S           Silent operation: no highlighting, just warnings

			Notes:   The source code is displayed with highlighted ECHOed text,
			         comments and doublequoted strings (BatHL: Batch HighLighter).
			         A warning message will be displayed if any unterminated quotes,
			         parenthesis, or variables, or other syntax errors were found.
			         A non-zero return code means something was wrong, either on
			         the command line or in the source code. A return code higher
			         than 1 indicates the line number where an error was detected.
			         If an existing log file is specified, results will be appended.
			         If no errors are detected, nothing will be logged.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			 */

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "BatHL,  Version 0.51 beta" );
			Console.Error.WriteLine( "Search batch source code for unterminated quotes, parenthesis and percent signs" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "BATHL  batchfile  [ /L:\"logfile\" ]  [ /N | /S ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "batchfile" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    is the source code to check and highlight" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /L:\"logfile\"" );
			Console.ResetColor( );
			Console.Error.Write( " tells the program to" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "l" );
			Console.ResetColor( );
			Console.Error.WriteLine( "og the results to a file" );
			Console.Error.Write( "         /N" );
			Console.ResetColor( );
			Console.Error.Write( "           tells the program " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "N" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ot to display line numbers" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /S           S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ilent operation: no highlighting, just warnings" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Notes:   The source code is displayed with highlighted ECHOed text," );
			Console.Error.WriteLine( "         comments and doublequoted strings (BatHL: Batch HighLighter)." );
			Console.Error.WriteLine( "         A warning message will be displayed if any unterminated quotes," );
			Console.Error.WriteLine( "         parenthesis, or variables, or other syntax errors were found." );
			Console.Error.WriteLine( "         A non-zero return code means something was wrong, either on" );
			Console.Error.WriteLine( "         the command line or in the source code. A return code higher" );
			Console.Error.WriteLine( "         than 1 indicates the line number where an error was detected." );
			Console.Error.WriteLine( "         If an existing log file is specified, results will be appended." );
			Console.Error.WriteLine( "         If no errors are detected, nothing will be logged." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			Console.OpenStandardOutput( );
			return 1;
		}

		#endregion Error Handling
	}
}
