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

        private MSEScript script;
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
        private void LoadStrings()
        {
            var strings = new List<StringsListItem>();

            foreach (var (lineItem, index) in Script.Strings.Select((x, index) => (x, index)))
            {
                foreach (var item in lineItem.Texts)
                {
                    strings.Add(new StringsListItem(index, item));
                }
            }
            Strings = strings;
        }
        private void LoadOthers()
        {
                var contents = new List<ContentsListItem>();

                IEnumerable<ContentItem> contentItems = Script.ContentItems.Values;

                if (OnlyJapanese)
                {
                    contentItems = contentItems.Where(x => x.IsJapanese);
                }

                foreach (var (contentItem, index) in contentItems.Select((x, index) => (x, index)))
                {
                    foreach (var item in contentItem.Texts)
                    {
                        contents.Add(new ContentsListItem(index, contentItem.Title, item));
                    }
                }
            Contents = contents;
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
                    LoadStrings();
                    LoadOthers();
                }
                else if (e.PropertyName == nameof(OnlyJapanese))
                {
                    OnlyJapaneseCheckBox.IsEnabled = false;
                    LoadOthers();
                    OnlyJapaneseCheckBox.IsEnabled = true;
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
            IOUtility.ImportOthers(script);
            LoadOthers();
        }
        private void ExportOthersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ExportOthers(script, FileName, OnlyJapanese);
        }

        private void ImportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ImportStrings(script);
            LoadStrings();
        }

        private void ExportStringsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IOUtility.ExportStrings(script, FileName);
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

        private void InsertLineNumberMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = 0;
                foreach(var strings in Script.Strings)
                {
                    index++;
                    strings.Texts[0].Text = "<"+index.ToString() + ">"+strings.Texts[0].Text;
                }
            }
            catch
            {

            }
        }

        private class ChapterString
        {
            public ChapterString()
            {

            }
            public ChapterString(string name, uint index)
            {
                Name = name;
                Index = index;
            }

            public string Name { get; set; }
            public uint Index { get; set; }
        }
        //private StringsItem FindLastString(int currentIndex)
        //{
        //    for(var i = currentIndex;i != -1; i--)
        //    {
        //        var command = Script.Commands[i];
        //        if(command.Type == CommandType.PUSH_STR_BYTE || command.Type == CommandType.PUSH_STR_SHORT || command.Type == CommandType.PUSH_STR_INT)
        //        {
        //            return command.Args[0].Str;
        //        }
        //    }
        //    return null;
        //}
        static string ExtractValue(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        private void HandleCommand(CommandItem command)
        {
            switch (command.Type)
            {
                case CommandType.JMP:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.JNZ:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.JZ:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.CALL_UINT_ID:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.CALL_BYTE_ID:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.POP_R32:
                    vmStack.Pop();
                    break;
                case CommandType.PUSH_INT32:
                    pParma = (uint)command.Args[0].Value;
                    vmStack.Push(pParma | 0x80000000);
                    break;
                case CommandType.PUSH_UINT32:
                    pParma = (uint)command.Args[0].Value;
                    vmStack.Push(pParma | 0x80000000);
                    break;
                case CommandType.PUSH_STR_BYTE:
                    {
                        pParma = (uint)command.Args[0].Value;
                        pLastString = command.Args[0].Str.Text;
                        var pos = pLastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            pLastString = pLastString.Substring(0, pos);
                        }
                        vmStack.Push((uint)command.Args[0].Str.Offset);
                        break;
                    }
                case CommandType.PUSH_STR_SHORT:
                    {
                        pParma = (uint)command.Args[0].Value;
                        pLastString = command.Args[0].Str.Text;
                        var pos = pLastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            pLastString = pLastString.Substring(0, pos);
                        }
                        vmStack.Push((uint)command.Args[0].Str.Offset);
                        break;
                    }
                case CommandType.NONE:
                    break;
                case CommandType.PUSH_STR_INT:
                    {
                        pParma = (uint)command.Args[0].Value;
                        pLastString = command.Args[0].Str.Text;
                        var pos = pLastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            pLastString = pLastString.Substring(0, pos);
                        }
                        vmStack.Push((uint)command.Args[0].Str.Offset);
                        break;
                    }
                case CommandType.POP:
                    vmStack.Pop();
                    break;
                case CommandType.PUSH_0:    
                    vmStack.Push(0|0x80000000);
                    break;
                case CommandType.UNKNOWN_1:
                    break;
                case CommandType.PUSH_0x:
                    pParma = (uint)command.Args[0].Value;
                    vmStack.Push(pParma|0x80000000);
                    break;
                case CommandType.ADD:
                    vmStack.Pop();
                    break;
                case CommandType.SUB:
                    vmStack.Pop();
                    break;
                case CommandType.MUL:
                    vmStack.Pop();
                    break;
                case CommandType.DIV:
                    vmStack.Pop();
                    break;
                case CommandType.MOD:
                    vmStack.Pop();
                    break;
                case CommandType.AND:
                    vmStack.Pop();
                    break;
                case CommandType.OR:
                    vmStack.Pop();
                    break;
                case CommandType.XOR:
                    vmStack.Pop();
                    break;
                case CommandType.BOOL2:
                    vmStack.Pop();
                    break;
                case CommandType.BOOL3:
                    vmStack.Pop();
                    break;
                case CommandType.ISL:
                    vmStack.Pop();
                    break;
                case CommandType.ISLE:
                    vmStack.Pop();
                    break;
                case CommandType.ISNLE:
                    vmStack.Pop();
                    break;
                case CommandType.ISNL:
                    vmStack.Pop();
                    break;
                case CommandType.ISEQ:
                    vmStack.Pop();
                    break;
                case CommandType.ISNEQ:
                    vmStack.Pop();
                    break;
                case CommandType.SHL:
                    //vmStack.Pop();
                    vmStack.Peek();
                    break;
                case CommandType.SAR:
                    vmStack.Pop();
                    break;
                case CommandType.CALL_UINT_NO_PARAM:
                    pParma = (uint)command.Args[0].Value;
                    vmStack.Push(pParma | 0x80000000);
                    break;
                case CommandType.INITSTACK:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.UNKNOWN_2:
                    pParma = (uint)command.Args[0].Value;
                    break;
                case CommandType.RET:
                    pParma = (uint)command.Args[0].Value;
                    break;
                default:
                    break;
            }
        }

        private int ParseJmpTable(List<uint> jmpTable, int currentIndex)
        {
            var MALIE_END = Script.Functions["MALIE_END"].Id;
            for (var command = Script.Commands[currentIndex]; command.Type != CommandType.RET; command = Script.Commands[currentIndex])
            {
                HandleCommand(command);
                if (command.Type == CommandType.CALL_UINT_NO_PARAM)
                {
                    if(pParma == MALIE_END)
                    {
                        break;
                    }
                }
                else if(command.Type == CommandType.JMP)
                {
                    jmpTable.Add((uint)pParma);
                }
                currentIndex++;
            }
            return currentIndex;
        }
        Stack<uint> vmStack;
        string pLastString;
        uint pParma;

        private List<ChapterString> ParseScenario(List<string> chapterName,List<uint> chapterIndex)
        {
            var v = new List<ChapterString>();
            var offset = Script.Functions["maliescenario"].VMCodeOffset;
            var currentIndex = Script.Commands.FindIndex(x => x.Offset == offset);

            var _ms_message = Script.Functions["_ms_message"].Id;
            var MALIE_NAME = Script.Functions["MALIE_NAME"].Id;
            var MALIE_LABLE = Script.Functions["MALIE_LABLE"].Id;
            var tag = Script.Functions["tag"].Id;
            var FrameLayer_SendMessage = Script.Functions["FrameLayer_SendMessage"].Id;
            var System_GetResult = Script.Functions["System_GetResult"].Id;

            ChapterString moji = new ChapterString();
            var jmpTable = new List<uint>();
            var selectTable = new List<uint>();
            var jmpIteratorIndex = -1;
            vmStack = new Stack<uint>(); ;

            for (var command = Script.Commands[currentIndex]; command.Type != CommandType.RET; command = Script.Commands[currentIndex])
            {
                HandleCommand(command);
                if (jmpTable.Count !=0)
                {
                    if(jmpIteratorIndex != jmpTable.Count && offset > jmpTable[jmpIteratorIndex])
                    {
                        jmpIteratorIndex++;
                        chapterIndex.Add((uint)v.Count);
                    }
                }
                if(command.Type == CommandType.CALL_UINT_NO_PARAM||command.Type == CommandType.CALL_BYTE_ID || command.Type == CommandType.CALL_UINT_ID)//vCall
                {
                    if (pParma == tag)
                    {
                        var pos = pLastString.IndexOf("<chapter");
                        if (pos != -1)
                        {
                            var Title = ExtractValue(pLastString, @"<chapter name='(.*?)'>");
                            
                            chapterName.Add(Title);
                            
                            chapterIndex.Add((uint)v.Count);
                        }
                    }
                    else if (pParma == _ms_message)
                    {
                        vmStack.Pop();
                        moji.Index = vmStack.Peek() & ~0x80000000;
                        while (vmStack.Count != 0) vmStack.Pop();
                        vmStack.Push(0);
                        v.Add(moji);
                        moji = new ChapterString();
                        moji.Name = "";
                        selectTable.Clear();
                    }
                    else if (pParma == MALIE_NAME)
                    {
                        moji.Name = pLastString;
                    }
                    else if (pParma == MALIE_LABLE && v.Count == 0)
                    {
                        if (pLastString == "_index")
                        {
                            currentIndex = ParseJmpTable(jmpTable,currentIndex);
                            jmpIteratorIndex = 0;
                            continue;
                        }
                    }
                    else if(pParma == System_GetResult)
                    {
                        foreach(var x in selectTable)
                        {
                            Console.WriteLine("Select: " + x.ToString());
                        }
                    }
                    else if(pParma == FrameLayer_SendMessage && vmStack.Count > 4)
                    {
                        vmStack.Pop(); vmStack.Pop(); vmStack.Pop(); vmStack.Pop();
                        var loc = vmStack.Peek();
                        if (loc > 0)
                        {
                            selectTable.Add(loc);
                        }
                    }
                }
                currentIndex++;
            }


            return v;
        }
        private void ExportScenesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var chapterNames = new List<string>();
                var chapterIndex = new List<uint>();
                var moji = ParseScenario(chapterNames, chapterIndex);
                var chapterRegion = chapterIndex.Skip(1).Append((uint)moji.Count).ToList();

                if (!Directory.Exists("output"))
                {
                    Directory.CreateDirectory("output");
                }

                for(int i = 0;i < chapterIndex.Count;i++)
                {
                    var index = chapterIndex[i];
                    string chapterName = chapterNames[i];
                    var endIndex = (int)chapterRegion[i];
                    var filename = $"output/{i+1}.{chapterName}.{index}-{endIndex} .txt";
                    using (var writer = new StreamWriter(filename,false, Encoding.UTF8))
                    {
                        for (var j = (int)index; j < endIndex; j++)
                        {
                            var name = moji[j].Name;
                            var text = Script.Strings[(int)moji[j].Index];
                            if (string.IsNullOrEmpty(name))
                            {
                                writer.WriteLine(text);
                            }
                            else {
                                writer.WriteLine($"{name}: {text}");
                            }
                            writer.WriteLine();
                        }
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}