using FileSleuth;
using FileSleuth.FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static FileSleuth.Enums;
using static FileSleuth.FileManager.FileManagerObjects;
using static FileSleuth.MFTManager.MFTMethods;
using static FileSleuth.Structs;

namespace FileSleuth_UnitTests
{

    [TestClass]
    public class FileHandlerTests
    {

        private T DeserializeXMLToObject<T>(string xmlResourcePath) where T : new()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            XmlDocument xml = new XmlDocument();
            Stream? stream = null;

            stream = assembly.GetManifestResourceStream(xmlResourcePath);

            if (stream != null)
            {
                using (stream)
                {
                    xml.Load(stream);
                }
            }
            else
            {
                Assert.Fail("XML Test File not loaded: " + xmlResourcePath);
            }

            T xmlResultObject = new T();

            using (XmlReader reader = new XmlNodeReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));

                object? result = serializer.Deserialize(reader);

                if (result != null)
                {
                    xmlResultObject = (T)result;
                }
                else
                {
                    Assert.Fail("Failed to deserialize file into " + xmlResultObject.GetType() + " from: " + xmlResourcePath);
                }
            }

            return xmlResultObject;
        }

        private T DeserializeXMLToPrimitive<T>(string xmlResourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            XmlDocument xml = new XmlDocument();
            Stream? stream = null;

            stream = assembly.GetManifestResourceStream(xmlResourcePath);

            if (stream != null)
            {
                using (stream)
                {
                    xml.Load(stream);
                }
            }
            else
            {
                Assert.Fail("XML Test File not loaded: " + xmlResourcePath);
            }

            T? xmlResultObject = default;

            using (XmlReader reader = new XmlNodeReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));

                object? result = serializer.Deserialize(reader);

                if (result != null)
                {
                    xmlResultObject = (T)result;
                }
                else
                {
                    Assert.Fail("Failed to deserialize file into " + typeof(T).Name + " from: " + xmlResourcePath);
                }
            }

            if (xmlResultObject == null)
            {
                Assert.Fail("xmlResultObject returned null");
            }
            
            return xmlResultObject;
            
        }

        [TestMethod]
        public void FileRecordHandler_FindFileRecord_ValidMagicNum()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.validFileRecord.xml";
            FileRecordHeader fileRecordHeader = DeserializeXMLToObject<FileRecordHeader>(xmlTestFile);

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
            FileRecordHeader fileRecordHeader = DeserializeXMLToObject<FileRecordHeader>(xmlTestFile);

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
            FileRecordHeader fileRecordHeader = DeserializeXMLToObject<FileRecordHeader>(xmlTestFile);

            FileRecordHandler fileHandler = new FileRecordHandler();
            Nullable<FileRecordHeader> fileRecord = fileHandler.FindFileRecord(0, StructToBytes<FileRecordHeader>(fileRecordHeader));

            Assert.IsNull(fileRecord);
        }

        [TestMethod]
        public void FileRecordHandler_GetFileNameAttribute_FileNameAttribute_Found()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.validFileNameAttributeByteArray.xml";
            ushort testFirstAttributeOffset = 56;
            ulong testFilePos = 0;

            byte[] array = DeserializeXMLToPrimitive<byte[]>(xmlTestFile);
            

            FileRecordHandler fileHandler = new FileRecordHandler();
            (FileNameAttributeHeader fileNameAttribute, int attributePosition) = fileHandler.GetFileNameAttribute(array, testFirstAttributeOffset, testFilePos, out AttributeListInfo _);


            Assert.IsTrue(fileNameAttribute.attributeType == (uint)AttributeTypes.FileName);        
        }

        [TestMethod]
        public void FileRecordHandler_GetFileNameAttribute_AttributeListFound()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.attributeListFoundByteArray.xml";
            ushort testFirstAttributeOffset = 56;
            ulong testFilePos = 9;

            byte[] array = DeserializeXMLToPrimitive<byte[]>(xmlTestFile);


            FileRecordHandler fileHandler = new FileRecordHandler();
            (FileNameAttributeHeader fileNameAttribute, int attributePosition) = fileHandler.GetFileNameAttribute(array, testFirstAttributeOffset, testFilePos, out AttributeListInfo info);


            Assert.IsTrue(info.hasAttributeList);
        }

        [TestMethod]
        public void FileRecordHandler_GetFileNameAttribute_FileNameAttribute_Not_Found()
        {

            string xmlTestFile = "FileSleuth_UnitTests.FileRecordHandler_TestFiles.fileNameAtrributeNotFoundByteArray.xml";
            ushort testFirstAttributeOffset = 56;
            ulong testFilePos = 12;

            byte[] array = DeserializeXMLToPrimitive<byte[]>(xmlTestFile);


            FileRecordHandler fileHandler = new FileRecordHandler();
            (FileNameAttributeHeader fileNameAttribute, int attributePosition) = fileHandler.GetFileNameAttribute(array, testFirstAttributeOffset, testFilePos, out AttributeListInfo info);


            Assert.IsTrue(!info.hasAttributeList && fileNameAttribute.Equals(default(FileNameAttributeHeader)));
        }
    }
}
