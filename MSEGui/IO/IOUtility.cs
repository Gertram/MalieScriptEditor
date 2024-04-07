using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using MSELib;
using System.IO;
using FolderBrowserEx;
using Newtonsoft.Json;
using MSELib.classes;

namespace MSEGui.IO
{
    public static class IOUtility
    {
        public delegate void ImportOthersFunc(IReadOnlyList<StringItem> strings, StreamReader reader);
        public delegate void ExportOthersFunc(IEnumerable<StringItem> strings, StreamWriter reader);
        public delegate void ImportStringsFunc(IReadOnlyList<LineItem> strings, StreamReader reader);
        public delegate void ExportStringsFunc(IEnumerable<LineItem> strings, StreamWriter reader);
        public delegate void ImportChapterFunc(IReadOnlyList<ChapterString> chapters, StreamReader reader);
        public delegate void ExportChapterFunc(IEnumerable<ChapterString> chapters, StreamWriter reader);
        public static void ImportOthers(IReadOnlyList<StringItem> strings, string filename, ImportOthersFunc func)
        {
            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportOthers(IReadOnlyList<StringItem> strings, byte[] raw, ImportOthersFunc func)
        {
            using (var stream = new MemoryStream(raw))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportOthers(IReadOnlyList<StringItem> strings)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt"
                };
                if (!(ofd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(ofd.FileName))
                {
                    case ".json":
                        ImportOthers(strings, ofd.FileName, JsonUtil.ImportOthers);
                        break;
                    case ".txt":
                        ImportOthers(strings, ofd.FileName, TextUtil.ImportOthers);
                        break;
                    default:
                        break;
                }
            }
            catch (SoManyStringsException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ContentItemNotFound ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ContentItemNotAll ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        public static void ExportOthers(IEnumerable<StringItem> strings,string filename, ExportOthersFunc func)
        {
            using (var writer = new StreamWriter(filename,false, Encoding.UTF8))
            {
                func(strings, writer);
            }
        }
        public static byte[] ExportOthers(IEnumerable<StringItem> strings, ExportOthersFunc func)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                func(strings, writer);
                return stream.ToArray();
            }
        }
        public static void ExportOthers(IEnumerable<StringItem> strings, string fileName)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt",
                    InitialDirectory = Path.GetDirectoryName(fileName),
                    FileName = Path.GetFileNameWithoutExtension(fileName) + "_others"
                };
                if (!(sfd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        ExportOthers(strings, sfd.FileName, JsonUtil.ExportOthers);
                        break;
                    case ".txt":
                        ExportOthers(strings, sfd.FileName, TextUtil.ExportOthers);
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
        public static void ImportStrings(IReadOnlyList<LineItem> strings, string filename,ImportStringsFunc func)
        {
            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportStrings(IReadOnlyList<LineItem> strings, byte[] raw, ImportStringsFunc func)
        {
            using (var stream = new MemoryStream(raw))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportStrings(IReadOnlyList<LineItem> strings)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt"
                };
                if (!(ofd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(ofd.FileName))
                {
                    case ".json":
                        ImportStrings(strings, ofd.FileName, JsonUtil.ImportStrings);
                        break;
                    case ".txt":
                        {
                            ImportStrings(strings, ofd.FileName, TextUtil.ImportStrings);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (SoManyStringsException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SoManyLinesException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static byte[] ExportStrings(IEnumerable<LineItem> strings,ExportStringsFunc func)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                func(strings, writer);
                return stream.ToArray();
            }
        }
        public static void ExportStrings(IEnumerable<LineItem> strings, string filename,ExportStringsFunc func)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                func(strings, writer);
            }
        }
        public static void ExportStrings(IEnumerable<LineItem> strings, string fileName)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt",
                    InitialDirectory = Path.GetDirectoryName(fileName),
                    FileName = Path.GetFileNameWithoutExtension(fileName) + "_strings"
                };
                if (!(sfd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        ExportStrings(strings,sfd.FileName,JsonUtil.ExportStrings);
                        break;
                    case ".txt":
                        ExportStrings(strings, sfd.FileName,TextUtil.ExportStrings);
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
        public static void ExportChapter(IEnumerable<ChapterString> chapter, string filename,ExportChapterFunc func)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                func(chapter, writer);
            }
        }
        public static byte[] ExportChapter(IEnumerable<ChapterString> strings, ExportChapterFunc func)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                func(strings, writer);
                return stream.ToArray();
            }
        }
        public static void ExportChapter(Chapter chapter)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt",
                    FileName = $"{chapter.Title}.{chapter.Start}-{chapter.End}.txt"
                };
                if (!(sfd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        ExportChapter(chapter.Strings,sfd.FileName, JsonUtil.ExportChapter);
                        break;
                    case ".txt":
                        ExportChapter(chapter.Strings, sfd.FileName, TextUtil.ExportChapter);
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
        public static void ImportChapter(IReadOnlyList<ChapterString> strings, string filename,ImportChapterFunc func)
        {
            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportChapter(IReadOnlyList<ChapterString> strings, byte[] raw, ImportChapterFunc func)
        {
            using (var stream = new MemoryStream(raw))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                func(strings, reader);
            }
        }
        public static void ImportChapter(Chapter chapter)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    DefaultExt = "json",
                    Filter = "Supported files|*.json;*.txt|JSON|*.json|All text|*.txt"
                };
                if (!(ofd.ShowDialog() is true))
                {
                    return;
                }
                switch (Path.GetExtension(ofd.FileName))
                {
                    case ".json":
                        ImportChapter(chapter.Strings, ofd.FileName, JsonUtil.ImportChapter);
                        break;
                    case ".txt":
                        ImportChapter(chapter.Strings, ofd.FileName, TextUtil.ImportChapter);
                        break;
                    default:
                        break;
                }
            }
            catch (SoManyStringsException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SoManyLinesException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static void ExportChapters(IEnumerable<Chapter> chapters)
        {
            try
            {
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                foreach(var (index,chapter) in chapters.Select((x,index)=>(index,x)))
                {
                    var filepath = Path.Combine(fbd.SelectedFolder, $"{index}.{chapter.Title}.{chapter.Start}-{chapter.End}.txt");
                    ExportChapter(chapter.Strings, filepath, TextUtil.ExportChapter);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        public static void ImportChapters(IEnumerable<Chapter> chapters)
        {
            try
            {
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                foreach(var filepath in Directory.GetFiles(fbd.SelectedFolder))
                {
                    var chapter = chapters.FirstOrDefault(x => filepath.Contains(x.Title));
                    if(chapter == null)
                    {
                        continue;
                    }
                    ImportChapter(chapter.Strings, filepath, TextUtil.ImportChapter);
                }
            }
            catch (SoManyStringsException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SoManyLinesException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
