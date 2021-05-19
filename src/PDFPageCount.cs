using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RobvanderWoude
{
	class PDFPageCount
	{
		static int Main( string[] args )
		{
			#region Get help

			if ( args.Length == 0 )
			{
				ShowHelp( );
				return 0;
			}

			foreach ( string arg in args )
			{
				if ( arg == "/?" || arg == "-?" || arg.ToLower( ) == "--help" )
				{
					ShowHelp( );
					return 0;
				}
			}

			#endregion

			int errors = 0;

			foreach ( string arg in args )
			{
				try
				{
					Regex regexp = new Regex( @"^(.*)\\([^\\]+\.pdf)$", RegexOptions.IgnoreCase );
					if ( regexp.IsMatch( arg ) )
					{
						// Match means the filespec has a valid format (i.e. *.pdf)
						string[] matches = regexp.Split( arg );
						string folder = matches[1];
						string filespec = matches[2];
						if ( Directory.Exists( folder ) )
						{
							// Folder exists, check for matching files
							string[] fileList = Directory.GetFiles( folder, filespec );
							if ( fileList.Length == 0 )
							{
								// No matching files in this folder
								ShowError( "ERROR: No files matching \"{0}\" were found in \"{1}\"", filespec, folder );
								errors += 1;
							}
							else
							{
								// Iterate through list of matching files
								foreach ( string file in fileList )
								{
									int pagecount = PageCount( file );
									if ( pagecount == -1 )
									{
										// Just increase the error count, the PageCount( )
										// procedure already wrote an error message to screen
										errors += 1;
									}
									else
									{
										// No pages means there is a problem with the file
										if ( pagecount == 0 )
										{
											Console.ForegroundColor = ConsoleColor.Red;
											errors += 1;
										}
										// Display the formated result on screen
										Console.WriteLine( "{0,4} {1,-10} {2}", pagecount.ToString( ), ( pagecount == 1 ? "page" : "pages" ), file );
										if ( pagecount == 0 )
										{
											Console.ForegroundColor = ConsoleColor.Gray;
										}
									}
								}
							}
						}
						else
						{
							// Folder doesn't exist
							ShowError( "ERROR: Folder \"{0}\" not found", folder );
							errors += 1;
						}
					}
					else
					{
						// No match for the regular expression means the filespec was invalid
						ShowError( "ERROR: Invalid filespec \"{0}\", please specify PDF files only", arg );
						errors += 1;
					}
				}
				catch ( Exception e )
				{
					// All other errors: display an error message and then continue
					ShowError( "ERROR: {0}", e.Message );
					errors += 1;
				}
			}

			if ( errors != 0 )
			{
				ShowError( "                {0} finished with {1} error{2}", GetExeName( ), errors.ToString( ), ( errors == 1 ? "" : "s" ) );
			}
			return errors;
		}


		static string GetExeName( )
		{
			string exe = Application.ExecutablePath.ToString( );
			Regex regexp = new Regex( @"\\([^\\]+)$" );
			return regexp.Split( exe )[1];
		}


		static int PageCount( string filename )
		{
			//Function for finding the number of pages in a given PDF file, based on
			// http://www.dotnetspider.com/resources/21866-Count-pages-PDF-file.aspx

			Regex regexp = new Regex( @"\.pdf$", RegexOptions.IgnoreCase );
			if ( regexp.IsMatch( filename ) )
			{
				try
				{
					FileStream fs = new FileStream( filename, FileMode.Open, FileAccess.Read );
					StreamReader sr = new StreamReader( fs );
					string pdfText = sr.ReadToEnd( );
					regexp = new Regex( @"/Type\s*/Page[^s]" );
					MatchCollection matches = regexp.Matches( pdfText );
					return matches.Count;
				}
				catch ( Exception e )
				{
					ShowError( "ERROR: {0} ({1})", e.Message, filename );
					return -1;
				}
			}
			else
			{
				ShowError( "ERROR: {0} is not a PDF file", filename );
				return -1;
			}
		}


		static void ShowError( string message, string param1, string param2 = "", string param3 = "" )
		{
			Console.Error.WriteLine( );
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine( message, param1, param2, param3 );
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Error.WriteLine( );
		}


		#region Display help text

		static void ShowHelp( )
		{
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "{0},  Version 1.02", GetExeName( ) );
			Console.Error.WriteLine( "Return the page count for the specified PDF file(s)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Usage:  {0}  filespec  [ filespec  [ filespec  [ ... ] ] ]", GetExeName( ).ToUpper( ) );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Where:  \"filespec\"        is a file specification for the PDF file(s) to" );
			Console.Error.WriteLine( "                          be listed (wildcards * and ? are allowed)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Note:   The program's return code equals the number of errors encountered." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
		}

		#endregion
	}
}
