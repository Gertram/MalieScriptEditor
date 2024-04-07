using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Text;
using System.Threading.Tasks;

namespace MSEGui
{
    public class WindowCommands
    {
        public static RoutedCommand Cut { get; set; } = new RoutedCommand("Cut", typeof(MainWindow));
        public static RoutedCommand Copy { get; set; } = new RoutedCommand("Copy", typeof(MainWindow));
        public static RoutedCommand Paste { get; set; } = new RoutedCommand("Paste", typeof(MainWindow));
        public static RoutedCommand ImportOthers { get; set; } = new RoutedCommand("ImportOthers", typeof(MainWindow));
        public static RoutedCommand ExportOthers { get; set; } = new RoutedCommand("ExportOthers", typeof(MainWindow));
        public static RoutedCommand ImportStrings { get; set; } = new RoutedCommand("ImportStrings", typeof(MainWindow));
        public static RoutedCommand ExportStrings { get; set; } = new RoutedCommand("ExportStrings", typeof(MainWindow));
        public static RoutedCommand ExportChapters { get; set; } = new RoutedCommand("ExportChapters", typeof(MainWindow));
        public static RoutedCommand ImportChapters { get; set; } = new RoutedCommand("ImportChapters", typeof(MainWindow));
        public static RoutedCommand ExportChapter { get; set; } = new RoutedCommand("ExportChapter", typeof(MainWindow));
        public static RoutedCommand ImportChapter { get; set; } = new RoutedCommand("ImportChapter", typeof(MainWindow));
        public static RoutedCommand Find { get; set; } = new RoutedCommand("Find", typeof(MainWindow));
        public static RoutedCommand SelectFindable { get; set; } = new RoutedCommand("SelectFindable", typeof(MainWindow));
        public static RoutedCommand InsertTrash { get; set; } = new RoutedCommand("InsertTrash", typeof(MainWindow));
    }
}
