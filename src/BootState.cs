using System;
using System.Management;
using Microsoft.Win32;


namespace RobvanderWoude
{
	class BootState
	{
		static string progver = "1.00";


		static int Main( string[] args )
		{
			if ( args.Length > 0 )
			{
				return ShowHelp( );
			}

			// Check for Windows PE first
			foreach ( string regkey in Registry.LocalMachine.OpenSubKey( @"SYSTEM\ControlSet001\Control" ).GetSubKeyNames( ) )
			{
				if ( regkey.ToUpper( ) == "MININT" )
				{
					Console.Write( "Windows PE" );
					return 3;
				}
			}

			// Check "regular" boot modes
			string[] bootupstates = new string[] { "Normal boot", "Fail-safe boot", "Fail-safe with network boot" };
			ManagementObjectSearcher searcher = new ManagementObjectSearcher( "SELECT BootupState FROM Win32_ComputerSystem" );
			foreach ( ManagementObject item in searcher.Get( ) )
			{
				string bootupstate = item["BootupState"].ToString( );
				Console.Write( bootupstate.Replace( "Fail-safe", "Safe mode" ).Replace( " boot", "" ) );
				for ( int i = 0; i < bootupstates.Length; i++ )
				{
					if ( bootupstate.ToUpper( ) == bootupstates[i].ToUpper( ) )
					{
						return i;
					}
				}
			}

			// Return error if boot mode not determined
			Console.Write( "Unknown" );
			return -1;
		}


		static int ShowHelp( )
		{
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "BootState.exe,  Version {0}", progver );
			Console.Error.WriteLine( "Show Windows' boot state" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Usage:    BootState.exe" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Notes:    Boot state is returned both as a string and as return code:" );
			Console.Error.WriteLine( "              \"Normal\"                    (rc = 0)" );
			Console.Error.WriteLine( "              \"Safe mode\"                 (rc = 1)" );
			Console.Error.WriteLine( "              \"Safe mode with network\"    (rc = 2)" );
			Console.Error.WriteLine( "              \"Windows PE\"                (rc = 3)" );
			Console.Error.WriteLine( "          In case of (command line) errors, the return code will be -1." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Credits:  Windows PE detection based on a tip by Mitch Tulloch" );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "          http://techgenix.com/HowtodetectwhetheryouareinWindowsPE/" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return -1;
		}
	}
}
