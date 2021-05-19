using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RobvanderWoude
{
	class Tail
	{
		static int Main( string[] args )
		{
			try
			{
				// Default number of lines is 1
				int numlines = 1;
				
				#region Command Line Parsing

				string filename = string.Empty;
				bool redirected;
				bool set_l = false;
				bool set_input = false;

				foreach ( string arg in args )
				{
					if ( arg[0] == '/' )
					{
						if ( arg.Length > 3 )
						{
							if ( arg.ToUpper( )[1] == 'L' )
							{
								if ( arg[2] != ':' )
								{
									return WriteError( "Invalid argument: " + arg );
								}
								try
								{
									if ( set_l )
									{
										return WriteError( "Duplicate /L argument" );
									}
									else
									{
										numlines = Convert.ToInt32( arg.Substring( 3 ) );
										if ( numlines < 1 )
										{
											return WriteError( "Number of lines must be 1 or greater" );
										}
										set_l = true;
									}
								}
								catch ( FormatException )
								{
									return WriteError( "Invalid number of lines: " + arg );
								}
							}
						}
						else
						{
							if ( arg == "/?" )
							{
								return WriteError( );
							}
							else
							{
								return WriteError( "Invalid argument: " + arg );
							}
						}
					}
					else
					{
						if ( set_input )
						{
							return WriteError( "Duplicate file argument" );
						}
						else
						{
							if ( File.Exists( arg ) )
							{
								filename = arg;
							}
							else
							{
								return WriteError( "Invalid filename: " + arg );
							}
							set_input = true;
						}
					}
				}

				if ( ConsoleEx.InputRedirected )
				{
					if ( set_input )
					{
						return WriteError( "Use either file name or redirection, not both" );
					}
					else
					{
						set_input = true;
						redirected = true;
					}
				}
				else
				{
					if ( args.Length == 0 )
					{
						return WriteError( );
					}
					redirected = false;
				}

				#endregion Command Line Parsing


				int index = 0;
				string[] output = new string[numlines];

				if ( redirected )
				{
					// Read standard input and store the lines in a list
					while ( Console.In.Peek( ) > -1 )
					{
						output.SetValue( Console.In.ReadLine( ), index );
						index = ( index + 1 ) % numlines;
					}
				}
				else
				{
					// Read the file and store the lines in an array
					Encoding enc = GetEncoding( filename );
					using ( FileStream fsi = File.Open( filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
					using ( BufferedStream bsi = new BufferedStream( fsi ) )
					using ( StreamReader sri = new StreamReader( bsi, enc ) )
					{
						while ( sri.Peek( ) > -1 )
						{
							output.SetValue( sri.ReadLine( ), index );
							index = ( index + 1 ) % numlines;
						}
					}
				}

				// Display the lines in the correct order
				for ( int i = index; i < output.Length; i++ )
				{
					Console.WriteLine( output[i] );
				}
				if ( index > 0 )
				{
					for ( int i = 0; i < index; i++ )
					{
						Console.WriteLine( output[i] );
					}
				}
				return 0;
			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}
		}


		#region Redirection Detection

		// Code to detect redirection by Hans Passant on StackOverflow.com/a/3453272
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


		#region File Encoding Detection

		// Code by Jason Pierce on http://stackoverflow.com/a/19283954
		/// <summary>
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to the system default when detection of the text file's endianness fails.
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding( string filename )
		{
			// Read the BOM
			var bom = new byte[4];
			using ( var file = new FileStream( filename, FileMode.Open ) ) file.Read( bom, 0, 4 );
			// Analyze the BOM
			if ( bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 ) return Encoding.UTF7;
			if ( bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf ) return Encoding.UTF8;
			if ( bom[0] == 0xff && bom[1] == 0xfe ) return Encoding.Unicode; //UTF-16LE
			if ( bom[0] == 0xfe && bom[1] == 0xff ) return Encoding.BigEndianUnicode; //UTF-16BE
			if ( bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff ) return Encoding.UTF32;
			return Encoding.Default;
		}

		#endregion File Encoding Detection


		#region Error Handling

		public static int WriteError( Exception e = null )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
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
			Tail,  Version 1.01
			Return the specified number of lines from the end of the specified file

			Usage:   TAIL  filename  [ option ]
			   or:   TAIL  [ option ]  <  filename

			Where:   filename   is the file to be read
			         /L:n       read the last n Lines (default: 1 line)

			Examples:
			TAIL  filename                 read the last line (default)
			TAIL  filename  /L:5           read the last 5 lines
			TAIL  /L:5  <  filename        read the last 5 lines

			Check for redirection by Hans Passant on StackOverflow.com/a/3453272
			Check file encoding by Jason Pierce on StackOverflow.com/a/19283954

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Tail,  Version 1.01" );
			Console.Error.WriteLine( "Return the specified number of lines from the end of the specified file" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "TAIL  filename  [ option ]" );
			Console.ResetColor( );
			Console.Error.Write( "   or:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "TAIL  [ option ]  <  filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "filename" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the file to be read" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /L:n" );
			Console.ResetColor( );
			Console.Error.Write( "       read the last " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "n L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ines (default: 1 line)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Examples:" );
			Console.Error.WriteLine( "TAIL  filename                 read the last line (default)" );
			Console.Error.WriteLine( "TAIL  filename  /L:5           read the last 5 lines" );
			Console.Error.WriteLine( "TAIL  /L:5  <  filename        read the last 5 lines" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Check for redirection by Hans Passant on " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "StackOverflow.com/a/3453272" );
			Console.ResetColor( );
			Console.Error.Write( "Check file encoding by Jason Pierce on " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "StackOverflow.com/a/19283954" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}

		#endregion Error Handling
	}
}
