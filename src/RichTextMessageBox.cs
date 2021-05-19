using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class RichTextMessageBox
	{
		static readonly string progver = "1.03";


		#region Global Default Values

		static readonly int defaultbuttonheight = 25;
		static readonly int defaultbuttonwidth = 100;
		static readonly string defaultfontfamily = "Sans-Serif";
		static readonly float defaultfontsize = 12;
		static readonly FontStyle defaultfontstyle = FontStyle.Regular;
		static readonly Color defaulttextcolor = Color.Black;
		static readonly string defaulttitle = string.Format( "RichTextMessageBox,  Version {0}", progver );
		static readonly int defaultwindowheight = 480;
		static readonly int defaultwindowwidth = 640;
		static readonly int screenheight = Screen.PrimaryScreen.Bounds.Height;
		static readonly int screenwidth = Screen.PrimaryScreen.Bounds.Width;

		#endregion Global Default Values


		#region Global Variables

		static Form rtmbform;
		static RichTextBox rtmbox;
		static string button1text = "OK";
		static string button2text = string.Empty;
		static string button3text = string.Empty;
		static string buttonclickedtext = "Cancel";
		static int buttonclickednumber = -1;
		static int buttoncount = 1;
		static int buttonheight = defaultbuttonheight;
		static int buttonwidth = defaultbuttonwidth;
		static int defaultbutton = -1;
		static string message = string.Empty;
		static string title = string.Empty;
		static double timeout = 0;
		static bool timeoutelapsed = false;
		static System.Timers.Timer timer;
		static int windowheight = defaultwindowheight;
		static int windowwidth = defaultwindowwidth;

		#endregion Global Variables


		[STAThread]
		static int Main( string[] args )
		{
			#region Initial Values

			int windowx = -1;
			int windowy = -1;
			string fontfamily = defaultfontfamily;
			float fontsize = defaultfontsize;
			FontStyle fontstyle = defaultfontstyle;
			bool literal = false;
			bool showhelp = false;
			bool showintaskbar = false;
			Color textcolor = defaulttextcolor;
			bool topmost = false;

			#endregion Initial Values


			#region Parse Command Line

			if ( args.Length == 0 || args.Contains( "/?" ) )
			{
				showhelp = true;
			}

			foreach ( string arg in args )
			{
				if ( arg.Length > 2 && ( arg[0] == '/' || arg[0] == '-' ) )
				{
					if ( arg.IndexOf( ':' ) > 1 )
					{
						string key = arg.ToUpper( ).Substring( 1, arg.IndexOf( ':' ) - 1 );
						string val = arg.Substring( arg.IndexOf( ':' ) + 1 );
						switch ( key )
						{
							case "B1": // /B1:"text for button 1"
								button1text = val;
								break;
							case "B2": // /B2:"text for button 2"
								button2text = val;
								buttoncount = 2;
								break;
							case "B3": // /B3:"text for button 3"
								button3text = val;
								buttoncount = 3;
								break;
							case "BH": // /BH:button_height
								if ( !int.TryParse( val, out buttonheight ) )
								{
									showhelp = true;
								}
								break;
							case "BW": // /BW:button_width
								if ( !int.TryParse( val, out buttonwidth ) )
								{
									showhelp = true;
								}
								break;
							case "C": // /C"text_color"
								try
								{
									textcolor = Color.FromName( val ); // if FromName does not recognize the value of val as color it will return black
								}
								catch ( Exception )
								{
									showhelp = true;
								}
								break;
							case "DB": // /DB:default_button (1 is always valid, 2 or 3 only if there are that many buttons)
								if ( !int.TryParse( val, out defaultbutton ) )
								{
									if ( val.ToUpper( ) == button1text.ToUpper( ) )
									{
										defaultbutton = 1;
									}
									else if ( val.ToUpper( ) == button2text.ToUpper( ) )
									{
										defaultbutton = 2;
									}
									else if ( val.ToUpper( ) == button3text.ToUpper( ) )
									{
										defaultbutton = 3;
									}
									else
									{
										showhelp = true;
									}
								}
								break;
							case "FONT":
								fontfamily = val;
								try
								{
									FontFamily testfont = new FontFamily( fontfamily );
								}
								catch
								{
									showhelp = true;
								}
								break;
							case "FS": // /FS:font_size
								if ( !float.TryParse( val, out fontsize ) )
								{
									showhelp = true;
								}
								break;
							case "T": // /T:timeout_seconds
								if ( !double.TryParse( val, out timeout ) )
								{
									showhelp = true;
								}
								timeout *= 1000; // specified in seconds, timer requires milliseconds
								break;
							case "WH": // /WH:window_height
								if ( !int.TryParse( val, out windowheight ) )
								{
									showhelp = true;
								}
								break;
							case "WW": // /WW:window_width
								if ( !int.TryParse( val, out windowwidth ) )
								{
									showhelp = true;
								}
								break;
							case "X": // /X:X_coordinate of upper left window corner
								if ( !int.TryParse( val, out windowx ) )
								{
									showhelp = true;
								}
								break;
							case "Y": // /Y:Y_coordinate of upper left window corner
								if ( !int.TryParse( val, out windowy ) )
								{
									showhelp = true;
								}
								break;
						}
					}
					else
					{
						switch ( arg.ToUpper( ).Substring( 1 ) )
						{
							case "ALWAYSONTOP":
							case "MODAL":
							case "TOPMOST":
								topmost = true;
								break;
							case "BOLD":
								fontstyle |= FontStyle.Bold;
								break;
							case "ITALIC":
							case "ITALICS":
								fontstyle |= FontStyle.Italic;
								break;
							case "LITERAL":
								literal = true;
								break;
							case "SHOWINTASKBAR":
							case "TASKBAR":
								showintaskbar = true;
								break;
							case "STRIKE":
							case "STRIKEOUT":
								fontstyle |= FontStyle.Strikeout;
								break;
							case "UNDERLINE":
							case "UNDERLINED":
								fontstyle |= FontStyle.Underline;
								break;
							default:
								showhelp = true;
								break;
						}
					}
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( message ) )
					{
						message = arg; // the message to be displayed in the dialog
					}
					else if ( string.IsNullOrWhiteSpace( title ) )
					{
						title = arg; // the title of the dialog window
					}
					else
					{
						showhelp = true;
					}
				}
			}

			#endregion Parse Command Line


			#region Validate Command Line Settings

			if ( !literal && !string.IsNullOrWhiteSpace( message ) )
			{
				message = UnEscapeString( message );
			}

			// Check mandatory button text, and if default button number is valid
			if ( string.IsNullOrWhiteSpace( message ) )
			{
				showhelp = true;
			}

			if ( string.IsNullOrWhiteSpace( title ) )
			{
				title = defaulttitle;
			}

			if ( string.IsNullOrWhiteSpace( button1text ) )
			{
				showhelp = true;
			}

			if ( string.IsNullOrWhiteSpace( button2text ) && defaultbutton > 1 )
			{
				showhelp = true;
			}

			if ( string.IsNullOrWhiteSpace( button3text ) && defaultbutton > 2 )
			{
				showhelp = true;
			}

			if ( showhelp )
			{
				ShowHelp( );
				// Restore defaults before showing help in GUI
				button1text = "OK";
				button2text = string.Empty;
				button3text = string.Empty;
				buttonheight = defaultbuttonheight;
				buttonwidth = defaultbuttonwidth;
				fontfamily = "Courier New";
				fontsize = 10; // slightly smaller font to fit in the help text
				fontstyle = defaultfontstyle;
				textcolor = defaulttextcolor;
				title = defaulttitle;
				windowheight = defaultwindowheight;
				windowwidth = defaultwindowwidth;
				windowx = Convert.ToInt32( ( screenwidth - defaultwindowwidth ) / 2 );
				windowy = Convert.ToInt32( ( screenheight - defaultwindowheight ) / 2 );
			}
			else
			{
				windowheight = Math.Min( windowheight, screenheight );
				windowwidth = Math.Min( windowwidth, screenwidth );
				if ( windowx == -1 )
				{
					windowx = Convert.ToInt32( ( screenwidth - windowwidth ) / 2 );
				}
				if ( windowy == -1 )
				{
					windowy = Convert.ToInt32( ( screenheight - windowheight ) / 2 );
				}
				windowx = Math.Min( windowx, screenwidth - windowwidth );
				windowy = Math.Min( windowy, screenheight - windowheight );
			}

			#endregion Validate Command Line Settings


			#region Prepare Dialog Form

			// The dialog form itself
			rtmbform = new Form
			{
				Text = title,
				ClientSize = new Size( windowwidth, windowheight ),
				Location = new Point( windowx, windowy ),
				MaximizeBox = false,
				SizeGripStyle = SizeGripStyle.Hide,
				ShowInTaskbar = showintaskbar,
				StartPosition = FormStartPosition.Manual,
				TopMost = topmost,
				WindowState = FormWindowState.Normal
			};
			rtmbform.BringToFront( );

			// The rich text box
			rtmbox = new RichTextBox
			{
				Text = message,
				Height = ( windowheight - buttonheight - 30 ),
				Width = ( windowwidth - 20 ),
				Font = new Font( fontfamily, fontsize, fontstyle ),
				ForeColor = textcolor,
				Location = new Point( 10, 10 ),
				ReadOnly = true
			};
			rtmbform.Controls.Add( rtmbox );

			// Button 1
			Button button1 = new Button
			{
				Text = button1text,
				Height = buttonheight,
				Width = buttonwidth,
				Location = ButtonLocation( 1 )
			};
			button1.Click += Button1_Click;
			rtmbform.Controls.Add( button1 );
			if ( defaultbutton == 1 )
			{
				rtmbform.AcceptButton = button1;
				button1.Focus( );
			}

			// Optional button 2
			if ( !string.IsNullOrWhiteSpace( button2text ) )
			{
				Button button2 = new Button
				{
					Text = button2text,
					Height = buttonheight,
					Width = buttonwidth,
					Location = ButtonLocation( 2 )
				};
				button2.Click += Button2_Click;
				rtmbform.Controls.Add( button2 );
				if ( defaultbutton == 2 )
				{
					rtmbform.AcceptButton = button2;
					button2.Focus( );
				}

				// Optional button 3, only if button 2 is also specified
				if ( !string.IsNullOrWhiteSpace( button3text ) )
				{
					Button button3 = new Button
					{
						Text = button3text,
						Height = buttonheight,
						Width = buttonwidth,
						Location = ButtonLocation( 3 )
					};
					button3.Click += Button3_Click;
					rtmbform.Controls.Add( button3 );
					if ( defaultbutton == 3 )
					{
						rtmbform.AcceptButton = button3;
						button3.Focus( );
					}
				}
			}

			#endregion Prepare Dialog Form


			// Optional timer for timeout feature
			if ( timeout > 0 )
			{
				timer = new System.Timers.Timer( );
				timer.Elapsed += new ElapsedEventHandler( Timer_Elapsed );
				timer.Interval = timeout;
				timer.Start( );
			}

			// Show dialog window
			rtmbform.ShowDialog( );

			// Interpret the result to be returned
			if ( timeoutelapsed )
			{
				buttonclickednumber = defaultbutton;
				switch ( defaultbutton )
				{
					case 1:
						buttonclickedtext = button1text;
						break;
					case 2:
						buttonclickedtext = button2text;
						break;
					case 3:
						buttonclickedtext = button3text;
						break;
					default:
						buttonclickedtext = "Timeout";
						buttonclickednumber = 4;
						break;
				}
			}

			if ( showhelp )
			{
				return -1;
			}
			else
			{
				Console.WriteLine( buttonclickedtext );
				return buttonclickednumber;
			}
		}


		private static void Button1_Click( object sender, EventArgs e )
		{
			buttonclickedtext = button1text;
			buttonclickednumber = 1;
			rtmbform.Close( );
		}


		private static void Button2_Click( object sender, EventArgs e )
		{
			buttonclickedtext = button2text;
			buttonclickednumber = 2;
			rtmbform.Close( );
		}


		private static void Button3_Click( object sender, EventArgs e )
		{
			buttonclickedtext = button3text;
			buttonclickednumber = 3;
			rtmbform.Close( );
		}

		public static void Timer_Elapsed( object sender, System.EventArgs e )
		{
			timeoutelapsed = true;
			FormClose( );
		}


		private static void FormClose()
		{
			if ( rtmbform.InvokeRequired )
			{
				FormCloseCallback fccb = new FormCloseCallback( FormClose );
				rtmbform.Invoke( fccb );
			}
			else
			{
				rtmbform.Close( );
			}
		}

		
		delegate void FormCloseCallback( );


		private static Point ButtonLocation( int button )
		{
			Point location = new Point( );
			switch ( buttoncount )
			{
				case 1:
					location.X = ( windowwidth - buttonwidth ) / 2; // center
					break;
				case 2:
					if ( button == 1 )
					{
						location.X = windowwidth / 2 - buttonwidth - 10; // left
					}
					else
					{
						location.X = windowwidth / 2 + buttonwidth + 10; // right
					}
					break;
				case 3:
					if ( button == 1 )
					{
						location.X = ( windowwidth - buttonwidth ) / 2 - buttonwidth - 10; // left
					}
					else if ( button == 2 )
					{
						location.X = ( windowwidth - buttonwidth ) / 2; // center
					}
					else
					{
						location.X = ( windowwidth - buttonwidth ) / 2 + buttonwidth + 10; // right
					}
					break;
			}
			location.Y = windowheight - buttonheight - 10;
			return location;
		}


		static string UnEscapeString( string message )
		{
			// Unescaping tabs, linefeeds and quotes
			message = message.Replace( "\\n", "\n" );
			message = message.Replace( "\\r", "\r" );
			message = message.Replace( "\\t", "\t" );
			message = message.Replace( "\\007", "\t" );
			message = message.Replace( "\\012", "\n" );
			message = message.Replace( "\\015", "\r" );
			message = message.Replace( "\\042", "\"" );
			message = message.Replace( "\\047", "'" );
			// Unescaping Unicode, technique by "dtb" on StackOverflow.com: http://stackoverflow.com/a/8558748
			message = Regex.Replace( message, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString( (char) ushort.Parse( m.Groups[1].Value, NumberStyles.AllowHexSpecifier ) ) );
			return message;
		}


		static void ShowHelp( )
		{
			/*
			RichTextMessageBox,  Version 1.00
			Show a fully customizable message dialog and return which button is clicked

			Usage:    RichTextMessageBox.exe  message  [ title ]  [ options ]

			          message       is the text to be displayed in the dialog
			          title         is the dialog's window title
									(default: program name and version)

			Options:  /AlwaysOnTop  Modal window, always on top
			          /B1:"caption" Caption for button 1 (default: OK)
			          /B2:"caption" Caption for button 2 (default: empty)
			          /B3:"caption" Caption for button 3 (default: empty)
			          /BH:height    Button height (default: 25)
			          /Bold         Bold text for message
			          /BW:width     Button width (default: 100)
			          /C:color      Text color for dialog (default: Black)
			          /DB:default   Default button (default: 1; values 2 or 3 are
			                        valid only if there are that many buttons)
			          /Font:name    Font family name (default: Sans-Serif)
			          /FS:fontsize  Font size (default: 12)
			          /Italic       Italic text for message
			          /Literal      Treat message as literal, do not interpret special
			                        characters \n, \012, \t, \007 or unicode \u****
			                        (default: interpret special characters and unicode)
			          /Strike       Strikeout text
			          /T:seconds    Timeout in seconds (default: no timeout)
			          /Taskbar      Show in taskbar
			          /Underline    Underline text
			          /WH:height    Window height (default: 480)
			          /WW:width     Window width (default: 640)
			          /X:x          X-coordinate of upper left window corner
			          /Y:y          Y-coordinate of upper left window corner
			                        (default: center window on screen)

			Notes:    The caption of the button that is clicked will be sent to the
			          console, the number of the button is returned as "errorlevel".
			          In case of errors, the "errorlevel" will be -1, if a timeout
			          elapsed and no default button was specified, the "errorlevel"
			          will be 4, and the text "Timeout" is sent to the console.
			          If an invalid text color is specified, it will be ignored.

			Written by Rob van der Woude
			https://www.robvanderwoude.com/
			*/

			message = string.Format( "RichTextMessageBox,  Version {0}\n", progver );
			message += "Show a fully customizable message dialog and return which button is clicked\n\n";
			message += "Usage:    RichTextMessageBox.exe  message  [ title ]  [ options ]\n\n";
			message += "          message       is the text to be displayed in the dialog\n";
			message += "          title         is the dialog's window title\n";
			message += "                        (default: program name and version)\n\n";
			message += "Options:  /AlwaysOnTop  Modal window, always on top\n";
			message += "          /B1:\"caption\" Caption for button 1 (default: OK)\n";
			message += "          /B2:\"caption\" Caption for button 2 (default: empty)\n";
			message += "          /B3:\"caption\" Caption for button 3 (default: empty)\n";
			message += string.Format( "          /BH:height    Button height (default: {0})\n", defaultbuttonheight );
			message += "          /Bold         Bold text for message\n";
			message += string.Format( "          /BW:width     Button width (default: {0})\n", defaultbuttonwidth );
			message += "          /C:color      Text color for dialog (default: Black)\n";
			message += "          /DB:default   Default button (default: 1; values 2 or 3 are\n";
			message += "                        valid only if there are that many buttons)\n";
			message += string.Format( "          /Font:name    Font family name (default: {0})\n", defaultfontfamily );
			message += string.Format( "          /FS:fontsize  Font size (default: {0})\n", defaultfontsize );
			message += "          /Italic       Italic text for message\n";
			message += "          /Literal      Treat message as literal, do not interpret special\n";
			message += "                        characters \\n, \\012, \\t, \\007 or unicode \\u****\n";
			message += "                        (default: interpret special chars and unicode)\n";
			message += "          /Strike       Strikeout text\n";
			message += "          /T:seconds    Timeout in seconds (default: no timeout)\n";
			message += "          /Taskbar      Show in taskbar\n";
			message += "          /Underline    Underline text\n";
			message += string.Format( "          /WH:height    Window height (default: {0})\n", defaultwindowheight );
			message += string.Format( "          /WW:width     Window width (default: {0})\n", defaultwindowwidth );
			message += "          /X:x          X-coordinate of upper left window corner\n";
			message += "          /Y:y          Y-coordinate of upper left window corner\n";
			message += "                        (default: center window on screen)\n\n";
			message += "Notes:    The caption of the button that is clicked will be sent to the\n";
			message += "          console, the number of the button is returned as \"errorlevel\".\n";
			message += "          In case of errors, the \"errorlevel\" will be -1, if a timeout\n";
			message += "          elapsed and no default button was specified, the \"errorlevel\"\n";
			message += "          will be 4, and the text \"Timeout\" is sent to the console.\n";
			message += "          If an invalid text color is specified, it will be ignored.\n\n";
			message += "Written by Rob van der Woude\n";
			message += "https://www.robvanderwoude.com/";

			Console.Error.WriteLine( message );
		}
	}
}
