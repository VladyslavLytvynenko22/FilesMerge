using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FilesMerge.Views
{
    public partial class MainForm : Form, IView
    {
        public event Action<string> OpenFolder_Click;
        public event Action ShowMarginFIle_Click;
        public event Action<string> NodeSelectChange;
        public event Action<bool> TextMargedChanged;
        public event Action<string> Save;
        private bool fileOpened = false;
        public MainForm()
        {
            InitializeComponent();
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveFile();
                e.SuppressKeyPress = true;
            }
        }

        public void Run() { Application.Run(this); }

        private void toolStripButtonOpenFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = this.folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string folderPath = this.folderBrowserDialog1.SelectedPath.ToString();
                if (folderPath != null || folderPath != string.Empty || !System.IO.Directory.Exists(folderPath))
                {
                    OpenFolder_Click(folderPath);
                    TextMargedChanged(false);
                    fileOpened = false;
                }
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        public void ShowFileName(List<string> fileNames)
        {
            foreach (var item in fileNames)
            {
                this.treeView1.Nodes.Add(new TreeNode(item));
            }
        }

        public void ShowMarginText(string marginText)
        {
            this.richTextBox1.Text = marginText;
        }

        private void btnShowMarginFIle_Click(object sender, EventArgs e)
        {
            ShowMarginFIle_Click();
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            NodeSelectChange(e.Node.Text);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            TextMargedChanged(true);
        }

        private void toolStripButtonSaveMarginFile_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            if (!fileOpened)
            {
                MessageBox.Show("No file for save.\nNot opened folder!");
                return;
            }
            this.saveFileDialog1.Title = "Save an Class file";
            this.saveFileDialog1.Filter = "Class Files|*.cs";
            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK && this.saveFileDialog1.FileName != null && this.saveFileDialog1.FileName != string.Empty)
            {
                Save(System.IO.Path.GetFullPath(this.saveFileDialog1.FileName));
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        public string GetChangedText()
        {
            return this.richTextBox1.Text;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (System.IO.Directory.Exists(path))
                {
                    OpenFolder_Click(path);
                    TextMargedChanged(false);
                }
            }
        }

        private void MainForm_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
    }
}
