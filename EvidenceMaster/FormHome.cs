using System.Drawing.Imaging;
using Microsoft.VisualBasic;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.Collections.Specialized;

namespace EvidenceMaster
{
    public partial class FormHome : Form
    {
        const string SYMBOL_TITLE = "+";
        const string SYMBOL_IMAGE = "-";

        private Contents _contents = new Contents();

        public FormHome()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void listViewContents_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + V
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsImage())
                {
                    System.Drawing.Image image = Clipboard.GetImage();

                    string imageFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png");
                    image.Save(imageFilePath, ImageFormat.Png);

                    using var formInputContent = new FormInputContent(Content.Types.Image, imageFilePath);
                    if (formInputContent.ShowDialog() == DialogResult.OK)
                    {
                        listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_IMAGE, formInputContent.ContentName));
                        _contents.AddImage(formInputContent.ContentName, imageFilePath);
                    }
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    StringCollection fileDropList = Clipboard.GetFileDropList();
                    if (fileDropList != null && fileDropList.Count == 1)
                    {
                        string filePath = fileDropList[0];
                        string extension = Path.GetExtension(filePath);

                        List<string> supportedExtensions = new List<string> { ".bmp", ".jpg", ".jpeg", ".png", ".gif" };

                        if (supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png");
                            File.Copy(filePath, tempFilePath, true);

                            using var formInputContent = new FormInputContent(Content.Types.Image, tempFilePath);
                            if (formInputContent.ShowDialog() == DialogResult.OK)
                            {
                                _contents.AddImage(formInputContent.ContentName, tempFilePath);
                                listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_IMAGE, formInputContent.ContentName));
                            }
                        }
                    }
                }
            }
            // Delete
            else if (e.KeyCode == Keys.Delete && listViewContents.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewContents.SelectedItems[0];
                int selectedIndex = selectedItem.Index;
                listViewContents.Items.Remove(selectedItem);
                _contents.RemoveAt(selectedIndex);
            }
            // Ctrl+Up
            else if (e.Control && e.KeyCode == Keys.Up && listViewContents.SelectedItems.Count > 0)
            {
                int selectedIndex = listViewContents.SelectedItems[0].Index;

                if (selectedIndex > 0)
                {
                    ListViewItem selectedItem = listViewContents.SelectedItems[0];
                    ListViewItem previousItem = listViewContents.Items[selectedIndex - 1];

                    listViewContents.Items.Remove(selectedItem);
                    listViewContents.Items.Insert(selectedIndex - 1, selectedItem);

                    _contents.MoveUp(selectedIndex);

                    listViewContents.Items[selectedIndex - 1].Selected = true;
                }
            }
            // Ctrl + Down
            else if (e.Control && e.KeyCode == Keys.Down && listViewContents.SelectedItems.Count > 0)
            {
                int selectedIndex = listViewContents.SelectedItems[0].Index;

                if (selectedIndex < listViewContents.Items.Count - 1)
                {
                    ListViewItem selectedItem = listViewContents.SelectedItems[0];
                    ListViewItem nextItem = listViewContents.Items[selectedIndex + 1];

                    listViewContents.Items.Remove(selectedItem);
                    listViewContents.Items.Insert(selectedIndex + 1, selectedItem);

                    _contents.MoveDown(selectedIndex);

                    listViewContents.Items[selectedIndex + 1].Selected = true;
                }
            }
            // V
            else if (e.KeyCode == Keys.V)
            {
                if (listViewContents.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewContents.SelectedItems[0];
                    int selectedIndex = selectedItem.Index;

                    using var formInputContent = new FormInputContent(_contents[selectedIndex].Type, _contents[selectedIndex].FilePath);
                    formInputContent.setDefaultName(_contents[selectedIndex].Name);
                    if (formInputContent.ShowDialog() == DialogResult.OK)
                    {
                        selectedItem.Text = String.Format("{0} {1}", _contents[selectedIndex].Type == Content.Types.Title ? SYMBOL_TITLE : SYMBOL_IMAGE, formInputContent.ContentName);
                        _contents[selectedIndex].Name = formInputContent.ContentName;
                    }
                }
            }
            // T
            else if (e.KeyCode == Keys.T)
            {
                using var formInputContent = new FormInputContent(Content.Types.Title);
                if (formInputContent.ShowDialog() == DialogResult.OK)
                {
                    int selectedIndex = listViewContents.SelectedIndices.Count > 0 ? listViewContents.SelectedIndices[0] : -1;

                    if (selectedIndex >= 0)
                    {
                        listViewContents.Items.Insert(selectedIndex, String.Format("{0} {1}", SYMBOL_TITLE, formInputContent.ContentName));
                        _contents.AddTitle(formInputContent.ContentName, selectedIndex);
                    }
                    else
                    {
                        listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_TITLE, formInputContent.ContentName));
                        _contents.AddTitle(formInputContent.ContentName);
                    }
                }
            }
            //  Ctrl+C
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (listViewContents.SelectedItems.Count > 0)
                {
                    string selectedItemText = listViewContents.SelectedItems[0].Text;
                    Clipboard.SetText(selectedItemText.Substring(2));
                }
            }
        }

        private void listViewContents_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }

        }

        private void listViewContents_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1) // Verifica se è stato trascinato un solo file
            {
                return;
            }

            string file = files[0]; // Prende il primo (ed unico) file trascinato

            string extension = Path.GetExtension(file);
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            if (supportedExtensions.Contains(extension.ToLower()))
            {

                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png");
                File.Copy(file, tempFilePath, true);

                using var formInputContent = new FormInputContent(Content.Types.Image, tempFilePath);
                if (formInputContent.ShowDialog() == DialogResult.OK)
                {
                    _contents.AddImage(formInputContent.ContentName, tempFilePath);
                    listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_IMAGE, formInputContent.ContentName));
                }
            }
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), String.Format("{0}_{1:yyyyMMddHHmmss}.docx", textBoxReference.Text, DateTime.Now));
            if (isShiftPressed)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Documento Word (*.docx)|*.docx";
                saveFileDialog.Title = "Salva il file";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    outputPath = saveFileDialog.FileName;
                }
            }

            string header = String.Format("{0}\t\t\t{1}\t\t\t{2}", comboBoxCI.Text, textBoxReference.Text, Environment.UserName);

            if (_contents.CreateDocx(outputPath, header))
            {
                MessageBox.Show(String.Format("File salvato al percorso: {0}", outputPath));
            }
            else
            {
                MessageBox.Show("Si è verificato un errore durante la creazione del file.");
            }
        }
    }
}