using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using MSELib;
using Newtonsoft.Json;

namespace MSEGui.IO
{
    internal static class IOUtility
    {
        public static void ExportStrings(MSEScript script, string fileName)
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
                        using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            var jsonWriter = new JsonTextWriter(writer);
                            var jsonSettings = new JsonSerializerSettings();
                            jsonSettings.Formatting = Formatting.Indented;
                            JsonSerializer.CreateDefault(jsonSettings)
                                .Serialize(jsonWriter, script.Strings.Select(x => x.Texts.Select(y => y.Text)));
                        }
                        break;
                    case ".txt":
                        File.WriteAllText(sfd.FileName, string.Join("\n\n", script.Strings
                            .Select(x => string.Join("\n", x.Texts.Select(y => y.Text)))), Encoding.UTF8);

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
        public static void ImportStrings(MSEScript script)
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
                        {
                            using (var reader = new StreamReader(ofd.FileName))
                            {
                                var jsonReader = new JsonTextReader(reader);
                                var tokens = JsonSerializer.CreateDefault().Deserialize<List<List<string>>>(jsonReader);
                                foreach (var (strings, index) in tokens.Select((x, index) => (x, index)))
                                {
                                    var item = script.Strings[index];
                                    foreach (var (stringItem, ind) in item.Texts.Select((x, ind) => (x, ind)))
                                    {
                                        stringItem.Text = strings[ind];
                                    }
                                }
                            }
                        }
                        break;
                    case ".txt":
                        {
                            //var text = File.ReadAllText(ofd.FileName);
                            //var tokens = text.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                            //foreach (var (strings, index) in tokens.Select((x, index) => (x.Split(new string[] { "<br/>" }, StringSplitOptions.RemoveEmptyEntries), index)))
                            //{
                            //    var item = script.Strings[index];
                            //    foreach (var (stringItem, ind) in item.Texts.Where(x => !x.IsDelimeter && x.Type == StringType.Text).Select((x, ind) => (x, ind)))
                            //    {
                            //        stringItem.Text = strings[ind].Replace("\r\n", "\n");
                            //    }
                            //}
                            TXTUtil.ImportStrings(script, ofd.FileName);
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
        public static void ExportOthers(MSEScript script, string fileName,bool onlyJapanese)
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
                var contents = onlyJapanese ? script.ContentItems.Where(x => x.IsJapanese) : script.ContentItems;
                switch (Path.GetExtension(sfd.FileName))
                {
                    case ".json":
                        using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            var jsonWriter = new JsonTextWriter(writer);
                            var jsonSettings = new JsonSerializerSettings();
                            jsonSettings.Formatting = Formatting.Indented;
                            JsonSerializer.CreateDefault(jsonSettings)
                                .Serialize(jsonWriter, contents.ToDictionary(x => x.Title, x => x.Texts.Select(y => y.Text)));
                        }
                        break;
                    case ".txt":
                        File.WriteAllText(sfd.FileName, string.Join("\n\n", contents.Select(x => string.Join("\n", x.Texts.Prepend(x.Title).Select(y => y.Text)))), Encoding.UTF8);
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
        public static void ImportOthers(MSEScript script)
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
                        {
                            using (var reader = new StreamReader(ofd.FileName))
                            {
                                var jsonReader = new JsonTextReader(reader);
                                var tokens = JsonSerializer.CreateDefault().Deserialize<Dictionary<string, List<string>>>(jsonReader);
                                foreach (var (pair, index) in tokens.Select((x, index) => (x, index)))
                                {
                                    var key = pair.Key;
                                    var items = pair.Value;
                                    var content = script.ContentItems.First(x => x.Title.Text == key);
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
                            //var text = File.ReadAllText(ofd.FileName);
                            //var tokens = text.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                            //foreach (var (strings, index) in tokens.Select((x, index) => (x.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Unescape()), index)))
                            //{
                            //    var key = strings.First();
                            //    var content = script.ContentItems.First(x => x.Title.Text == key);
                            //    foreach (var (stringItem, ind) in strings.Skip(1).Select((x, ind) => (x, ind)))
                            //    {
                            //        content.Texts[ind].Text = stringItem;
                            //    }

                            //}
                            TXTUtil.ImportOthers(script, ofd.FileName);
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
    }
}
