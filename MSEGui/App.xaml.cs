using ControlzEx.Theming;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MSEGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void SetTheme()
        {
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                SetTheme();
            }
        }

        private static List<CultureInfo> m_Languages = new List<CultureInfo>();

        public static List<CultureInfo> Languages
        {
            get
            {
                return m_Languages;
            }
        }

        public App()
        {
            InitializeComponent();
            LanguageChanged += App_LanguageChanged;

            m_Languages.Clear();
            m_Languages.Add(new CultureInfo("en-US")); //Neutral culture for this project
            m_Languages.Add(new CultureInfo("ru-RU"));

            Language = MSEGui.Properties.Settings.Default.DefaultLanguage;

            SetTheme();

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }


        //Event for notifying all application windows
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Change the application language:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Create a ResourceDictionary for a new culture
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri(string.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                //3. Find the old ResourceDictionary and delete it and add a new ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.Contains("lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Call an event to notify all windows.
                LanguageChanged(Application.Current, new EventArgs());
            }
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            MSEGui.Properties.Settings.Default.DefaultLanguage = Language;
            MSEGui.Properties.Settings.Default.Save();
        }
    }
}
