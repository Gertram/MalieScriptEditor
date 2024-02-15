using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using MahApps.Metro.Controls;
using MahApps.Metro;
using Newtonsoft.Json;
using System.Security.Cryptography;
using ControlzEx.Standard;
using System.Xml.Serialization;
using MSELib;
using System.Globalization;
using MSEGui.Config;
using ConfigLib;

namespace MSEGui
{
    internal enum SelectedTab
    {
        Strings,
        Others
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged, IFindable
    {
        private FindWindow findWindow;
        private List<StringsListItem> strings;
        private List<ContentsListItem> contents;
        private SelectedTab SelectedTab { get; set; } = SelectedTab.Strings;
        private bool IsChanged { get; set; } = false;

        private Dat script;
        private string fileName;
        private bool onlyJapanese;
        public bool OnlyJapanese
        {
            get => onlyJapanese;
            set
            {
                onlyJapanese = value;
                Config.OnlyJapanese = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnlyJapanese)));
            }
        }
        public Dat Script
        {
            get => script;
            set
            {
                script = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Script)));
            }
        }
        private string FileName
        {
            get => fileName;

            set
            {
                fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }
        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            }
        }
        private UserConfig Config { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguages();
            LoadConfig();
            PropertyChanged += MainWindow_PropertyChanged;
        }
        public List<StringsListItem> Strings
        {
            get => strings; 
            private set
            {
                strings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Strings)));
            }
        }
        public List<ContentsListItem> Contents
        {
            get => contents; 
            private set
            {
                contents = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Contents)));
            }
        }
        private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(Script))
                {
                    if(Script == null)
                    {
                        Strings = null;
                        Contents = null;
                        return;
                    }
                    var strings = new List<StringsListItem>();

                    foreach (var (lineItem,index) in Script.Strings.Select((x,index)=>(x,index)))
                    {
                        foreach (var item in lineItem.VisibleTexts)
                        {
                            strings.Add(new StringsListItem(index, item));
                        }
                    }
                    var contents = new List<ContentsListItem>();

                    foreach (var (contentItem,index) in Script.ContentItems.Select((x,index)=>(x,index)))
                    {
                        foreach (var item in contentItem.Texts)
                        {
                            contents.Add(new ContentsListItem(index,contentItem.Title, item)
                            {
                                OnlyJapanese=OnlyJapanese
                            });
                        }
                    }

                    Strings = strings;
                    Contents = contents;
                }
                else if (e.PropertyName == nameof(OnlyJapanese))
                {
                    contents.ForEach(x => x.OnlyJapanese = OnlyJapanese);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex.ToString());
            }
        }

        private void InitializeLanguages()
        {
            try
            {
                App.LanguageChanged += App_LanguageChanged; ; ;

                CultureInfo currLang = App.Language;

                //Filling out the language change menu:
                menuLanguage.Items.Clear();
                foreach (var lang in App.Languages)
                {
                    MenuItem menuLang = new MenuItem
                    {
                        Header = lang.DisplayName,
                        Tag = lang,
                        IsChecked = lang.Equals(currLang)
                    };
                    menuLang.Click += MenuLang_Click; ; ;
                    menuLanguage.Items.Add(menuLang);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex.ToString());
            }
        }

        private void MenuLang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem mi)
                {
                    if (mi.Tag is CultureInfo lang)
                    {
                        App.Language = lang;
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(ex.ToString());
            }
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;
            Config.Language = currLang.Name;

            //Mark the desired language change item as the selected language
            foreach (MenuItem i in menuLanguage.Items)
            {
                i.IsChecked = i.Tag is CultureInfo ci && ci.Equals(currLang);
            }
        }

        private bool LoadConfig()
        {
            try
            {
                Config = new UserConfig();
                if (Config.Language != null)
                {
                    Language = System.Windows.Markup.XmlLanguage.GetLanguage(Config.Language);
                }
                OnlyJapanese = Config.OnlyJapanese;
                Config.PropertyChanged += Config_PropertyChanged;
                return true;
            }
            catch (Exception exp)
            {
                ReportError(exp.ToString());
                return false;
            }
        }
        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Language")
            {
                return;
            }
            Language = System.Windows.Markup.XmlLanguage.GetLanguage(Config.Language);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool AskSave()
        {
            if (!IsChanged)
            {
                return true;
            }
            var res = MessageBox.Show(this, this.GetResourceString("m_UnsavedChanges"), this.GetResourceString("m_Attention"), MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                return Save();
            }
            return res != MessageBoxResult.Cancel;
        }
        private void OpenFile(string fileName)
        {
            try
            {
                if (!AskSave())
                {
                    return;
                }
                var task = Task.Run(() =>
                {
                    var dat = new Dat(fileName);

                    return dat;
                });
                task.GetAwaiter().OnCompleted(() =>
                {
                    Script = task.Result;
                    Config.LastFile = fileName;
                    FileName = fileName;
                    Title = FileName;

                    IsChanged = false;
                });
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = this.GetResourceString("m_ChooseFileName"),
                DefaultExt = "dat",
                Filter = "Dat file|*.dat"
            };
            if (!(ofd.ShowDialog() is true))
            {
                return;
            }

            OpenFile(ofd.FileName);
        }

        private bool Save()
        {
            if (FileName == null)
            {
                return SaveAs();
            }
            Save(FileName);
            return true;
        }

        private void Save(string fileName)
        {
            try
            {
                Script.Save(fileName);
                Title = fileName;
                FileName = fileName;
                IsChanged = false;
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        private bool SaveAs()
        {
            var sfd = new SaveFileDialog
            {
                Title = this.GetResourceString("m_ChooseFileName"),
                FileName = Path.GetFileName(FileName),
                DefaultExt = "dat",
                Filter = "Dat file|*.dat",
                InitialDirectory = Path.GetDirectoryName(FileName)
            };
            if (!(sfd.ShowDialog() is true))
            {
                return false;
            }
            Save(sfd.FileName);
            return true;
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Script == null)
            {
                return;
            }
            Save();
        }

        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }

        private void FileCommands_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Script != null;
        }


        private void ImportOthersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = "json",
                    Filter = "SJON|*.json|All text|*.txt"
                };
                if (!(ofd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(ofd.FileName))
                {
                    case ".json":
                        {
                            using (var reader = new StreamReader(ofd.FileName))
                            {
                                var jsonReader = new JsonTextReader(reader);
                                var tokens = JsonSerializer.CreateDefault().Deserialize<Dictionary<string, List<string>>>(jsonReader);
                                foreach (var (pair, index) in tokens.Select((x, index) => (x, index)))
                                {
                                    var key = pair.Key;
                                    var items = pair.Value;
                                    var content = Script.ContentItems.First(x => x.Title.Text == key);
                                    foreach (var (stringItem, ind) in items.Select((x, ind) => (x, ind)))
                                    {
                                        content.Texts[ind].Text = stringItem;
                                    }
                                }

                            }
                        }
                        break;
                    case ".txt":
                        {
                            var text = File.ReadAllText(ofd.FileName);
                            var tokens = text.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var (strings, index) in tokens.Select((x, index) => (x.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Unescape()), index)))
                            {
                                var key = strings.First();
                                var content = Script.ContentItems.First(x => x.Title.Text == key);
                                foreach (var (stringItem, ind) in strings.Skip(1).Select((x, ind) => (x, ind)))
                                {
                                    content.Texts[ind].Text = stringItem;
                                }

                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void ExportOthersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog();
                sfd.DefaultExt = "json";
                sfd.Filter = "JSON|*.json|Text|*.txt";
                sfd.InitialDirectory = Path.GetDirectoryName(FileName);
                sfd.FileName = Path.GetFileNameWithoutExtension(FileName) + "_others";
                if (!(sfd.ShowDialog() is true))
                {
                    return;
                }
                var contents = OnlyJapanese ? Script.ContentItems.Where(x => x.IsJapanese) : Script.ContentItems;
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            var jsonWriter = new JsonTextWriter(writer);
                            var jsonSettings = new JsonSerializerSettings();
                            jsonSettings.Formatting = Formatting.Indented;
                            JsonSerializer.CreateDefault(jsonSettings)
                                .Serialize(jsonWriter, contents.ToDictionary(x => x.Title, x => x.Texts));
                        }
                        break;
                    case ".txt":
                        File.WriteAllText(sfd.FileName, string.Join("\n\n", contents.Select(x => string.Join("\n", x.Texts.Prepend(x.Title).Select(y => y.Text.Escape())))), Encoding.UTF8);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void ImportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog();
                ofd.DefaultExt = "josn";
                ofd.Filter = "JSON|*.json|All text|*.txt";
                if (!(ofd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(ofd.FileName))
                {
                    case ".json":
                        {
                            using (var reader = new StreamReader(ofd.FileName))
                            {
                                var jsonReader = new JsonTextReader(reader);
                                var tokens = JsonSerializer.CreateDefault().Deserialize<List<List<string>>>(jsonReader);
                                foreach (var (strings, index) in tokens.Select((x, index) => (x, index)))
                                {
                                    var item = Script.Strings[index];
                                    foreach (var (stringItem, ind) in item.Texts.Where(x => !x.IsDelimeter && x.Type == StringType.Text).Select((x, ind) => (x, ind)))
                                    {
                                        stringItem.Text = strings[ind];
                                    }
                                }
                            }
                        }
                        break;
                    case ".txt":
                        {
                            var text = File.ReadAllText(ofd.FileName);
                            var tokens = text.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var (strings, index) in tokens.Select((x, index) => (x.Split(new string[] { "<br/>" }, StringSplitOptions.RemoveEmptyEntries), index)))
                            {
                                var item = Script.Strings[index];
                                foreach (var (stringItem, ind) in item.Texts.Where(x => !x.IsDelimeter && x.Type == StringType.Text).Select((x, ind) => (x, ind)))
                                {
                                    stringItem.Text = strings[ind].Replace("\r\n", "\n");
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void ExportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog();
                sfd.DefaultExt = "json";
                sfd.Filter = "JSON|*.json|Text|*.txt";
                sfd.InitialDirectory = Path.GetDirectoryName(FileName);
                sfd.FileName = Path.GetFileNameWithoutExtension(FileName) + "_strings";
                if (!(sfd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            var jsonWriter = new JsonTextWriter(writer);
                            var jsonSettings = new JsonSerializerSettings();
                            jsonSettings.Formatting = Formatting.Indented;
                            JsonSerializer.CreateDefault(jsonSettings)
                                .Serialize(jsonWriter, Script.Strings.Select(x => x.Texts.Where(y => !y.IsDelimeter && y.Type == StringType.Text).Select(y => y.Text)));
                        }
                        break;
                    case ".txt":
                        File.WriteAllText(sfd.FileName, string.Join("\n\n", Script.Strings.Select(x => string.Join("<br/>", x.Texts.Where(y => !y.IsDelimeter && y.Type == StringType.Text).Select(y => y.Text.Escape())))), Encoding.UTF8);

                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private FindWindow FindWindow
        {
            get
            {
                if (findWindow == null)
                {
                    var temp = new FindWindow(this);
                    temp.Owner = this;
                    temp.Closed += FindWindow_Closed;
                    temp.Show();
                    FindWindow = temp;
                }
                return findWindow;
            }
            set => findWindow = value;
        }

        private void FindWindow_Closed(object sender, EventArgs e)
        {
            FindWindow = null;
        }

        private void FindCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FindWindow.Focus();
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];

                OpenFile(files[0]);

                e.Handled = true;
            }
        }

        private void Root_Closing(object sender, CancelEventArgs e)
        {
            if (!AskSave())
            {
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            findWindow?.Close();
        }
        private void LoadLastFile()
        {
            if (Config.LastFile != null && File.Exists(Config.LastFile))
            {
                OpenFile(Config.LastFile);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLastFile();
        }
        private void ReportError(Exception ex)
        {
            ReportError(ex.ToString());
        }
        private void ReportError(string message)
        {
            MessageBox.Show(message, this.GetResourceString("m_Error"));
        }

        public void Find(string text, int start)
        {
            if (SelectedTab == SelectedTab.Strings)
            {
                foreach (var item in Strings.Skip(start))
                {
                    if (item.Value.Text.Contains(text))
                    {
                        StringsListView.SelectedItem = item;
                        StringsListView.ScrollIntoView(item);
                        findWindow.Start = StringsListView.SelectedIndex + 1;
                        break;
                    }
                }
            }
            else if (SelectedTab == SelectedTab.Others)
            {
                foreach (var item in Contents.Skip(start))
                {
                    if (item.Value.Text.Contains(text))
                    {
                        ContentsListView.SelectedItem = item;
                        ContentsListView.ScrollIntoView(item);
                        findWindow.Start = ContentsListView.SelectedIndex + 1;
                        break;
                    }
                }
            }
        }

        private void ContentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedIndex = ContentsListView.SelectedIndex;
        }

        private void StringsListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedIndex = StringsListView.SelectedIndex;
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (SelectedTab == SelectedTab.Strings && textBox.DataContext is StringsListItem stringsItem)
                {
                    StringsListView.SelectedIndex = Strings.IndexOf(stringsItem);
                }else if(SelectedTab == SelectedTab.Others && textBox.DataContext is ContentsListItem contentItem)
                {
                    ContentsListView.SelectedIndex = Contents.IndexOf(contentItem);
                }
            }
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AskSave();
            FileName = null;
            Script = null;
        }

        private void mTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (mTabControl.SelectedIndex)
            {
                case 0:
                    SelectedTab = SelectedTab.Strings;
                    break;
                case 1:
                    SelectedTab = SelectedTab.Others;
                    break;
                default:
                    return;
            }
            if (findWindow != null)
            {
                findWindow.Start = 0;
            }
        }

    }
}