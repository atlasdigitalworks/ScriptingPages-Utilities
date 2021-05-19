using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace RobvanderWoude
{
	class TestCommandLineArgs
	{
		static readonly string progver = "1.00";


		static int Main( string[] args )
		{
			if ( args.Length == 0 || ( args.Length == 1 && args[0] == "/?" ) )
			{
				return ShowHelp( );
			}

			bool founddoublequote = false;
			int argscount = 0;

			Console.WriteLine( "Command line: {0}", Environment.CommandLine );
			Console.WriteLine( );
			Console.WriteLine( "Traditional command line parsing: {0} command line argument{1}{2}", args.Length, ( args.Length == 1 ? "" : "s" ), ( args.Length == 0 ? "" : ":" ) );
			for ( int i = 0; i < args.Length; i++ )
			{
				if ( args[i].IndexOf( "\"" ) > -1 )
				{
					founddoublequote = true;
					Console.Write( "[{0}]\t{1}", i, args[i] );
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine( "\t(contains doublequote)" );
					Console.ResetColor( );
				}
				else
				{
					Console.WriteLine( "[{0}]\t{1}", i, args[i] );
				}
			}
			if ( founddoublequote )
			{
				Console.WriteLine( );
				string pattern = @"^(?<quoteexec>\""?)(?<exec>[^\""]+?)\k<quoteexec>";
				pattern += @"(?:\s+(?<quotearg0>\""?)(?<arg0>[^\""]+?)\k<quotearg0>";
				pattern += @"(?:\s+(?<quotearg1>\""?)(?<arg1>[^\""]+?)\k<quotearg1>";
				pattern += @"(?:\s+(?<quotearg2>\""?)(?<arg2>[^\""]+?)\k<quotearg2>";
				pattern += @"(?:\s+(?<quotearg3>\""?)(?<arg3>[^\""]+?)\k<quotearg3>";
				pattern += @"(?:\s+(?<quotearg4>\""?)(?<arg4>[^\""]+?)\k<quotearg4>";
				pattern += @"(?:\s+(?<quotearg5>\""?)(?<arg5>[^\""]+?)\k<quotearg5>";
				pattern += @"(?:\s+(?<quotearg6>\""?)(?<arg6>[^\""]+?)\k<quotearg6>";
				pattern += @"(?:\s+(?<quotearg7>\""?)(?<arg7>[^\""]+?)\k<quotearg7>";
				pattern += @"(?:\s+(?<quotearg8>\""?)(?<arg8>[^\""]+?)\k<quotearg8>";
				pattern += @"(?:\s+(?<quotearg9>\""?)(?<arg9>[^\""]+?)\k<quotearg9>)?)?)?)?)?)?)?)?)?)?\s*$";
				Regex regex = new Regex( pattern );
				if ( regex.IsMatch( Environment.CommandLine ) )
				{
					Match match = regex.Match( Environment.CommandLine );
					Console.WriteLine( );
					for ( int i = 4; i < 21; i += 2 )
					{
						if ( !string.IsNullOrEmpty( match.Groups[i].Value ) )
						{
							argscount = i / 2 - 1;
						}
					}
					Console.WriteLine( "Alternative command line parsing: {0} command line argument{1}{2}", argscount, ( argscount == 1 ? "" : "s" ), ( argscount == 0 ? "" : ":" ) );
					if ( !string.IsNullOrEmpty( match.Groups["arg0"].Value ) )
					{
						Console.WriteLine( "[0]\t{0}", match.Groups["arg0"].Value );
						if ( !string.IsNullOrEmpty( match.Groups["arg1"].Value ) )
						{
							Console.WriteLine( "[1]\t{0}", match.Groups["arg1"].Value );
							if ( !string.IsNullOrEmpty( match.Groups["arg2"].Value ) )
							{
								Console.WriteLine( "[2]\t{0}", match.Groups["arg2"].Value );
								if ( !string.IsNullOrEmpty( match.Groups["arg3"].Value ) )
								{
									Console.WriteLine( "[3]\t{0}", match.Groups["arg3"].Value );
									if ( !string.IsNullOrEmpty( match.Groups["arg4"].Value ) )
									{
										Console.WriteLine( "[4]\t{0}", match.Groups["arg4"].Value );
										if ( !string.IsNullOrEmpty( match.Groups["arg5"].Value ) )
										{
											Console.WriteLine( "[5]\t{0}", match.Groups["arg5"].Value );
											if ( !string.IsNullOrEmpty( match.Groups["arg6"].Value ) )
											{
												Console.WriteLine( "[6]\t{0}", match.Groups["arg6"].Value );
												if ( !string.IsNullOrEmpty( match.Groups["arg7"].Value ) )
												{
													Console.WriteLine( "[7]\t{0}", match.Groups["arg7"].Value );
													if ( !string.IsNullOrEmpty( match.Groups["arg8"].Value ) )
													{
														Console.WriteLine( "[8]\t{0}", match.Groups["arg8"].Value );
														if ( !string.IsNullOrEmpty( match.Groups["arg9"].Value ) )
														{
															Console.WriteLine( "[9]\t{0}", match.Groups["arg9"].Value );
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return argscount;
		}


		#region Error handling

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
			TestCommandLineArgs.exe,  Version 1.00
			Test C# command line parsing

			Usage:   TestCommandLineArgs.exe  [ up to 9 command line arguments ]

			Returns: Displays each command line argument on a separate line, first
			         trying traditional method (parsing string[] args), and if that fails
			         an alternative method using a regex on Envrionment.CommandLine
			         Traditional parsing is considered unsuccessful if one of the arguments
			         contains at least one doublequote, not counting the enclosing quotes.
			         Return code -1 if no arguments, 0 if traditional parsing succeeded,
			         otherwise the number of arguments found by the alternative parser.

			Example:
			
			TestCommandLineArgs.exe "1 2 3" 4 "D:\" "C:\windows" 12 XYZ

			Output:
			
			Command line: "D:\TestCommandLineArgs.exe" "1 2 3" 4 "D:\" "C:\windows" 12 XYZ

			Traditional command line parsing: 3 command line arguments:
			[0]     1 2 3
			[1]     4
			[2]     D:" C:\windows 12 XYZ       (contains doublequote)


			Alternative command line parsing: 6 command line arguments:
			[0]     1 2 3
			[1]     4
			[2]     D:\
			[3]     C:\windows
			[4]     12
			[5]     XYZ

			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "TestCommandLineArgs.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Test C# command line parsing" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "TestCommandLineArgs.exe  [ up to 9 command line arguments ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Returns: Displays each command line argument on a separate line, first" );

			Console.Error.WriteLine( "         trying traditional method (parsing string[] args), and if that fails" );

			Console.Error.WriteLine( "         an alternative method using a regex on Envrionment.CommandLine" );

			Console.Error.WriteLine( "         Traditional parsing is considered unsuccessful if one of the arguments" );

			Console.Error.WriteLine( "         contains at least one doublequote, not counting the enclosing quotes." );

			Console.Error.WriteLine( "         Return code -1 if no arguments, 0 if traditional parsing succeeded," );

			Console.Error.WriteLine( "         otherwise the number of arguments found by the alternative parser." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Example:" );

			Console.Error.WriteLine( );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "TestCommandLineArgs.exe \"1 2 3\" 4 \"D:\\\" \"C:\\windows\" 12 XYZ" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Output:" );

			Console.Error.WriteLine( );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "Command line: \"D:\\TestCommandLineArgs.exe\" \"1 2 3\" 4 \"D:\\\" \"C:\\windows\" 12 XYZ" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Traditional command line parsing: 3 command line arguments:" );

			Console.Error.WriteLine( "[0]     1 2 3" );

			Console.Error.WriteLine( "[1]     4" );

			Console.Error.Write( "[2]     D:\" C:\\windows 12 XYZ       " );
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine( "(contains doublequote)" );
			Console.ForegroundColor = ConsoleColor.White;

			Console.Error.WriteLine( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Alternative command line parsing: 6 command line arguments:" );

			Console.Error.WriteLine( "[0]     1 2 3" );

			Console.Error.WriteLine( "[1]     4" );

			Console.Error.WriteLine( "[2]     D:\\" );

			Console.Error.WriteLine( "[3]     C:\\windows" );

			Console.Error.WriteLine( "[4]     12" );

			Console.Error.WriteLine( "[5]     XYZ" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Display Help Text


			return -1;
		}

		#endregion Error handling
	}
}
