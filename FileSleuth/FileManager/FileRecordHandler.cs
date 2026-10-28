using FileSleuth.Model;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using static FileSleuth.Constants;
using static FileSleuth.Enums;
using static FileSleuth.FileManager.FileManagerObjects;
using static FileSleuth.MFTManager.MFTMethods;
using static FileSleuth.MFTManager.MFTObjects;
using static FileSleuth.Structs;

namespace FileSleuth.FileManager
{
    public class FileRecordHandler
    {

        public FileRecordHandler()
        {

        }

        /// <returns>Return a MFT File Record if one is found, or null if the file record found was not in use</returns>
        public FileRecord FindFileRecord(ulong filePos, byte[] mftBuffer)
        {

            int fileRecordPosition = MFT_FILE_SIZE * (int)filePos;

            FileRecord fileRecord = new FileRecord();

            FileRecordHeader fileRecordHeader =
                BytesToStruct<FileRecordHeader>(mftBuffer[fileRecordPosition..(fileRecordPosition + Marshal.SizeOf(typeof(FileRecordHeader)))]);



            if ((fileRecordHeader.flags & (ushort)FileRecordFlags.InUse) != 1)
            {
                fileRecord.fileRecordHeader = null;
                return fileRecord;
            }

            if (fileRecordHeader.magicNum == MFT_MAGIC_NUMBER)
            {

                int fixupArrayStart = fileRecordPosition + fileRecordHeader.updateSequenceOffset;

                fileRecord.fileRecordHeader = fileRecordHeader;
                fileRecord.fixupArray = mftBuffer[fixupArrayStart..(fixupArrayStart + fileRecordHeader.updateSequenceSize)];

                return fileRecord;
            }
            else
            {
                throw new Exceptions.InvalidMFTMagicNumberException();
            }

        }

        public (FileNameAttributeHeader fileNameAttribute, int attributePosition) GetFileNameAttribute(byte[] mftBuffer, ushort firstAttributeOffset, ulong filePos, out AttributeListInfo attributeListInfo)
        {
            int fileRecordPosition = MFT_FILE_SIZE * (int)filePos;
            int attributePosition = fileRecordPosition + firstAttributeOffset;
            attributeListInfo = new AttributeListInfo();

            AttributeHeader attribute = BytesToStruct<AttributeHeader>(mftBuffer[attributePosition..(attributePosition + Marshal.SizeOf(typeof(AttributeHeader)))]);
            FileNameAttributeHeader fileNameAttribute = new FileNameAttributeHeader();

            while (attributePosition - fileRecordPosition < MFT_FILE_SIZE)
            {

                if (attribute.attributeType == (uint)AttributeTypes.FileName)
                {
                    int fileNameAttributeSize = attributePosition + Marshal.SizeOf(typeof(FileNameAttributeHeader));
                    fileNameAttribute = BytesToStruct<FileNameAttributeHeader>(mftBuffer[attributePosition..fileNameAttributeSize]);

                    if (fileNameAttribute.namespaceType != 2)
                    {
                        if (!fileNameAttribute.Equals(default(FileNameAttributeHeader)) && fileNameAttribute.nonResident == 0)
                        {
                            return (fileNameAttribute, attributePosition);
                        }
                    }
                }
                else if (attribute.attributeType == (uint)AttributeTypes.AttributeList)
                {
                    attributeListInfo.hasAttributeList = true;
                    attributeListInfo.attributeListPosition = attributePosition;

                    int attributeListSize = attributePosition + Marshal.SizeOf(typeof(ResidentAttributeHeader));
                    attributeListInfo.attributeListAttribute = BytesToStruct<ResidentAttributeHeader>(mftBuffer[attributePosition..attributeListSize]);
                }
                else if (attribute.attributeType > (uint)AttributeTypes.FileName)
                {
                    break;
                }

                attributePosition += (int)attribute.length;
                int attributeEnd = attributePosition + Marshal.SizeOf(typeof(AttributeHeader));

                if ((uint)attributePosition < mftBuffer.Length - 1) //attributePos can get so large it overflows int, so we convert to uint
                {
                    attribute = BytesToStruct<AttributeHeader>(mftBuffer[attributePosition..attributeEnd]);
                }
                else
                {
                    attributePosition = mftBuffer.Length - 1 - Marshal.SizeOf(typeof(AttributeHeader));
                    attributeEnd = attributePosition + Marshal.SizeOf(typeof(AttributeHeader));
                    attribute = BytesToStruct<AttributeHeader>(mftBuffer[attributePosition..attributeEnd]);
                    break;
                }
            }

            return (fileNameAttribute, 0);

        }

        public FileData GetFileData(FileRecord fileRecord, FileNameAttributeHeader fileNameAttribute, int attributePosition, byte[] mftBuffer)
        {
            FileData file = new FileData();

            if (fileRecord.fileRecordHeader.HasValue)
            {
                int attributeSize = attributePosition + Marshal.SizeOf(typeof(FileNameAttributeHeader));

                int fileNameStart = attributeSize - 1;
                int fileNameEnd = fileNameStart + fileNameAttribute.fileNameLength * 2;

                for (int i = fileNameStart; i < fileNameEnd; i = i + 2)
                {
                    if (mftBuffer[i..(i + 2)].SequenceEqual(fileRecord.fixupArray[0..2]))
                    {
                        Array.Clear(mftBuffer, i, 2);

                        int j = i;

                        foreach (byte b in fileRecord.fixupArray[2..^0])
                        {
                            mftBuffer[j] = b;
                            j++;
                        }
                    }
                }

                string s = Encoding.Unicode.GetString(mftBuffer[fileNameStart..fileNameEnd]);

                file.ParentIndex = fileNameAttribute.GetParentRecordNumber();
                file.FileIndex = fileRecord.fileRecordHeader.Value.recordNum;
                file.FileName = s;
            }

            return file;
        }
        //unit test
        public void GetAttributeListFileNameEntry(byte[] mftBuffer, ResidentAttributeHeader attributeListAttribute, Dictionary<ulong, ulong> extensionRecordNums, ulong fileRecordNumber, int attributePosition)
        {
            int attributeListStart = attributePosition;
            int attributeListEnd = 0;


            attributeListStart += attributeListAttribute.attributeOffset;
            attributeListEnd = attributeListStart + Marshal.SizeOf(typeof(AttributeListEntry));

            if (attributeListAttribute.nonResident == 0)
            {
                uint listSize = attributeListAttribute.attributeLength;

                while (listSize > 0)
                {
                    AttributeListEntry entry = BytesToStruct<AttributeListEntry>(mftBuffer[attributeListStart..attributeListEnd]);

                    if (entry.attributeType == (uint)AttributeTypes.FileName)
                    {
                        if (entry.GetBaseRecordNumber() != fileRecordNumber)
                        {
                            extensionRecordNums[fileRecordNumber] = entry.GetBaseRecordNumber();
                            break;
                        }
                    }
                    attributeListStart += entry.attributeSize;
                    attributeListEnd = attributeListStart + Marshal.SizeOf(typeof(AttributeListEntry));
                    listSize -= entry.attributeSize;
                }
            }
        }
        public StringBuilder GetFilePath(FileData file,
            Dictionary<ulong, FileData> filesDict,
            Dictionary<ulong, ulong> extensionRecordNums)
        {
            StringBuilder filePath = new StringBuilder();
            ulong fallbackParent;
            FileData? parentFile;

            extensionRecordNums.TryGetValue(file.ParentIndex, out fallbackParent);

            bool isParentFound =
                filesDict.TryGetValue(file.ParentIndex, out parentFile) ||
                filesDict.TryGetValue(fallbackParent, out parentFile);

            if (isParentFound && parentFile != null)
            {
                if (file.FileIndex == file.ParentIndex)
                {
                    filePath = new StringBuilder(file.FileName);
                    return filePath;
                }

                if (!String.IsNullOrEmpty(parentFile.FilePath))
                {
                    filePath.Append(parentFile.FilePath);
                    filePath.Append("\\");
                    filePath.Append(file.FileName);
                    return filePath;
                }

                filePath.Append(GetFilePath(parentFile, filesDict, extensionRecordNums));

                filePath.Append("\\");
                filePath.Append(file.FileName);
                    
                return filePath;
            }
            else
            {
                return new StringBuilder(file.FileName);
            }

        }
    }
}
