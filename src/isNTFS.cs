using System;
using System.IO;

namespace RobvanderWoude
{
	class IsNTFS
	{
		public static int Main( string[] args )
		{
			try
			{
				if ( args.Length == 0 )
				{
					return WriteError( string.Empty );
				}
				if ( args.Length > 1 )
				{
					return WriteError( "Invalid number of arguments." );
				}

				string drive = args[0].ToUpper( );

				DriveInfo[] allDrives = DriveInfo.GetDrives( );

				foreach ( DriveInfo drv in allDrives )
				{
					if ( drive == drv.Name.Substring( 0, 2 ) )
					{
						if ( drv.IsReady )
						{
							Console.WriteLine( drv.DriveFormat.ToUpper( ) );
							if ( drv.DriveFormat == "NTFS" )
							{
								return 0;
							}
							else
							{
								return 2;
							}
						}
						else
						{
							Console.WriteLine( drv.DriveType.ToString( ).ToUpper( ) );
							return 1;
						}
					}
				}
				return WriteError( "Invalid drive specification." );
			}
			catch ( Exception e )
			{
				// Display help text with error message
				return WriteError( e );
			}
		}
		// Code to display help and optional error message, by Bas van der Woude
		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			string fullpath = Environment.GetCommandLineArgs( ).GetValue( 0 ).ToString( );
			string[] program = fullpath.Split( '\\' );
			string exeName = program[program.GetUpperBound( 0 )];
			exeName = exeName.Substring( 0, exeName.IndexOf( '.' ) );

			/*
			IsNTFS,  Version 1.00
			Return 'errorlevel' 0 if the specified drive is NTFS formated

			Usage:    ISNTFS  drive:
			
			Note:     Returns 'errorlevel' 0 if NTFS, 2 if not, 1 if not ready or invalid

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/
			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR: " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "IsNTFS,  Version 1.00" );
			Console.Error.WriteLine( "Return 'errorlevel' 0 if the specified drive is NTFS formated" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "{0}  drive:", exeName.ToUpper( ) );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Note:     Returns 0 if NTFS, 2 if not, 1 if not ready or invalid." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}