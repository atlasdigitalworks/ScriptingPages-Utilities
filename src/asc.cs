using System;

namespace RobvanderWoude
{
	class Asc
	{
		static int Main( string[] args )
		{
			if ( args.Length != 1 )
			{
				return WriteError( string.Empty );
			}
			if ( args[0].Length != 1 )
			{
				return WriteError( string.Empty );
			}
			try
			{
				int result = Convert.ToInt32( Convert.ToByte( Convert.ToChar( args[0].Substring( 0, 1 ) ) ) );
				Console.WriteLine( "{0}", result );
				return result;
			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}
		}

		// Code to display help and optional error message, by Bas van der Woude
		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			/*
			Asc,  Version 1.00
			Return the decimal character code for the specified ASCII character

			Usage:  ASC  character

			Where:  character  is an ASCII character

			Note:   The result will be displayed on screen and returned as exit
			        code ("errorlevel"); in case of errors, exit code will be 0.

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
			Console.Error.WriteLine( "Asc,  Version 1.00" );
			Console.Error.WriteLine( "Return the decimal character code for the specified ASCII character" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ASC  character" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "character" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  is an ASCII character" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Note:   The result will be displayed on screen and returned as exit" );
			Console.Error.WriteLine( "        code (\"errorlevel\"); in case of errors, exit code will be 0." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 0;
		}
	}
}
