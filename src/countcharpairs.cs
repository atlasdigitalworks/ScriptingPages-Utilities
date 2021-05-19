using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace RobvanderWoude
{
	class CountCharPairs
	{
		static string progver = "1.00";


		static int Main( string[] args )
		{
			if ( args.Length > 1 )
			{
				int rc = 0;
				string file = String.Empty;
				bool fileset = false;
				List<string> testchars = new List<string>( );
				string[] allowed = { "[]", "][", "()", ")(", "{}", "}{", "<>", "><" };

				foreach ( string arg in args )
				{
					if ( arg.Length == 2 && allowed.Contains( arg ) )
					{
						testchars.Add( arg );
					}
					else if ( !fileset && File.Exists( arg ) )
					{
						file = arg;
						fileset = true;
					}
					else
					{
						return ShowHelp( );
					}
				}
				if ( testchars.Count == 0 )
				{
					return ShowHelp( );
				}
				StreamReader sr = new StreamReader( file );
				string text = sr.ReadToEnd( );
				sr.Close( );
				Regex regex;
				foreach ( string testpair in testchars )
				{
					regex = new Regex( Regex.Escape( testpair[0].ToString( ) ) );
					int openchars = regex.Matches( text ).Count;
					regex = new Regex( Regex.Escape( testpair[1].ToString( ) ) );
					int closechars = regex.Matches( text ).Count;
					rc += Math.Abs( openchars - closechars );
					if ( openchars == closechars )
					{
						Console.WriteLine( "{0}\tOK\t({1} pairs)", testpair, openchars );
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine( "{0}\tERROR\t{1} x {2}, {3} x {4}", testpair, openchars, testpair[0], closechars, testpair[1] );
						Console.ResetColor( );
					}
				}
				return rc;
			}
			else
			{
				return ShowHelp( );
			}
		}

		static int ShowHelp( params string[] errmsg )
		{
			/*
			CountCharPairs.exe,  Version 1.00
			Count character pairs in source files, e.g. {} in a C# source file
			
			Usage:    CountCharPairs  sourcefile charpair  [ charpair  [ ... ] ]
			
			Where:    sourcefile      is the source file to be investigated
			          charpair        is a pair of characters to search for in the
			                          source code (allowed: "[]", "()", "{}" and "<>")
			
			The program will count the number of opening characters and the number of
			closing characters for each specified character pair.
			If the number of opening characters equals the number of closing characters,
			the program returns an "OK" message; if not, the numbers of occurences for
			each character will be shown.
			The program's return code equals the total number of mismatches, or 1 if
			this error screen is shown.
			
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

			Console.Error.WriteLine( "CountCharPairs.exe,  Version {0}", progver );

			Console.Error.WriteLine( "Count character pairs in source files, e.g. {} in a C# source file" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CountCharPairs  sourcefile charpair  [ charpair  [ ... ] ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:    " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "sourcefile" );
			Console.ResetColor( );
			Console.Error.WriteLine( "      is the source file to be investigated" );

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.Write( "          charpair" );
			Console.ResetColor( );
			Console.Error.WriteLine( "        is a pair of characters to search for in the" );

			Console.Error.WriteLine( "                          source code (allowed: \"[]\", \"()\", \"{}\" and \"<>\")" );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "The program will count the number of opening characters and the number of" );

			Console.Error.WriteLine( "closing characters for each specified character pair." );

			Console.Error.WriteLine( "If the number of opening characters equals the number of closing characters," );

			Console.Error.WriteLine( "the program returns an \"OK\" message; if not, the numbers of occurences for" );

			Console.Error.WriteLine( "each character will be shown." );

			Console.Error.WriteLine( "The program's return code equals the total number of mismatches, or 1 if" );

			Console.Error.WriteLine( "this error screen is shown." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}

	}
}
