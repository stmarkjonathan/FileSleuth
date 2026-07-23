using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth
{
    public static class Constants
    {
        public const int MFT_FILE_SIZE = 1024;
        public const int MFT_FILES_PER_BUFFER = 65536;
        public const uint MFT_MAGIC_NUMBER = 0x454C4946;
    }
}
