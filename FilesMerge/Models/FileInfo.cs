using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FilesMerge.Models
{
    public class FileInfo : IModel, IComparable
    {
        #region members
        public string _path{ get; private set; }

        public string _name { get; private set; }

        public string _fullPathName { get; private set; }

        public string _fullFileText { get; private set; }

        public List<string> _imports { get; private set; }
        public string _namespace { get; private set; }

        public string _classTextInNamespace { get; private set; }

        public bool _hasMainMethod { get; private set; }


        static private string _mergedFileText;
        public string MergedFileText { get { return FileInfo._mergedFileText; } set { FileInfo._mergedFileText = value; } }

        static private ObservableCollection<string> _importsOfAllFiles = new ObservableCollection<string>();
        public ObservableCollection<string> ImportsOfAllFiles { get { return FileInfo._importsOfAllFiles; }  set { FileInfo._importsOfAllFiles = value; } }

        static private ObservableCollection<FileInfo> _fileList = new ObservableCollection<FileInfo>();
        public ObservableCollection<FileInfo> FileList { get { return FileInfo._fileList; } set { FileInfo._fileList = value; } }

        static private string _folderPath;
        public string FolderPath { get { return FileInfo._folderPath; } set { FileInfo._folderPath = value; } }

        static public string _saveFolderPath;

        public string SaveFolderPath { get { return _saveFolderPath; } set { FileInfo._saveFolderPath = value; } }
        #endregion

        #region Constructors
        public FileInfo() { }

        public FileInfo(string fullPathName)
        {
            OpenFiles(fullPathName);
        }
        #endregion

        #region Private methods
        private void FindMainMethodAndSetProperty(string textFromFile)
        {
            string pattern = @"Main\(string\s*\[\s*\]\s+\w+\)\s*\{";
            Match match = Regex.Match(textFromFile, pattern);

            this._hasMainMethod = match.Equals(Match.Empty) ? false : true;
        }

        private void ParseAllClassesInNamespace(string textFromFile)
        {
            string pattern = @"namespace\s+\S+\w+\s+\{(?<allClasses>[\w\W]+)\}";
            Match match = Regex.Match(textFromFile, pattern);

            this._classTextInNamespace = match.Groups["allClasses"].Value;
        }

        private void ParseNamespace(string textFromFile)
        {
            string pattern = @"namespace\s+(?<namespace>\w+)";
            Match match = Regex.Match(textFromFile, pattern);

            this._namespace = match.Groups["namespace"].Value;
        }

        private void ParseImports(string textFromFile)
        {
            this._imports = new List<string>();
            string pattern = @"using\s+(?<import>[\w\.]+);";

            foreach (Match match in Regex.Matches(textFromFile, pattern))
            {
                this._imports.Add(match.Groups["import"].Value);
            }
        }

        public void LoadFiles()
        {
            try
            {
                var files = System.IO.Directory.EnumerateFiles(FileInfo._folderPath, "*.cs", System.IO.SearchOption.AllDirectories);

                if (files.Count() == 0) return;

                string[] fileNames = files.ToArray();

                foreach (string fileName in fileNames)
                {
                    FileInfo fileInfo = new FileInfo(fileName);

                    AddFileInfoIfNotFoundInList(fileInfo);
                }

                List<FileInfo> updatedFileList = new List<FileInfo>(FileInfo._fileList);
                updatedFileList.Sort();

                FileInfo._fileList = new ObservableCollection<FileInfo>(updatedFileList);

                RebuildMergedFileText();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RebuildMergedFileText()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(Environment.NewLine);

                foreach (string import in FileInfo._importsOfAllFiles)
                {
                    stringBuilder.Append("using " + import + ";" + Environment.NewLine);
                }

                stringBuilder.Append(Environment.NewLine + "namespace " + FileInfo._fileList[0]._namespace +
                    Environment.NewLine + "{" + Environment.NewLine);

                foreach (FileInfo file in FileInfo._fileList)
                {
                    foreach (string import in FileInfo._importsOfAllFiles)
                    {
                        string[] usg = import.Split('.');
                        if (!usg.Contains(FileInfo._fileList[0]._namespace)) continue;
                        file.DeleteUnnecessaryUsingsInNamespaceClassText(usg, import);
                    }

                    int lineLength = 80;

                    string commentLine = new String('/', lineLength) + Environment.NewLine;
                    string fileNamePrefix = "//  Code from: " + file._name;

                    string fileNameSuffix = new String(' ', lineLength - fileNamePrefix.Length - 2) +
                        "//" + Environment.NewLine;

                    stringBuilder.Append(Environment.NewLine + commentLine + fileNamePrefix +
                        fileNameSuffix + commentLine);

                    stringBuilder.Append(file._classTextInNamespace);
                }

                stringBuilder.Append("}" + Environment.NewLine);

                FileInfo._mergedFileText = stringBuilder.ToString();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

        }

        private void AddFileInfoIfNotFoundInList(FileInfo fileInfo)
        {
            try
            {
                if (!FileInfo._fileList.Any(file => file._fullPathName.Equals(fileInfo._fullPathName)))
                {
                    FileInfo._fileList.Add(fileInfo);
                    AddImportsFromFileInfo(fileInfo);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void AddImportsFromFileInfo(FileInfo fileInfo)
        {
            try
            {
                List<string> updatedImports = new List<string>(fileInfo._imports.Union(FileInfo._importsOfAllFiles));
                updatedImports.Sort();

                FileInfo._importsOfAllFiles = new ObservableCollection<string>(updatedImports);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Public methods
        public void OpenFiles(string fullPathName)
        {
            string[] pathComponents = fullPathName.Split('\\');
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pathComponents.Length - 1; i++)
            {
                if (i > 0)
                {
                    sb.Append('\\');
                }
                sb.Append(pathComponents[i]);
            }

            this._path = sb.ToString();
            this._name = pathComponents[pathComponents.Length - 1];
            this._fullPathName = fullPathName;

            _fullFileText = File.ReadAllText(@fullPathName);

            ParseImports(_fullFileText);
            ParseNamespace(_fullFileText);
            ParseAllClassesInNamespace(_fullFileText);

            FindMainMethodAndSetProperty(_fullFileText);
        }
        

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            FileInfo otherFileInfo = obj as FileInfo;

            if (otherFileInfo != null)
            {
                if (this._hasMainMethod && !otherFileInfo._hasMainMethod)
                {
                    return -1;
                }
                else if (!this._hasMainMethod && otherFileInfo._hasMainMethod)
                {
                    return 1;
                }
                else
                {

                    if (this._name.Equals(otherFileInfo._name))
                    {
                        return this._fullPathName.CompareTo(otherFileInfo._fullPathName);
                    }
                    else
                    {
                        return this._name.CompareTo(otherFileInfo._name);
                    }

                }
            }
            else
            {
                throw new ArgumentException("Object is not a FileInfo");
            }
        }

        public  void DeleteUnnecessaryUsingsInNamespaceClassText(string[] unnecessaryUsingsInNamespaceClassText, string unnecessaryUsingInNamespaceClassText)
        {
            this._classTextInNamespace = this._classTextInNamespace.Replace(unnecessaryUsingInNamespaceClassText + ".", "");
            foreach (var item in unnecessaryUsingsInNamespaceClassText)
            {

                this._classTextInNamespace = this._classTextInNamespace.Replace("(" + item + ".", "(");
                this._classTextInNamespace = this._classTextInNamespace.Replace(" " + item + ".", " ");
                this._classTextInNamespace = this._classTextInNamespace.Replace("[" + item + ".", "[");
            }
        }
        #endregion
    }
}
