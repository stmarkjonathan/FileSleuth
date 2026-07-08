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

        public FileRecordHeader? FindFileRecord(int filePos, byte[] mftBuffer)
        {
            int fileRecordPosition = MFT_FILE_SIZE * (int)filePos;         

            FileRecordHeader fileRecord =
                BytesToStruct<FileRecordHeader>(mftBuffer[fileRecordPosition..(fileRecordPosition + Marshal.SizeOf(typeof(FileRecordHeader)))]);

            if ((fileRecord.flags & (ushort)FileRecordFlags.InUse) != 1)
            {
                return null;
            }

            return fileRecord;
        }

    }
}
