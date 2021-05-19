using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace RobvanderWoude
{
    static class WPD
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int exitcode = 1;
            Application.EnableVisualStyles( );
            Application.SetCompatibleTextRenderingDefault( false );
            string[] arguments = System.Environment.GetCommandLineArgs( ).Skip( 1 ).ToArray( );
            if ( arguments.Length == 1 )
            {
                if ( arguments[0] == "/?" )
                {
                    string message = "WPD.exe, Version 1.00\n";
                    message += "Open a Word document with identical name and location\n";
                    message += "when doubeclicking a WordPerfect document\n\n";
                    message += "Usage:\n";
                    message += "  *\tRegister this program for WordPerfect files (*.wpd)\n";
                    message += "  *\tWhenever you doubleclick a WordPerfect file, this\n";
                    message += "   \tprogram will search for a Word document with the\n";
                    message += "   \tsame name and location, and open that Word\n";
                    message += "   \tdocument instead.\n";
                    message += "  *\tIf more than one matching Word file exists\n";
                    message += "   \t(e.g. *.doc as well as *.docx), a popup message\n";
                    message += "   \twill tell you to select the Word document directly.\n\n";
                    message += "Return code for this program is 0 if a single matching Word\n";
                    message += "document was found and opened, otherwise 1.\n\n";
                    message += "Written by Rob van der Woude\n";
                    message += "https://www.robvanderwoude.com";
                    string title = "Help for WPD.exe";
                    MessageBox.Show( message, title );
                }
                else
                {
                    string ext = Path.GetExtension( arguments[0].ToLower( ) );
                    string workingdir = Directory.GetParent( arguments[0] ).FullName;
                    string basename = Path.GetFileNameWithoutExtension( arguments[0] );
                    if ( ext.Substring( 0, 3 ) == ".wp" )
                    {
                        List<string> docfiles = Directory.GetFiles( workingdir, basename + ".doc*" ).ToList<string>( );
                        switch ( docfiles.Count )
                        {
                            case 1:
                                try
                                {
                                    _ = Process.Start( new ProcessStartInfo( docfiles[0] ) );
                                    exitcode = 0;
                                }
                                catch
                                {
                                    // ignore, exit code will be 1
                                }
                                break;
                            default:
                                MessageBox.Show( string.Format( "There are {0} files matching \"{1}.doc*\"", docfiles.Count, Path.Combine( workingdir, basename ) ), "Ambiguity" );
                                break;
                        }
                    }
                }
            }
            Environment.ExitCode = exitcode;
        }
    }
}
