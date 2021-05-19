using System;
using System.Collections.Generic;
using System.IO;


namespace RobvanderWoude
{
	class Tee
	{
		static string progver = "1.03";


		static int Main( string[] args )
		{
			#region Command Line Parsing

			string filename = String.Empty;

			if ( args.Length == 1 || Console.IsInputRedirected )
			{
				if ( args[0].IndexOf( "?" ) > -1 )
				{
					return ShowHelp( );
				}
				filename = Path.GetFullPath( args[0] );
				if ( !Directory.Exists( Path.GetDirectoryName( filename ) ) )
				{
					return ShowHelp( "Invalid parent folder for output file" );
				}
				if ( Directory.Exists( filename ) )
				{
					return ShowHelp( "Please specify an output file name" );
				}
			}
			else
			{
				return ShowHelp( );
			}

			#endregion

			try
			{
				int inputn;
				string inputc;

				StreamWriter file = new StreamWriter( filename, true );

				do
				{
					inputn = Console.In.Read( );
					if ( inputn != -1 )
					{
						inputc = Convert.ToChar( Convert.ToByte( inputn ) ).ToString( );
						Console.Write( inputc );
						file.Write( inputc );
					}
				} while ( inputn != -1 );

				file.Close( );

				return 0;
			}
			catch ( Exception e )
			{
				return ShowHelp( e.Message );
			}
		}


		#region Error Handling

		public static int ShowHelp( params string[] errmsg )
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
			Tee,  Version 1.03
			Tee port for Windows: redirect Standard Input to Standard Output AND to a file

			Usage:  somecommand  |  TEE.EXE  filename

			Where:  somecommand     command whose output is piped into TEE's Standard Input
			        filename        the file that TEE's Standard Input will be appended to

			Note:   This program requires .NET Framework 4.5; use an older version
			        of this program if you don't have .NET Framework 4.5 installed.

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Tee,  Version {0}", progver );

			Console.Error.WriteLine( "Tee port for Windows: redirect Standard Input to Standard Output AND to a file" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "somecommand  |  TEE.EXE  filename" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "somecommand" );
			Console.ResetColor( );
			Console.Error.WriteLine( "     command whose output is piped into TEE's Standard Input" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( "        the file that TEE's Standard Input will be appended to" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Note:   This program requires .NET Framework 4.5; use an older version" );

			Console.Error.WriteLine( "        of this program if you don't have .NET Framework 4.5 installed." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help Text

			return 1;
		}

		#endregion
	}
}
