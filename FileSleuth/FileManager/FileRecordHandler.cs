using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.Enums;
using static FileSleuth.Structs;
using static FileSleuth.MFTManager.MFTMethods;
using static FileSleuth.Constants;
using static FileSleuth.FileManager.FileManagerObjects;

namespace FileSleuth.FileManager
{
    public class FileRecordHandler
    {

        public FileRecordHandler() 
        { 
             
        }

        /// <returns>Return a MFT File Record if one is found, or null if the file record found was not in use</returns>
        public Nullable<FileRecordHeader> FindFileRecord(ulong filePos, byte[] mftBuffer) 
        {

            int fileRecordPosition = MFT_FILE_SIZE * (int)filePos;         

            FileRecordHeader fileRecord =
                BytesToStruct<FileRecordHeader>(mftBuffer[fileRecordPosition..(fileRecordPosition + Marshal.SizeOf(typeof(FileRecordHeader)))]);

            if((fileRecord.flags & (ushort)FileRecordFlags.InUse) != 1)
            {
                return null;
            }

            if (fileRecord.magicNum == MFT_MAGIC_NUMBER)
            {
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

                    return (fileNameAttribute, attributePosition);
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

        public FileData GetFileData(FileRecordHeader fileRecord, FileNameAttributeHeader fileNameAttribute, int attributePosition, byte[] mftBuffer)
        {
            FileData file = new FileData();

            int attributeSize = attributePosition + Marshal.SizeOf(typeof(FileNameAttributeHeader));

            int fileNameStart = attributeSize - 1;
            int fileNameEnd = fileNameStart + fileNameAttribute.fileNameLength * 2;

            char[] chars = Encoding.Unicode.GetString(mftBuffer[fileNameStart..fileNameEnd]).ToCharArray();

            file.ParentIndex = fileNameAttribute.GetParentRecordNumber();
            file.FileIndex = fileRecord.recordNum;
            file.FileName = new string(chars);

            return file;
        }
    }
}
