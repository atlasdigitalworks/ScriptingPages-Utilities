using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class CatchStrayWindows
	{
		static readonly string progver = "1.00";

		static readonly int screenwidth = Screen.PrimaryScreen.WorkingArea.Width;
		static readonly int screenheight = Screen.PrimaryScreen.WorkingArea.Height;

		static bool includeminimized = false;
		static bool includeanonymous = false;


		static int Main( string[] args )
		{
			// Set console encoding to UTF-8
			Console.OutputEncoding = System.Text.Encoding.UTF8;


			#region Parse Command Line

			foreach ( string arg in args )
			{
				switch ( arg.ToUpper( ) )
				{
					case "/?":
						return ShowHelp( );
					case "/IA":
						if ( includeanonymous )
						{
							return ShowHelp( "Duplicate command line switch /IA" );
						}
						includeanonymous = true;
						break;
					case "/IM":
						if ( includeminimized )
						{
							return ShowHelp( "Duplicate command line switch /IM" );
						}
						includeminimized = true;
						break;
					default:
						return ShowHelp( "Invalid command line argument \"{0}\"", arg );
				}
			}

			#endregion Parse Command Line


			// List active windows
			GetDesktopWindowHandlesAndTitles( out windows );


			#region Restore Minimized Windows

			// First restore/maximize any minimized window(s) that should be included
			foreach ( IntPtr handle in windows.Keys )
			{
				if ( windows[handle] != "(no title)" || includeanonymous )
				{
					if ( !IsIconic( handle ) || includeminimized )
					{
						string debugmessage = string.Format( "Maximized \"{0}\"", windows[handle] );
						//string debugmessage = string.Format( "Restored \"{0}\"", windows[handle] );
						if ( IsIconic( handle ) && includeminimized )
						{
							Console.WriteLine( debugmessage );
							ShowWindow( handle, SW_SHOWMAXIMIZED );
							//ShowWindow( handle, SW_SHOWMAXIMIZED );
						}
					}
				}
			}

			#endregion Restore Minimized Windows


			#region Move Windows to Primary Monitor

			// Next move the window(s) to the primary monitor
			int count = 0;

			foreach ( IntPtr handle in windows.Keys )
			{
				if ( windows[handle] != "(no title)" || includeanonymous )
				{
					if ( !IsIconic( handle ) || includeminimized )
					{
						RECT rect = new RECT( );
						if ( GetWindowRect( handle, ref rect ) )
						{
							if ( ( rect.Left > screenwidth || rect.Right < 20 || rect.Top > screenheight || rect.Bottom < 20 ) && ( rect.Left != 0 && rect.Top != 0 ) )
							{
								int width = rect.Right - rect.Left;
								int height = rect.Bottom - rect.Top;
								bool repaint = true;
								string debugmessage = string.Format( "Moved \"{0}\" from ({1},{2}) to ({3},{4})", windows[handle], rect.Left, rect.Top, 0, 0 );
								if ( MoveWindow( handle, 0, 0, width, height, repaint ) )
								{
									Console.WriteLine( debugmessage );
									count += 1;
								}
							}
						}
					}
				}
			}

			#endregion Move Windows to Primary Monitor


			return count;
		}



		#region Check If Minimized

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool IsIconic( IntPtr hWnd ); // true if window is minimized

		#endregion Check If Minimized


		#region Get Window Size

		// Get window position and size
		// https://stackoverflow.com/a/1434577
		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool GetWindowRect( IntPtr hWnd, ref RECT lpRect );
		[StructLayout( LayoutKind.Sequential )]


		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		#endregion Get Window Size


		#region List Desktop Windows

		// Based on code by Rod Stephens
		// List desktop windows in C#
		// http://csharphelper.com/blog/2016/08/list-desktop-windows-in-c/

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool IsWindowVisible( IntPtr hWnd );

		[DllImport( "user32.dll", EntryPoint = "GetWindowText",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true )]
		private static extern int GetWindowText( IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount );

		[DllImport( "user32.dll", EntryPoint = "EnumDesktopWindows",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true )]
		private static extern bool EnumDesktopWindows( IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam );

		// Define the callback delegate's type.
		private delegate bool EnumDelegate( IntPtr hWnd, int lParam );

		// Save window titles and handles in Dictionary
		private static Dictionary<IntPtr, string> windows;

		// Return a list of the desktop windows' handles and titles.
		public static void GetDesktopWindowHandlesAndTitles( out Dictionary<IntPtr, string> windowslist )
		{
			windowslist = new Dictionary<IntPtr, string>( );

			if ( !EnumDesktopWindows( IntPtr.Zero, FilterCallback, IntPtr.Zero ) )
			{
				windowslist = null;
			}
			else
			{
				windowslist = windows;
			}
		}


		// We use this function to filter windows
		// This version selects visible windows that aren't minimized
		private static bool FilterCallback( IntPtr hWnd, int lParam )
		{
			// Get the window's title
			StringBuilder sb_title = new StringBuilder( 1024 );
			int _ = GetWindowText( hWnd, sb_title, sb_title.Capacity );
			string title = sb_title.ToString( );

			// If the window is visible and has a title, save it.
			if ( IsWindowVisible( hWnd ) )
			{
				if ( string.IsNullOrEmpty( title ) )
				{
					title = "(no title)";
				}
				windows.Add( hWnd, title );
			}

			// Return true to indicate that we should continue enumerating windows
			return true;
		}

		#endregion List Desktop Windows


		#region Move Window

		// Based on code by Roberto Luis Bisb\u00E9
		// Moving windows programatically with Windows API, the path to WinResize
		// https://rlbisbe.net/2013/11/19/moving-windows-programatically-with-windows-api-the-path-to-winresize/

		[DllImport( "user32.dll" )]
		public static extern bool MoveWindow( IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint );

		#endregion Move Window


		#region Restore Window

		// private const int SW_SHOWNORMAL = 1;
		// private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;


		[DllImport( "user32.dll" )]
		private static extern bool ShowWindow( IntPtr hWnd, int nCmdShow );


		[DllImport( "user32.dll" )]
		private static extern bool ShowWindowAsync( IntPtr hWnd, int nCmdShow );

		#endregion Restore Window


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
			CatchStrayWindows.exe,  Version 1.00
			Move out-of-sight windows back to the primary screen
 
			Usage:   CatchStrayWindows  [ /IA ]  [ /IM ]
 
			Options: /IA	Include Anonymous windows, i.e. those without title
			         /IM    Include Minimized windows
 
			Credits: Get window position and size
			         https://stackoverflow.com/a/1434577
			         List desktop windows based on code by Rod Stephens
			         http://csharphelper.com/blog/2016/08/list-desktop-windows-in-c/
			         Moving windows based on code by Roberto Luis Bisb\u00E9
			         https://rlbisbe.net/2013/11/19/
			         moving-windows-programatically-with-windows-api-the-path-to-winresize/

			Note:    Return code equals number of moved windows, or -1 in case of errors.

			Written by Rob van der Woude
			https://www.robvanderwoude.com
			*/

			#endregion Help Text


			#region Display Help Text

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "CatchStrayWindows.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Move out-of-sight windows back to the primary screen" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/IA    I" );
			Console.ResetColor( );
			Console.Error.Write( "nclude " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.WriteLine( "nonymous windows, i.e. those without title" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "         /IM    I" );
			Console.ResetColor( );
			Console.Error.Write( "nclude " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "M" );
			Console.ResetColor( );
			Console.Error.WriteLine( "inimized windows" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits: Get window position and size" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         https://stackoverflow.com/a/1434577" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         List desktop windows based on code by Rod Stephens" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         http://csharphelper.com/blog/2016/08/list-desktop-windows-in-c/" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         Moving windows based on code by Roberto Luis Bisb\u00E9" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         https://rlbisbe.net/2013/11/19/" );
			Console.Error.WriteLine( "         moving-windows-programatically-with-windows-api-the-path-to-winresize/" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Note:    Return code equals number of moved windows, or -1 in case of errors." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "https://www.robvanderwoude.com" );

			#endregion Display Help Text

			return -1;
		}

		#endregion Error handling
	}
}
