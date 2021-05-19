using System;


namespace RobvanderWoude
{
	class Chr
	{
		static string progver = "1.01";


		static int Main( string[] args )
		{
			if ( args.Length != 1 )
			{
				return ShowHelp( );
			}
			try
			{
				if ( Convert.ToInt32( args[0] ).ToString( ) != args[0] )
				{
					return ShowHelp( );
				}
				if ( Convert.ToInt32( args[0] ) > 255 || Convert.ToInt32( args[0] ) < 0 )
				{
					return ShowHelp( );
				}
				Console.Write( "{0}", Convert.ToChar( Convert.ToByte( Convert.ToInt32( args[0] ) ) ) );
			}
			catch ( Exception )
			{
				return ShowHelp( );
			}
			return 0;
		}


		public static int ShowHelp( )
		{
			/*
			Chr,  Version 1.01
			Return the ASCII character for the specified decimal character code

			Usage:  CHR  charcode

			Where:  charcode  is a decimal number in the range 0..255

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Chr,  Version {0}", progver );
			Console.Error.WriteLine( "Return the ASCII character for the specified decimal character code" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CHR  charcode" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "charcode" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  is a decimal number in the range 0..255" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}
