using System;
using System.Threading;

///<summary>
/// A time delay of 900..1100 milliseconds
///</summary>

namespace RobvanderWoude
{
	class WaitASecond
	{
		static void Main( string[] args )
		{
			// Choose a random delay between 900 and 1100 milliseconds
			Random randomNums = new Random( );
			int delay = randomNums.Next( 900, 1100 );
			// Display the actual time delay if ANY command line parameter was passed
			if ( args.Length > 0 )
			{
				Console.WriteLine( "waiting {0} milliseconds . . .", delay );
			}
			// Now wait...
			Thread.Sleep( delay );
		}
	}
}
