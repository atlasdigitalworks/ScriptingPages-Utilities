using System;
using System.IO;
using System.Windows.Forms;

namespace RobvanderWoude
{
	class Paste
	{
		[STAThread]
		static int Main( string[] args )
		{
			string file = string.Empty;
			bool overwrite = false;
			StreamWriter stream;

			#region CommandLine

			if ( args.Length > 0 )
			{
				if ( args[0][0] == '/' )
				{
					return WriteError( string.Empty, 3 );
				}
				else
				{
					file = args[0];
				}
			}
			if ( args.Length == 2 )
			{
				if ( args[1].ToUpper( ).Trim( ) == "/O" )
				{
					overwrite = true;
				}
				else
				{
					return WriteError( "Invalid command line switch", 3 );
				}
			}
			if ( args.Length > 2 )
			{
				return WriteError( "Too many command line arguments", 3 );
			}

			#endregion CommandLine

			try
			{
				try
				{
					string clipText;
					if ( Clipboard.ContainsText( ) )
					{
						clipText = Clipboard.GetText( );
					}
					else
					{
						return 1;
					}
					if ( string.IsNullOrWhiteSpace( file ) )
					{
						Console.Write( clipText );
					}
					else
					{
						if ( !File.Exists( file ) || overwrite )
						{
							stream = new StreamWriter( file );
						}
						else
						{
							stream = File.AppendText( file );
						}
						stream.Write( clipText );
						stream.Close( );
					}
					return 0;
				}
				catch ( IOException e )
				{
					return WriteError( e.Message, 2 );
				}
			}
			catch ( Exception e )
			{
				Console.Error.WriteLine( e.Message, 3 );
				return 2;
			}
		}

		public static int WriteError( string errorMessage, int rc )
		{
			/*
			Paste.exe,  Version 2.00
			Read text from the clipboard and write to a file or the screen

			Usage:  PASTE  [ textfile  [ /O ] ]

			Where:  textfile   is the optional file to write the text from the clipboard to
			                   (default: write to Standard Output, i.e. the screen)
			        /O         tells the program to overwrite textfile if it exists
			                   (default: append to existing file, create file if it doesn't exist)

			Note:   The program returns the following 'errorlevels':
			        0    success
			        1    no text available in clipboard
			        2    file I/O error
			        3    command line or unknown error

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			if ( !string.IsNullOrWhiteSpace( errorMessage ) )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "{0}ERROR: ", ( rc == 2 ? "I/O " : String.Empty ) );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Paste.exe,  Version 2.00" );
			Console.Error.WriteLine( "Read text from the clipboard and write to a file or the screen" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Usage:  PASTE  [ textfile  [ /O ] ]" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Where:  textfile   is the optional file to write the text from the clipboard to" );
			Console.Error.WriteLine( "                   (default: write to Standard Output, i.e. the screen)" );
			Console.Error.WriteLine( "        /O         tells the program to overwrite textfile if it exists" );
			Console.Error.WriteLine( "                   (default: append to existing file, create file if it doesn't exist)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Note:   The program returns the following 'errorlevels':" );
			Console.Error.WriteLine( "        0    success" );
			Console.Error.WriteLine( "        1    no text available in clipboard" );
			Console.Error.WriteLine( "        2    file I/O error" );
			Console.Error.WriteLine( "        3    command line or unknown error" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return rc;
		}
	}
}
