    using System;

    using System.Collections.Generic;

    using System.Diagnostics;

    using System.Globalization;

     

     

    namespace RobvanderWoude

    {

    	class Kolor

    	{

    		static string progver = "1.03";

     

     

    		static int Main( string[] args )

    		{

    			#region Initialize Variables

     

    			bool bgonly = false;

    			bool fgonly = false;

    			bool checkupdate = false;

    			bool colorsset = false;

    			bool enforce = false;

    			bool test = false;

    			int bgcolor = (int) Console.BackgroundColor;

    			int fgcolor = (int) Console.ForegroundColor;

    			int rc = 0;

    			int switchcount = 0;

     

    			#endregion Initialize Variables

     

    			#region Command Line Parsing

     

    			if ( args.Length > 2 )

    			{

    				return ShowHelp( );

    			}

    			/*

    						if ( args.Length == 0 )

    						{

    							Console.WriteLine( "{0:X}{1:X}", (int) Console.BackgroundColor, (int) Console.ForegroundColor );

    							return 0;

    						}

    			*/

    			if ( args.Length > 0 )

    			{

    				foreach ( string arg in args )

    				{

    					if ( arg[0] == '/' )

    					{

    						switchcount += 1;

    						if ( arg.Length == 2 )

    						{

    							switch ( arg.ToUpper( ) )

    							{

    								case "/?":

    									return ShowHelp( );

    								case "/B":

    									if ( bgonly )

    									{

    										return ShowHelp( "Duplicate command line switch /B" );

    									}

    									bgonly = true;

    									break;

    								case "/F":

    									if ( fgonly )

    									{

    										return ShowHelp( "Duplicate command line switch /F" );

    									}

    									fgonly = true;

    									break;

    								case "/R":

    									// ignored, for backwards compatibility only

    									break;

    								case "/T":

    									if ( test )

    									{

    										return ShowHelp( "Duplicate command line switch /T" );

    									}

    									test = true;

    									break;

    								case "/U":

    									if ( checkupdate )

    									{

    										return ShowHelp( "Duplicate command line switch /U" );

    									}

    									checkupdate = true;

    									break;

    								case "/Y":

    									if ( enforce )

    									{

    										return ShowHelp( "Duplicate command line switch /Y" );

    									}

    									enforce = true;

    									break;

    								default:

    									return ShowHelp( "Invalid command line switch {0}", arg.ToUpper( ) );

    							}

    						}

    						else

    						{

    							return ShowHelp( "Invalid command line switch {0}", arg.ToUpper( ) );

    						}

    					}

    					else

    					{

    						try

    						{

    							bgcolor = int.Parse( args[0], NumberStyles.HexNumber ) / 16; // first hex digit

    							fgcolor = int.Parse( args[0], NumberStyles.HexNumber ) % 16; // second hex digit

    							colorsset = true;

    						}

    						catch ( Exception e )

    						{

    							return ShowHelp( e.Message );

    						}

    					}

    				}

    				if ( args.Length > 1 )

    				{

    					if ( !colorsset || ( colorsset && !enforce ) )

    					{

    						return ShowHelp( "Invalid combination of command line arguments" );

    					}

    				}

    			}

     

    			#endregion Command Line Parsing

     

    			if ( colorsset )

    			{

    				if ( bgcolor == fgcolor )

    				{

    					rc = 1;

    					if ( !enforce )

    					{

    						return rc; // ignore equal background and foreground color, unless /Y was used

    					}

    				}

    				else

    				{

    					rc = 0;

    				}

    				Console.BackgroundColor = (ConsoleColor) bgcolor;

    				Console.ForegroundColor = (ConsoleColor) fgcolor;

    			}

    			else if ( bgonly )

    			{

    				rc = (int) Console.BackgroundColor;

    				Console.WriteLine( "{0:X}", (int) Console.BackgroundColor );

    			}

    			else if ( fgonly )

    			{

    				rc = (int) Console.ForegroundColor;

    				Console.WriteLine( "{0:X}", (int) Console.ForegroundColor );

    			}

    			else if ( checkupdate )

    			{

    				CheckUpdate( );

    				rc = 0;

    			}

    			else if ( test )

    			{

    				TestColors( );

    				rc = 1;

    			}

    			else

    			{

    				rc = 16 * (int) Console.BackgroundColor + (int) Console.ForegroundColor;

    				Console.WriteLine( "{0:X}{1:X}", (int) Console.BackgroundColor, (int) Console.ForegroundColor );

    			}

    			return rc;

    		}

     

     

    		public static void CheckUpdate( )

    		{

    			string updatecheckurl = String.Format( "http://www.robvanderwoude.com/getlatestver.php?progfile=Kolor.exe&version={0}", progver );

    			Process.Start( updatecheckurl );

    		}

     

     

    		public static void TestColors( )

    		{

    			int width = Console.WindowWidth * 2;

    			string text;

    			for ( int i = 0; i < 16; i++ )

    			{

    				Console.BackgroundColor = (ConsoleColor) i;

    				Console.ForegroundColor = ConsoleColor.White;

    				text = String.Format( "{0,3}  {1,-21}  {0,3}  {1,-21}  {0,3}  {1,-21}", i, ( (ConsoleColor) i ).ToString( ) );

    				if ( i > 9 )

    				{

    					Console.ForegroundColor = ConsoleColor.Black;

    				}

    				Console.Write( "{0,-" + width + "}", text );

    				Console.ResetColor( );

    			}

    		}

     

     

    		public static int ShowHelp( params string[] errmsg )

    		{

    			#region Help Text

    			/*

    			Kolor.exe,  Version 1.03

    			Set or get console background and foreground colors

     

    			Usage:  Kolor.exe  xy [ /Y ]

     

    			   or:  Kolor.exe  [ /B | /F | /T | /U ]

     

    			Where:  x    is a hexadecimal digit for the new background color

    			        y    is a hexadecimal digit for the new foreground color

    			        /B   returns the current Background color on screen and as return code

    			        /F   returns the current Foreground color on screen and as return code

    			        /T   Test: show console colors and their associated numbers

    			        /U   check for program Updates

    			        /Y   enforce new colors even if background and foreground colors are

    			             equal (default: when equal, change is ignored, return code 1)

     

    			Notes:  Unlike CMD's internal COLOR command, which changes the colors for the

    			        entire screen, KOLOR will only change the colors of the text displayed

    			        after the command is issued.

    			        Use KOLOR /T to see the available colors and their numbers.

    			        Kolor.exe without parameters returns current background and foreground

    			        colors as 16 * background + foreground, on screen and as return code.

    			        When setting colors, return code is 0, or 1 on errors.

    			        When reading colors, return code is requested color(s), or 0 on errors.

    			        Returned colors are shown in hexadecimal, for decimal use return code.

     

    			Written by Rob van der Woude

    			http://www.robvanderwoude.com

    			*/

     

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

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "Kolor.exe,  Version {0}", progver );

     

    			Console.Error.WriteLine( "Set or get console background and foreground colors" );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.Write( "Usage:  " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.WriteLine( "Kolor.exe  xy [ /Y ]" );

    			Console.ResetColor( );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.Write( "   or:  " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.WriteLine( "Kolor.exe  [ /B | /F | /T | /U ]" );

    			Console.ResetColor( );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.Write( "Where:  " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "x" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "    is a hexadecimal digit for the new background color" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        y" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "    is a hexadecimal digit for the new foreground color" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        /B" );

    			Console.ResetColor( );

    			Console.Error.Write( "   returns the current " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "B" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "ackground color on screen and as return code" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        /F" );

    			Console.ResetColor( );

    			Console.Error.Write( "   returns the current " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "F" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "oreground color on screen and as return code" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        /T   T" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "est: show console colors and their associated numbers" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        /U" );

    			Console.ResetColor( );

    			Console.Error.Write( "   check for program " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "U" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "pdates" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        /Y" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "   enforce new colors even if background and foreground colors are" );

     

    			Console.Error.WriteLine( "             equal (default: when equal, change is ignored, return code 1)" );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "Notes:  Unlike CMD's internal COLOR command, which changes the colors for the" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        entire screen" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( ", KOLOR will only change the colors of the text displayed" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "        after" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( " the command is issued." );

     

    			Console.Error.Write( "        Use " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "KOLOR /T" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( " to see the available colors and their numbers." );

     

    			Console.Error.WriteLine( "        Kolor.exe without parameters returns current background and foreground" );

     

    			Console.Error.Write( "        colors as " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "16 * background + foreground" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( ", on screen and as return code." );

     

    			Console.Error.Write( "        When " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "setting" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( " colors, return code is 0, or 1 on error." );

     

    			Console.Error.Write( "        When " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "reading" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( " colors, return code is requested color(s), or 0 on error." );

     

    			Console.Error.WriteLine( "        Returned colors are shown in hexadecimal, for decimal use return code." );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "Written by Rob van der Woude" );

     

    			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

     

    			#endregion Help Text

     

    			return 1;

    		}

    	}

    }

     