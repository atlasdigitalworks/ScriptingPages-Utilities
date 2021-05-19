using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace RobvanderWoude
{
	class ListNICs
	{
		public static ArrayList nics = new ArrayList( );
		public static string computer = string.Empty;

		// Use global variables, so we only need to run the WMI queries once
		public static string nsrootwmi = computer + "root\\WMI";
		public static string nsrootcimv2 = computer + "root\\CIMV2";
		public static ManagementObjectSearcher searcher1 = new ManagementObjectSearcher( nsrootwmi, "SELECT * FROM MSNdis_PhysicalMediumType" );
		public static ManagementObjectCollection wmi1 = searcher1.Get( );
		public static ManagementObjectSearcher searcher2 = new ManagementObjectSearcher( nsrootcimv2, "SELECT * FROM Win32_NetworkAdapter" );
		public static ManagementObjectCollection wmi2 = searcher2.Get( );
		public static ManagementObjectSearcher searcher3 = new ManagementObjectSearcher( nsrootwmi, "SELECT * FROM MSNdis_LinkSpeed" );
		public static ManagementObjectCollection wmi3 = searcher3.Get( );


		static int Main( string[] args )
		{
			try
			{
				bool listBluetooth = true;
				bool listWired = true;
				bool listWireless = true;


				#region Command line parsing


				// Only 2 optional argument allowed: remote computer name and/or adapter type
				if ( args.Length > 2 )
				{
					return WriteError( "Invalid command line arguments" );
				}
				if ( args.Length > 0 )
				{
					foreach ( string arg in args )
					{
						// We'll display a 'friendly' message if help was requested
						if ( arg.StartsWith( "/" ) || arg.StartsWith( "-" ) )
						{
							switch ( arg.ToUpper( ) )
							{
								case "/?":
								case "-?":
									return WriteError( string.Empty );
								case "/B":
								case "/BLUETOOTH":
									if ( ( listBluetooth && listWired && listWireless ) == false )
									{
										return WriteError( "Select a single adapter type only, or omit type to select all" );
									}
									listWired = false;
									listWireless = false;
									break;
								case "/W":
								case "/WIRED":
									if ( ( listBluetooth && listWired && listWireless ) == false )
									{
										return WriteError( "Select a single adapter type only, or omit type to select all" );
									}
									listBluetooth = false;
									listWireless = false;
									break;
								case "/WL":
								case "/WIFI":
								case "/WIRELESS":
									if ( ( listBluetooth && listWired && listWireless ) == false )
									{
										return WriteError( "Select a single adapter type only, or omit type to select all" );
									}
									listBluetooth = false;
									listWired = false;
									break;
								default:
									return WriteError( "Invalid command line argument" );
							}
						}
						else
						{
							if ( !string.IsNullOrEmpty( computer ) )
							{
								return WriteError( "Do not specify more than one computer name" );
							}
							computer = "\\\\" + arg + "\\";
						}

					}
				}


				#endregion Command line parsing


				foreach ( ManagementObject queryObj1 in wmi1 )
				{
					if ( queryObj1["NdisPhysicalMediumType"].ToString( ) == "10" )
					{
						if ( listBluetooth )
						{
							AddAdapter( queryObj1["InstanceName"].ToString( ), "Bluetooth" );
						}
					}
					if ( queryObj1["NdisPhysicalMediumType"].ToString( ) == "0" )
					{
						if ( listWired )
						{
							AddAdapter( queryObj1["InstanceName"].ToString( ), "Wired" );
						}
					}
					if ( queryObj1["NdisPhysicalMediumType"].ToString( ) == "1" )
					{
						if ( listWireless )
						{
							AddAdapter( queryObj1["InstanceName"].ToString( ), "Wireless" );
						}
					}
				}

				nics.Sort( );

				foreach ( string nic in nics )
				{
					Console.WriteLine( nic );
				}


				return 0;
			}
			catch ( Exception e )
			{
				return WriteError( e );
			}
		}


		public static void AddAdapter( string name, string type )
		{
			foreach ( ManagementObject queryObj2 in wmi2 )
			{
				if ( ( queryObj2["Name"].ToString( ) == name ) && Convert.ToBoolean( queryObj2["PhysicalAdapter"] ) )
				{
					foreach ( ManagementObject queryObj3 in wmi3 )
					{
						if ( queryObj3["InstanceName"].ToString( ) == name )
						{
							nics.Add( String.Format( "{0,6}", Convert.ToInt32( queryObj3["NdisLinkSpeed"] ) / 10000 ) + " Mb/s\t" + String.Format( "{0,-11}", "[" + type + "]" ) + "\t" + name );
						}
					}
				}
			}
		}


		#region Error handling


		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}


		public static int WriteError( string errorMessage )
		{
			/*
			ListNICs,  Version 1.00
			List physical network adapters on the specified computer

			Usage:  LISTNICS  [ computername ]  [ /Bluetooth | /Wired | /WireLess ]

			Where:  "computername"    is a remote computer name    (default: this computer)
			        /Bluetooth or /B  list Bluetooth adapters only (default: all)
			        /Wired     or /W  list wired adapters only     (default: all)
			        /Wireless  or /WL list wireless adapters only  (default: all)

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			if ( string.IsNullOrEmpty( errorMessage ) == false )
			{
				Console.Error.WriteLine( );
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.Write( "ERROR:  " );
				Console.ForegroundColor = ConsoleColor.White;
				Console.Error.WriteLine( errorMessage );
				Console.ResetColor( );
			}
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "ListNICs,  Version 1.00" );
			Console.Error.WriteLine( "List physical network adapters on the specified computer" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "LISTNICS" );
			Console.ResetColor( );
			Console.Error.Write( "  [ computername ]  [ " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/B" );
			Console.ResetColor( );
			Console.Error.Write( "luetooth | " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.Write( "ired | " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.Write( "ire" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ess ]" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Where:  \"computername\"    is a remote computer name    (default: this computer)" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /B" );
			Console.ResetColor( );
			Console.Error.Write( "luetooth or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/B" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  list Bluetooth adapters only (default: all)" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /W" );
			Console.ResetColor( );
			Console.Error.Write( "ired     or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "  list wired adapters only     (default: all)" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "        /W" );
			Console.ResetColor( );
			Console.Error.Write( "ire" );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.Write( "ess  or " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "/WL" );
			Console.ResetColor( );
			Console.Error.WriteLine( " list wireless adapters only  (default: all)" );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 1;
		}


		#endregion Error handling
	}
}
