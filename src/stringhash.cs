using System;

using System.Text;

using System.Security.Cryptography;

 

namespace StringHash

{

	class StringHash

	{

		static int Main(string[] args)

		{

			try

			{

				string hash = string.Empty;

				string str = string.Empty;

				string result;

 

				if (args.Length != 2)

				{

					return WriteError("Missing or invalid parameters");

				}

 

				foreach (string arg in args)

				{

					switch (arg.Substring(0, 3).ToUpper())

					{

						case "/A:":

						case "/H:":

							hash = arg.Substring(3).ToUpper();

							break;

						case "/S:":

							str = arg.Substring(3);

							break;

						default:

							return WriteError("Invalid parameter: " + arg);

					}

				}

 

				if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(str))

				{

					return WriteError("Missing required parameters");

				}

 

				HashAlgorithm ha;

				switch (hash)

				{

					case "MD5":

						ha = new MD5CryptoServiceProvider();

						break;

					case "SHA1":

					case "SHA-1":

						ha = new SHA1CryptoServiceProvider();

						break;

					case "SHA256":

					case "SHA-256":

						ha = new SHA256CryptoServiceProvider();

						break;

					case "SHA384":

					case "SHA-384":

						ha = new SHA384CryptoServiceProvider();

						break;

					case "SHA512":

					case "SHA-512":

						ha = new SHA512CryptoServiceProvider();

						break;

					default:

						return WriteError("Invalid hash type");

				}

				result = BitConverter.ToString(ha.ComputeHash(StrToByteArray(str)));

				ha.Clear();

 

				StringBuilder sb = new StringBuilder(result.ToLowerInvariant());

				Console.OpenStandardOutput();

				Console.WriteLine(sb.Replace("-", ""));

 

				return 0;

			}

			catch (Exception e)

			{

				return WriteError(e);

			}

		}

 

 

		// C# to convert a string to a byte array

		// http://www.chilkatsoft.com/faq/dotnetstrtobytes.html

		public static byte[] StrToByteArray(string instring)

		{

			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

			return encoding.GetBytes(instring);

		}

 

 

		public static int WriteError(Exception e)

		{

			return WriteError(e == null ? null : e.Message);

		}

 

 

		public static int WriteError(string errorMessage)

		{

			Console.OpenStandardError();

			if (string.IsNullOrEmpty(errorMessage) == false)

			{

				Console.WriteLine();

				Console.WriteLine("ERROR: {0}", errorMessage);

			}

			Console.WriteLine();

			Console.WriteLine("StringHash,  Version 1.00");

			Console.WriteLine("Get the MD5 or SHA* hash value for the specified string");

			Console.WriteLine();

			Console.WriteLine("Usage:  STRINGHASH  /A:hashAlgorithm  /S:\"string\"");

			Console.WriteLine();

			Console.WriteLine("Where:  hashAlgorithm  is either MD5, SHA1, SHA256, SHA384 or SHA512");

			Console.WriteLine("        string         must be enclosed in doublequotes if it contains spaces");

			Console.WriteLine();

			Console.WriteLine("Written by Rob van der Woude");

			Console.WriteLine("http://www.robvanderwoude.com");

			Console.OpenStandardOutput();

			return 1;

		}

	}

}

 