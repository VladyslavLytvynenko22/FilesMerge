using System.Collections.ObjectModel;

namespace FilesMerge.Models
{
    interface IModel
    {
        void LoadFiles();
        string SaveFolderPath { get; set; }
        string MergedFileText { get; set; }
        ObservableCollection<string> ImportsOfAllFiles { get; set; }
        ObservableCollection<FileInfo> FileList { get; set; }
        string FolderPath { get; set; }
    }
}
