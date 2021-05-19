using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Win32;


namespace RobvanderWoude
{
	class ListProgs
	{
		static string progver = "1.03";

		static SortedList<string, string[]> reglist = new SortedList<string, string[]>( );
		static string pattern = String.Empty;

		static int Main( string[] args )
		{
			#region Initialize Variables

			string display = "List";
			bool separators = false;
			int rc = 0;
			int switchcount = 0;

			#endregion Initialize Variables

			#region Command Line parsing

			if ( args.Length > 2 )
			{
				return ErrorMessage( "Too many command line arguments" );
			}

			foreach ( string arg in args )
			{
				switch ( arg.ToUpper( ) )
				{
					case "/?":
						return ErrorMessage( );
					case "/A":
						if ( switchcount > 0 )
						{
							return ErrorMessage( "Invalid command line argument(s)" );
						}
						display = "Aligned";
						switchcount += 1;
						break;
					case "/S":
						if ( switchcount > 0 )
						{
							return ErrorMessage( "Invalid command line argument(s)" );
						}
						display = "Aligned";
						separators = true;
						switchcount += 1;
						break;
					case "/T":
						if ( switchcount > 0 )
						{
							return ErrorMessage( "Invalid command line argument(s)" );
						}
						display = "TabDelimited";
						switchcount += 1;
						break;
					default:
						if ( String.IsNullOrEmpty( pattern ) && VerifyRegExPattern( arg ) )
						{
							pattern = arg;
						}
						else
						{
							return ErrorMessage( "Invalid command line argument \"{0}\"", arg );
						}
						break;
				}
			}

			if ( args.Length - switchcount > 1 )
			{
				return ErrorMessage( "Invalid command line argument(s)" );
			}

			#endregion Command Line parsing

			#region Read Registry

			// Check 32-bit software on 32-bit OS or 64-bit software on 64-bit OS
			bool listreg = ListReg( "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall" );
			// Check 32-bit software on 64-bit OS
			if ( Is64bitOS( ) )
			{
				bool listreg2 = ListReg( "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" );
				listreg = listreg || listreg2;
			}
			if ( !listreg )
			{
				return ErrorMessage( "Error reading the registry.\n\tMake sure you have sufficient permissions to read the registry." );
			}

			#endregion Read Registry

			#region Show Results

			switch ( display )
			{
				case "Aligned":
					rc = ShowTable( separators );
					break;
				case "TabDelimited":
					rc = ShowTabDelimited( );
					break;
				default:
					rc = ShowList( );
					break;
			}

			#endregion Show Results

			return rc;
		}

		#region Subroutines

		static int ErrorMessage( params string[] errmsg )
		{
			/*
			ListProgs.exe,  Version 1.03
			List "all" installed program names and versions found in the registry

			Usage:  LISTPROGS  [ "pattern" ]  [ /A | /S | /T ]

			Where:  "pattern"  limits output to DisplayName values matching the regular
							   expression "pattern"                  (case insensitive)
					/A         shows results in Aligned table           (default: List)
					/S         aligned table with Separator lines       (default: List)
					/T         shows results Tab delimited              (default: List)

			Notes:  Searches HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall and
					HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall
					for subkeys that have both DisplayName and DisplayVersion set.
					Registry keys are displayed in the default List output, but not in
					Aligned output nor in Tab delimited output.
					With Aligned output (/A) a window width of 110 columns or wider
					is recommended - may be set with the command:  MODE CON COLS=110

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

			Console.Error.WriteLine( "ListProgs.exe,  Version {0}", progver );

			Console.Error.WriteLine( "List \"all\" installed program names and versions found in the registry" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ListProgs  [ \"pattern\" ]  [ /A | /S | /T ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"pattern\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  limits output to DisplayName values matching the regular" );

			Console.Error.Write( "                   expression " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "\"pattern\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "                  (case insensitive)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /A" );
			Console.ResetColor( );
			Console.Error.Write( "         shows results in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ligned table           (default: List)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /S" );
			Console.ResetColor( );
			Console.Error.Write( "         aligned table with " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "eparator lines       (default: List)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /T" );
			Console.ResetColor( );
			Console.Error.Write( "         shows results " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ab delimited              (default: List)" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( @"Notes:  Searches HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall and" );

			Console.Error.WriteLine( @"        HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall" );

			Console.Error.WriteLine( "        for subkeys that have both DisplayName and DisplayVersion set." );

			Console.Error.WriteLine( "        Registry keys are displayed in the default List output, but not in" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        A" );
			Console.ResetColor( );
			Console.Error.Write( "ligned output nor in " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ab delimited output." );

			Console.Error.Write( "        With " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.Write( "ligned output (" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/A" );
			Console.ResetColor( );
			Console.Error.WriteLine( ") a window width of 110 columns or wider" );

			Console.Error.Write( "        is recommended - may be set with the command:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "MODE CON COLS=110" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}

		static bool Is64bitOS( )
		{
			UInt16 addresswidth = 0;
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "root\\CIMV2", "SELECT * FROM Win32_Processor" );
			foreach ( ManagementObject queryObj in searcher.Get( ) )
			{
				addresswidth = (UInt16) ( queryObj["AddressWidth"] );
			}
			return ( addresswidth == 64 );
		}

		static bool ListReg( string regpath )
		{
			try
			{
				Regex regex = new Regex( pattern, RegexOptions.IgnoreCase );
				RegistryKey regkey = Registry.LocalMachine.OpenSubKey( regpath );
				foreach ( var regsubkey in regkey.GetSubKeyNames( ) )
				{
					RegistryKey subkey = regkey.OpenSubKey( regsubkey );
					try
					{
						string displayname = subkey.GetValue( "DisplayName" ).ToString( );
						if ( !String.IsNullOrEmpty( displayname ) )
						{
							if ( String.IsNullOrEmpty( pattern ) || regex.IsMatch( displayname ) )
							{
								string displayversion = subkey.GetValue( "DisplayVersion" ).ToString( );
								if ( !String.IsNullOrEmpty( displayversion ) )
								{
									// Chop DisplayVersion string at first occurence of linebreak or null character
									if ( displayversion.IndexOfAny( "\0\n\r".ToCharArray( ) ) > -1 )
									{
										displayversion = displayversion.Split( "\0\n\r".ToCharArray( ) )[0];
									}
									// Remove all but the version number
									string trimpattern = @"(?:\b|\s|^)(?:v\.?)?(\d+(?:\.\d+)+[a-z]?)(?:\b|\s|$)";
									Regex trimregex = new Regex( trimpattern, RegexOptions.IgnoreCase );
									if ( trimregex.IsMatch( displayversion ) )
									{
										displayversion = trimregex.Match( displayversion ).ToString( );
									}
									// Add the entry to the list, if it wasn't added before
									string[] progval = new string[] { displayversion, subkey.ToString( ) };
									if ( !reglist.ContainsKey( displayname ) )
									{
										reglist.Add( displayname, progval );
									}
								}
							}
						}
					}
					catch ( Exception )
					{
						// ignore
					}
					subkey.Close( );
				}
				regkey.Close( );
				return true;
			}
			catch ( Exception )
			{
				return false;
			}

		}

		static int ShowList( )
		{
			foreach ( KeyValuePair<string, string[]> prog in reglist )
			{
				Console.WriteLine( "Registry Key    = {0}", prog.Value[1] );
				Console.WriteLine( "Display Name    = {0}", prog.Key );
				Console.WriteLine( "Display Version = {0}", prog.Value[0] );
				Console.WriteLine( );
			}
			return 0;
		}

		static int ShowTabDelimited( )
		{
			foreach ( KeyValuePair<string, string[]> prog in reglist )
			{
				Console.WriteLine( "{0}\t{1}", prog.Key, prog.Value[0] );
			}
			return 0;
		}

		static int ShowTable( bool separators = false )
		{
			string separator;
			int maxnamelen = 0;
			int maxverlen = 0;
			int totalwidth = Console.WindowWidth;

			if ( separators )
			{
				separator = new String( '-', totalwidth - 1 );
			}
			else
			{
				separator = String.Empty;
			}

			foreach ( KeyValuePair<string, string[]> prog in reglist )
			{
				maxnamelen = Math.Max( maxnamelen, prog.Key.Length );
				maxverlen = Math.Max( maxverlen, prog.Value[0].Length );
			}
			int col2width = maxverlen + 2;
			int col1width = totalwidth - col2width - 3;
			foreach ( KeyValuePair<string, string[]> prog in reglist )
			{
				Console.Write( separator );
				string key = prog.Key;
				if ( key.Length > col1width )
				{
					key = key.Substring( 0, col1width - 2 ) + "...";
				}
				Console.WriteLine( " {0,-" + col1width + "}  {1," + col2width + "}", key, prog.Value[0] );
			}
			Console.Write( separator );
			return 0;
		}

		static bool VerifyRegExPattern( string testpattern )
		{
			// Test validity of RegEx pattern
			// Based on http://stackoverflow.com/questions/218680/can-i-test-if-a-regex-is-valid-in-c-sharp-without-throwing-exception
			if ( String.IsNullOrWhiteSpace( testpattern ) )
			{
				return false;
			}
			try
			{
				Regex.Match( "", testpattern );
				return true;
			}
			catch ( ArgumentException )
			{
				return false;
			}
		}

		#endregion Subroutines
	}
}
