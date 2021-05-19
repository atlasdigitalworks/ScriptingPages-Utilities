    using System;

    using System.Collections.Generic;

    using System.Diagnostics;

    using System.Runtime.InteropServices;

    using System.Windows.Forms;

     

     

    namespace RobvanderWoude

    {

    	class SystemInformationWrapper

    	{

    		static string progver = "1.03";

     

     

    		static int Main( string[] args )

    		{

    			#region Initialize Variables

     

    			int rc = 0;

    			int maxnamelength = 0;

    			SortedList<string, string> requestedproperties = new SortedList<string, string>( );

    			SortedList<string, string> propertynames = new SortedList<string, string>( );

    			SortedList<string, int> propertyindices = new SortedList<string, int>( );

    			bool ignoreinvalidproperties = false;

    			bool listall = true;

    			bool specificproperties = false;

    			bool openurl = false;

    			string url = "https://msdn.microsoft.com/library/system.windows.forms.systeminformation.aspx";

     

    			#endregion Initialize Variables

     

     

    			#region Parse Command Line

     

    			if ( args.Length > 0 )

    			{

    				foreach ( string arg in args )

    				{

    					if ( arg == "/?" )

    					{

    						return ShowHelp( );

    					}

    					else if ( arg.ToUpper( ) == "/I" )

    					{

    						if ( ignoreinvalidproperties )

    						{

    							return ShowHelp( "Duplicate command line switch /I" );

    						}

    						ignoreinvalidproperties = true;

    					}

    					else if ( arg.ToUpper( ) == "/L" )

    					{

    						listall = true;

    					}

    					else if ( arg.ToUpper( ) == "/U" )

    					{

    						if ( openurl )

    						{

    							return ShowHelp( "Duplicate command line switch /U" );

    						}

    						openurl = true;

    					}

    					else if ( arg[0] == '/' )

    					{

    						return ShowHelp( "Invalid command line switch {0}", arg.ToUpper( ) );

    					}

    					else

    					{

    						requestedproperties.Add( arg.ToLower( ), arg );

    						specificproperties = true;

    						listall = false;

    					}

    				}

    			}

     

     

    			if ( listall && specificproperties )

    			{

    				return ShowHelp( "Either specificy one or more properties, or /L to list all, but not both" );

    			}

     

    			#endregion Parse Command Line

     

     

    			#region Make a List of Available Properties

     

    			int index = 0;

    			foreach ( _PropertyInfo sp in typeof( SystemInformation ).GetProperties( ) )

    			{

    				propertynames.Add( sp.Name.ToLower( ), sp.Name );

    				propertyindices.Add( sp.Name, index );

    				if ( sp.Name.Length > maxnamelength )

    				{

    					maxnamelength = sp.Name.Length;

    				}

    				if ( !listall )

    				{

    					if ( requestedproperties.Keys.Contains( sp.Name.ToLower( ) ) )

    					{

    						requestedproperties[sp.Name.ToLower( )] = sp.Name;

    					}

    					// Translate requested indices to property names

    					if ( requestedproperties.Keys.Contains( index.ToString( ) ) )

    					{

    						requestedproperties[sp.Name.ToLower( )] = sp.Name;

    						requestedproperties.Remove( index.ToString( ) );

    					}

    				}

    				index += 1;

    			}

     

    			#endregion Make a List of Available Properties

     

     

    			if ( listall )

    			{

    				// Default: list all properties and their values

    				// Show table head

    				Console.WriteLine( "{0,-" + maxnamelength + "}    Index:      Value:", "Property Name:" );

    				Console.WriteLine( "{0,-" + maxnamelength + "}    ======      ======", "==============" );

    				foreach ( string property in propertynames.Values )

    				{

    					object propval = typeof( SystemInformation ).GetProperty( property ).GetValue( typeof( SystemInformation ), null );

    					string propertyvalue = propval.ToString( );

    					// PowerStatus has to be handled separately, by default it only returns a string "System.Windows.Forms.PowerStatus"

    					if ( property == "PowerStatus" )

    					{

    						propertyvalue = GetPowerStatus( );

    					}

    					Console.WriteLine( "{0,-" + maxnamelength + "}    {1,6}      {2}", property, propertyindices[property], propertyvalue );

    				}

    			}

    			else

    			{

    				foreach ( string requestedproperty in requestedproperties.Keys )

    				{

    					if ( propertynames.Keys.Contains( requestedproperty ) )

    					{

    						// Get the selected property's value

    						string propertyvalue = Convert.ToString( typeof( SystemInformation ).GetProperty( requestedproperties[requestedproperty] ).GetValue( typeof( SystemInformation ), null ) );

    						// PowerStatus has to be handled separately, by default it only returns a string "System.Windows.Forms.PowerStatus"

    						if ( requestedproperties[requestedproperty] == "PowerStatus" )

    						{

    							propertyvalue = GetPowerStatus( );

    						}

    						// Try if the return value can be set to the selected property's value

    						try

    						{

    							rc = Convert.ToInt32( propertyvalue );

    						}

    						catch ( Exception )

    						{

    							rc = 0;

    						}

    						// Display selected property and its value

    						Console.WriteLine( "{0}={1}", requestedproperties[requestedproperty], propertyvalue );

    					}

    					else if ( !ignoreinvalidproperties )

    					{

    						return ShowHelp( "Invalid property \"{0}\"", requestedproperty );

    					}

    				}

     

     

    				if ( requestedproperties.Count > 1 )

    				{

    					rc = 0;

    				}

    			}

     

     

    			// Open the URL with the list of available properties

    			if ( openurl )

    			{

    				Process process = new Process( );

    				process.StartInfo = new ProcessStartInfo( url );

    				process.Start( );

    			}

     

     

    			return rc;

    		}

     

     

    		static string GetPowerStatus( )

    		{

    			string powerstatus = String.Empty;

    			object propval = typeof( SystemInformation ).GetProperty( "PowerStatus" ).GetValue( typeof( SystemInformation ), null );

    			string bcs = typeof( PowerStatus ).GetProperty( "BatteryChargeStatus" ).GetValue( propval, null ).ToString( );

    			string bfl = typeof( PowerStatus ).GetProperty( "BatteryFullLifetime" ).GetValue( propval, null ).ToString( );

    			string blp = ( Convert.ToInt32( typeof( PowerStatus ).GetProperty( "BatteryLifePercent" ).GetValue( propval, null ) ) * 100 ).ToString( ) + "%";

    			string blr = typeof( PowerStatus ).GetProperty( "BatteryLifeRemaining" ).GetValue( propval, null ).ToString( );

    			string pls = typeof( PowerStatus ).GetProperty( "PowerLineStatus" ).GetValue( propval, null ).ToString( );

    			powerstatus = String.Format( "{{BatteryChargeStatus={0}", bcs );

    			powerstatus += String.Format( ", BatteryFullLifetime={0}", ( bfl == "-1" ? "Unknown" : bfl ) );

    			powerstatus += String.Format( ", BatteryLifePercent={0}", blp );

    			powerstatus += String.Format( ", BatteryLifeRemaining={0}", ( blr == "-1" ? "Unknown" : blr ) );

    			powerstatus += String.Format( ", PowerLineStatus={0}}}", pls );

    			return powerstatus;

    		}

     

     

    		static int ShowHelp( params string[] errmsg )

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

    			SystemInformation.exe,  Version 1.03

    			Wrapper for the .NET SystemInformation class

     

    			Usage:   SystemInformation  [ property [ property [..] ] | /L ]  [ /I ]  [ /U ]

     

    			Where:   property   name or index of a SystemInformation property to be tested

    			                    (multiple properties allowed, default: list all)

    			         /I         Ignore and skip invalid property names specified on the

    			                    command line (default: abort on invalid property name)

    			         /L         List each property name and its index and value, sorted by

    			                    name (default, implemented for backwards compatibility)

    			         /U         open URL with list of properties and their meanings

     

    			Notes:   If a single property value is requested, the return code will equal

    			         the value returned by the function if it is numerical, or 0 if not.

    			         If multiple properties and their values are listed, return code is 0.

    			         In case of (command line) errors the return code will be -1.

    			         Be careful with the /I switch when requesting multiple properties,

    			         as you will be unable to determine which particular one is ignored.

    			         The meaning of the returned values can be found at

    			         msdn.microsoft.com/library/system.windows.forms.systeminformation.aspx

     

    			Written by Rob van der Woude

    			http://www.robvanderwoude.com

    			*/

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "SystemInformation.exe,  Version {0}", progver );

     

    			Console.Error.WriteLine( "Wrapper for the .NET SystemInformation class" );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.Write( "Usage:   " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.WriteLine( "SystemInformation  [ property [ property [..] ] | /L ]  [ /I ]  [ /U ]" );

    			Console.ResetColor( );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.Write( "Where:   " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "property" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "   name or index of a SystemInformation property to be tested" );

     

    			Console.Error.WriteLine( "                    (multiple properties allowed, default: list all)" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "         /I         I" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "gnore and skip invalid property names specified on the" );

     

    			Console.Error.WriteLine( "                    command line (default: abort on invalid property name)" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "         /L         L" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "ist each property name and its index and value, sorted by" );

     

    			Console.Error.WriteLine( "                    name (default, implemented for backwards compatibility)" );

     

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "         /U" );

    			Console.ResetColor( );

    			Console.Error.Write( "         open " );

    			Console.ForegroundColor = ConsoleColor.White;

    			Console.Error.Write( "U" );

    			Console.ResetColor( );

    			Console.Error.WriteLine( "RL with list of properties and their meanings" );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "Notes:   If a single property value is requested, the return code will equal" );

     

    			Console.Error.WriteLine( "         the value returned by the function if it is numerical, or 0 if not." );

     

    			Console.Error.WriteLine( "         If multiple properties and their values are listed, return code is 0." );

     

    			Console.Error.WriteLine( "         In case of (command line) errors the return code will be -1." );

     

    			Console.Error.WriteLine( "         Be careful with the /I switch when requesting multiple properties," );

     

    			Console.Error.WriteLine( "         as you will be unable to determine which particular one is ignored." );

     

    			Console.Error.WriteLine( "         The meaning of the returned values can be found online at" );

     

    			Console.ForegroundColor = ConsoleColor.DarkGray;

    			Console.Error.WriteLine( "         msdn.microsoft.com/library/system.windows.forms.systeminformation.aspx" );

    			Console.ResetColor( );

     

    			Console.Error.WriteLine( );

     

    			Console.Error.WriteLine( "Written by Rob van der Woude" );

     

    			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

     

    			#endregion Help Text

     

     

    			return -1;

    		}

    	}

    }

     

