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

    }
}
