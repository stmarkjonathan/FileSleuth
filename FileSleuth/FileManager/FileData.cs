using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileSleuth.FileManager
{
    public class FileData
    {
        public ulong ParentIndex { get; set; }
        public ulong FileIndex { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public FileData()
        {
            ParentIndex = 0;
            FileIndex = 0;
            FileName = "";
            FilePath = "";
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
