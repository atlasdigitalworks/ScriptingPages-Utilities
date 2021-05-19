using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class RxReplace
	{
		static readonly string progver = "1.02";


		static int Main( string[] args )
		{
			#region Initialize variables

			string filename = string.Empty;
			string input = string.Empty;
			string pattern = string.Empty;
			string replacement = string.Empty;
			string result = string.Empty;
			string targetfile = string.Empty;
			string[] options = null;
			RegexOptions regexoptions = RegexOptions.None;
			Encoding encoding = Encoding.Default;
			bool errorredirected = ConsoleEx.ErrorRedirected;
			bool inputredirected = ConsoleEx.InputRedirected;
			bool outputredirected = ConsoleEx.OutputRedirected;
			bool caseset = false;
			bool outputset = false;
			bool skipset = false;
			bool takeset = false;
			bool unescape = true;
			int redirectnum = ( inputredirected ? 1 : 0 );
			int searchbytes = -1;
			int skipmatches = 0;
			int takematches = 0;

			#endregion Initialize variables


			#region Command Line Parsing

			if ( args.Length == 0 )
			{
				return ShowHelp( );
			}

			// Check for /? on the command line
			foreach ( string arg in args )
			{
				if ( arg == "/?" )
				{
					return ShowHelp( );
				}
			}

			// Check and interpret command line arguments
			if ( args.Length + redirectnum == 3 )
			{
				if ( inputredirected )
				{
					pattern = args[0];
					replacement = args[1];
				}
				else
				{
					filename = args[0];
					pattern = args[1];
					replacement = args[2];
				}
			}
			else if ( args.Length + redirectnum > 7 )
			{
				return ShowHelp( );
			}
			else
			{
				if ( inputredirected )
				{
					pattern = args[0];
					replacement = args[1];
					options = args.Slice( 2, args.Length );
				}
				else
				{
					filename = args[0];
					pattern = args[1];
					replacement = args[2];
					options = args.Slice( 3, args.Length );
				}
			}

			if ( options != null )
			{
				foreach ( string option in options )
				{
					switch ( option.ToUpper( ).Substring( 0, 2 ) )
					{
						case "/F":
							if ( searchbytes != -1 )
							{
								return ShowHelp( "Duplicate command line switch /F" );
							}
							try
							{
								searchbytes = Convert.ToInt32( option.Substring( 3 ) );
							}
							catch ( Exception )
							{
								return ShowHelp( "Invalid command line switch {0}", option );
							}
							break;
						case "/I":
							if ( caseset )
							{
								return ShowHelp( "Duplicate command line switch /I" );
							}
							regexoptions |= RegexOptions.IgnoreCase;
							caseset = true;
							break;
						case "/L":
							if ( !unescape )
							{
								return ShowHelp( "Duplicate command line switch /L" );
							}
							unescape = false;
							break;
						case "/O":
							if ( outputset )
							{
								return ShowHelp( "Duplicate switch /O" );
							}
							if ( outputredirected )
							{
								return ShowHelp( "Cannot combine command line switch /O and output redirection" );
							}
							if ( option.Trim( '"' ).Length > 4 && option[2] == ':' )
							{
								targetfile = option.Substring( 3 );
								if ( !Directory.GetParent( targetfile ).Exists )
								{
									return ShowHelp( "Invalid output path \"{0}\"", targetfile );
								}
							}
							else if ( !inputredirected )
							{
								targetfile = filename;
							}
							else
							{
								return ShowHelp( "Output file name is mandatory when combining command line switch /O and input redirection" );
							}
							outputset = true;
							break;
						case "/S":
							if ( skipset )
							{
								return ShowHelp( "Duplicate command line switch /S" );
							}
							try
							{
								skipmatches = Convert.ToInt32( option.Substring( 3 ) );
							}
							catch ( Exception e )
							{
								Console.Error.WriteLine( "Error: {0}", e.Message );
								return ShowHelp( "Invalid command line argument \"{0}\"", option );
							}
							skipset = true;
							break;
						case "/T":
							if ( takeset )
							{
								return ShowHelp( "Duplicate switch /T" );
							}
							try
							{
								takematches = Convert.ToInt32( option.Substring( 3 ) );
							}
							catch ( Exception e )
							{
								Console.Error.WriteLine( "Error: {0}", e.Message );
								return ShowHelp( "Invalid command line argument \"{0}\"", option );
							}
							takeset = true;
							break;
						default:
							return ShowHelp( "Invalid command line {0}: \"{1}\"", ( option[0] == '/' ? "switch" : "argument" ), option );
					}
				}
			}

			if ( unescape )
			{
				// Unescape replacement to allow unicode characters, newlines, tabs, etc.
				replacement = UnEscapeString( replacement );
			}

			#endregion Command Line Parsing


			#region Read input

			if ( inputredirected )
			{
				// Read the redirected Standard Input
				input = Console.In.ReadToEnd( );
				if ( searchbytes > -1 && searchbytes < input.Length )
				{
					input = input.Substring( 0, searchbytes );
				}
			}
			else
			{
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
					// Get file size
					long filesize = new FileInfo( filename ).Length;
					// Read the file content
					using ( StreamReader file = new StreamReader( filename, true ) )
					{
						if ( searchbytes > -1 && searchbytes < filesize )
						{
							char[] buffer = new char[searchbytes];
							file.Read( buffer, 0, searchbytes );
							input = string.Join( string.Empty, buffer );
						}
						else
						{
							input = file.ReadToEnd( );
						}
						encoding = file.CurrentEncoding;
						file.Close( );
					}
				}
				else
				{
					return ShowHelp( "File not found: \"{0}\"", filename );
				}
			}

			#endregion Read input


			#region Replace text

			if ( skipmatches == 0 && takematches == 0 )
			{
				result = Regex.Replace( input, pattern, replacement );
			}
			else
			{
				// Get all matches
				Regex regex = new Regex( pattern, regexoptions );
				MatchCollection matches = Regex.Matches( input, pattern, regexoptions );
				if ( matches.Count > skipmatches )
				{
					int counter = 0;
					int lastindex = 0;
					int replaced = 0;
					foreach ( Match match in matches )
					{
						if ( counter >= skipmatches && ( replaced < takematches || takematches == 0 ) )
						{
							result += input.Substring( lastindex, match.Index ) + match.ToString( );
							lastindex = match.Index + match.Length;
							replaced += 1;
						}
						counter += 1;
					}
					if ( lastindex < input.Length )
					{
						result += input.Substring( lastindex );
					}
				}
			}

			#endregion Replace text


			#region Return results

			if ( string.IsNullOrWhiteSpace( targetfile ) )
			{
				Console.Write( result );
			}
			else
			{
				using ( StreamWriter file = new StreamWriter( targetfile, false, Encoding.GetEncoding( encoding.CodePage ) ) )
				{
					file.Write( result );
					file.Close( );
				}
			}

			return 0;

			#endregion Return results
		}


		static string UnEscapeString( string text )
		{
			// Unescaping tabs, linefeeds and quotes
			text = text.Replace( "\\n", "\n" );
			text = text.Replace( "\\r", "\r" );
			text = text.Replace( "\\t", "\t" );
			text = text.Replace( "\\007", "\t" );
			text = text.Replace( "\\012", "\n" );
			text = text.Replace( "\\015", "\r" );
			text = text.Replace( "\\042", "\"" );
			text = text.Replace( "\\047", "'" );
			// Unescaping Unicode, technique by "dtb" on StackOverflow.com: http://stackoverflow.com/a/8558748
			text = Regex.Replace( text, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString( (char) ushort.Parse( m.Groups[1].Value, NumberStyles.AllowHexSpecifier ) ) );
			return text;
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

		#endregion Redirection Detection


		#region Error Handling

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
			RxReplace,  Version 1.02
			Multi-line regex find and replace tool

			Usage:   RXREPLACE  filename  pattern  replacement  [ options ]
			or:      some_command  |  RXREPLACE  pattern  replacement  [ options ]

			Where:   filename       is the file to be filtered
			         some_command   is the command whose standard output is to be filtered
			         pattern        is the regex pattern for text to be replaced
			         replacement    replaces all matches (backreferences allowed, e.g. $1)
			
			Options: /F:bytes       search the First specified number of bytes only
			         /I             makes the search case Insensitive
			         /L             treat replacement as Literal text
			                        (default: unescape replacement)
			         /O[:newfile]   Overwrite original, or write Output to newfile
			         /S:nn          Skip the first nn matches
			         /T:nn          Take only nn matches

			Notes:   If /F:bytes is used and a file is specified, only the first bytes
			         of that file will be read; if the input is redirected, it is read
			         entirely, and will then be chopped to the specified number of bytes
			         before being searched. In both cases, the remainder of the input will
			         be discarded.
 			         Unless /O switch is used, result will be written to Standard Output.
			         Backreferences in replacement can not be used with /S or /T.
			
			Credits: Check for redirection by Hans Passant on StackOverflow.com:
			         http://stackoverflow.com/a/3453272
			         Array Slice extension by Sam Allen on DotNetPerls.com:
			         http://www.dotnetperls.com/array-slice
			         Unescaping Unicode by "dtb" on StackOverflow.com:
			         http://stackoverflow.com/a/8558748

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			 */

			#endregion Help Text

			
			#region Display Help

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "RxReplace,  Version {0}", progver );

			Console.Error.WriteLine( "Multi-line regex find and replace tool" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "RXREPLACE  filename  pattern  replacement  [ options ]" );
			Console.ResetColor( );

			Console.Error.Write( "or:      " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "some_command  |  RXREPLACE  pattern  replacement  [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( "       is the file to be filtered" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         some_command" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the command whose standard output is to be filtered" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         pattern" );
			Console.ResetColor( );
			Console.Error.WriteLine( "        is the regex pattern for text to be replaced" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         replacement" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    replaces all matches (backreferences allowed, e.g. $1)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/F:bytes" );
			Console.ResetColor( );
			Console.Error.Write( "       search the " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "F" );
			Console.ResetColor( );
			Console.Error.Write( "irst specified number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "bytes" );
			Console.ResetColor( );
			Console.Error.WriteLine( " only" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /I" );
			Console.ResetColor( );
			Console.Error.Write( "             makes the search case " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "I" );
			Console.ResetColor( );
			Console.Error.WriteLine( "nsensitive" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /L" );
			Console.ResetColor( );
			Console.Error.Write( "             treat " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "replacement" );
			Console.ResetColor( );
			Console.Error.Write( " as " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "iteral text" );

			Console.Error.Write( "                        (default: unescape " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "replacement" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /O[:newfile]   O" );
			Console.ResetColor( );
			Console.Error.Write( "verwrite original, or write " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "O" );
			Console.ResetColor( );
			Console.Error.Write( "utput to " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "newfile" );
			Console.ResetColor( );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /S:nn          S" );
			Console.ResetColor( );
			Console.Error.Write( "kip the first " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "nn" );
			Console.ResetColor( );
			Console.Error.WriteLine( " matches" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /T:nn          T" );
			Console.ResetColor( );
			Console.Error.Write( "ake only " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "nn" );
			Console.ResetColor( );
			Console.Error.WriteLine( " matches" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:   If " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/F:bytes" );
			Console.ResetColor( );
			Console.Error.Write( " is used and a file is specified, only the first " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "bytes" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         of that file will be read; if the input is redirected, it is read" );

			Console.Error.Write( "         entirely, and will then be chopped to the specified number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "bytes" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         before being searched. In both cases, the remainder of the input will" );

			Console.Error.WriteLine( "         be discarded." );

			Console.Error.Write( "         Unless " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/O" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch is used, result will be written to Standard Output." );

			Console.Error.Write( "         Backreferences in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "replacement" );
			Console.ResetColor( );
			Console.Error.Write( " can " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "not" );
			Console.ResetColor( );
			Console.Error.Write( " be used with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.Write( " or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits: Check for redirection by Hans Passant on StackOverflow.com:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         http://stackoverflow.com/a/3453272" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         Array Slice extension by Sam Allen on DotNetPerls.com:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         http://www.dotnetperls.com/array-slice" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         Unescaping Unicode by \"dtb\" on StackOverflow.com:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         http://stackoverflow.com/a/8558748" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Display Help


			return 1;
		}

		#endregion Error Handling
	}


	#region Extensions

	// Array Slice
	// http://www.dotnetperls.com/array-slice
	public static class Extensions
	{
		/// <summary>
		/// Get the array slice between the two indexes.
		/// ... Inclusive for start index, exclusive for end index.
		/// </summary>
		public static T[] Slice<T>( this T[] source, int start, int end )
		{
			// Handles negative ends.
			if ( end < 0 )
			{
				end = source.Length + end;
			}
			int len = end - start;

			// Return new array.
			T[] res = new T[len];
			for ( int i = 0; i < len; i++ )
			{
				res[i] = source[i + start];
			}
			return res;
		}
	}

	#endregion Extensions
}
