using System.Linq;


namespace RobvanderWoude
{
	public class CapsLockIcon : System.Windows.Forms.Form
	{
		static string progver = "1.02";
		static string copyrightsyear = "2018";

		#region Global Variables

		private System.Windows.Forms.NotifyIcon capslockicon;
		private System.Windows.Forms.ContextMenu contextmenu;
		private System.Windows.Forms.MenuItem menuitemexit;
		private System.Windows.Forms.MenuItem menuitemsettings;
		private System.Windows.Forms.Form formsettings;
		private System.Windows.Forms.TextBox textbox;
		private System.Windows.Forms.ComboBox dropdowncoloroff;
		private System.Windows.Forms.ComboBox dropdowncoloron;
		private System.Windows.Forms.CheckBox checkboxflashwhenoff;
		private System.Windows.Forms.CheckBox checkboxflashwhenon;
		private System.Collections.Generic.List<System.Drawing.Icon> iconsoff;
		private System.Collections.Generic.List<System.Drawing.Icon> iconson;
		private System.Windows.Forms.Timer timer;
		private System.Drawing.Brush black = System.Drawing.Brushes.Black;
		private System.Boolean capslock;

		#endregion Global Variables


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[System.STAThread]
		static void Main( )
		{
			System.Windows.Forms.Application.EnableVisualStyles( );
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault( false );
			System.Windows.Forms.Application.Run( new CapsLockIcon( ) );
		}


		public CapsLockIcon( )
		{
			ReadSettings( );
			capslockicon = new System.Windows.Forms.NotifyIcon( );
			// Determine which icon should be displayed
			iconsoff = new System.Collections.Generic.List<System.Drawing.Icon>( ) { CreateIcon( GlobalSettings.IndicatorText, black, GlobalSettings.IndicatorColorOFF ), CreateIcon( GlobalSettings.IndicatorText, GlobalSettings.IndicatorColorOFF, black ) };
			iconson = new System.Collections.Generic.List<System.Drawing.Icon>( ) { CreateIcon( GlobalSettings.IndicatorText, black, GlobalSettings.IndicatorColorON ), CreateIcon( GlobalSettings.IndicatorText, GlobalSettings.IndicatorColorON, black ) };
			capslock = System.Console.CapsLock;
			this.capslockicon.Icon = iconson[0];
			// Context Menu
			this.contextmenu = new System.Windows.Forms.ContextMenu( );
			// Context Menu: Settings
			this.menuitemsettings = new System.Windows.Forms.MenuItem( "&Settings", new System.EventHandler( MenuItemSettings_Click ) );
			this.contextmenu.MenuItems.Add( menuitemsettings );
			// Context Menu: Exit
			this.menuitemexit = new System.Windows.Forms.MenuItem( "E&xit", new System.EventHandler( MenuItemExit_Click ) );
			this.contextmenu.MenuItems.Add( menuitemexit );
			this.capslockicon.ContextMenu = this.contextmenu;
			this.capslockicon.Visible = true;
			// Timer for key monitoring interval
			this.timer = new System.Windows.Forms.Timer( );
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler( Timer_Tick );
			this.timer.Start( );
			// Store version information in the registry
			if ( ReadRegValue( "Version", System.String.Empty ) != progver )
			{
				WriteRegValue( "Version", progver );
				WriteRegValue( "URL", "https://www.robvanderwoude.com/csharpexamples.php#CapsLockIcon" );
				WriteRegValue( "Requirement", RequiredNetVersion( ) );
			}
		}


		/// <summary>
		/// Code to dynamically generate icons by Joshua Flanagan on CodeProject.com
		/// https://www.codeproject.com/Articles/7122/Dynamically-Generating-Icons-safely
		/// </summary>
		public System.Drawing.Icon CreateIcon( System.String text, System.Drawing.Brush fgcolor, System.Drawing.Brush bgcolor )
		{
			System.Drawing.Icon icon = null;
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( 16, 16 );
			System.Drawing.Font font = new System.Drawing.Font( System.Drawing.FontFamily.GenericSansSerif, 8F, System.Drawing.FontStyle.Bold );
			using ( System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage( bitmap ) )
			{
				graphic.FillEllipse( bgcolor, 0, 0, 16, 16 );
				System.Drawing.SizeF textsize = graphic.MeasureString( text, font );
				System.Single x = System.Convert.ToSingle( System.Math.Floor( ( bitmap.Width - textsize.Width ) / 2 ) );
				System.Single y = System.Convert.ToSingle( System.Math.Ceiling( ( bitmap.Height - textsize.Height ) / 2 ) );
				graphic.DrawString( text, font, fgcolor, x, y, System.Drawing.StringFormat.GenericDefault );
				icon = System.Drawing.Icon.FromHandle( bitmap.GetHicon( ) );
			}
			return icon;
		}


		public System.Drawing.Size GetTextSize( System.String text )
		{
			return System.Windows.Forms.TextRenderer.MeasureText( text, formsettings.Font );

		}


		public void OpenURL( string url )
		{
			System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo( url );
			System.Diagnostics.Process.Start( startinfo );
		}


		public void Quit( )
		{
			this.timer.Stop( );
			this.timer.Dispose( );
			this.capslockicon.Visible = false;
			this.capslockicon.Dispose( );
			System.Windows.Forms.Application.Exit( );
		}


		public void ReadSettings( )
		{
			GlobalSettings.FlashIfOFF = ReadRegValue( "FlashIfOFF", GlobalSettings.FlashIfOFF );
			GlobalSettings.FlashIfON = ReadRegValue( "FlashIfON", GlobalSettings.FlashIfON );
			System.String currentcolor = GlobalSettings.Colors.Where( c => c.Value.Equals( GlobalSettings.IndicatorColorOFF ) ).First( ).Key; // requires System.Linq
			GlobalSettings.IndicatorColorOFF = GlobalSettings.Colors[ReadRegValue( "IndicatorColorOFF", currentcolor )];
			currentcolor = GlobalSettings.Colors.Where( c => c.Value.Equals( GlobalSettings.IndicatorColorON ) ).First( ).Key; // requires System.Linq
			GlobalSettings.IndicatorColorON = GlobalSettings.Colors[ReadRegValue( "IndicatorColorON", currentcolor )];
			GlobalSettings.IndicatorText = ReadRegValue( "IndicatorText", GlobalSettings.IndicatorText );
		}


		/// <summary>
		/// Code to get the required .NET Framework version by Fernando Gonzalez Sanchez on StackOverflow.com
		/// https://stackoverflow.com/a/18623516
		/// </summary>
		static string RequiredNetVersion( )
		{
			object[] list = System.Reflection.Assembly.GetExecutingAssembly( ).GetCustomAttributes( true );
			var attribute = list.OfType<System.Runtime.Versioning.TargetFrameworkAttribute>( ).First( ); // requires Linq
			string frameworkname = attribute.FrameworkName;
			string frameworkdisplayname = attribute.FrameworkDisplayName;
			return frameworkdisplayname;
		}


		public void SaveSettings( )
		{
			// Adjust global settings
			GlobalSettings.FlashIfOFF = checkboxflashwhenoff.Checked;
			GlobalSettings.FlashIfON = checkboxflashwhenon.Checked;
			GlobalSettings.IndicatorColorOFF = GlobalSettings.Colors[dropdowncoloroff.Text];
			GlobalSettings.IndicatorColorON = GlobalSettings.Colors[dropdowncoloron.Text];
			if ( !System.String.IsNullOrWhiteSpace( textbox.Text ) )
			{
				GlobalSettings.IndicatorText = textbox.Text.Trim( );
			}
			// Update program status
			iconsoff = new System.Collections.Generic.List<System.Drawing.Icon>( ) { CreateIcon( GlobalSettings.IndicatorText, black, GlobalSettings.IndicatorColorOFF ), CreateIcon( GlobalSettings.IndicatorText, GlobalSettings.IndicatorColorOFF, black ) };
			iconson = new System.Collections.Generic.List<System.Drawing.Icon>( ) { CreateIcon( GlobalSettings.IndicatorText, black, GlobalSettings.IndicatorColorON ), CreateIcon( GlobalSettings.IndicatorText, GlobalSettings.IndicatorColorON, black ) };
			// Save settings to registry
			System.Boolean success = true;
			success = success && WriteRegValue( "FlashIfOFF", GlobalSettings.FlashIfOFF );
			success = success && WriteRegValue( "FlashIfON", GlobalSettings.FlashIfON );
			success = success && WriteRegValue( "IndicatorColorOFF", GlobalSettings.Colors.Where( c => c.Value.Equals( GlobalSettings.IndicatorColorOFF ) ).First( ).Key );
			success = success && WriteRegValue( "IndicatorColorON", GlobalSettings.Colors.Where( c => c.Value.Equals( GlobalSettings.IndicatorColorON ) ).First( ).Key );
			success = success && WriteRegValue( "IndicatorText", GlobalSettings.IndicatorText );
			if ( success )
			{
				System.Windows.Forms.MessageBox.Show( "Settings were successfully stored in the registry", "Settings Saved", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information );
			}
			else
			{
				System.Windows.Forms.MessageBox.Show( "Unable to store the settings in the registry", "Error Saving Settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error );
			}
			// Close settings window
			formsettings.Close( );
		}


		public void Settings( )
		{
			ReadSettings( );

			System.Int32 column1width = 0;
			System.Int32 column2width = 0;
			System.Int32 rowheight = 30;

			formsettings = new System.Windows.Forms.Form( );
			formsettings.Text = System.String.Format( "CapsLockIcon {0} Settings \u00A9 {1} Rob van der Woude", progver, copyrightsyear );
			formsettings.Size = new System.Drawing.Size( 500, 425 );
			formsettings.Font = new System.Drawing.Font( System.Drawing.FontFamily.GenericSansSerif, 10F );

			// Column 1 Row 1
			System.Windows.Forms.Label labeltext = new System.Windows.Forms.Label( );
			labeltext.Text = "Indicator text";
			column1width = System.Math.Max( column1width, GetTextSize( labeltext.Text ).Width );
			labeltext.Location = new System.Drawing.Point( 20, 20 );
			formsettings.Controls.Add( labeltext );

			// Column 1 Row 2
			System.Windows.Forms.Label labelcoloroff = new System.Windows.Forms.Label( );
			labelcoloroff.Text = "Indicator color when OFF";
			labelcoloroff.AutoSize = true;
			column1width = System.Math.Max( column1width, GetTextSize( labelcoloroff.Text ).Width );
			labelcoloroff.Location = new System.Drawing.Point( labeltext.Location.X, labeltext.Location.Y + labeltext.Height + rowheight );
			formsettings.Controls.Add( labelcoloroff );

			// Column 1 Row 3
			System.Windows.Forms.Label labelcoloron = new System.Windows.Forms.Label( );
			labelcoloron.Text = "Indicator color when ON";
			labelcoloron.AutoSize = true;
			column1width = System.Math.Max( column1width, GetTextSize( labelcoloron.Text ).Width );
			labelcoloron.Location = new System.Drawing.Point( labelcoloroff.Location.X, labelcoloroff.Location.Y + labelcoloroff.Height + rowheight );
			formsettings.Controls.Add( labelcoloron );

			// Column 2 Row 1
			textbox = new System.Windows.Forms.TextBox( );
			textbox.Text = GlobalSettings.IndicatorText;
			textbox.MaxLength = 2;
			textbox.SelectionStart = textbox.Text.Length;
			textbox.SelectionLength = 0;
			column2width = System.Math.Max( column2width, textbox.Width );
			textbox.Location = new System.Drawing.Point( column1width + 30, labeltext.Location.Y );
			formsettings.Controls.Add( textbox );

			ComboboxItem item;

			// Column 2 Row 2
			dropdowncoloroff = new System.Windows.Forms.ComboBox( );
			foreach ( System.String color in GlobalSettings.Colors.Keys )
			{
				item = new ComboboxItem( );
				item.Text = color;
				item.Value = GlobalSettings.Colors[color];
				dropdowncoloroff.Items.Add( item );
			}
			dropdowncoloroff.SelectedIndex = GlobalSettings.Colors.Values.ToList<System.Drawing.Brush>( ).IndexOf( GlobalSettings.IndicatorColorOFF ); // requires System.Linq
			column2width = System.Math.Max( column2width, dropdowncoloroff.Width );
			dropdowncoloroff.Location = new System.Drawing.Point( column1width + 30, labelcoloroff.Location.Y );
			formsettings.Controls.Add( dropdowncoloroff );

			// Column 2 Row 3
			dropdowncoloron = new System.Windows.Forms.ComboBox( );
			foreach ( System.String color in GlobalSettings.Colors.Keys )
			{
				item = new ComboboxItem( );
				item.Text = color;
				item.Value = GlobalSettings.Colors[color];
				dropdowncoloron.Items.Add( item );
			}
			dropdowncoloron.SelectedIndex = GlobalSettings.Colors.Values.ToList<System.Drawing.Brush>( ).IndexOf( GlobalSettings.IndicatorColorON ); // requires System.Linq
			column2width = System.Math.Max( column2width, dropdowncoloron.Width );
			dropdowncoloron.Location = new System.Drawing.Point( column1width + 30, labelcoloron.Location.Y );
			formsettings.Controls.Add( dropdowncoloron );

			// Column 3 Row 2
			checkboxflashwhenoff = new System.Windows.Forms.CheckBox( );
			checkboxflashwhenoff.Text = " Flash when OFF";
			checkboxflashwhenoff.AutoSize = true;
			checkboxflashwhenoff.Checked = GlobalSettings.FlashIfOFF;
			checkboxflashwhenoff.Location = new System.Drawing.Point( column1width + column2width + 50, dropdowncoloroff.Location.Y );
			formsettings.Controls.Add( checkboxflashwhenoff );

			// Column 3 Row 3
			checkboxflashwhenon = new System.Windows.Forms.CheckBox( );
			checkboxflashwhenon.Text = " Flash when ON";
			checkboxflashwhenon.AutoSize = true;
			checkboxflashwhenon.Checked = GlobalSettings.FlashIfON;
			checkboxflashwhenon.Location = new System.Drawing.Point( column1width + column2width + 50, dropdowncoloron.Location.Y );
			formsettings.Controls.Add( checkboxflashwhenon );

			// Buttons
			System.Windows.Forms.Button buttonsave = new System.Windows.Forms.Button( );
			buttonsave.Text = "Save";
			buttonsave.Click += new System.EventHandler( ButtonSave_Click );
			buttonsave.Size = new System.Drawing.Size( 100, 32 );
			buttonsave.Location = new System.Drawing.Point( formsettings.ClientSize.Width / 2 - buttonsave.Width - 20, labelcoloron.Location.Y + 2 * rowheight );
			formsettings.Controls.Add( buttonsave );

			System.Windows.Forms.Button buttoncancel = new System.Windows.Forms.Button( );
			buttoncancel.Text = "Cancel";
			buttoncancel.Click += new System.EventHandler( ButtonCancel_Click );
			buttoncancel.Size = new System.Drawing.Size( 100, 32 );
			buttoncancel.Location = new System.Drawing.Point( formsettings.ClientSize.Width / 2 + 20, buttonsave.Location.Y );
			formsettings.Controls.Add( buttoncancel );

			// URLs
			System.Windows.Forms.Label labelurlrvdw = new System.Windows.Forms.Label( );
			labelurlrvdw.Text = "Written by Rob van der Woude\nhttps://www.robvanderwoude.com";
			labelurlrvdw.Click += new System.EventHandler( LabelUrlRvdw_Click );
			labelurlrvdw.AutoSize = true;
			labelurlrvdw.Location = new System.Drawing.Point( 20, buttonsave.Location.Y + 2 * rowheight );
			formsettings.Controls.Add( labelurlrvdw );

			System.Windows.Forms.Label labelurljficos = new System.Windows.Forms.Label( );
			labelurljficos.Text = "Code to dynamically generate icons by Joshua Flanagan on CodeProject.com\nhttps://www.codeproject.com/Articles/7122/Dynamically-Generating-Icons-safely";
			labelurljficos.Font = new System.Drawing.Font( labelurljficos.Font.FontFamily, labelurljficos.Font.Size * 0.9F, labelurljficos.Font.Style );
			labelurljficos.Click += new System.EventHandler( LabelUrlJfIcos_Click );
			labelurljficos.AutoSize = true;
			labelurljficos.Location = new System.Drawing.Point( 20, System.Convert.ToInt32( labelurlrvdw.Location.Y + 1.5 * rowheight ) );
			formsettings.Controls.Add( labelurljficos );

			System.Windows.Forms.Label labelurlchw = new System.Windows.Forms.Label( );
			labelurlchw.Text = "Code to hide main form by Chriz on StackOverflow.com\nhttps://stackoverflow.com/a/11831856";
			labelurlchw.Font = new System.Drawing.Font( labelurlchw.Font.FontFamily, labelurlchw.Font.Size * 0.9F, labelurlchw.Font.Style );
			labelurlchw.Click += new System.EventHandler( LabelUrlChw_Click );
			labelurlchw.AutoSize = true;
			labelurlchw.Location = new System.Drawing.Point( 20, System.Convert.ToInt32( labelurljficos.Location.Y + 1.1 * rowheight ) );
			formsettings.Controls.Add( labelurlchw );

			System.Windows.Forms.Label labelurlfgs = new System.Windows.Forms.Label( );
			labelurlfgs.Text = "Code to get the required .NET Framework version by Fernando Gonzalez Sanchez\nhttps://stackoverflow.com/a/18623516";
			labelurlfgs.Font = new System.Drawing.Font( labelurlfgs.Font.FontFamily, labelurlfgs.Font.Size * 0.9F, labelurlfgs.Font.Style );
			labelurlfgs.Click += new System.EventHandler( LabelUrlFgs_Click );
			labelurlfgs.AutoSize = true;
			labelurlfgs.Location = new System.Drawing.Point( 20, System.Convert.ToInt32( labelurlchw.Location.Y + 1.1 * rowheight ) );
			formsettings.Controls.Add( labelurlfgs );

			formsettings.Show( );
		}


		#region Registry

		static System.Boolean ReadRegValue( System.String name, System.Boolean current )
		{
			System.Boolean value = false;
			Microsoft.Win32.RegistryKey regkey = null;
			try
			{
				regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( "SOFTWARE\\RobvanderWoude\\CapsLockIcon", Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree );
				value = ( regkey.GetValue( name, ( current ? 1 : 0 ) ).ToString( ) == "1" );
				regkey.Close( );
				return value;
			}
			catch ( System.Exception )
			{
				if ( regkey != null )
				{
					regkey.Close( );
				}
				return false;
			}
		}


		static System.String ReadRegValue( System.String name, System.String current )
		{
			System.String value = current;
			Microsoft.Win32.RegistryKey regkey = null;
			try
			{
				regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( "SOFTWARE\\RobvanderWoude\\CapsLockIcon", Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree );
				value = regkey.GetValue( name, current ).ToString( );
				regkey.Close( );
			}
			catch ( System.Exception )
			{
				if ( regkey != null )
				{
					regkey.Close( );
				}
			}
			return value;
		}


		static System.Boolean WriteRegValue( System.String name, System.Boolean value )
		{
			Microsoft.Win32.RegistryKey regkey = null;
			try
			{
				regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( "SOFTWARE\\RobvanderWoude\\CapsLockIcon", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree );
				regkey.SetValue( name, ( value ? 1 : 0 ), Microsoft.Win32.RegistryValueKind.DWord );
				regkey.Close( );
				return true;
			}
			catch ( System.Exception )
			{
				if ( regkey != null )
				{
					regkey.Close( );
				}
				return false;
			}
		}

		
		static System.Boolean WriteRegValue( System.String name, System.String value )
		{
			Microsoft.Win32.RegistryKey regkey = null;
			try
			{
				regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( "SOFTWARE\\RobvanderWoude\\CapsLockIcon", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree );
				regkey.SetValue( name, value, Microsoft.Win32.RegistryValueKind.String );
				regkey.Close( );
				return true;
			}
			catch ( System.Exception )
			{
				if ( regkey != null )
				{
					regkey.Close( );
				}
				return false;
			}
		}

		#endregion Registry


		#region Event Handlers

		private void ButtonCancel_Click( object sender, System.EventArgs e )
		{
			formsettings.Close( );
		}


		private void ButtonSave_Click( object sender, System.EventArgs e )
		{
			SaveSettings( );
		}


		private void LabelUrlChw_Click( object sender, System.EventArgs e )
		{
			OpenURL( "https://stackoverflow.com/a/11831856" );
		}


		private void LabelUrlFgs_Click( object sender, System.EventArgs e )
		{
			OpenURL( "https://stackoverflow.com/a/18623516" );
		}

		
		private void LabelUrlJfIcos_Click( object sender, System.EventArgs e )
		{
			OpenURL( "https://www.codeproject.com/Articles/7122/Dynamically-Generating-Icons-safely" );
		}


		private void LabelUrlRvdw_Click( object sender, System.EventArgs e )
		{
			OpenURL( "https://www.robvanderwoude.com/" );
		}


		private void MenuItemExit_Click( System.Object Sender, System.EventArgs e )
		{
			Quit( );
		}


		private void MenuItemSettings_Click( System.Object Sender, System.EventArgs e )
		{
			Settings( );
		}


		private void Timer_Tick( System.Object sender, System.EventArgs e )
		{
			capslock = System.Console.CapsLock;
			if ( capslock )
			{
				if ( GlobalSettings.FlashIfON )
				{
					if ( this.capslockicon.Icon.Equals( iconson[0] ) )
					{
						this.capslockicon.Icon = iconson[1];
					}
					else
					{
						this.capslockicon.Icon = iconson[0];
					}
				}
				else
				{
					this.capslockicon.Icon = iconson[0];
				}
			}
			else
			{
				if ( GlobalSettings.FlashIfOFF )
				{
					if ( this.capslockicon.Icon.Equals( iconsoff[0] ) )
					{
						this.capslockicon.Icon = iconsoff[1];
					}
					else
					{
						this.capslockicon.Icon = iconsoff[0];
					}
				}
				else
				{
					this.capslockicon.Icon = iconsoff[0];
				}
			}
		}

		#endregion Event Handlers


		#region Overrides

		/// <summary>
		/// Code to hide main form by Chriz on StackOverflow.com
		/// https://stackoverflow.com/a/11831856
		/// </summary>
		protected override void OnLoad( System.EventArgs e )
		{
			Visible = false; // Hide form window.
			ShowInTaskbar = false; // Remove from taskbar.
			Opacity = 0;
			base.OnLoad( e );
		}


		protected override void OnClosed( System.EventArgs e )
		{
			Quit( );
			base.OnClosed( e );
		}

		#endregion Overrides


		public static class GlobalSettings
		{
			private static System.Collections.Generic.Dictionary<string, System.Drawing.Brush> _colors = new System.Collections.Generic.Dictionary<string, System.Drawing.Brush>( ) { { "LightCyan", System.Drawing.Brushes.LightCyan }, { "LightGreen", System.Drawing.Brushes.LightGreen }, { "Orange", System.Drawing.Brushes.Orange }, { "Red", System.Drawing.Brushes.Red }, { "Yellow", System.Drawing.Brushes.Yellow } };
			public static System.Collections.Generic.Dictionary<string, System.Drawing.Brush> Colors
			{
				get
				{
					return _colors;
				}
			}

			private static string _indicatortext = "C";
			public static string IndicatorText
			{
				get
				{
					return _indicatortext;
				}
				set
				{
					_indicatortext = value;
				}
			}

			private static System.Boolean _flashifoff = false;
			public static System.Boolean FlashIfOFF
			{
				get
				{
					return _flashifoff;
				}
				set
				{
					_flashifoff = value;
				}
			}

			private static System.Boolean _flashifon = true;
			public static System.Boolean FlashIfON
			{
				get
				{
					return _flashifon;
				}
				set
				{
					_flashifon = value;
				}
			}

			private static System.Drawing.Brush _indicatorcoloroff = System.Drawing.Brushes.LightGreen;
			public static System.Drawing.Brush IndicatorColorOFF
			{
				get
				{
					return _indicatorcoloroff;
				}
				set
				{
					_indicatorcoloroff = value;
				}
			}

			private static System.Drawing.Brush _indicatorcoloron = System.Drawing.Brushes.Red;
			public static System.Drawing.Brush IndicatorColorON
			{
				get
				{
					return _indicatorcoloron;
				}
				set
				{
					_indicatorcoloron = value;
				}
			}
		}
	}


	public class ComboboxItem
	{
		public System.String Text
		{
			get; set;
		}

		public System.Object Value
		{
			get; set;
		}

		public override System.String ToString( )
		{
			return Text;
		}
	}
}
