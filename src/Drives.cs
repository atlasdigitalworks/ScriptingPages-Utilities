using System;
using System.Collections.Generic;
using System.IO;

namespace RobvanderWoude
{
	class Drives
	{
		static int Main( string[] args )
		{
			bool showused = true;
			bool showavailable = true;
			bool showtype = false;
			bool showNotready = true;

			#region Command line parsing

			// Only 1 optional argument allowed
			if ( args.Length > 1 )
			{
				return WriteError( "Too many command line arguments" );
			}
			if ( args.Length == 1 )
			{
				// We'll display a 'friendly' message if help was requested
				if ( args[0].StartsWith( "/" ) || args[0].StartsWith( "-" ) )
				{
					switch ( args[0].ToUpper( ) )
					{
						case "/A":
						case "-A":
							showused = false;
							break;
						case "/T":
						case "-T":
							showtype = true;
							showavailable = false;
							break;
						case "/R":
						case "-R":
							showtype = true;
							showavailable = false;
							showNotready = false;
							break;
						case "/U":
						case "-U":
							showavailable = false;
							break;
						case "/?":
						case "-?":
						case "/H":
						case "-H":
						case "--H":
						case "/HELP":
						case "-HELP":
						case "--HELP":
							return WriteError( string.Empty );
						default:
							return WriteError( "Invalid command line argument" );
					}
				}
				else
				{
					return WriteError( string.Empty );
				}
			}

			#endregion

			// Based on code found at
			// http://www.dreamincode.net/code/snippet4795.htm


			if ( showavailable )
			{
				List<string> letters = new List<string>( );

				// Get all avilable drive letters
				for ( int i = Convert.ToInt16( 'A' ); i < Convert.ToInt16( 'Z' ); i++ )
				{
					letters.Add( new string( new char[] { (char) i } ) );
				}

				// Loop through each and remove it's drive letter from our list
				foreach ( DriveInfo drive in DriveInfo.GetDrives( ) )
				{
					letters.Remove( drive.Name.Substring( 0, 1 ).ToUpper( ) );
				}

				// display the list
				if ( showused )
				{
					Console.Write( "Available : " );
				}
				foreach ( string letter in letters )
				{
					Console.Write( "{0}: ", letter );
				}
				Console.WriteLine( );
			}

			if ( showused )
			{
				if ( showavailable )
				{
					Console.Write( "Used      : " );
				}
				foreach ( DriveInfo drive in DriveInfo.GetDrives( ) )
				{
					if ( showtype )
					{
						bool isready = drive.IsReady;
						if ( showNotready )
						{
							Console.WriteLine( "{0}\t{1,-12}\t{2}", drive.Name.Substring( 0, 2 ).ToUpper( ), drive.DriveType, ( isready ? drive.DriveFormat : "-- not ready --" ) );
						}
						else
						{
							if ( isready )
							{
								Console.WriteLine( "{0}\t{1,-12}\t{2}", drive.Name.Substring( 0, 2 ).ToUpper( ), drive.DriveType, drive.DriveFormat );
							}
						}
					}
					else
					{
						Console.Write( "{0} ", drive.Name.Substring( 0, 2 ).ToUpper( ) );
					}
				}
				Console.WriteLine( );
			}

			return 0;
		}

		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			/*
			Drives,  Version 2.00
			List available and/or used drive letters

			Usage:  DRIVES  [ /A | /R | /T | /U ]

			Where:  /A      lists available drive letters only (default: all)
			        /R      skip drives that are not ready     (implies /T)
			        /T      display drive type and filesystem  (implies /U)
			        /U      lists used drive letters only      (default: all)

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			string fullpath = Environment.GetCommandLineArgs( ).GetValue( 0 ).ToString( );
			string[] program = fullpath.Split( '\\' );
			string exename = program[program.GetUpperBound( 0 )];
			exename = exename.Substring( 0, exename.IndexOf( '.' ) );

			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR:  " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}
			Console.Error.WriteLine( );
			Console.Error.WriteLine( exename + ",  Version 2.00" );
			Console.Error.WriteLine( "List available and/or used drive letters" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( exename.ToUpper( ) );
			Console.ResetColor( );
			Console.Error.Write( "  [ " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/A" );
			Console.ResetColor( );
			Console.Error.Write( " | " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/R" );
			Console.ResetColor( );
			Console.Error.Write( " | " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.Write( " | " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/U" );
			Console.ResetColor( );
			Console.Error.WriteLine( " ]" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/A" );
			Console.ResetColor( );
			Console.Error.Write( "      lists " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "a" );
			Console.ResetColor( );
			Console.Error.WriteLine( "vailable drive letters only (default: all)" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /R" );
			Console.ResetColor( );
			Console.Error.Write( "      skip drives that are not " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "r" );
			Console.ResetColor( );
			Console.Error.Write( "eady     (implies " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/T" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /T" );
			Console.ResetColor( );
			Console.Error.Write( "      display drive " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "t" );
			Console.ResetColor( );
			Console.Error.Write( "ype and filesystem  (implies " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/U" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /U" );
			Console.ResetColor( );
			Console.Error.Write( "      lists " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "u" );
			Console.ResetColor( );
			Console.Error.WriteLine( "sed drive letters only      (default: all)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}
