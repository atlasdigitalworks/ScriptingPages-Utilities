using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace RobvanderWoude
{
	class GetTitle
	{
		// DLL imports required to read window title
		[DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		internal static extern int GetConsoleTitle( StringBuilder sb, int capacity );

		static int Main( string[] args )
		{
			try
			{
				// By default, do not strip anything
				bool trimPrefix = false;
				bool trimCmdline = false;
				string cmdline = Environment.CommandLine;

				// Read the full window title
				string windowTitle = GetWindowTitle( );

				#region Command Line Parsing

				// Check command line arguments
				foreach ( string arg in args )
				{
					switch ( arg.ToLower( ) )
					{
						case "/nc":
						case "-nc":
							if ( trimCmdline )
							{
								return WriteError( "Duplicate command line switch " + arg.ToUpper( ) );
							}
							trimCmdline = true;
							break;
						case "/np":
						case "-np":
							if ( trimPrefix )
							{
								return WriteError( "Duplicate command line switch " + arg.ToUpper( ) );
							}
							// Ignore the /NP switch if we're not running with elevated privileges
							trimPrefix = UacHelper.IsProcessElevated;
							break;
						case "/?":
						case "-?":
						case "/h":
						case "-h":
						case "/help":
						case "-help":
						case "--help":
							// Display help text without error message
							return WriteError( string.Empty );
						default:
							return WriteError( "Invalid command line argument(s)" );
					}
				}
				if ( args.Length > 2 )
				{
					return WriteError( "Invalid command line argument(s)" );
				}
				#endregion Command Line Parsing

				// If /NP is used in elevated process,
				// check if the first space in the title comes after the first colon, which must be
				// after the second position (otherwise it may be the colon for the drive letter);
				// if so, remove everything from the start through the firts colon,
				// then trim leading spaces
				if ( trimPrefix )
				{
					int posC = windowTitle.IndexOf( ':', 2 );
					int posS = windowTitle.IndexOf( ' ' );
					if ( posC > 2 && posS > posC )
					{
						windowTitle = windowTitle.Substring( posC + 1 ).Trim( );
					}
				}

				// Strip the command line from the window title if /NC is used
				if ( trimCmdline )
				{
					int posL = windowTitle.IndexOf( " - " + cmdline );
					if ( posL > 0 )
					{
						windowTitle = windowTitle.Substring( 0, posL + 1 );
					}
				}

				// Display the window title
				Console.WriteLine( windowTitle );

				return 0;
			}
			catch ( Exception e )
			{
				// Display help text with error message
				return WriteError( e );
			}
		}

		// Code to read window title, by Justin Goldspring
		public static string GetWindowTitle( )
		{
			const int nChars = 256;
			IntPtr handle = IntPtr.Zero;
			StringBuilder Buff = new StringBuilder( nChars );

			if ( GetConsoleTitle( Buff, nChars ) > 0 )
			{
				return Buff.ToString( );
			}
			return string.Empty;
		}

		// Code to check if running with elevated privileges, by StackOverflow.com:
		// http://www.stackoverflow.com/questions/1220213/c-detect-if-running-with-elevated-privileges
		public static class UacHelper
		{
			private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
			private const string uacRegistryValue = "EnableLUA";

			private static uint STANDARD_RIGHTS_READ = 0x00020000;
			private static uint TOKEN_QUERY = 0x0008;
			private static uint TOKEN_READ = ( STANDARD_RIGHTS_READ | TOKEN_QUERY );

			[DllImport( "advapi32.dll", SetLastError = true )]
			[return: MarshalAs( UnmanagedType.Bool )]
			static extern bool OpenProcessToken( IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle );

			[DllImport( "advapi32.dll", SetLastError = true )]
			public static extern bool GetTokenInformation( IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength );

			public enum TOKEN_INFORMATION_CLASS
			{
				TokenUser = 1,
				TokenGroups,
				TokenPrivileges,
				TokenOwner,
				TokenPrimaryGroup,
				TokenDefaultDacl,
				TokenSource,
				TokenType,
				TokenImpersonationLevel,
				TokenStatistics,
				TokenRestrictedSids,
				TokenSessionId,
				TokenGroupsAndPrivileges,
				TokenSessionReference,
				TokenSandBoxInert,
				TokenAuditPolicy,
				TokenOrigin,
				TokenElevationType,
				TokenLinkedToken,
				TokenElevation,
				TokenHasRestrictions,
				TokenAccessInformation,
				TokenVirtualizationAllowed,
				TokenVirtualizationEnabled,
				TokenIntegrityLevel,
				TokenUIAccess,
				TokenMandatoryPolicy,
				TokenLogonSid,
				MaxTokenInfoClass
			}

			public enum TOKEN_ELEVATION_TYPE
			{
				TokenElevationTypeDefault = 1,
				TokenElevationTypeFull,
				TokenElevationTypeLimited
			}

			public static bool IsUacEnabled
			{
				get
				{
					RegistryKey uacKey = Registry.LocalMachine.OpenSubKey( uacRegistryKey, false );
					bool result = uacKey.GetValue( uacRegistryValue ).Equals( 1 );
					return result;
				}
			}

			public static bool IsProcessElevated
			{
				get
				{
					// Skip this test and return false if the OS is XP or even older
					OperatingSystem osInfo = Environment.OSVersion;
					if ( osInfo.Version.Major > 5 )
					{
						if ( IsUacEnabled )
						{
							IntPtr tokenHandle;
							if ( !OpenProcessToken( Process.GetCurrentProcess( ).Handle, TOKEN_READ, out tokenHandle ) )
							{
								throw new ApplicationException( "Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error( ) );
							}

							TOKEN_ELEVATION_TYPE elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

							int elevationResultSize = Marshal.SizeOf( (int) elevationResult );
							uint returnedSize = 0;
							IntPtr elevationTypePtr = Marshal.AllocHGlobal( elevationResultSize );

							bool success = GetTokenInformation( tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint) elevationResultSize, out returnedSize );
							if ( success )
							{
								elevationResult = (TOKEN_ELEVATION_TYPE) Marshal.ReadInt32( elevationTypePtr );
								bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
								return isProcessAdmin;
							}
							else
							{
								throw new ApplicationException( "Unable to determine the current elevation." );
							}
						}
						else
						{
							WindowsIdentity identity = WindowsIdentity.GetCurrent( );
							WindowsPrincipal principal = new WindowsPrincipal( identity );
							bool result = principal.IsInRole( WindowsBuiltInRole.Administrator );
							return result;
						}
					}
					else
					{
						return false;
					}
				}
			}
		}

		// Code to display help and optional error message, by Bas van der Woude
		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			string exeName = Process.GetCurrentProcess( ).ProcessName;

			/*
			GetTitle,  Version 4.00
			Return the current window's title, optionally stripping the 'Administrator:'
			prefix on UAC enabled systems, and/or the command line

			Usage:    GETTITLE  [ /NC ]  [ /NP ]

			Where:    /NC   strips the command line if appended to the title
			          /NP   strips the 'Administrator:' prefix on UAC enabled systems
			                (ignored if the process does not run with elevated privileges)

			Credits:  Check for elevated privileges based on a thread on StackOverflow.com
			          /questions/1220213/c-detect-if-running-with-elevated-privileges
			          Code to read console window title by Justin Goldspring.

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
			Console.Error.WriteLine( "GetTitle,  Version 4.00" );
			Console.Error.WriteLine( "Return the current window's title, optionally stripping the 'Administrator:'" );
			Console.Error.WriteLine( "prefix on UAC enabled systems, and/or the command line" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "{0}  [ /NC ]  [ /NP ]", exeName.ToUpper( ) );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/NC" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   strips the command line if appended to the title" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          /NP" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   strips the 'Administrator:' prefix on UAC enabled systems" );
			Console.Error.WriteLine( "                (ignored if the process does not run with elevated privileges)" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Credits:  Check for elevated privileges based on a thread on " );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "StackOverflow.com" );
			Console.Error.WriteLine( "          /questions/1220213/c-detect-if-running-with-elevated-privileges" );
			Console.ResetColor( );
			Console.Error.WriteLine( "          Code to read console window title by Justin Goldspring." );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}
	}
}