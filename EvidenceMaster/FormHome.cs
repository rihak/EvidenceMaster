using System.Drawing.Imaging;
using System.Collections.Specialized;

namespace EvidenceMaster
{
    public partial class FormHome : Form
    {
        private Contents _contents = new Contents();

        //private AutoCompleteStringCollection autoCompleteCollection;

        public FormHome()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            string ciFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ci.txt");
            if (File.Exists(ciFilePath))
            {
                string[] ci = File.ReadAllLines(ciFilePath);
                Array.Sort(ci);
                comboBoxCI.Items.AddRange(ci);
            }

            //autoCompleteCollection = new AutoCompleteStringCollection();
            //comboBoxCI.AutoCompleteCustomSource = autoCompleteCollection;
            //autoCompleteCollection.AddRange(comboBoxCI.Items.Cast<string>().ToArray());

        }

        //private void comboBoxCI_TextChanged(object sender, EventArgs e)
        //{
        //    var suggestions = comboBoxCI.Items.Cast<string>()
        //        .Where(item => item.ToLower().Contains(comboBoxCI.Text.ToLower()))
        //        .ToArray();

        //    comboBoxCI.AutoCompleteCustomSource.Clear();
        //    comboBoxCI.AutoCompleteCustomSource.AddRange(suggestions);
        //}

        private void listViewContents_Update()
        {
            listViewContents.Items.Clear();
            foreach (string item in _contents.ListDisplay())
            {
                listViewContents.Items.Add(item);
            }
        }

        private void listViewContents_KeyDown(object sender, KeyEventArgs e)
        {
            int selectedIndex = listViewContents.SelectedIndices.Count > 0 ? listViewContents.SelectedIndices[0] : -1;

            // CTRL+V - Incolla Immagine o File
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Immagine
                if (Clipboard.ContainsImage())
                {
                    string imageFilePath = tempRandomPngPath();
                    Clipboard.GetImage().Save(imageFilePath, ImageFormat.Png);

                    using var formInputContent = new FormInputContent(Content.Types.Image, imageFilePath);
                    if (formInputContent.ShowDialog() == DialogResult.OK)
                    {
                        _contents.AddImage(formInputContent.ContentName, imageFilePath);
                        listViewContents_Update();
                    }
                }
                // File
                else if (Clipboard.ContainsFileDropList())
                {
                    StringCollection fileDropList = Clipboard.GetFileDropList();
                    if (fileDropList != null && fileDropList.Count == 1)
                    {
                        string filePath = fileDropList[0] ?? "";
                        string extension = Path.GetExtension(filePath);

                        List<string> supportedExtensions = new List<string> { ".png" };

                        if (supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            string tempFilePath = tempRandomPngPath();
                            File.Copy(filePath, tempFilePath, true);

                            using var formInputContent = new FormInputContent(Content.Types.Image, tempFilePath);
                            if (formInputContent.ShowDialog() == DialogResult.OK)
                            {
                                _contents.AddImage(formInputContent.ContentName, tempFilePath);
                                listViewContents_Update();
                            }
                        }
                    }
                }
            }
            // DEL - Elimina Contenuto
            else if (e.KeyCode == Keys.Delete && listViewContents.SelectedItems.Count > 0)
            {
                _contents.RemoveAt(selectedIndex);
                listViewContents_Update();
            }
            // CTRL+UP - Sposta Contenuto Sopra
            else if (e.Control && e.KeyCode == Keys.Up && listViewContents.SelectedItems.Count > 0)
            {
                if (_contents.MoveUp(selectedIndex))
                {
                    listViewContents_Update();
                    listViewContents.Items[selectedIndex - 1].Selected = true;
                }
            }
            // CTRL+DOWN - Sposta Contenuto Sotto
            else if (e.Control && e.KeyCode == Keys.Down && listViewContents.SelectedItems.Count > 0)
            {
                if (_contents.MoveDown(selectedIndex))
                {
                    listViewContents_Update();
                    listViewContents.Items[selectedIndex + 1].Selected = true;
                }
            }
            // R|V - Rinomina o Visualizza Contenuto
            else if (e.KeyCode == Keys.R || e.KeyCode == Keys.V)
            {
                if (selectedIndex >= 0)
                {
                    using var formInputContent = new FormInputContent(_contents[selectedIndex].Type, _contents[selectedIndex].FilePath);
                    formInputContent.setDefaultName(_contents[selectedIndex].Name);
                    if (formInputContent.ShowDialog() == DialogResult.OK)
                    {
                        _contents[selectedIndex].Name = formInputContent.ContentName;
                        listViewContents_Update();
                    }
                }
            }
            // T - Aggiungi Titolo
            else if (e.KeyCode == Keys.T)
            {
                using var formInputContent = new FormInputContent(Content.Types.Title);
                if (formInputContent.ShowDialog() == DialogResult.OK)
                {
                    if (selectedIndex >= 0)
                    {
                        _contents.AddTitle(formInputContent.ContentName, selectedIndex);
                    }
                    else
                    {
                        _contents.AddTitle(formInputContent.ContentName);
                    }
                    listViewContents_Update();
                }
            }
            //  CTRL+C - Copia Nome Contenuto
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (selectedIndex >= 0)
                {
                    Clipboard.SetText(_contents[selectedIndex].Name);
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

            if (files.Length != 1)
            {
                return;
            }

            string file = files[0];

            string extension = Path.GetExtension(file);
            string[] supportedExtensions = { ".png" };

            if (supportedExtensions.Contains(extension.ToLower()))
            {
                string tempFilePath = tempRandomPngPath();
                File.Copy(file, tempFilePath, true);

                using var formInputContent = new FormInputContent(Content.Types.Image, tempFilePath);
                if (formInputContent.ShowDialog() == DialogResult.OK)
                {
                    _contents.AddImage(formInputContent.ContentName, tempFilePath);
                    listViewContents_Update();
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

            string header = String.Format("{0}\t\t{1}\t\t{2}", comboBoxCI.Text, textBoxReference.Text, Environment.UserName);

            if (_contents.CreateDocx(outputPath, header))
            {
                MessageBox.Show(String.Format("File salvato al percorso: {0}", outputPath));
            }
            else
            {
                MessageBox.Show("Si è verificato un errore durante la creazione del file.");
            }
        }

        private string tempRandomPngPath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png");
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Sei sicuro di voler reimpostare i contenuti?", "Conferma", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result != DialogResult.OK)
            {
                return;
            }

            comboBoxCI.Text = String.Empty;
            textBoxReference.Text = String.Empty;
            _contents.Clear();
            listViewContents_Update();
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string testoPopup = @"EvidenceMaster Help
                
Interfaccia:
    - Menu Configuration Item
    - Campo Riferimenti
    - Lista Contenuti
    - Pulsante Reset
    - Pulsante Go

Comandi Lista Contenuti:
    - CTRL+V - AGGIUNGI - Incolla Immagine o File Immagine PNG
    - TRASCINA - AGGIUNGI - Trascina un File Immagine PNG
    - T - AGGIUNGI - Aggiungi un Titolo
    - CTRL+C - RECUPERA - Copia il Nome del Contenuto
    - R/V - AGGIORNA - Rinomina o Visualizza il Contenuto Selezionato
    - CTRL+UP - AGGIORNA - Sposta Sopra il Contenuto Selezionato
    - CTRL+DOWN - AGGIORNA - Sposta Sotto il Contenuto Selezionato
    - DEL - ELIMINA  - Elimina il Contenuto Selezionato

Reset:
    Cancella tutti i contenuti.

Go:
    - CLICK - Crea il Documento DOCX sul Desktop secondo i Contenuti
    - SHIFT+CLICK - Crea il Documento DOCX sulla posizione scelta secondo i Contenuti
";

            MessageBox.Show(testoPopup, "Help", MessageBoxButtons.OK);
        }
    }
}