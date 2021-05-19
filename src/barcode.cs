using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class Barcode
	{
		static readonly string progver = "1.01";


		// BlanchedAlmond is an invalid color for this program, and is used to signal an error
		static readonly Brush errorbrush = Brushes.BlanchedAlmond;
		static readonly Color errorcolor = Color.BlanchedAlmond;


		static int Main( string[] args )
		{
			#region Set Defaults

			string imgfile = String.Empty;
			string text = String.Empty;
			Color bgcolor = Color.White;
			Brush fgcolor = Brushes.Black;
			RotateFlipType rotation = RotateFlipType.RotateNoneFlipNone;
			ImageFormat format = ImageFormat.Jpeg;
			int degrees;
			int fontsize = 48;

			#endregion Set Defaults


			#region Parse Command Line

			if ( args.Length < 2 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' )
				{
					#region Named Switches

					switch ( arg.ToLower( )[1] )
					{
						case '?':
							return ShowHelp( );
						case 'b':
							if ( bgcolor != Color.White )
							{
								return ShowHelp( "Duplicate command line switch /B" );
							}
							bgcolor = GetColor( arg.Substring( 3 ).ToLower( ) );
							if( bgcolor == errorcolor )
							{
								return ShowHelp( "Invalid background color specified: \"{0}\"", arg );
							}
							break;
						case 'f':
							if ( fgcolor != Brushes.Black )
							{
								return ShowHelp( "Duplicate command line switch /F" );
							}
							fgcolor = GetBrush( arg.Substring( 3 ).ToLower( ) );
							if ( fgcolor == errorbrush )
							{
								return ShowHelp( "Invalid foreground color specified: \"{0}\"", arg );
							}
							break;
						case 'r':
							if ( rotation != RotateFlipType.RotateNoneFlipNone )
							{
								return ShowHelp( "Duplicate command line switch /R" );
							}
							if ( arg.Length < 5 || arg.Length > 7 )
							{
								return ShowHelp( "Invalid rotation specified: \"{0}\"", arg );
							}
							try
							{
								degrees = Convert.ToInt32( arg.Substring( 3 ) );
								if ( degrees % 90 != 0 )
								{
									return ShowHelp( "Invalid rotation specified: \"{0}\"", arg );
								}
								degrees %= 360;
							}
							catch
							{
								return ShowHelp( "Invalid rotation specified: \"{0}\"", arg );
							}
							rotation = (RotateFlipType) ( degrees / 90 );
							break;
						case 's':
							if ( fontsize != 48 )
							{
								return ShowHelp( "Duplicate command line switch /S" );
							}
							try
							{
								fontsize = Convert.ToInt32( arg.Substring( 3 ) );
							}
							catch
							{
								return ShowHelp( "Invalid rotation specified: \"{0}\"", arg );
							}
							break;
						default:
							return ShowHelp( "Invalid command line argument: \"{0}\"", arg );
					}

					#endregion Named Switches
				}
				else // Unnamed switches
				{
					#region Unnamed Switches

					if ( String.IsNullOrWhiteSpace( imgfile ) )
					{
						if ( Directory.Exists( Directory.GetParent( arg ).FullName ) )
						{
							imgfile = Path.GetFullPath( arg );
							string ext = Path.GetExtension( imgfile ).ToLower( );
							switch ( ext )
							{
								case ".bmp":
									format = ImageFormat.Bmp;
									break;
								case ".gif":
									format = ImageFormat.Gif;
									break;
								case ".jpg":
								case ".jpeg":
									format = ImageFormat.Jpeg;
									break;
								case ".png":
									format = ImageFormat.Png;
									break;
								case ".tif":
								case ".tiff":
									format = ImageFormat.Tiff;
									break;
								default:
									return ShowHelp( "Invalid file type: \"{0}\"", imgfile );
							}
						}
						else
						{
							return ShowHelp( "Invalid path for outputfile: \"{0}\"", arg );
						}
					}
					else if ( String.IsNullOrWhiteSpace( text ) )
					{
						text = arg;
					}
					else
					{
						return ShowHelp( );
					}

					#endregion Unnamed Switches
				}
			}

			#endregion Parse Command Line


			#region Check Font EAN-13

			if ( !CheckFont( fontsize ) )
			{
				if ( CheckFont( ) )
				{
					return ShowHelp( "{0} is not a valid font size for the EAN-13 TrueType font.", fontsize.ToString( ) );
				}
				else
				{
					string url = "http://www.fontpalace.com/font-download/EAN-13/";
					string msg = String.Format( "This program uses the EAN-13 TrueType Font, available at\n\n{0}\n\nDo you want to download this font?", url );
					string title = "Download Missing Font";
					if ( MessageBox.Show( msg, title, MessageBoxButtons.YesNo ) == DialogResult.Yes )
					{
						Process browser = new Process
						{
							StartInfo = new ProcessStartInfo( url )
						};
						browser.Start( );
					}
					return ShowHelp( );
				}
			}

			#endregion Check Font EAN-13


			Bitmap bitmap = new Bitmap( 1, 1 );
			Graphics graphics = Graphics.FromImage( bitmap );
			Font font = new Font( "EAN-13", fontsize );
			// Instantiating object of bitmap image again with the correct size for the text and font.
			SizeF stringsize = graphics.MeasureString( text, font );
			bitmap = new Bitmap( bitmap, (int) stringsize.Width, (int) stringsize.Height );
			graphics = Graphics.FromImage( bitmap );
			// Set background color
			graphics.Clear( bgcolor );
			graphics.DrawString( text, font, fgcolor, 0, 0 );
			font.Dispose( );
			graphics.Flush( );
			graphics.Dispose( );
			// Rotate bitmap image
			bitmap.RotateFlip( rotation );
			// Save bitmap image 
			bitmap.Save( imgfile, format );
			int rc = (int) stringsize.Width;
			return rc;
		}


		public static bool CheckFont( int size = 48 )
		{
			// Font test by Jeff Hillman
			// https://stackoverflow.com/a/114003
			using ( Font fonttest = new Font( "EAN-13", size, FontStyle.Regular, GraphicsUnit.Pixel ) )
			{
				return ( fonttest.Name == "EAN-13" );
			}
		}


		public static Brush GetBrush( string colorstring )
		{
			Brush brush;
			switch ( colorstring.ToLower( ) )
			{
				case "black":
					brush = Brushes.Black;
					break;
				case "blue":
					brush = Brushes.Blue;
					break;
				case "brown":
					brush = Brushes.Brown;
					break;
				case "cyan":
					brush = Brushes.Cyan;
					break;
				case "darkblue":
					brush = Brushes.DarkBlue;
					break;
				case "darkcyan":
					brush = Brushes.DarkCyan;
					break;
				case "darkgray":
				case "darkgrey":
					brush = Brushes.DarkGray;
					break;
				case "darkgreen":
					brush = Brushes.DarkGreen;
					break;
				case "darkmagenta":
					brush = Brushes.DarkMagenta;
					break;
				case "darkorange":
					brush = Brushes.DarkOrange;
					break;
				case "dark":
					brush = Brushes.DarkRed;
					break;
				case "gold":
					brush = Brushes.Gold;
					break;
				case "gray":
				case "grey":
					brush = Brushes.Gray;
					break;
				case "green":
					brush = Brushes.Green;
					break;
				case "lightblue":
					brush = Brushes.LightBlue;
					break;
				case "lightcyan":
					brush = Brushes.LightCyan;
					break;
				case "lightgray":
				case "lightgrey":
					brush = Brushes.LightGray;
					break;
				case "lightgreen":
					brush = Brushes.LightGreen;
					break;
				case "lightyellow":
					brush = Brushes.LightYellow;
					break;
				case "magenta":
					brush = Brushes.Magenta;
					break;
				case "orange":
					brush = Brushes.Orange;
					break;
				case "pink":
					brush = Brushes.Pink;
					break;
				case "red":
					brush = Brushes.Red;
					break;
				case "silver":
					brush = Brushes.Silver;
					break;
				case "white":
					brush = Brushes.White;
					break;
				case "yellow":
					brush = Brushes.Yellow;
					break;
				case "yellowgreen":
					brush = Brushes.YellowGreen;
					break;
				default:
					brush = errorbrush;
					break;
			}
			return brush;
		}


		public static Color GetColor( string colorstring )
		{
			try
			{
				return Color.FromName( colorstring );
			}
			catch
			{
				return errorcolor;
			}
		}


		#region Error handling

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
			Barcode.exe,  Version 1.01
			Generate barcode bitmaps using EAN-13 TrueType font
 
			Usage:    BARCODE   outfile   text   [ options ]
 
			Where:    outfile   is the output file path (type: bmp, gif, jpg, png or tif)
			          text      is the text to be converted to barcode

			Options:  /R:deg    Rotate by number of degrees (multiple of 90; default: 0)
			          /S:size   font Size in pt  (default: 48)
			          /B:color  Background color (default: white)
			          /F:color  Foreground color (default: black)
 
			Credits:  Code to convert text to bitmap by RaviRanjanKr:
			          https://www.codeproject.com/Tips/184102/Convert-Text-to-Image
			          Font test by Jeff Hillman:
			          https://stackoverflow.com/a/114003
			          EAN-13 font made available by Fontpalace.com:
			          http://www.fontpalace.com/font-download/EAN-13/

			Notes:    If the required EAN-13 TrueType font is not installed, you will be
			          prompted to download it.
			          Though the font name may suggest that the barcode conforms to the
			          EAN-13 standard, it does not! You have to validate the specified
			          text yourself to make sure it is a valid EAN-13 code.
			          Supported background and foreground colors are: Black, Blue *, Brown,
			          Cyan *, Gold, Gray *, Grey *, Green *, LightYellow, Magenta *,
			          Orange *, Pink, Red *, Silver, White, Yellow and YellowGreen (* means
			          Dark and Light variants are also supported, e.g. DarkBlue, LightGreen).
			          Return code ("errorlevel") equals the output image width in pixels,
			          or -1 in case of errors.
 
			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Barcode.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Generate barcode bitmaps using EAN-13 TrueType font" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "BARCODE   outfile   text   [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "outfile" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the output file path (type: bmp, gif, jpg, png or tif)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          text" );
			Console.ResetColor( );
			Console.Error.WriteLine( "      is the text to be converted to barcode" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options:" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "  /R:deg    R" );
			Console.ResetColor( );
			Console.Error.Write( "otate by number of " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "deg" );
			Console.ResetColor( );
			Console.Error.WriteLine( "rees (multiple of 90; default: 0)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /S:size" );
			Console.ResetColor( );
			Console.Error.Write( "   font " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ize in pt  (default: 48)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /B:color  B" );
			Console.ResetColor( );
			Console.Error.Write( "ackground " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "color" );
			Console.ResetColor( );
			Console.Error.WriteLine( " (default: white)" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /F:color  F" );
			Console.ResetColor( );
			Console.Error.Write( "oreground " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "color" );
			Console.ResetColor( );
			Console.Error.WriteLine( " (default: black)" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits:  Code to convert text to bitmap by RaviRanjanKr:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "          https://www.codeproject.com/Tips/184102/Convert-Text-to-Image" );
			Console.ResetColor( );

			Console.Error.WriteLine( "          Font test by Jeff Hillman:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "          https://stackoverflow.com/a/114003" );
			Console.ResetColor( );

			Console.Error.WriteLine( "          EAN-13 font made available by Fontpalace.com:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "          http://www.fontpalace.com/font-download/EAN-13/" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Notes:    If the required EAN-13 TrueType font is not installed, you will be" );

			Console.Error.WriteLine( "          prompted to download it." );

			Console.Error.WriteLine( "          Though the font name may suggest that the barcode conforms to the" );

			Console.Error.WriteLine( "          EAN-13 standard, it does not! You have to validate the specified" );

			Console.Error.WriteLine( "          text yourself to make sure it is a valid EAN-13 code." );

			Console.Error.WriteLine( "          Supported background and foreground colors are: Black, Blue *, Brown," );

			Console.Error.WriteLine( "          Cyan *, Gold, Gray *, Grey *, Green *, LightYellow, Magenta *," );

			Console.Error.WriteLine( "          Orange *, Pink, Red *, Silver, White, Yellow and YellowGreen (* means" );

			Console.Error.WriteLine( "          Dark and Light variants are also supported, e.g. DarkBlue)." );

			Console.Error.WriteLine( "          Return code (\"errorlevel\") equals the output image width in pixels," );

			Console.Error.WriteLine( "          or -1 in case of errors." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			#endregion Help Text

			return -1;
		}

		#endregion Error handling
	}
}
