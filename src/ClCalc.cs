using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Threading;


namespace RobvanderWoude
{
	class ClCalc
	{
		static int Main( string[] args )
		{
			if ( args.Length == 0 )
			{
				return WriteError( String.Empty );
			}
			string expression = string.Empty;
			foreach ( string arg in args )
			{
				if ( arg == "/?" )
				{
					return WriteError( String.Empty );
				}
				else
				{
					expression += " " + arg;
				}
			}
			expression = expression.Trim( );
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo( "en-US" );
				Double result = JScriptEvaluator.EvalToDouble( expression );
				Console.Error.Write( "{0} = ", expression );
				Console.WriteLine( "{0}", result );
				try
				{
					return Convert.ToInt32( result );
				}
				catch ( Exception )
				{
					return 0;
				}

			}
			catch ( Exception e )
			{
				return WriteError( e.Message );
			}

		}

		public static int WriteError( Exception e )
		{
			return WriteError( e == null ? null : e.Message );
		}

		public static int WriteError( string errorMessage )
		{
			/*
			ClCalc,  Version 1.01
			Command Line Calculator

			Usage:  CLCALC  expression

			Notes:  Result is displayed on screen and returned as exit code ("errorlevel").
			        Exit code is integer value of result or 0 in case of error or overflow.
			        Result is displayed in Standard Output stream, expression in Standard
			        Error stream, to allow capturing or redirection of result value only.
			        "Culture" is set to "en-US", so use and expect decimal dots, no commas.
			        Based on Eval function (using JScript) by "markman":
			        www.codeproject.com/Articles/11939/Evaluate-C-Code-Eval-Function

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
			Console.Error.WriteLine( "ClCalc,  Version 1.01" );
			Console.Error.WriteLine( "Command Line Calculator" );
			Console.Error.WriteLine( );
			Console.Error.Write( "Usage:  " );
			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine( "CLCALC  expression" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Notes:  Result is displayed on screen and returned as exit code (\"errorlevel\")." );
			Console.Error.WriteLine( "        Exit code is integer value of result or 0 in case of error or overflow." );
			Console.Error.WriteLine( "        Result is displayed in Standard Output stream, expression in Standard" );
			Console.Error.WriteLine( "        Error stream, to allow capturing or redirection of result value only." );
			Console.Error.WriteLine( "        Culture is set to \"en-US\", so use and expect decimal dots, not commas." );
			Console.Error.WriteLine( "        Based on Eval function (using JScript) by \"markman\":" );
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "        www.codeproject.com/Articles/11939/Evaluate-C-Code-Eval-Function" );
			Console.ResetColor( );
			Console.Error.WriteLine( );
			Console.Error.WriteLine( "Written by Rob van der Woude" );
			Console.Error.WriteLine( "http://www.robvanderwoude.com" );
			return 0;
		}
	}


	// Eval function using JScript, by "markman"
	// http://www.codeproject.com/Articles/11939/Evaluate-C-Code-Eval-Function

	public class JScriptEvaluator
	{
		public static int EvalToInteger( string statement )
		{
			string s = EvalToString( statement );
			return int.Parse( s.ToString( ) );
		}

		public static double EvalToDouble( string statement )
		{
			string s = EvalToString( statement );
			return double.Parse( s );
		}

		public static string EvalToString( string statement )
		{
			object o = EvalToObject( statement );
			return o.ToString( );
		}

		public static object EvalToObject( string statement )
		{
			return _evaluatorType.InvokeMember(
							  "Eval",
							  BindingFlags.InvokeMethod,
							  null,
							  _evaluator,
							  new object[] { statement }
						);
		}

		static JScriptEvaluator( )
		{
			CodeDomProvider provider = new Microsoft.JScript.JScriptCodeProvider( );

			CompilerParameters parameters;
			parameters = new CompilerParameters( );
			parameters.GenerateInMemory = true;

			CompilerResults results;
			results = provider.CompileAssemblyFromSource( parameters, _jscriptSource );

			Assembly assembly = results.CompiledAssembly;
			_evaluatorType = assembly.GetType( "Evaluator.Evaluator" );

			_evaluator = Activator.CreateInstance( _evaluatorType );
		}

		private static object _evaluator = null;
		private static Type _evaluatorType = null;
		private static readonly string _jscriptSource =
			  @"package Evaluator
                  {
                     class Evaluator
                     {
                           public function Eval(expr : String) : String
                           {
                              return eval(expr);
                           }
                     }
                  }";
	}
}
