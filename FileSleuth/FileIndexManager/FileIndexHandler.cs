using FileSleuth.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth.FileIndexManager
{
    internal class FileIndexHandler
    {
        public static Dictionary<string, FileData> IndexFiles(List<FileData> files)
        {
            Dictionary<string, FileData> fileIndex = new Dictionary<string, FileData>();

            foreach (FileData file in files)
            {
                if (file != null)
                {
                    fileIndex[file.FilePath] = file;
                }
            }

            return fileIndex;
        }
    }
}
