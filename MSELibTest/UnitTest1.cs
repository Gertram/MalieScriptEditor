using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSELib;
using MSEGui.IO;
using System;
using System.IO;

namespace MSELibTest
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly string FileName = "EXEC.DAT";
        private static void CompareWithOriginal(byte[] OriginalBytes,byte[] newbytes)
        {
            if (newbytes.Length!= OriginalBytes.Length) {
                Assert.Fail("Files length not equal");
            }
            for (int i = 0; i < OriginalBytes.Length; i++)
            {
                var value1 = OriginalBytes[i];
                var value2 = newbytes[i];
                if (value1 != value2)
                {
                    Assert.Fail($"Byte 0x{i:X} not equal ({value1:X},{value2:X})");
                    return;
                }
            }
        }

        [TestMethod]
        public void SimpleSaveTest()
        {
            var OriginalBytes = File.ReadAllBytes(FileName);
            var Script = new MSEScript(OriginalBytes);
            var newbytes = Script.Save();
            CompareWithOriginal(OriginalBytes, newbytes);
        }
        [TestMethod]
        public void ExportStringsTest()
        {

            var OriginalBytes = File.ReadAllBytes(FileName);
            var Script = new MSEScript(OriginalBytes);

            IOUtility.ExportStrings(Script.Strings, "temp_strings.txt", TextUtil.ExportStrings);

            IOUtility.ImportStrings(Script.Strings, "temp_strings.txt", TextUtil.ImportStrings);

            var newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);

            IOUtility.ExportStrings(Script.Strings, "temp_strings.json", JsonUtil.ExportStrings);

            IOUtility.ImportStrings(Script.Strings, "temp_strings.json", JsonUtil.ImportStrings);

            newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);
        }
        [TestMethod]
        public void ExportOthersTest()
        {

            var OriginalBytes = File.ReadAllBytes(FileName);
            var Script = new MSEScript(OriginalBytes);
            IOUtility.ExportOthers(Script.DataStrings,"temp.txt",TextUtil.ExportOthers);

            IOUtility.ImportOthers(Script.DataStrings, "temp.txt", TextUtil.ImportOthers);

            var newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);

            IOUtility.ExportOthers(Script.DataStrings,"temp.json", JsonUtil.ExportOthers);

            IOUtility.ImportOthers(Script.DataStrings, "temp.json", JsonUtil.ImportOthers);

            newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);
        }
        [TestMethod]
        public void ExportChapterTest()
        {

            var OriginalBytes = File.ReadAllBytes(FileName);
            var Script = new MSEScript(OriginalBytes);
            IOUtility.ExportChapter(Script.Chapters[0].Strings, "temp_chapter.txt", TextUtil.ExportChapter);

            IOUtility.ImportChapter(Script.Chapters[0].Strings, "temp_chapter.txt", TextUtil.ImportChapter);

            var newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);

            IOUtility.ExportChapter(Script.Chapters[0].Strings, "temp_chapter.json", JsonUtil.ExportChapter);

            IOUtility.ImportChapter(Script.Chapters[0].Strings, "temp_chapter.json", JsonUtil.ImportChapter);

            newbytes = Script.Save();

            CompareWithOriginal(OriginalBytes, newbytes);
        }
    }
}
