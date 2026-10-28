using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.Structs;

namespace FileSleuth.MFTManager
{
    public static class MFTObjects
    {
        public class FileRecord
        {
            public Nullable<FileRecordHeader> fileRecordHeader { get; set; }
            public byte[] fixupArray { get; set; }

            public FileRecord()
            {
                fixupArray = new byte[0];
            }
           
        }
    }
}
