using FileSleuth.FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static FileSleuth.Structs;
using static FileSleuth.MFTManager.MFTMethods;
using FileSleuth;
using System.Linq.Expressions;

namespace FileSleuth_UnitTests
{

    [TestClass]
    public class FileHandlerTests
    {
        [TestMethod]
        public void FileRecordHandler_FindFileRecord_ValidMagicNum()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.validFileRecord.xml";
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            XmlDocument xml = new XmlDocument();
            Stream? stream = null;

            stream = assembly.GetManifestResourceStream(xmlTestFile);

            if (stream != null)
            {
                using (stream)
                {
                    xml.Load(stream);
                }
            }
            else
            {
                Assert.Fail("XML Test File not loaded: " + xmlTestFile);
            }

            FileRecordHeader fileRecordHeader = new FileRecordHeader();

            using (XmlReader reader = new XmlNodeReader(xml))
            {
                var serializer = new XmlSerializer(typeof(FileRecordHeader));

                object? result = serializer.Deserialize(reader);

                if (result != null)
                {
                    fileRecordHeader = (FileRecordHeader)result;
                }
                else
                {
                    Assert.Fail("Failed to deserialize file into " + fileRecordHeader.GetType() + " from: " + xmlTestFile);
                }
            }

            FileRecordHandler fileHandler = new FileRecordHandler();
            Nullable<FileRecordHeader> fileRecord = fileHandler.FindFileRecord(0, StructToBytes<FileRecordHeader>(fileRecordHeader));

            if(fileRecord != null)
            {
                Assert.AreEqual(Constants.MFT_MAGIC_NUMBER, fileRecord.Value.magicNum);
            }
            else
            {
                throw new ArgumentNullException(nameof(fileRecord));
            }                     
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.InvalidMFTMagicNumberException),
        "File Record with Invalid Magic Number did not throw error")]
        public void FileRecordHandler_FindFileRecord_InvalidMagicNum()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.invalidFileRecord.xml";
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            XmlDocument xml = new XmlDocument();
            Stream? stream = null;

            stream = assembly.GetManifestResourceStream(xmlTestFile);

            if (stream != null)
            {
                using (stream)
                {
                    xml.Load(stream);
                }
            }
            else
            {
                Assert.Fail("XML Test File not loaded: " + xmlTestFile);
            }

            FileRecordHeader fileRecordHeader = new FileRecordHeader();

            using (XmlReader reader = new XmlNodeReader(xml))
            {
                var serializer = new XmlSerializer(typeof(FileRecordHeader));

                object? result = serializer.Deserialize(reader);

                if (result != null)
                {
                    fileRecordHeader = (FileRecordHeader)result;
                }
                else
                {
                    Assert.Fail("Failed to deserialize file into " + fileRecordHeader.GetType() + " from: " + xmlTestFile);
                }
            }

            FileRecordHandler fileHandler = new FileRecordHandler();

            Nullable<FileRecordHeader> fileRecord = fileHandler.FindFileRecord(0, StructToBytes<FileRecordHeader>(fileRecordHeader));

            if (fileRecord == null)
            {
                throw new ArgumentNullException(nameof(fileRecord));
            }
        }

        [TestMethod]
        public void FileRecordHandler_FindFileRecord_File_NotInUse()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.fileRecord_NotInUse.xml";
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            XmlDocument xml = new XmlDocument();
            Stream? stream = null;

            stream = assembly.GetManifestResourceStream(xmlTestFile);

            if (stream != null)
            {
                using (stream)
                {
                    xml.Load(stream);
                }
            }
            else
            {
                Assert.Fail("XML Test File not loaded: " + xmlTestFile);
            }

            FileRecordHeader fileRecordHeader = new FileRecordHeader();

            using (XmlReader reader = new XmlNodeReader(xml))
            {
                var serializer = new XmlSerializer(typeof(FileRecordHeader));

                object? result = serializer.Deserialize(reader);

                if (result != null)
                {
                    fileRecordHeader = (FileRecordHeader)result;
                }
                else
                {
                    Assert.Fail("Failed to deserialize file into " + fileRecordHeader.GetType() + " from: " + xmlTestFile);
                }
            }

            FileRecordHandler fileHandler = new FileRecordHandler();
            Nullable<FileRecordHeader> fileRecord = fileHandler.FindFileRecord(0, StructToBytes<FileRecordHeader>(fileRecordHeader));

            Assert.IsNull(fileRecord);
        }
    }
}
