﻿using System;
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
using MSELib.classes;
using System.Globalization;
using MSEGui.Config;
using ConfigLib;
using MSEGui.IO;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace MSEGui
{
    internal enum SelectedTab
    {
        Chapters,
        Strings,
        Others
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged, IFindable
    {
        private FindWindow findWindow;
        private List<StringListItem> strings;
        private List<StringItem> contents;
        private List<Chapter> chapters;
        private Chapter selectedChapter;
        private SelectedTab SelectedTab { get; set; } = SelectedTab.Strings;
        private bool IsChanged { get; set; } = false;

        private MSEScript script;
        private string fileName;
        public MSEScript Script
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
        private int selectedChapterIndex;
        public int SelectedChapterIndex
        {
            get => selectedChapterIndex;
            set
            {
                selectedChapterIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChapterIndex)));
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
        public List<StringListItem> Strings
        {
            get => strings; 
            private set
            {
                strings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Strings)));
            }
        }
        public List<StringItem> Others
        {
            get => contents;
            private set
            {
                contents = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Others)));
            }
        }
        public List<Chapter> Chapters
        {
            get => chapters;
            private set
            {
                chapters = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chapters)));
            }
        }
        public Chapter SelectedChapter
        {
            get => selectedChapter;
            private set
            {
                selectedChapter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChapter)));
            }
        }
        private void LoadStrings()
        {
            var strings = new List<StringListItem>();

            foreach (var (lineItem, index) in Script.Strings.Select((x, index) => (x, index)))
            {
                foreach (var item in lineItem.Texts)
                {
                    strings.Add(new StringListItem(index, item));
                }
            }
            Strings = strings;
        }
        private void LoadOthers()
        {
            if(Script == null)
            {
                Others = null;
                return;
            }
            var contents = new List<StringItem>();
            var filterIndex = FilterComboBox.SelectedIndex;
            if(filterIndex <= 0)
            {
                Others = Script.DataStrings;
                return;
            }

            if (filterIndex == 1)
            {
                Others = Script.DataStrings.Where(x => x.Text.ContainsJapanese(0)).ToList();
                return;
            }

            var tag = (StringTag)(filterIndex-1);

            Others = Script.DataStrings.Where(x => x.Tag == tag).ToList();
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
                        Others = null;
                        return;
                    }
                    LoadStrings();
                    LoadOthers();
                    Chapters = Script.Chapters;
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
                App.LanguageChanged += App_LanguageChanged;

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
                    menuLang.Click += MenuLang_Click;
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
                var dat = new MSEScript(fileName);

                //using (var writer = new StreamWriter("temp.txt"))
                //{
                //    foreach (var (item,index) in dat.TitleItems.Where(x => x.Parameters.Count == 1).OrderBy(x => x.Parameters[0]).Select((x,index)=>(x,index)))
                //    {
                //        writer.WriteLine($"{index,-4}.{item.OffsetHex}={item.Parameters[0]:X}"); ;
                //    }
                //}

                Script = dat;
                Config.LastFile = fileName;
                FileName = fileName;
                Title = FileName;

                IsChanged = false;

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
                if (File.Exists(fileName) && File.GetAttributes(fileName).HasFlag(FileAttributes.ReadOnly))
                {
                    MessageBox.Show(this.GetResourceString("m_FileReadOnly"));
                    return;
                }
                Script.Save(fileName);

                Title = fileName;
                FileName = fileName;
                IsChanged = false;

            }
            catch (UnauthorizedAccessException)
            {
                ReportError(string.Format(this.GetResourceString("m_FileWriteError"),fileName));
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
            IOUtility.ImportOthers(Others);
            LoadOthers();
        }
        private void ExportOthersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ExportOthers(Others, FileName);
        }

        private void ImportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ImportStrings(script.Strings);
            LoadStrings();
        }

        private void ExportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ExportStrings(script.Strings, FileName);
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

        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
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
                foreach (var item in Others.Skip(start))
                {
                    if (item.Text.Contains(text))
                    {
                        ContentsListView.SelectedItem = item;
                        ContentsListView.ScrollIntoView(item);
                        findWindow.Start = ContentsListView.SelectedIndex + 1;
                        break;
                    }
                }
            } if (SelectedTab == SelectedTab.Others)
            {
                foreach (var item in Others.Skip(start))
                {
                    if (item.Text.Contains(text))
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
                if (SelectedTab == SelectedTab.Strings && textBox.DataContext is StringListItem stringsItem)
                {
                    StringsListView.SelectedIndex = Strings.IndexOf(stringsItem);
                }
                else if (SelectedTab == SelectedTab.Others && textBox.DataContext is StringItem stringitem)
                {
                    ContentsListView.SelectedIndex = Others.IndexOf(stringitem);
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

        private void InsertLineNumberMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = 0;
                foreach (var strings in Script.Strings)
                {
                    index++;
                    strings.Texts[0].Text = "<" + index.ToString() + ">" + strings.Texts[0].Text;
                }
            }
            catch
            {

            }
        }

        private void ExportScenesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //    try
            //    {
            //        var chapterNames = new List<string>();
            //        var chapterIndex = new List<uint>();
            //        var moji = ParseScenario(chapterNames, chapterIndex);
            //        var chapterRegion = chapterIndex.Skip(1).Append((uint)moji.Count).ToList();

            //        if (!Directory.Exists("output"))
            //        {
            //            Directory.CreateDirectory("output");
            //        }

            //        for (int i = 0; i < chapterIndex.Count; i++)
            //        {
            //            var index = chapterIndex[i];
            //            string chapterName = chapterNames[i];
            //            var endIndex = (int)chapterRegion[i];
            //            var filename = $"output/{i + 1}.{chapterName}.{index}-{endIndex} .txt";
            //            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            //            {
            //                for (var j = (int)index; j < endIndex; j++)
            //                {
            //                    var name = moji[j].Name;
            //                    var text = Script.Strings[(int)moji[j].Index];
            //                    if (string.IsNullOrEmpty(name))
            //                    {
            //                        writer.WriteLine(text);
            //                    }
            //                    else
            //                    {
            //                        writer.WriteLine($"{name}: {text}");
            //                    }
            //                    writer.WriteLine();
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString());
            //    }
        }

        private void ScenesListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ChaptersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedChapter = ChaptersListView.SelectedItem as Chapter;
            SelectedChapterIndex = ChaptersListView.SelectedIndex;
        }

        private void ExportChapterCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedChapter != null;
        }

        private void ExportChaptersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            IO.IOUtility.ExportChapters(Chapters);
        }

        private void ImportChaptersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            IO.IOUtility.ImportChapters(Chapters);
        }

        private void ImportChapterCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            IO.IOUtility.ImportChapter(SelectedChapter);
        }

        private void ExportChapterCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            IO.IOUtility.ExportChapter(SelectedChapter);
        }

        private void FilterCheckBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOthers();
        }
    }
}