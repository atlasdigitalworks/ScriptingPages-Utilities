    using System;

    using System.IO;

     

    namespace RobvandeWoude

    {

    	class NewTempFile

    	{

    		static int Main( string[] args )

    		{

    			if ( args.Length > 0 )

    			{

    				return WriteError( string.Empty );

    			}

     

    			try

    			{

    				Console.WriteLine( Path.GetTempFileName( ) );

    				return 0;

    			}

    			catch ( Exception e )

    			{

    				return WriteError( e.Message );

    			}

    		}

     

    		public static int WriteError( string errorMessage )

    		{

    			/*

    				NewTempFile,  Version 1.00

    				Create a zero-byte temporary file and display its name on screen

     

    				Usage:    NEWTEMPFILE

     

    				Example:  In a batch file, use the following code to "capture" the name:

     

    				          FOR /F "tokens=*" %%A IN ('NEWTEMPFILE.EXE') DO SET TempFile="%%~A"

     

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

    			Console.Error.WriteLine( "NewTempFile,  Version 1.00" );

    			Console.Error.WriteLine( "Create a zero-byte temporary file and display its name on screen" );

    			Console.Error.WriteLine( );

    			Console.Error.Write( "Usage:    " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.WriteLine( "NEWTEMPFILE" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( );

    			Console.Error.WriteLine( "Example:  In a batch file, use the following code to \"capture\" the name:" );

    			Console.Error.WriteLine( );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.WriteLine( "          FOR /F \"tokens=*\" %%A IN ('NEWTEMPFILE.EXE') DO SET TempFile=\"%%~A\"" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( );

    			Console.Error.WriteLine( "Written by Rob van der Woude" );

    			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

    			return 1;

    		}

    	}

    }

     