    using System;

    using System.Collections.Generic;

    using System.Diagnostics;

    using System.Drawing;

    using System.IO;

    using System.Linq;

    using System.Runtime.InteropServices;

    using System.Text;

    using System.Text.RegularExpressions;

    using System.Timers;

    using System.Windows.Forms;

     

     

    namespace RobvanderWoude

    {

        static class InputBoxWF

        {

            /// <summary>

            /// The main entry point for the application.

            /// </summary>

     

            public const string progver = "1.00";

     

     

            #region Global Variables

     

            public const int defheight = 110;

            public const int deftimeout = 60;

            public const int defwidth = 200;

            public const string defaulttitle = "© 2019 Rob van der Woude";

     

            public static MaskedTextBox maskedtextbox;

            public static TextBox textbox;

            public static RegexOptions casesensitivity = RegexOptions.None;

            public static bool asciionly = false;

            public static bool filtered = true;

            public static bool oddrow = false;

            public static bool ontheflyregexset = false;

            public static bool password = false;

            public static bool timeoutelapsed = false;

            public static bool usemask = false;

            public static bool regexset = false;

            public static bool returnunmasked = false;

            public static bool sendoutputtoclipboard = false;

            public static string currentinput = String.Empty;

            public static string defanswer = "Default answer";

            public static string ontheflypattern = ".*";

            public static string outputfile = string.Empty;

            public static string previousinput = String.Empty;

            public static string regexpattern = ".*";

     

            #endregion Global Variables

     

     

            [STAThread]

            static void Main()

            {

                Application.EnableVisualStyles( );

                Application.SetCompatibleTextRenderingDefault( false );

                string[] arguments = Environment.GetCommandLineArgs( ).Skip( 1 ).ToArray( );

                InputBox( arguments );

            }

     

     

            [STAThread]

            static int InputBox( string[] args )

            {

                // Based on code by Gorkem Gencay on StackOverflow.com:

                // http://stackoverflow.com/questions/97097/what-is-the-c-sharp-version-of-vb-nets-inputdialog#17546909

     

                #region Initialize variables

     

                const string deftitle = "Title";

                const string deftext = "Prompt";

     

                bool heightset = false;

                bool showpassword = false;

                bool timeoutset = false;

                bool widthset = false;

                string input = string.Empty;

                string mask = String.Empty;

                string showpasswordprompt = "Show password";

                string text = deftext;

                string title = deftitle;

                int height = defheight;

                int timeout = 0;

                int width = defwidth;

                string cancelcaption = "&Cancel";

                string okcaption = "&OK";

                string localizationstring = String.Empty;

                bool localizedcaptionset = false;

     

                #endregion Initialize variables

     

     

                #region Command Line Parsing

     

                if ( args.Length == 0 )

                {

                    return ShowHelp( );

                }

     

                foreach ( string arg in args )

                {

                    if ( arg == "/?" )

                    {

                        return ShowHelp( );

                    }

                }

     

                text = String.Empty;

                title = String.Empty;

                defanswer = String.Empty;

     

                foreach ( string arg in args )

                {

                    if ( arg[0] == '/' )

                    {

                        if ( arg.Length == 1 )

                        {

                            return ShowHelp( );

                        }

                        else if ( arg.Length == 2 )

                        {

                            switch ( arg.ToString( ).ToUpper( ) )

                            {

                                case "/A":

                                    if ( asciionly )

                                    {

                                        return ShowHelp( "Duplicate command line switch /A" );

                                    }

                                    asciionly = true;

                                    break;

                                case "/C":

                                    if ( !string.IsNullOrWhiteSpace( outputfile ) )

                                    {

                                        return ShowHelp( "Command line switches /C and /E are mutually exclusive" );

                                    }

                                    if ( sendoutputtoclipboard )

                                    {

                                        return ShowHelp( "Duplicate command line switch /C" );

                                    }

                                    sendoutputtoclipboard = true;

                                    break;

                                case "/E":

                                    if ( sendoutputtoclipboard )

                                    {

                                        return ShowHelp( "Command line switches /C and /E are mutually exclusive" );

                                    }

                                    if ( !string.IsNullOrWhiteSpace( outputfile ) )

                                    {

                                        return ShowHelp( "Duplicate command line switch /E" );

                                    }

                                    sendoutputtoclipboard = false;

                                    outputfile = Path.Combine( Directory.GetParent( Application.ExecutablePath ).FullName, Path.GetFileNameWithoutExtension( Application.ExecutablePath ) + ".tmp" );

                                    break;

                                case "/I":

                                    if ( casesensitivity == RegexOptions.IgnoreCase )

                                    {

                                        return ShowHelp( "Duplicate command line switch /I" );

                                    }

                                    casesensitivity = RegexOptions.IgnoreCase;

                                    break;

                                case "/L":

                                    if ( localizedcaptionset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /L" );

                                    }

                                    localizedcaptionset = true;

                                    break;

                                case "/M":

                                    return HelpMessage( "mask" );

                                case "/N":

                                    if ( !filtered )

                                    {

                                        return ShowHelp( "Duplicate command line switch /N" );

                                    }

                                    filtered = false;

                                    break;

                                case "/P":

                                    if ( password )

                                    {

                                        return ShowHelp( "Duplicate command line switch /P" );

                                    }

                                    password = true;

                                    break;

                                case "/S":

                                    if ( showpassword )

                                    {

                                        return ShowHelp( "Duplicate command line switch /S" );

                                    }

                                    showpassword = true;

                                    break;

                                case "/T":

                                    if ( timeoutset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /T" );

                                    }

                                    timeout = deftimeout;

                                    timeoutset = true;

                                    break;

                                case "/U":

                                    if ( returnunmasked )

                                    {

                                        return ShowHelp( "Duplicate command line switch /U" );

                                    }

                                    returnunmasked = true;

                                    break;

                                default:

                                    return ShowHelp( "Invalid command line switch {0}", arg );

                            }

                        }

                        else if ( arg.Length > 3 && arg[2] == ':' )

                        {

                            switch ( arg.Substring( 0, 3 ).ToUpper( ) )

                            {

                                case "/E:":

                                    if ( sendoutputtoclipboard )

                                    {

                                        return ShowHelp( "Command line switches /C and /E are mutually exclusive" );

                                    }

                                    if ( !string.IsNullOrWhiteSpace( outputfile ) )

                                    {

                                        return ShowHelp( "Duplicate command line switch /E" );

                                    }

                                    sendoutputtoclipboard = false;

                                    outputfile = String.Format( "^{0}$", arg.Substring( 3 ) );

                                    break;

                                case "/F:":

                                    if ( ontheflyregexset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /F" );

                                    }

                                    ontheflypattern = String.Format( "^{0}$", arg.Substring( 3 ) );

                                    ontheflyregexset = true;

                                    break;

                                case "/H:":

                                    if ( heightset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /H" );

                                    }

                                    try

                                    {

                                        height = Convert.ToInt32( arg.Substring( 3 ) );

                                        if ( height < defheight || height > Screen.PrimaryScreen.Bounds.Height )

                                        {

                                            return ShowHelp( "Invalid screen height: \"{0}\"\n\tHeight must be an integer between {1} and {2} (screen height)", arg.Substring( 3 ), defheight.ToString( ), Screen.PrimaryScreen.Bounds.Height.ToString( ) );

                                        }

                                        heightset = true;

                                    }

                                    catch ( FormatException e )

                                    {

                                        return ShowHelp( "Invalid height: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );

                                    }

                                    break;

                                case "/L:":

                                    if ( localizedcaptionset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /L" );

                                    }

                                    localizedcaptionset = true;

                                    localizationstring = arg.Substring( 3 );

                                    break;

                                case "/M:":

                                    if ( usemask )

                                    {

                                        return ShowHelp( "Duplicate command line switch /M" );

                                    }

                                    mask = arg.Substring( 3 ).Trim( "\"".ToCharArray( ) );

                                    if ( String.IsNullOrWhiteSpace( mask ) )

                                    {

                                        ShowHelp( "No mask specified with /M" );

                                        Console.WriteLine( "\n\n" );

                                        return HelpMessage( "mask" );

                                    }

                                    usemask = true;

                                    break;

                                case "/R:":

                                    if ( regexset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /R" );

                                    }

                                    regexpattern = arg.Substring( 3 );

                                    regexset = true;

                                    break;

                                case "/S:":

                                    if ( showpassword )

                                    {

                                        return ShowHelp( "Duplicate command line switch /S" );

                                    }

                                    showpassword = true;

                                    showpasswordprompt = arg.Substring( 3 );

                                    break;

                                case "/T:":

                                    if ( timeoutset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /T" );

                                    }

                                    try

                                    {

                                        timeout = Convert.ToInt32( arg.Substring( 3 ) ) * 1000;

                                        if ( timeout < 1000 )

                                        {

                                            return ShowHelp( "Invalid timeout: \"{0}\"\n\tTimeout value must be a positive integer, at least 1.", arg.Substring( 3 ) );

                                        }

                                        timeoutset = true;

                                    }

                                    catch ( FormatException e )

                                    {

                                        return ShowHelp( "Invalid timeout: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );

                                    }

                                    break;

                                case "/W:":

                                    if ( widthset )

                                    {

                                        return ShowHelp( "Duplicate command line switch /W" );

                                    }

                                    try

                                    {

                                        width = Convert.ToInt32( arg.Substring( 3 ) );

                                        if ( width < defwidth || width > Screen.PrimaryScreen.Bounds.Width )

                                        {

                                            return ShowHelp( "Invalid screen width: \"{0}\"\n\tWidth must be an integer between {1} and {2} (screen width)", arg.Substring( 3 ), defwidth.ToString( ), Screen.PrimaryScreen.Bounds.Width.ToString( ) );

                                        }

                                        widthset = true;

                                    }

                                    catch ( FormatException e )

                                    {

                                        return ShowHelp( "Invalid width: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );

                                    }

                                    break;

                                default:

                                    return ShowHelp( "Invalid command line switch \"{0}\"", arg );

                            }

                        }

                        else

                        {

                            return ShowHelp( "Invalid command line argument \"{0}\"", arg );

                        }

                    }

                    else

                    {

                        if ( String.IsNullOrWhiteSpace( text ) )

                        {

                            text = arg;

                        }

                        else if ( String.IsNullOrWhiteSpace( title ) )

                        {

                            title = arg;

                        }

                        else if ( String.IsNullOrWhiteSpace( defanswer ) )

                        {

                            defanswer = arg;

                        }

                        else

                        {

                            return ShowHelp( "Invalid command line argument \"{0}\"", arg );

                        }

                    }

                }

     

                // Default title if none specified

                if ( String.IsNullOrWhiteSpace( title ) )

                {

                    title = defaulttitle;

                }

     

                // Switch /A requires /M

                if ( asciionly && !usemask )

                {

                    return ShowHelp( "Command line switch /A (ASCII only) can only be used together with /M" );

                }

     

                // Switch /S implies /P

                if ( showpassword )

                {

                    password = true;

                }

     

                // Set timer if /T:timeout was specified

                if ( timeoutset )

                {

                    System.Timers.Timer timer = new System.Timers.Timer( );

                    timer.Elapsed += new ElapsedEventHandler( Timer_Elapsed );

                    timer.Interval = timeout;

                    timer.Start( );

                }

     

                // For /S (Show password checkbox) add 25 px to window height unless height is specified

                if ( showpassword && !heightset )

                {

                    height += 25;

                }

     

                #endregion Command Line Parsing

     

     

                #region Set Localized Captions

     

                if ( localizedcaptionset )

                {

                    cancelcaption = Load( "user32.dll", 801, cancelcaption );

                    okcaption = Load( "user32.dll", 800, okcaption );

                    if ( !String.IsNullOrWhiteSpace( localizationstring ) )

                    {

                        string pattern = @"^((OK|Cancel)=[^;\""]*;)*((OK|Cancel)=[^;\""]*);?$";

                        Regex regex = new Regex( pattern, RegexOptions.IgnoreCase );

                        if ( regex.IsMatch( localizationstring ) )

                        {

                            string[] locstrings = localizationstring.Split( ";".ToCharArray( ) );

                            foreach ( string locstring in locstrings )

                            {

                                string key = locstring.Substring( 0, locstring.IndexOf( '=' ) );

                                string val = locstring.Substring( Math.Min( locstring.IndexOf( '=' ) + 1, locstring.Length - 1 ) );

                                if ( !String.IsNullOrWhiteSpace( val ) )

                                {

                                    switch ( key.ToUpper( ) )

                                    {

                                        case "OK":

                                            okcaption = val;

                                            break;

                                        case "CANCEL":

                                            cancelcaption = val;

                                            break;

                                        default:

                                            return ShowHelp( "Invalid localization key \"{0}\"", key );

                                    }

                                }

                            }

                        }

                        else

                        {

                            return ShowHelp( "Invalid localization string:\n\t{0}", localizationstring );

                        }

                    }

                }

     

                #endregion Set Localized Captions

     

     

                #region Define Form

     

                Size size = new Size( width, height );

                Form inputBox = new Form

                {

                    FormBorderStyle = FormBorderStyle.FixedDialog,

                    MaximizeBox = false,

                    MinimizeBox = false,

                    StartPosition = FormStartPosition.CenterParent,

                    ClientSize = size,

                    Text = title

                };

     

                Label labelPrompt = new Label

                {

                    Size = new Size( width - 20, height - 90 ),

                    Location = new Point( 10, 10 ),

                    Text = text.Replace( "\\n", "\n" )

                };

                inputBox.Controls.Add( labelPrompt );

     

                textbox = new TextBox

                {

                    Size = new Size( width - 20, 25 )

                };

                if ( showpassword )

                {

                    textbox.Location = new Point( 10, height - 100 );

                }

                else

                {

                    textbox.Location = new Point( 10, height - 75 );

                }

                if ( password )

                {

                    textbox.PasswordChar = '*';

                    if ( showpassword )

                    {

                        // Insert a checkbox with label "Show password" 25 px below the textbox

                        CheckBox checkbox = new CheckBox

                        {

                            Checked = false,

                            Location = new Point( 11, textbox.Location.Y + 25 ),

                            Width = inputBox.Width - 22

                        };

                        checkbox.Click += new EventHandler( Checkbox_Click );

                        checkbox.Text = showpasswordprompt;

                        inputBox.Controls.Add( checkbox );

                    }

                }

                else

                {

                    textbox.Text = defanswer;

                }

     

                maskedtextbox = new MaskedTextBox

                {

                    Mask = mask,

                    Location = textbox.Location,

                    PasswordChar = textbox.PasswordChar,

                    Text = textbox.Text,

                    TextMaskFormat = MaskFormat.ExcludePromptAndLiterals, // return only the raw input

                    Size = textbox.Size,

                    AsciiOnly = asciionly

                };

     

                if ( usemask )

                {

                    maskedtextbox.KeyUp += new KeyEventHandler( Maskedtextbox_KeyUp );

                    inputBox.Controls.Add( maskedtextbox );

                }

                else

                {

                    textbox.KeyUp += new KeyEventHandler( Textbox_KeyUp );

                    inputBox.Controls.Add( textbox );

                }

     

                Button okButton = new Button

                {

                    DialogResult = DialogResult.OK,

                    Name = "okButton",

                    Size = new Size( 80, 25 ),

                    Text = okcaption,

                    Location = new Point( width / 2 - 10 - 80, height - 40 )

                };

                inputBox.Controls.Add( okButton );

     

                Button cancelButton = new Button

                {

                    DialogResult = DialogResult.Cancel,

                    Name = "cancelButton",

                    Size = new Size( 80, 25 ),

                    Text = cancelcaption,

                    Location = new Point( width / 2 + 10, height - 40 )

                };

                inputBox.Controls.Add( cancelButton );

     

                inputBox.AcceptButton = okButton;  // OK on Enter

                inputBox.CancelButton = cancelButton; // Cancel on Esc

                inputBox.Activate( );

                inputBox.BringToFront( );

                inputBox.Focus( );

     

                if ( usemask )

                {

                    maskedtextbox.BringToFront( ); // Bug workaround

                    maskedtextbox.Select( 0, 0 ); // Move cursor to begin

                    maskedtextbox.Focus( );

                }

                else

                {

                    textbox.BringToFront( ); // Bug workaround

                    textbox.Select( 0, 0 ); // Move cursor to begin

                    textbox.Focus( );

                }

     

                #endregion Define Form

     

     

                #region Show Dialog and Return Result

     

                DialogResult result = inputBox.ShowDialog( );

                if ( result == DialogResult.OK )

                {

                    int rc = ValidateAndShowResult( );

                    return rc;

                }

                else

                {

                    if ( timeoutelapsed )

                    {

                        ValidateAndShowResult( );

                        return 3;

                    }

                    else

                    {

                        return 2;

                    }

                }

     

                #endregion Show Dialog and Return Result

            }

     

     

            #region Event Handlers

     

            public static void Checkbox_Click( object sender, System.EventArgs e )

            {

                // Toggle between hidden and normal text

                if ( usemask )

                {

                    if ( maskedtextbox.PasswordChar == '*' )

                    {

                        maskedtextbox.PasswordChar = '\0';

                    }

                    else

                    {

                        maskedtextbox.PasswordChar = '*';

                    }

                }

                else

                {

                    if ( textbox.PasswordChar == '*' )

                    {

                        textbox.PasswordChar = '\0';

                    }

                    else

                    {

                        textbox.PasswordChar = '*';

                    }

                }

            }

     

     

            private static void Maskedtextbox_KeyUp( object sender, KeyEventArgs e )

            {

                maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

                currentinput = maskedtextbox.Text;

                if ( Regex.IsMatch( currentinput, ontheflypattern, casesensitivity ) )

                {

                    previousinput = currentinput;

                }

                else

                {

                    currentinput = previousinput;

                }

                if ( maskedtextbox.Text != currentinput )

                {

                    maskedtextbox.Text = currentinput;

                    maskedtextbox.TextMaskFormat = MaskFormat.IncludeLiterals;

                    if ( currentinput.Length > 0 )

                    {

                        maskedtextbox.SelectionStart = maskedtextbox.Text.LastIndexOf( currentinput.Last<char>( ) ) + 1;

                    }

                    else

                    {

                        maskedtextbox.SelectionStart = 0;

                    }

                    maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

                }

            }

     

     

            private static void Textbox_KeyUp( object sender, KeyEventArgs e )

            {

                currentinput = textbox.Text;

                if ( Regex.IsMatch( currentinput, ontheflypattern, casesensitivity ) )

                {

                    previousinput = currentinput;

                }

                else

                {

                    currentinput = previousinput;

                }

                if ( textbox.Text != currentinput )

                {

                    textbox.Text = currentinput;

                    textbox.SelectionStart = currentinput.Length;

                }

            }

     

     

            public static void Timer_Elapsed( object sender, System.EventArgs e )

            {

                timeoutelapsed = true;

                Process.GetCurrentProcess( ).CloseMainWindow( );

            }

     

            #endregion Event Handlers

     

     

            public static int HelpMessage( string subject )

            {

                switch ( subject.ToLower( ) )

                {

                    case "mask":

                        string message = "The mask \"language\" is based on the Masked Edit control in Visual Basic 6.0:\n";

                        message+= "http://msdn.microsoft.com/en-us/library/system.windows.forms.maskedtextbox.mask.aspx#remarksToggle\n\n";

                        message += "Masking element     \tDescription\n";

                        message += "===========     \t========\n";

                        message += "0                                  \tDigit, required. This element will accept\n";

                        message+=  "                                   \tany single digit between 0 and 9.\n";

                        message += "9                                  \tDigit or space, optional.\n";

                        message += "#                                  \tDigit or space, optional. If this position\n";

                        message += "                                   \tis blank in the mask, it will be rendered\n";

                        message += "                                   \tas a space in the Text property. Plus (+)\n";

                        message += "                                   \tand minus (-) signs are allowed.\n";

                        message += "L                                  \tLetter, required. Restricts input to the\n";

                        message += "                                   \tASCII letters a-z and A-Z. This mask\n";

                        message += "                                   \telement is equivalent to [a-zA-Z] in\n";

                        message += "                                   \tregular expressions.\n";

                        message += "?                                  \tLetter, optional. Restricts input to the\n";

                        message += "                                   \tASCII letters a-z and A-Z. This mask\n";

                        message += "                                   \telement is equivalent to [a-zA-Z]? in\n";

                        message += "                                   \tregular expressions.\n";

                        message += "&                                  \tCharacter, required. Any non-control\n";

                        message += "                                   \tcharacter. If ASCII only is set (/A),\n";

                        message += "                                   \tthis element behaves like the \"A\" element.\n";

                        message += "C                                  \tCharacter, optional. Any non-control\n";

                        message += "                                   \tcharacter. If ASCII only is set (/A),\n";

                        message += "                                   \tthis element behaves like the \"a\" element.\n";

                        message += "A                                  \tAlphanumeric, required. If ASCII only is\n";

                        message += "                                   \tset (/A), the only characters it will\n";

                        message += "                                   \taccept are the ASCII letters a-z and\n";

                        message += "                                   \tA-Z and numbers. This mask element\n";

                        message += "                                   \tbehaves like the \"&\" element.\n";

                        message += "a                                  \tAlphanumeric, optional. If ASCII only is\n";

                        message += "                                   \tset (/A), the only characters it will\n";

                        message += "                                   \taccept are the ASCII letters a-z and\n";

                        message += "                                   \tA-Z and numbers. This mask element\n";

                        message += "                                   \tbehaves like the \"C\" element.\n";

                        message += ".                                  \tDecimal placeholder.\n";

                        message += ",                                  \tThousands placeholder.\n";

                        message += ":                                  \tTime separator.\n";

                        message += "/                                  \tDate separator.\n";

                        message += "$                                  \tCurrency symbol.\n";

                        message += "<                                  \tShift down. Converts all characters\n";

                        message += "                                   \tthat follow to lowercase.\n";

                        message += ">                                  \tShift up. Converts all characters\n";

                        message += "                                   \tthat follow to uppercase.\n";

                        message += "|                                  \tDisable a previous shift up or shift down.\n";

                        message += "\\                                  \tEscape. Escapes a mask character, turning\n";

                        message += "                                   \tit into a literal. \"\\\\\" is the escape sequence\n";

                        message += "                                   \tfor a backslash.\n";

                        message += "All other characters\tLiterals. All non-mask elements will appear\n";

                        message += "                                   \tas themselves within MaskedTextBox.\n";

                        message += "                                   \tLiterals always occupy a static position\n";

                        message += "                                   \tin the mask at run time, and cannot be\n";

                        message += "                                   \tmoved or deleted by the user.\n";

                        MessageBox.Show( message, "Help for command line switch /M:mask" );

                        break;

                    default:

                        return ShowHelp( );

                }

                return 1;

            }

     

     

            public static int ShowHelp( params string[] errmsg )

            {

                /*

    			InputBoxWF,  Version 1.00

    			Prompt for input (Windows Form edition)

     

    			Usage:   INPUTBOXWF  [ "prompt"  [ "title"  [ "default" ] ] ] [ options ]

     

    			Where:   "prompt"    is the text above the input field (use \n for new line)

    			         "title"     is the caption in the title bar

    			         "default"   is the default answer shown in the input field

     

    			Options: /A          accepts ASCII characters only (requires /M)

    			         /C          send input to Clipboard (default: send to file)

    			         /E[:file]   send input to filE (default file name: InputBoxWF.tmp)

    			         /F:regex    use regex to filter input on-the-Fly (see Notes)

    			         /H:height   sets the Height of the input box

    			                     (default: 110; minimum: 110; maximum: screen height)

    			         /I          regular expressions are case Insensitive

    			                     (default: regular expressions are case sensitive)

    			         /L[:string] use Localized or custom captions (see Notes)

    			         /M:mask     accept input only if it matches mask (template)

    			         /N          Not filtered, only doublequotes are removed from input

    			                     (default: remove & < > | ")

    			         /P          hides (masks) the input text (for Passwords)

    			         /R:regex    accept input only if it matches Regular expression regex

    			         /S[:text]   inserts a checkbox "Show password" (or specified text)

    			         /T[:sec]    sets the optional Timeout in seconds (default: 60)

    			         /U          return Unmasked input, without literals (requires /M)

    			                     (default: include literals in result)

    			         /W:width    sets the Width of the input box

    			                     (default: 200; minimum: 200; maximum: screen width)

     

    			Example: prompt for password

    			InputBox.exe "Enter your password:" "Login" /S

     

    			Example: fixed length hexadecimal input (enter as a single command line)

    			InputBox.exe "Enter a MAC address:" "MAC Address" "0022446688AACCEE"

    			             /M:">CC\:CC\:CC\:CC\:CC\:CC" /R:"[\dA-F]{16}"

    			             /F:"[\dA-F]{1,16}" /U /I

     

    			Notes:   For hidden input (/P and/or /S), "default" will be ignored.

    			         With /F, regex must test the unmasked input (without literals), e.g.

    			         /M:"CC:CC:CC:CC:CC:CC:CC:CC" /F:"[\dA-F]{0,16} /I" for MAC address.

    			         With /R, regex is used to test input after OK is clicked;

    			         with /F, regex is used to test input each time the input

    			         changes, so regex must be able to cope with partial input;

    			         e.g. /F:"[\dA-F]{0,16}" is OK, but /F:"[\dA-F]{16}" will fail.

    			         or redirect the result to a (temporary) file.

    			         Show password (/S) implies hiding the input text (/P).

    			         Use /M (without mask) to show detailed help on the mask language.

    			         Use /L for Localized "OK" and "Cancel" button captions.

    			         Custom captions require a string like /L:"OK=caption;Cancel=caption"

    			         (button=caption pairs separated by semicolons, each button optional).

    			         Text from input is written to clipboard or file only if "OK" is clicked.

    			         Return code is 0 for "OK", 1 for (command line) errors, 2 for

    			         "Cancel", 3 on timeout, 4 if no regex or mask match.

     

    			Credits: On-the-fly form based on code by Gorkem Gencay on StackOverflow:

    			         http://stackoverflow.com/questions/97097#17546909

    			         Code to retrieve localized button captions by Martin Stoeckli:

    			         http://martinstoeckli.ch/csharp/csharp.html#windows_text_resources

     

    			Written by Rob van der Woude

    			http://www.robvanderwoude.com

    			*/

     

                string message = "\n";

     

                if ( errmsg.Length > 0 )

                {

                    List<string> errargs = new List<string>( errmsg );

                    errargs.RemoveAt( 0 );

                    message += string.Format( "ERROR:\t{0} {1}\n", errmsg[0], string.Join( " ", errargs.ToArray( ) ) );

                }

     

                message += string.Format( "InputBoxWF,  Version {0}\n", progver );

                message += "Prompt for input (Windows Form edition)\n\n";

                message += "Usage:\tINPUTBOXWF  [ \"prompt\"  [ \"title\"  [ \"default\" ] ] ] [ options ]\n\n";

                message += "Where:\t\"prompt\"\t\tis the text above the input field\n";

                message += "         \t       \t\t(use \\n for new line)\n";

                message += "         \t\"title\"\t\tis the caption in the title bar\n";

                message += "         \t\"default\"\t\tis the default answer shown in\n";

                message += "          \t      \t\tthe input field\n\n";

                message += "Options:\t/A\t\taccepts ASCII characters only (requires /M)\n";

                message += "         \t/F:regex\t\tFilter input on-the-Fly with regex (see Note)\n";

                message += "         \t/H:height\tsets the Height of the input box\n";

                message += string.Format( "          \t      \t\t(default: {0}; minimum: {0};\n", defheight );

                message += "         \t       \t\tmaximum: screen height)\n";

                message += "         \t/I\t\tregular expressions are case Insensitive\n";

                message += "         \t       \t\t(default: regular expressions are case\n";

                message += "         \t       \t\tsensitive)\n";

                message += "         \t/L[:string]\tLocalized or custom captions (see Note)\n";

                message += "         \t/M:mask\t\taccept input only if it Matches mask\n";

                message += "         \t/N\t\tNot filtered, only doublequotes are\n";

                message += "         \t       \t\tremoved from input\n";

                message += "         \t       \t\t(default: remove & < > | \")\n";

                message += "         \t/R:regex\t\taccept input only if it matches Regular\n";

                message += "         \t       \t\texpression regex\n";

                message += "         \t/S[:text]\t\tinserts a checkbox \"Show password\"\n";

                message += "         \t       \t\t(or specified text)\n";

                message += "         \t/T[:sec]\t\tsets the optional Timeout in seconds\n";

                message += string.Format( "         \t       \t\t( default: {0})\n", deftimeout );

                message += "         \t/U\t\treturn Unmasked input, without literals\n";

                message += "         \t       \t\t(requires /M; default: include literals)\n";

                message += "         \t/W:width\tsets the Width of the input box\n";

                message += string.Format( "         \t       \t\t(default: {0}; minimum: {0};\n", defwidth );

                message += "         \t       \t\tmaximum: screen width)\n\n";

     

                message += "Written by Rob van der Woude\n";

     

                message += "http://www.robvanderwoude.com";

     

                MessageBox.Show( message, "Help for InputBoxWF.exe (part 1)" );

     

                message = "Example:\tprompt for password, send to clipboard\n";

                message += "\tInputBoxWF.exe \"Enter your password:\" \"Login\" /S /C\n\n";

     

                message += "Example:\tfixed length hex input (enter as a single command line)\n";

                message += "\tInputBoxWF.exe \"Enter a MAC address:\" \"MAC Address\"\n";

                message += "\t\"0022446688AACCEE\" /M:\">CC\\:CC\\:CC\\:CC\\:CC\\:CC\\:CC\\:CC\"\n";

                message += "\t/R:\"[\\dA-F]{16}\" /F:\"[\\dA-F]{0,16}\" /U /I /C\n\n";

     

                message += "Notes:\tFor hidden input (/P and/or /S), \"default\" will be ignored.\n\n";

     

                message += "       \tWith /F, regex must test the unmasked input (without literals),\n";

                message += "       \te.g. /M:\"CC:CC:CC:CC:CC:CC:CC:CC\" /F:\"[\\dA-F]{0,16}\" /I\n";

                message += "       \tfor MAC address.\n\n";

     

                message += "        \tWith /R, regex is used to test input after OK is clicked;\n";

                message += "        \twith /F, regex is used to test input each time the input\n";

                message += "        \tchanges, so  regex must be able to cope with partial input;\n";

                message += "        \te.g. /F:\"[\\dA-F]{0,16}\" is OK, but /F:\"[\\dA-F]{16}\" will fail.\n\n";

     

                message += "        \tBe careful with /N, use doublequotes for the result.\n\n";

     

                message += "        \tShow password (/S) implies hiding the input text (/P).\n\n";

     

                message += "        \tUse /M (without mask value) to show detailed help on\n";

                message += "        \tthe mask language.\n\n";

     

                message += "        \tUse /L for Localized \"OK\" and \"Cancel\" button captions.\n";

     

                message += "        \tCustom captions require a string like\n";

                message += "        \t / L:\"OK=caption;Cancel=caption\"\n";

                message += "        \t(button=caption pairs separated by semicolons,\n";

                message += "        \teach button optional).\n\n";

     

                message += "        \tText from input is written to clipboard or file\n";

                message += "        \tonly if \"OK\" is clicked.\n\n";

     

                message += "        \tReturn code is 0 for \"OK\", 1 for (command line) errors,\n";

                message += "        \t2 for \"Cancel\", 3 on timeout, 4 if no regex or mask match.\n\n";

     

                message += "Written by Rob van der Woude\n";

     

                message += "http://www.robvanderwoude.com";

     

                MessageBox.Show( message, "Help for InputBoxWF.exe (part 2)" );

     

                return 1;

            }

     

     

            public static int ValidateAndShowResult()

            {

                string input = String.Empty;

                // Read input from MaskedTextBox or TextBox

                if ( usemask )

                {

                    if ( returnunmasked )

                    {

                        maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

                    }

                    else

                    {

                        maskedtextbox.TextMaskFormat = MaskFormat.IncludeLiterals;

                    }

                    input = maskedtextbox.Text;

                    // Check if input complies with mask

                    if ( !maskedtextbox.MaskCompleted )

                    {

                        return 4;

                    }

                }

                else

                {

                    input = textbox.Text;

                }

     

                // Check if input complies with regex

                if ( regexset && Regex.IsMatch( input, regexpattern, casesensitivity ) )

                {

                    return 4;

                }

     

                // Remove ampersands and redirection symbols unless /N switch was used

                if ( filtered )

                {

                    input = Regex.Replace( input, @"[&<>|]", String.Empty );

                }

     

                // Remove doublequotes from output

                input = input.Replace( "\"", "" );

     

                // Send result to clipboard or file

                if ( sendoutputtoclipboard )

                {

                    Clipboard.SetText( input );

                }

                else

                {

                    using ( StreamWriter sw = new StreamWriter( outputfile, false ) )

                    {

                        sw.Write( input );

                    }

                }

                return 0;

            }

     

     

            #region Get Localized Captions

     

            // Code to retrieve localized captions by Martin Stoeckli

            // http://martinstoeckli.ch/csharp/csharp.html#windows_text_resources

     

            /// <summary>

            /// Searches for a text resource in a Windows library.

            /// Sometimes, using the existing Windows resources, you can make your code

            /// language independent and you don't have to care about translation problems.

            /// </summary>

            /// <example>

            ///   btnCancel.Text = Load("user32.dll", 801, "Cancel");

            ///   btnYes.Text = Load("user32.dll", 805, "Yes");

            /// </example>

            /// <param name="libraryName">Name of the windows library like "user32.dll"

            /// or "shell32.dll"</param>

            /// <param name="ident">Id of the string resource.</param>

            /// <param name="defaultText">Return this text, if the resource string could

            /// not be found.</param>

            /// <returns>Requested string if the resource was found,

            /// otherwise the <paramref name="defaultText"/></returns>

            public static string Load( string libraryName, UInt32 ident, string defaultText )

            {

                IntPtr libraryHandle = GetModuleHandle( libraryName );

                if ( libraryHandle != IntPtr.Zero )

                {

                    StringBuilder sb = new StringBuilder( 1024 );

                    int size = LoadString( libraryHandle, ident, sb, 1024 );

                    if ( size > 0 )

                        return sb.ToString( );

                }

                return defaultText;

            }

     

            [DllImport( "kernel32.dll", CharSet = CharSet.Auto )]

            private static extern IntPtr GetModuleHandle( string lpModuleName );

     

            [DllImport( "user32.dll", CharSet = CharSet.Auto )]

            private static extern int LoadString( IntPtr hInstance, UInt32 uID, StringBuilder lpBuffer, Int32 nBufferMax );

     

            #endregion Get Localized Captions

        }

     

    }

     

