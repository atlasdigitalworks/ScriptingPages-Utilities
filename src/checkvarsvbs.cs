using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class CheckVarsVBS
	{
		static readonly string progver = "1.05";


		static List<string> csserrors = new List<string>( );


		static int Main( string[] args )
		{
			#region Initialize Variables

			int rc = 0;
			SortedList<string, int> subroutines = new SortedList<string, int>( );
			SortedList<string, int> variables = new SortedList<string, int>( );
			bool htawindowevents = false;
			bool showsubs = true;
			bool showvars = true;
			bool unusedonly = false;
			string scriptcode = String.Empty;
			string scriptext = String.Empty;
			string scriptfile = String.Empty;
			int columnwidth = 12;
			int unusedsubs = 0;
			int unusedvars = 0;
			List<string> ignoredsubs = new List<string>( )
			{
				"window_onbeforeunload",
				"window_onblur",
				"window_onfocus",
				"window_onhelp",
				"window_onload",
				"window_onresize",
				"window_onunload"
			};

			#endregion Initialize Variables


			#region Command Line Parsing

			if ( args.Length == 0 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' )
				{
					if ( arg.ToUpper( ) == "/?" )
					{
						return ShowHelp( );
					}
					else if ( arg.ToUpper( ) == "/S" )
					{
						if ( !showvars )
						{
							return ShowHelp( "Duplicate command line switch /S" );
						}
						if ( !showsubs )
						{
							return ShowHelp( "Use /S or /V or neither, but not both" );
						}
						showvars = false;
					}
					else if ( arg.ToUpper( ) == "/U" )
					{
						if ( unusedonly )
						{
							return ShowHelp( "Duplicate command line switch /U" );
						}
						unusedonly = true;
					}
					else if ( arg.ToUpper( ) == "/V" )
					{
						if ( !showsubs )
						{
							return ShowHelp( "Duplicate command line switch /V" );
						}
						if ( !showvars )
						{
							return ShowHelp( "Use /S or /V or neither, but not both" );
						}
						showsubs = false;
					}
					else if ( arg.ToUpper( ) == "/W" )
					{
						if ( htawindowevents )
						{
							return ShowHelp( "Duplicate command line switch /W" );
						}
						htawindowevents = true;
					}
					else
					{
						return ShowHelp( "Invalid command line switch \"{0}\"", arg );
					}
				}
				else
				{
					if ( !String.IsNullOrWhiteSpace( scriptfile ) )
					{
						return ShowHelp( "Duplicate command line argument for VBScript file" );
					}
					if ( !File.Exists( arg ) )
					{
						return ShowHelp( "Invalid file name or file not found: \"{0}\"", arg );
					}
					scriptext = Path.GetExtension( arg ).ToLower( );
					if ( scriptext != ".hta" && scriptext != ".vbs" )
					{
						return ShowHelp( "Invalid file type \"{0}\"", arg );
					}
					scriptfile = Path.GetFullPath( arg );
				}

			}

			if ( String.IsNullOrWhiteSpace( scriptfile ) )
			{
				return ShowHelp( "Please specify a source file" );
			}

			#endregion Command Line Parsing


			#region Read File

			// Read the code from the file
			scriptcode = File.ReadAllText( scriptfile, Encoding.UTF8 );
			// Remove comment lines from the code (does NOT strip comments starting halfway on a line)
			string pattern = @"(^|\n|\r)[ \t]*'[^\n\r]+";
			Regex regex = new Regex( pattern );
			scriptcode = regex.Replace( scriptcode, String.Empty );

			#endregion Read File


			#region List Subroutines

			// Create a list of subroutines found in the code
			if ( showsubs )
			{
				pattern = @"(?:^|\n|\r)[ \t]*(?:Sub|Function)[ \t]+([A-Z][^\s\(]+)";
				regex = new Regex( pattern, RegexOptions.IgnoreCase );
				if ( regex.IsMatch( scriptcode ) )
				{
					MatchCollection matches = regex.Matches( scriptcode );
					if ( matches.Count > 0 )
					{
						foreach ( Match match in matches )
						{
							bool listed = false;
							string sub = match.Groups[1].Value;
							foreach ( string key in subroutines.Keys )
							{
								if ( sub.ToLower( ) == key.ToLower( ) )
								{
									listed = true;
								}
							}
							if ( !listed )
							{
								if ( sub.ToLower( ).StartsWith( "window_on" ) )
								{
									subroutines[sub] = 1;
								}
								else
								{
									subroutines[sub] = 0;
								}
								if ( sub.Length > columnwidth )
								{
									columnwidth = sub.Length;
								}
							}
						}
					}
				}
			}

			#endregion List Subroutines


			#region Check Subroutine Nesting

			if ( showsubs )
			{
				pattern = @"(?:^|\n|\r)[ \t]*End[ \t]+(?:Sub|Function)('|\s|$)";
				regex = new Regex( pattern, RegexOptions.IgnoreCase );
				if ( regex.IsMatch( scriptcode ) )
				{
					MatchCollection matches = regex.Matches( scriptcode );
					if ( matches.Count != subroutines.Count )
					{
						RedLine( "{0} Sub and Function statements found, against {1} End Sub and End Function statements\n", subroutines.Count, matches.Count );

					}
					pattern = @"(?:^|\r|\n)\s*(Sub|Function)\s.*?(?:^|\n|\r)[ \t]*End[ \t]+\1('|\s|$)";
					regex = new Regex( pattern, RegexOptions.Singleline );
					if ( regex.IsMatch( scriptcode ) )
					{
						matches = regex.Matches( scriptcode );
						foreach ( Match match in matches )
						{
							string subroutine = match.Groups[0].ToString( );
							List<string> subs = new List<string>( );
							int startsubs = 0;
							int endsubs = 0;
							pattern = @"(?:^|\r|\n)\s*(?:Sub|Function)\s(\w+)";
							regex = new Regex( pattern );
							if ( regex.IsMatch( subroutine ) )
							{
								MatchCollection submatches = regex.Matches( subroutine );
								startsubs = submatches.Count;
								foreach ( Match sub in submatches )
								{
									subs.Add( sub.Value.Trim( "\n\r\t ".ToCharArray( ) ) );
								}
							}
							pattern = @"[\n\r]+\s*End[ \t]+(Sub|Function)(?:\s|$)";
							regex = new Regex( pattern );
							if ( regex.IsMatch( subroutine ) )
							{
								MatchCollection submatches = regex.Matches( subroutine );
								endsubs = submatches.Count;
							}
							if ( startsubs > 1 || endsubs > 1 )
							{
								RedLine( "Possibly nested or improperly terminated functions and/or subroutines:" );
								Console.WriteLine( "\t{0}\n", String.Join( "\n\t", subs.ToArray( ) ) );
							}
						}
					}
				}
			}

			#endregion Check Subroutine Nesting


			#region List Variables

			// Create a list of variables found in the code
			if ( showvars )
			{
				pattern = @"(?:^|\n|\r)[ \t]*Dim[ \t]+([A-Z][^\n\r:]+)";
				regex = new Regex( pattern, RegexOptions.IgnoreCase );
				if ( regex.IsMatch( scriptcode ) )
				{
					MatchCollection matches = regex.Matches( scriptcode );
					if ( matches.Count > 0 )
					{
						foreach ( Match match in matches )
						{
							string[] vars = match.Groups[1].ToString( ).Split( ", ()".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );
							foreach ( string var in vars )
							{
								bool listed = false;
								foreach ( string key in subroutines.Keys )
								{
									if ( var.ToLower( ) == key.ToLower( ) )
									{
										listed = true;
									}
								}
								if ( !listed )
								{
									variables[var] = 0;
									if ( var.Length > columnwidth )
									{
										columnwidth = var.Length;
									}
								}
							}
						}
					}
				}
			}

			#endregion List Variables


			#region Count and Display Subroutines Usage

			// Iterate through the list of subroutines and count the occurrences of its name
			if ( showsubs )
			{
				List<string> keys = new List<string>( subroutines.Keys );
				foreach ( string sub in keys )
				{
					bool ignorethissub = !htawindowevents && scriptext == ".hta" && ignoredsubs.Contains( sub.ToLower( ) );
					if ( !ignorethissub )
					{
						pattern = String.Format( @"\b{0}\b", sub );
						regex = new Regex( pattern, RegexOptions.IgnoreCase );
						if ( regex.IsMatch( scriptcode ) )
						{
							if ( !sub.ToLower( ).StartsWith( "window_on" ) )
							{
								subroutines[sub] += regex.Matches( scriptcode ).Count - 1;
								if ( subroutines[sub] == 0 )
								{
									unusedsubs += 1;
								}
							}
						}
					}
				}
				// Show the results
				if ( unusedonly )
				{
					Console.WriteLine( "{0} Unused Subroutine{1}{2}", unusedsubs, ( unusedsubs == 1 ? String.Empty : "s" ), ( unusedsubs == 0 ? String.Empty : ":" ) );
					Console.WriteLine( "{0}=================={1}{2}", new String( '=', unusedsubs.ToString( ).Length ), ( unusedsubs == 1 ? String.Empty : "=" ), ( unusedsubs == 0 ? String.Empty : "=" ) );

				}
				else
				{
					Console.WriteLine( "{0,-" + columnwidth + "}    Occurrences:", "Subroutine:" );
					Console.WriteLine( "{0,-" + columnwidth + "}    ============", "===========" );
				}
				foreach ( string key in subroutines.Keys )
				{
					bool ignorethissub = !htawindowevents && scriptext == ".hta" && ignoredsubs.Contains( key.ToLower( ) );
					if ( subroutines[key] == 0 )
					{
						if ( unusedonly )
						{
							if ( !ignorethissub )
							{
								Console.WriteLine( key );
							}
						}
						else
						{
							if ( ignorethissub )
							{
								Console.WriteLine( "{0,-" + columnwidth + "}    {1}", key, subroutines[key] );
							}
							else
							{
								RedLine( string.Format( "{0,-" + columnwidth + "}    {1}", key, subroutines[key] ) );
							}
						}
						rc += 1;
					}
					else if ( !unusedonly )
					{
						Console.WriteLine( "{0,-" + columnwidth + "}    {1}", key, subroutines[key] );
					}
				}
				Console.WriteLine( );
			}

			#endregion Count and Display Subroutines Usage


			#region Count and Display Variables Usage

			// Iterate through the list of variables and count the occurrences of its name
			if ( showvars )
			{
				List<string> keys = new List<string>( variables.Keys );
				foreach ( string variable in keys )
				{
					pattern = String.Format( @"\b{0}\b", variable );
					regex = new Regex( pattern, RegexOptions.IgnoreCase );
					if ( regex.IsMatch( scriptcode ) )
					{
						variables[variable] = regex.Matches( scriptcode ).Count - 1;
						if ( variables[variable] == 0 )
						{
							unusedvars += 1;
						}
					}
				}
				// Show the results
				if ( unusedonly )
				{
					Console.WriteLine( "{0} Unused Variable{1}{2}", unusedvars, ( unusedvars == 1 ? String.Empty : "s" ), ( unusedvars == 0 ? String.Empty : ":" ) );
					Console.WriteLine( "{0}================{1}{2}", new String( '=', unusedvars.ToString( ).Length ), ( unusedvars == 1 ? String.Empty : "=" ), ( unusedvars == 0 ? String.Empty : "=" ) );
				}
				else
				{
					Console.WriteLine( "{0,-" + columnwidth + "}    Occurrences:", "Variable:" );
					Console.WriteLine( "{0,-" + columnwidth + "}    ============", "=========" );
				}
				foreach ( string key in variables.Keys )
				{
					if ( variables[key] == 0 )
					{
						if ( unusedonly )
						{
							Console.WriteLine( key );
						}
						else
						{
							RedLine( String.Format( "{0,-" + columnwidth + "}    {1}", key, variables[key] ) );
						}
						rc += 1;
					}
					else if ( !unusedonly )
					{
						Console.WriteLine( "{0,-" + columnwidth + "}    {1}", key, variables[key] );
					}
				}
				Console.WriteLine( );
			}

			#endregion Count and Display Variables Usage


			#region Check HTA Head

			if ( showsubs && showvars )
			{
				if ( Path.GetExtension( scriptfile ).ToLower( ) == ".hta" )
				{
					UnderLine( "HTA Head:" );
					int htaerrors = 0;
					pattern = @"<style[^>]*>((?:.|\n|\r)*?)</style>";
					regex = new Regex( pattern, RegexOptions.IgnoreCase );
					if ( regex.IsMatch( scriptcode ) )
					{
						MatchCollection matches = regex.Matches( scriptcode );
						if ( matches.Count > 0 )
						{
							foreach ( Match match in matches )
							{
								foreach ( Group submatch in match.Groups )
								{
									htaerrors += CheckStyles( submatch.ToString( ) );
								}
							}
						}
					}
					switch ( htaerrors )
					{
						case 0:
							Console.WriteLine( "No CSS errors found" );
							break;
						case 1:
							RedLine( "\n1 possible CSS error found" );
							break;
						default:
							RedLine( "\n{0} possible CSS errors found", htaerrors );
							break;
					}
				}
			}

			#endregion Check HTA Head


			#region Check HTA Event Handlers Case

			if ( showsubs && showvars )
			{
				if ( Path.GetExtension( scriptfile ).ToLower( ) == ".hta" )
				{
					UnderLine( "\nHTA Event Handlers:" );

					List<string> eventnames = Enum.GetNames( typeof( Events ) ).Cast<string>( ).ToList<string>( );
					List<string> warnings = new List<string>( );
					Regex regexci = new Regex( @"\bwindow_onload\b", RegexOptions.IgnoreCase );
					bool caseissue = false;
					foreach ( Match match in regexci.Matches( scriptcode ) )
					{
						Regex regexcs = new Regex( @"\bwindow_onload\b", RegexOptions.None );
						if ( !caseissue && regexci.IsMatch( scriptcode ) && !regexcs.IsMatch( match.Value ) )
						{
							warnings.Add( match.Value );
							caseissue = true;
						}
					}
					regexci = new Regex( @"\bwindow_onunload\b", RegexOptions.IgnoreCase );
					caseissue = false;
					foreach ( Match match in regexci.Matches( scriptcode ) )
					{
						Regex regexcs = new Regex( @"\bwindow_onunload\b", RegexOptions.None );
						if ( !caseissue && regexci.IsMatch( scriptcode ) && !regexcs.IsMatch( match.Value ) )
						{
							warnings.Add( match.Value );
							caseissue = true;
						}
					}
					foreach ( string eventname in eventnames )
					{
						string eventhandler = CheckEvent( eventname, scriptcode );
						if ( !String.IsNullOrEmpty( eventhandler ) )
						{
							warnings.Add( eventname );
						}
					}
					warnings.Sort( );
					switch ( warnings.Count )
					{
						case 0:
							Console.WriteLine( "No case mismatches for event handlers found" );
							break;
						case 1:
							RedLine( "\n1 possible case mismatch for event handler \"{0}\"", warnings[0] );
							break;
						default:
							foreach ( string warning in warnings )
							{
								Console.WriteLine( "Possible case mismatch for event handler \"{0}\"", warning );
							}
							RedLine( "\n{0} possible case mismatches for event handlers found", warnings.Count );
							break;
					}
				}
			}

			#endregion Check HTA Event Handlers Case


			return rc;
		}


		static string CheckEvent( string eventname, string code )
		{
			string pattern = string.Format( @"\s({0})=", eventname );
			Regex regexci = new Regex( pattern, RegexOptions.IgnoreCase );
			if ( regexci.IsMatch( code ) )
			{
				MatchCollection matches = regexci.Matches( code );
				Regex regexcs = new Regex( pattern, RegexOptions.None );
				foreach ( Match match in matches )
				{
					if( match.Groups[1].Value != eventname )
					{
						return match.Groups[1].Value;
					}
				}

			}
			return null;
		}


		static int CheckStyle( string styledef )
		{
			int errors = 0;
			string pattern = @"^([^@\{]+)\{([^\}]+)\}";
			Regex regex = new Regex( pattern );
			if ( regex.IsMatch( styledef ) )
			{
				string tagdef = regex.Match( styledef ).Groups[1].ToString( ).Trim( "\n\r\t ".ToCharArray( ) );
				string tagstyle = regex.Match( styledef ).Groups[2].ToString( ).Trim( "\n\r\t ".ToCharArray( ) );
				string[] tagcssall = tagstyle.Split( ';' );
				for ( int i = 0; i < tagcssall.Length; i++ )
				{
					tagcssall[i] = tagcssall[i].Trim( "\n\r\t ".ToCharArray( ) );
				}
				string pattern1 = @"^([\w-]+)\s*:\s*([^\n\r\;\{\}]+)$";
				Regex regex1 = new Regex( pattern1 );
				string pattern2 = @"^([\w-]+)\s*:";
				Regex regex2 = new Regex( pattern2 );
				foreach ( string line in tagcssall )
				{
					if ( String.IsNullOrWhiteSpace( line ) )
					{
						// No action required
					}
					else if ( regex1.IsMatch( line ) )
					{
						MatchCollection matches1 = regex1.Matches( line );
						string csskey = matches1[0].Groups[1].ToString( );
						string cssval = matches1[0].Groups[2].ToString( );
						string pattern3 = @"^[^\(]*\)|\([^\)]*$";
						regex = new Regex( pattern3 );
						if ( regex.IsMatch( cssval ) )
						{
							if ( !csserrors.Contains( csskey ) )
							{
								csserrors.Add( csskey );
								RedLine( "Possible CSS error for {0}:", csskey );
								Console.WriteLine( "\t{0}\n", cssval );
								errors += 1;
							}
						}
					}
					else if ( regex2.IsMatch( line ) )
					{
						MatchCollection matches2 = regex2.Matches( line );
						string csskey = matches2[0].Groups[1].ToString( );
						if ( !csserrors.Contains( csskey ) )
						{
							csserrors.Add( csskey );
							RedLine( "Possible CSS error(s) for {0}:", csskey );
							errors += 1;
						}
					}
					else
					{
						string csserror = styledef.Substring( 0, styledef.IndexOfAny( "{\n\r".ToCharArray( ) ) ).Trim( "{\n\r\t ".ToCharArray( ) );
						if ( !csserrors.Contains( csserror ) )
						{
							RedLine( "Possible CSS error(s):" );
							Console.WriteLine( "{0}\n", styledef );
							errors += 1;
						}
					}
				}
			}
			return errors;
		}


		static int CheckStyles( string stylesheet )
		{
			int errors = 0;
			string pattern = @"\/\*(.|\n|\r)*?\*\/";
			Regex regex = new Regex( pattern );
			if ( regex.IsMatch( stylesheet ) )
			{
				stylesheet = regex.Replace( stylesheet, string.Empty );
			}
			pattern = @"[^\n\r\{\}]+\s*\{[^\}]+\}";
			regex = new Regex( pattern, RegexOptions.IgnoreCase );
			if ( regex.IsMatch( stylesheet ) )
			{
				MatchCollection matches = regex.Matches( stylesheet );
				foreach ( Match match in matches )
				{
					errors += CheckStyle( match.ToString( ) );
				}
			}
			return errors;
		}


		static void RedLine( string line, params object[] rlargs )
		{
			Console.ForegroundColor = ConsoleColor.Red;
			if ( rlargs.Length > 0 )
			{
				Console.WriteLine( line, rlargs );
			}
			else
			{
				Console.WriteLine( line );
			}
			Console.ResetColor( );
		}


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
			CheckVarsVBS.exe,  Version 1.05
			Check VBScript code for unused variables and subroutines
 
			Usage:    CheckVarsVBS.exe  "vbsfile"  [ /S | /V ]  [ /U ]  [ /W ]
 
			Where:    "vbsfile"         is the VBScript or HTA file to be examined
			          /S                tests Subroutines only
			                            (default: subroutines as well as variables)
			          /U                list Unused subroutines and variables only
			                            (default: list all subroutines and variables)
			          /V                tests Variables only
			                            (default: subroutines as well as variables)
			          /W                include Window_On* subroutines for HTAs
			                            (default: ignore Window_On* subroutines in HTAs)
 
			Notes:    When checking subroutines, the program will also check for
			          improperly terminated and nested subroutines.
			          For HTAs only, the following special subroutines are ignored
			          (not listed in red, or not at all with /U switch) by default:
			          Window_OnBeforeUnload, Window_OnBlur, Window_OnFocus,
			          Window_OnHelp, Window_OnLoad, Window_OnResize, Window_OnUnload;
			          use /W to treat them as ordinary subroutines.
			          For HTAs only, unless checking for variables only (/V switch),
			          this program will search the HTA's head for CSS style definitions,
			          and check those for some common typos.
			          The program's return code equals the number of unused subroutines
			          and/or variables, or -1 in case of (command line) errors.
 
			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "CheckVarsVBS.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Check VBScript code for unused variables and subroutines" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CheckVarsVBS.exe  \"vbsfile\"  [ /S | /V ]  [ /U ]  [ /W ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"vbsfile\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "         is the VBScript or HTA file to be examined" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /S" );
			Console.ResetColor( );
			Console.Error.Write( "                tests " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ubroutines only" );

			Console.Error.WriteLine( "                            (default: subroutines as well as variables)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /U" );
			Console.ResetColor( );
			Console.Error.Write( "                list " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.WriteLine( "nused subroutines and variables only" );

			Console.Error.WriteLine( "                            (default: list all subroutines and variables)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /V" );
			Console.ResetColor( );
			Console.Error.Write( "                tests " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "V" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ariables only" );

			Console.Error.WriteLine( "                            (default: subroutines as well as variables)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /W" );
			Console.ResetColor( );
			Console.Error.Write( "                include " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "indow_On* subroutines for HTAs" );

			Console.Error.WriteLine( "                            (default: ignore Window_On* subroutines in HTAs)" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    When checking subroutines, the program will also check for" );

			Console.Error.WriteLine( "          improperly terminated and nested subroutines." );

			Console.Error.WriteLine( "          For HTAs only, the following special subroutines are ignored" );

			Console.Error.Write( "          (not listed in red, or not at all with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/U" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch) by default:" );

			Console.Error.WriteLine( "          Window_OnBeforeUnload, Window_OnBlur, Window_OnFocus," );

			Console.Error.WriteLine( "          Window_OnHelp, Window_OnLoad, Window_OnResize, Window_OnUnload;" );

			Console.Error.Write( "          use " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.WriteLine( " to treat them as ordinary subroutines." );

			Console.Error.Write( "          For HTAs only, unless checking for variables only (" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/V" );
			Console.ResetColor( );
			Console.Error.WriteLine( " switch)," );

			Console.Error.WriteLine( "          this program will search the HTA's head for CSS style definitions," );

			Console.Error.WriteLine( "          and check those for some common typos." );

			Console.Error.WriteLine( "          The program's return code equals the number of unused subroutines" );

			Console.Error.WriteLine( "          and/or variables, or -1 in case of (command line) errors." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Display Help Text


			return -1;
		}


		static void UnderLine( string text )
		{
			Console.WriteLine( text );
			Console.WriteLine( new string( '=', text.Replace( "\n", "" ).Replace( "\r", "" ).Length ) );
		}
	}


	public enum Events
	{
		onabort,
		onafterprint,
		onbeforeprint,
		onbeforeunload,
		onblur,
		oncanplay,
		oncanplaythrough,
		onchange,
		onclick,
		oncontextmenu,
		oncopy,
		oncut,
		ondblclick,
		ondrag,
		ondragend,
		ondragenter,
		ondragleave,
		ondragover,
		ondragstart,
		ondrop,
		ondurationchange,
		onemptied,
		onended,
		onerror,
		onfocus,
		onfocusin,
		onfocusout,
		onhashchange,
		oninput,
		oninvalid,
		onkeydown,
		onkeypress,
		onkeyup,
		onload,
		onloadeddata,
		onloadedmetadata,
		onloadstart,
		onmessage,
		onmousedown,
		onmouseenter,
		onmouseleave,
		onmousemove,
		onmouseout,
		onmouseover,
		onmouseup,
		onmousewheel,
		onoffline,
		ononline,
		onopen,
		onpagehide,
		onpageshow,
		onpaste,
		onpause,
		onplay,
		onplaying,
		onpopstate,
		onprogress,
		onratechange,
		onreset,
		onresize,
		onscroll,
		onsearch,
		onseeked,
		onseeking,
		onselect,
		onshow,
		onstalled,
		onstorage,
		onsubmit,
		onsuspend,
		ontimeupdate,
		ontoggle,
		onunload,
		onvolumechange,
		onwaiting,
		onwheel
	}
}
