using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileSleuth.FileIndexManager;
using FileSleuth.FileManager;
using FileSleuth.MFTManager;
using FileSleuth.Model;
using System.Collections.ObjectModel;
using System.IO;
using static FileSleuth.FileManager.FileManagerObjects;
using static FileSleuth.MFTManager.MFTObjects;
using static FileSleuth.Structs;
using static System.Net.WebRequestMethods;

namespace FileSleuth.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {      
        private List<FileData> filesList;

        private Dictionary<string, FileData> fileIndex;

        [ObservableProperty]
        private ObservableCollection<FileData> displayList;

        [ObservableProperty]
        private string searchQuery;

        public MainWindowViewModel() 
        {
            filesList = new List<FileData>();
            searchQuery = string.Empty;
            DisplayList = new ObservableCollection<FileData>();
            fileIndex = FileIndexHandler.IndexFiles(filesList);
        }

        [RelayCommand]
        private async Task ScanFiles()
        {
            string driveName = DriveInfo.GetDrives()[0].Name;
            filesList = new List<FileData>(await Task.Run(() => InitializeFileDictionary(driveName).Values));
            DisplayList = new ObservableCollection<FileData>(filesList);
            fileIndex = FileIndexHandler.IndexFiles(filesList);
        }

        public List<FileData> SearchFiles()
        {

            List<FileData> foundFiles = new List<FileData>();

            if (!String.IsNullOrWhiteSpace(SearchQuery))
            {

                foreach (FileData file in fileIndex.Values)
                {
                    if (file.FileName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        foundFiles.Add(file);
                    }
                }
            }

            return foundFiles;
        }

        private Dictionary<ulong, FileData> InitializeFileDictionary(string driveName)
        {
            Volume foo = new Volume(driveName);
            MFTFile mftFoo = new MFTFile(foo);
            DataRunHandler dataRunHandler = new DataRunHandler(mftFoo);

            List<(byte[] mftBuffer, ulong filesToLoad)> data = dataRunHandler.EnumerateDataRuns();
            Dictionary<ulong, ulong> extensionRecordNums = new Dictionary<ulong, ulong>();
            Dictionary<ulong, FileData> filesDict = new Dictionary<ulong, FileData>();
            FileRecordHandler fileRecordHandler = new FileRecordHandler();

            foreach ((byte[] mftBuffer, ulong filesToLoad) dataRunData in data)
            {
                ulong filePos = 0;

                while (filePos < dataRunData.filesToLoad)
                {

                    FileRecord fileRecord = fileRecordHandler.FindFileRecord(filePos, dataRunData.mftBuffer);

                    if (fileRecord.fileRecordHeader.HasValue == false)
                    {
                        filePos++;
                        continue;
                    }

                    FileRecordHeader fileRecordHeader = fileRecord.fileRecordHeader.Value;

                    (FileNameAttributeHeader fileNameAttribute, int attributePosition) = fileRecordHandler.GetFileNameAttribute(
                        dataRunData.mftBuffer, fileRecordHeader.firstAttributeOffset, filePos, out AttributeListInfo attributeListInfo);

                    if (!fileNameAttribute.Equals(default(FileNameAttributeHeader)))
                    {
                        FileData file = fileRecordHandler.GetFileData(fileRecord, fileNameAttribute, attributePosition, dataRunData.mftBuffer);
                        filesDict[file.FileIndex] = file;
                    }

                    if (attributeListInfo.hasAttributeList)
                    {
                        fileRecordHandler.GetAttributeListFileNameEntry(dataRunData.mftBuffer, attributeListInfo.attributeListAttribute,
                            extensionRecordNums, fileRecordHeader.recordNum, attributeListInfo.attributeListPosition);
                    }

                    filePos++;

                }

                filesDict[5].FileName = driveName.Substring(0, driveName.Length - 1);
            }

            foreach (var file in filesDict.Values)
            {
                if (file != null)
                {
                    file.FilePath = fileRecordHandler.GetFilePath(file, filesDict, extensionRecordNums).ToString();
                }

            }

            return filesDict;
        }
    }
}
