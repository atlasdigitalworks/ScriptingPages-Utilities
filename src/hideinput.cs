using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class HideInput
	{
		static string progver = "1.03";

		static bool oddrow = false;
		public static ConsoleColor bold = ConsoleColor.White;


		static int Main( string[] args )
		{
			bool clearscreen = false;
			bool filter = false;
			FilterAction filteraction = FilterAction.Remove;
			string mask = null;
			int rc = 0;


			#region Command Line Parsing

			if ( args.Length > 0 )
			{
				foreach ( string arg in args )
				{
					if ( arg.Length > 1 && arg[0] == '/' )
					{
						switch ( arg[1].ToString( ).ToUpper( ) )
						{
							case "?":
								return ShowHelp( );
							case "C":
								if ( clearscreen )
								{
									return ShowHelp( "Duplicate command line switch /C" );
								}
								clearscreen = true; // No longer necessary, provided for backwards compatibility only
								break;
							case "F":
								if ( filter )
								{
									return ShowHelp( "Duplicate command line switch /F" );
								}
								if ( arg.Length > 3 && arg[2] == ':' )
								{
									filter = true;
									string action = arg.Substring( 3 );
									switch ( action.ToUpper( ) )
									{
										case "A":
										case "ABORT":
											filteraction = FilterAction.Abort;
											break;
										case "D":
										case "DETECT":
										case "DETECTONLY":
											filteraction = FilterAction.DetectOnly;
											break;
										case "E":
										case "END":
										case "ENDOFINPUT":
											filteraction = FilterAction.EndOfInput;
											break;
										case "R":
										case "REMOVE":
											filteraction = FilterAction.Remove;
											break;
										case "W":
										case "WHITESPACE":
											filteraction = FilterAction.WhiteSpace;
											break;
										default:
											return ShowHelp( "Invalid filter argument \"{0}\"", arg );
									}
								}
								else
								{
									return ShowHelp( "Invalid filter argument \"{0}\"", arg );
								}
								break;
							case "M":
								if ( !String.IsNullOrEmpty( mask ) )
								{
									return ShowHelp( "Duplicate command line switch /M" );
								}
								if ( arg.Length == 2 )
								{
									return HelpForMask( );
								}
								if ( arg.Length > 3 && arg[2] == ':' )
								{
									mask = arg.Substring( 3 );
								}
								else
								{
									return ShowHelp( "Invalid mask specification \"{0}\"", arg );
								}
								break;
							default:
								return ShowHelp( "Invalid command line argument \"{0}\"", arg );
						}
					}
					else
					{
						return ShowHelp( "Invalid command line argument \"{0}\"", arg );
					}
				}
			}

			#endregion Command Line Parsing


			#region Read Input

			// Read 1 line of input from the console
			string input = String.Empty;
			char key = (char) 27;
			while ( key != (char) 13 )
			{
				ConsoleKeyInfo keyinfo = Console.ReadKey( true );
				key = keyinfo.KeyChar;
				if ( key != (char) 13 )
				{
					input += keyinfo.KeyChar.ToString( );
				}
			}

			#endregion Read Input


			#region Clear Screen

			// Though no longer necessary for safety, this feature is provided for backwards compatibility
			if ( clearscreen )
			{
				Console.Clear( );
			}

			#endregion Clear Screen


			#region Apply Mask to Input

			if ( !String.IsNullOrEmpty( mask ) )
			{
				using ( MaskedTextBox maskedtextbox = new MaskedTextBox( mask ) )
				{
					maskedtextbox.Text = String.Empty;
					maskedtextbox.AppendText( input );
					input = maskedtextbox.Text;
				}
			}

			#endregion Apply Mask to Input


			#region Apply Filter to Output

			if ( filter )
			{
				string specialchars= @"(\^|&|\||<|>|\(|\)|\""|'|\\|%|!)";
				Regex regex = new Regex( specialchars );
				if ( regex.IsMatch( input ) )
				{
					rc = 3;
					switch ( filteraction )
					{
						case FilterAction.Abort:
							input = String.Empty;
							break;
						case FilterAction.DetectOnly:
							break;
						case FilterAction.EndOfInput:
							int pos = input.IndexOfAny( "^&|<>()\"'\\%!".ToCharArray( ) );
							if ( pos != -1 )
							{
								input = input.Substring( 0, pos );
							}
							break;
						case FilterAction.Remove:
							input = regex.Replace( input, "" );
							break;
						case FilterAction.WhiteSpace:
							input = regex.Replace( input, " " );
							break;
					}
				}
			}

			#endregion Apply Filter to Output


			// Display the input - which should be redirected for this program to be of any use
			Console.WriteLine( input );

			// Returncode 0 for success, 2 if the input was empty or whitespace only, 3 if "forbidden" characters were detected
			if ( rc != 3 && string.IsNullOrWhiteSpace( input ) )
			{
				rc = 2;
			}
			return rc;
		}


		public static int HelpForMask()
		{
			int col1perc = 13;
			Console.Error.Write( "Help for command line switch " );
			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "/M:mask" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "The " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "mask" );
			Console.ResetColor( );
			Console.Error.WriteLine( " \"language\" is based on the Masked Edit control in Visual Basic 6.0:" );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			string url1 = "http://msdn.microsoft.com/en-us/library/";
			string url2 = "system.windows.forms.maskedtextbox.mask.aspx#remarksToggle";
			if ( url1.Length + url2.Length > Console.WindowWidth )
			{
				Console.Error.WriteLine( url1 );
				Console.Error.WriteLine( url2 );
			}
			else
			{
				Console.Error.WriteLine( url1 + url2 );
			}
			Console.ResetColor( );
			Console.Error.WriteLine( );
			WriteTableRow( "Masking element", "Description", col1perc, true, true );
			WriteTableRow( "0", "Digit, required. This element will accept any single digit between 0 and 9.", col1perc );
			WriteTableRow( "9", "Digit or space, optional.", col1perc );
			WriteTableRow( "#", "Digit or space, optional. If this position is blank in the mask, it will be rendered as a space in the Text property. Plus (+) and minus (-) signs are allowed.", col1perc );
			WriteTableRow( "L", "Letter, required. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z] in regular expressions.", col1perc );
			WriteTableRow( "?", "Letter, optional. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z]? in regular expressions.", col1perc );
			WriteTableRow( "&", "Character, required. Any non-control character. If ASCII only is set (/A), this element behaves like the \"A\" element.", col1perc );
			WriteTableRow( "C", "Character, optional. Any non-control character. If ASCII only is set (/A), this element behaves like the \"a\" element.", col1perc );
			WriteTableRow( "A", "Alphanumeric, required. If ASCII only is set (/A), the only characters it will accept are the ASCII letters a-z and A-Z and numbers. This mask element behaves like the \"&\" element.", col1perc );
			WriteTableRow( "a", "Alphanumeric, optional. If ASCII only is set (/A), the only characters it will accept are the ASCII letters a-z and A-Z and numbers. This mask element behaves like the \"C\" element.", col1perc );
			WriteTableRow( ".", "Decimal placeholder.", col1perc );
			WriteTableRow( ",", "Thousands placeholder.", col1perc );
			WriteTableRow( ":", "Time separator.", col1perc );
			WriteTableRow( "/", "Date separator.", col1perc );
			WriteTableRow( "$", "Currency symbol.", col1perc );
			WriteTableRow( "<", "Shift down. Converts all characters that follow to lowercase.", col1perc );
			WriteTableRow( ">", "Shift up. Converts all characters that follow to uppercase.", col1perc );
			WriteTableRow( "|", "Disable a previous shift up or shift down.", col1perc );
			WriteTableRow( @"\", "Escape. Escapes a mask character, turning it into a literal. \"\\\\\" is the escape sequence for a backslash.", col1perc );
			WriteTableRow( "All other characters", "Literals. All non-mask elements will appear as themselves within MaskedTextBox. Literals always occupy a static position in the mask at run time, and cannot be moved or deleted by the user.", col1perc );

			return 1;
		}


		public static int ShowHelp( params string[] errmsg )
		{
			#region Help Text

			/*
			HideInput,  Version 1.03
			Batch utility to read 1 line of input while hiding what's being typed

			Usage:   FOR /F "tokens=*" %%A IN ('HIDEINPUT  [ options ]') DO SET passwd=%%A

			   or:   HIDEINPUT  [ options ] > file

			Options: /C        Clear screen afterwards (no longer required to remove input
			                   from keyboard buffer; provided for backwards compatibility)
			         /F:action Filter special characters ^ & | < > ( ) " ' % ! and \
			                   action can be A = Abort (input is discarded), D = DetectOnly
			                   (input is left unchanged), E = EndOfInput (input is chopped
			                   at first "forbidden" character), R = Remove (remove
			                   "forbidden" characters) or W = WhiteSpace (replace
			                   "forbidden" characters with spaces)
			         /M:mask   specifies an optional mask that will be applied to the input
			
			Notes:   HIDEINPUT /M without a mask will show help for the "mask language".
			         Return code ("errorlevel") is 0 if valid input is received, 1 in case
			         of (command line) errors, 2 if input is empty or whitespace only, 3 if
			         "forbidden" characters were detected by the filter.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			#endregion Help Text


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


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "HideInput,  Version {0}", progver );

			Console.Error.WriteLine( "Batch utility to read 1 line of input while hiding what's being typed" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   FOR /F \"tokens=*\" %%A IN ('" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "HIDEINPUT  [ options ]" );
			Console.ResetColor( );
			Console.Error.WriteLine( "') DO SET passwd=%%A" );

			Console.Error.WriteLine( );

			Console.Error.Write( "   or:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "HIDEINPUT  [ options ] > file" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/C        C" );
			Console.ResetColor( );
			Console.Error.WriteLine( "lear screen afterwards (no longer required to remove input" );

			Console.Error.WriteLine( "                   from keyboard buffer; provided for backwards compatibility)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /F:action F" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ilter special characters ^ & | < > ( ) \" ' % ! and \\" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "                   action" );
			Console.ResetColor( );
			Console.Error.Write( " can be " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.Write( " = Abort (input is discarded), " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "D" );
			Console.ResetColor( );
			Console.Error.WriteLine( " = DetectOnly" );

			Console.Error.Write( "                   (input is left unchanged), " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "E" );
			Console.ResetColor( );
			Console.Error.WriteLine( " = EndOfInput (input is chopped" );

			Console.Error.Write( "                   at first \"forbidden\" character), " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "R" );
			Console.ResetColor( );
			Console.Error.WriteLine( " = Remove (remove" );

			Console.Error.Write( "                   \"forbidden\" characters) or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( " = WhiteSpace (replace" );

			Console.Error.WriteLine( "                   \"forbidden\" characters with spaces)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /M:mask" );
			Console.ResetColor( );
			Console.Error.Write( "   specifies an optional " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "mask" );
			Console.ResetColor( );
			Console.Error.WriteLine( " that will be applied to the input" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "HIDEINPUT /M" );
			Console.ResetColor( );
			Console.Error.WriteLine( " without a mask will show help for the \"mask language\"." );

			Console.Error.WriteLine( "         Return code (\"errorlevel\") is 0 if valid input is received, 1 in case" );

			Console.Error.WriteLine( "         of (command line) errors, 2 if input is empty or whitespace only, 3 if" );

			Console.Error.WriteLine( "         \"forbidden\" characters were detected by the filter." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Display Help Text


			return 1;
		}


		static void WriteTableRow( string col1text, string col2text, int col1percentage, bool col1bold = true, bool col2bold = false )
		{
			// Wrap text to fit in 2 columns
			oddrow = !oddrow;
			int windowwidth = Console.WindowWidth;
			int col1width = Convert.ToInt32( windowwidth * col1percentage / 100 );
			int col2width = windowwidth - col1width - 5; // Column separator = 4, subtract 1 extra to prevent automatic line wrap
			List<string> col1lines = new List<string>( );
			List<string> col2lines = new List<string>( );
			// Column 1
			if ( col1text.Length > col1width )
			{
				Regex regex = new Regex( @".{1," + col1width + @"}(?=\s|$)" );
				if ( regex.IsMatch( col1text ) )
				{
					MatchCollection matches = regex.Matches( col1text );
					foreach ( Match match in matches )
					{
						col1lines.Add( match.ToString( ).Trim( ) );
					}
				}
				else
				{
					while ( col1text.Length > 0 )
					{
						col1lines.Add( col1text.Trim( ).Substring( 0, Math.Min( col1width, col1text.Length ) ) );
						col1text = col1text.Substring( Math.Min( col1width, col1text.Length ) ).Trim( );
					}
				}
			}
			else
			{
				col1lines.Add( col1text.Trim( ) );
			}
			// Column 2
			if ( col2text.Length > col2width )
			{
				Regex regex = new Regex( @".{1," + col2width + @"}(?=\s|$)" );
				if ( regex.IsMatch( col2text ) )
				{
					MatchCollection matches = regex.Matches( col2text );
					foreach ( Match match in matches )
					{
						col2lines.Add( match.ToString( ).Trim( ) );
					}
				}
				else
				{
					while ( col2text.Length > 0 )
					{
						col2lines.Add( col2text.Trim( ).Substring( 0, Math.Min( col2width, col2text.Length ) ) );
						col2text = col2text.Substring( Math.Min( col2width, col2text.Length ) ).Trim( );
					}
				}
			}
			else
			{
				col2lines.Add( col2text.Trim( ) );
			}
			for ( int i = 0; i < Math.Max( col1lines.Count, col2lines.Count ); i++ )
			{
				if ( oddrow )
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				if ( col1bold || oddrow )
				{
					Console.ForegroundColor = bold;
				}
				Console.Write( "{0,-" + col1width + "}    ", ( i < col1lines.Count ? col1lines[i] : String.Empty ) );
				Console.ResetColor( );
				if ( oddrow )
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				if ( col2bold || oddrow )
				{
					Console.ForegroundColor = bold;
				}
				Console.WriteLine( "{0,-" + col2width + "}", ( i < col2lines.Count ? col2lines[i] : String.Empty ) );
				Console.ResetColor( );
			}
		}
	}


	public enum FilterAction
	{
		Abort,
		DetectOnly,
		EndOfInput,
		Remove,
		WhiteSpace
	}
}
