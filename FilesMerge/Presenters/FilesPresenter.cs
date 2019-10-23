using FilesMerge.Models;
using FilesMerge.Views;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FilesMerge.Presenters
{
    class FilesPresenter : IPresenter
    {
        #region members
        public bool _textMargedChanged { get; private set; }

        public IModel _fileInfo { get; set; }

        public IView _mainForm { get; set; }
        #endregion

        #region Constructors
        public FilesPresenter(IModel model, IView view)
        {
            this._fileInfo = model;
            this._mainForm = view;
            this._mainForm.OpenFolder_Click += MainForm_OpenFolder_Click;
            this._mainForm.ShowMarginFIle_Click += ShowMarginText;
            this._mainForm.NodeSelectChange += MainForm_NodeSelectChange;
            this._mainForm.TextMargedChanged += MainForm_TextMargedChanged;
            this._mainForm.Save += MainForm_Save;
        }
        #endregion

        #region Private methods
        private void MainForm_Save(string savePath)
        {
            try
            {
                _fileInfo.SaveFolderPath = savePath;
                if (_textMargedChanged)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want save changes?", "Save", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        _fileInfo.MergedFileText = _mainForm.GetChangedText();
                    }
                    else
                    {
                        this._mainForm.ShowMarginText(_fileInfo.MergedFileText);
                    }

                }
                CreateMergedFile();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void MainForm_TextMargedChanged(bool obj)
        {
            this._textMargedChanged = obj;
        }

        private void MainForm_NodeSelectChange(string nodeName)
        {
            try
            {
                foreach (var item in _fileInfo.FileList)
                {
                    if (item._name == nodeName)
                    {
                        this._mainForm.ShowMarginText(item._fullFileText);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void MainForm_OpenFolder_Click(string folderPath)
        {
            try
            {
                _fileInfo.FolderPath = folderPath;
                this._fileInfo.LoadFiles();
                List<string> listFileName = new List<string>();
                foreach (var item in _fileInfo.FileList)
                {
                    listFileName.Add(item._name);
                }
                this._mainForm.ShowFileName(listFileName);
                ShowMarginText();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void ShowMarginText()
        {
            try
            {
                this._mainForm.ShowMarginText(_fileInfo.MergedFileText);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void CreateMergedFile()
        {
            try
            {
                if (_fileInfo.MergedFileText != null) System.IO.File.WriteAllText(_fileInfo.SaveFolderPath, _fileInfo.MergedFileText);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        #endregion

        #region Public methods
        public void Run()
        {
            try
            {
                this._mainForm.Run();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}
