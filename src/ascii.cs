using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RobvanderWoude
{
	class ASCII
	{
		static int Main( string[] args )
		{
			bool isredirected = ConsoleEx.InputRedirected;
			if ( isredirected ^ args.Length == 0 )
			{
				return WriteError( );
			}
			if ( args.Length > 1 )
			{
				return WriteError( "Invalid command line arguments" );
			}

			if ( isredirected )
			{
				Console.Write( Encoding.Unicode.GetString( Encoding.ASCII.GetBytes( Console.In.ReadToEnd( ) ) ) );
			}
			else
			{
				string file = args[0];
				if ( file[0] == '/' )
				{
					return WriteError( ( file == "/?" ? "" : "Invalid command line argument" ) );
				}
				else
				{
					if ( File.Exists( file ) )
					{
						StreamReader sr = new StreamReader( file );
						Console.Write( Encoding.Unicode.GetString( Encoding.ASCII.GetBytes( sr.ReadToEnd( ) ) ) );
						sr.Close( );
					}
					else
					{
						return WriteError( "File not found" );
					}
				}
			}
			return 0;
		}

		#region Redirection Detection

		// Code to detect redirection by Hans Passant on StackOverflow.com
		// http://stackoverflow.com/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected
		public static class ConsoleEx
		{
			public static bool OutputRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stdout ) );
				}
			}

			public static bool InputRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stdin ) );
				}
			}

			public static bool ErrorRedirected
			{
				get
				{
					return FileType.Char != GetFileType( GetStdHandle( StdHandle.Stderr ) );
				}
			}

			// P/Invoke:
			private enum FileType { Unknown, Disk, Char, Pipe };
			private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };

			[DllImport( "kernel32.dll" )]
			private static extern FileType GetFileType( IntPtr hdl );

			[DllImport( "kernel32.dll" )]
			private static extern IntPtr GetStdHandle( StdHandle std );
		}

		#endregion Redirection Detection


		#region Error Handling

		public static int WriteError( Exception e = null )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			Console.OpenStandardError( );
			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR: " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}

			/*
			ASCII,  Version 1.00
			Convert a text file or Standard Input to ASCII and send it to Standard Output

			Usage:   some_command  |  ASCII  >  file
			   or:   some_command  |  ASCII  |  other_command
			   or:   ASCII  "textfile"  >  file
			   or:   ASCII  "textfile"  |  other_command

			Example: Compare the results of the following commands:
			         SUBINACL  /?  |  MORE
			         SUBINACL  /?  |  ASCII  |  MORE

			Check for redirection by Hans Passant on StackOverflow.com
			/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			 */

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "ASCII,  Version 1.00" );
			Console.Error.WriteLine( "Convert a text file or Standard Input to ASCII and send it to Standard Output" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "some_command  |  ASCII  >  file" );
			Console.ResetColor( );
			Console.Error.Write( "or:      " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "some_command  |  ASCII  |  other_command" );
			Console.ResetColor( );
			Console.Error.Write( "or:      " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ASCII  \"textfile\"  >  file" );
			Console.ResetColor( );
			Console.Error.Write( "or:      " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "ASCII  \"textfile\"  |  other_command" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Example: Compare the results of the following commands:" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "         SUBINACL  /?  |  MORE" );
			Console.Error.WriteLine( "         SUBINACL  /?  |  ASCII  |  MORE" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Check for redirection by Hans Passant on " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "StackOverflow.com" );
			Console.Error.WriteLine( "/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.Write( "http://www.robvanderwoude.com" );
			Console.OpenStandardOutput( );
			return 1;
		}

		#endregion Error Handling
	}
}
