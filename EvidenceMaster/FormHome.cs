using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;

namespace EvidenceMaster
{
    public partial class FormHome : Form
    {
        private Contents _contents;
        private string _defaultOutputDirectory;

        public FormHome()
        {
            InitializeComponent();

            // Impedisci di ridimensionare la finestra
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;

            string ciFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ci.txt");
            if (File.Exists(ciFilePath))
            {
                string[] ci = File.ReadAllLines(ciFilePath);
                Array.Sort(ci);
                comboBoxCI.Items.AddRange(ci);
            }

            string savedOutputDirectory = Properties.Settings.Default.savedOutputDirectory;
            _defaultOutputDirectory = String.IsNullOrEmpty(savedOutputDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : savedOutputDirectory;

            comboBoxCI.Text = Properties.Settings.Default.lastCI;
            textBoxReference.Text = Properties.Settings.Default.lastReference;
            string serializedContents = Properties.Settings.Default.lastContents;
            if (!String.IsNullOrEmpty(serializedContents))
            {
                _contents = new Contents(serializedContents);
                listViewContents_Update();
            }
            else
            {
                _contents = new Contents();
            }

            _contents.ProgressUpdated += onProgressUpdated;

        }

        private void FormHome_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.lastCI = this.comboBoxCI.Text;
            Properties.Settings.Default.lastReference = this.textBoxReference.Text;
            Properties.Settings.Default.lastContents = _contents.Serialize();
            Properties.Settings.Default.Save();
        }

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

        private void onProgressUpdated(object? sender, int progressPercentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => progressBarCreation.Value = progressPercentage));
            }
            else
            {
                progressBarCreation.Value = progressPercentage;
            }
        }

        private async void buttonGo_Click(object sender, EventArgs e)
        {
            bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            if (comboBoxCI.Text == "" || textBoxReference.Text == "" || listViewContents.Items.Count == 0)
            {
                MessageBox.Show("Impossibile proseguire. I campi CI, Reference e Lista dei contenuti non possono essere vuoti.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string outputPath = Path.Combine(_defaultOutputDirectory, String.Format("{0}.{1}", textBoxReference.Text, isCtrlPressed ? "pdf" : "docx"));

            if (isShiftPressed)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = _defaultOutputDirectory;
                saveFileDialog.FileName = textBoxReference.Text;
                saveFileDialog.Filter = isCtrlPressed ? "Documento PDF (*.pdf)|*.pdf" : "Documento Word (*.docx)|*.docx";
                saveFileDialog.Title = "Salva il file";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    outputPath = saveFileDialog.FileName;
                    string? outputDirectory = Path.GetDirectoryName(outputPath);
                    if (!String.IsNullOrEmpty(outputDirectory))
                    {
                        _defaultOutputDirectory = outputDirectory;
                        Properties.Settings.Default.savedOutputDirectory = outputDirectory;
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (File.Exists(outputPath))
                {
                    if (MessageBox.Show("Il file è già esistente. Vuoi sostituirlo?", "Sostituzione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        File.Delete(outputPath);
                    }
                    else
                    {
                        MessageBox.Show("Creazione del file interrotta.", "Interruzione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            string header = String.Format("{0}\t\t{1}", comboBoxCI.Text, textBoxReference.Text);
            string footer = "Footer Test";

            if (await _contents.CreateDocx(outputPath, isCtrlPressed, header, footer))
            {
                DialogResult confirmResult = MessageBox.Show(String.Format("File salvato al seguente percorso:\n\n{0}\n\nAprire la cartella di destinazione?", outputPath), "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    Process.Start("explorer.exe", "/select, \"" + outputPath + "\"");
                }
            }
            else
            {
                MessageBox.Show("Si è verificato un errore durante la creazione del file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            string testoPopup = $@"{this.Text}
Riccardo Pietrini

Interfaccia:
    - Menu Configuration Item
    - Campo Riferimenti
    - Lista Contenuti
    - Barra di Progresso
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
    - SHIFT+CLICK - Crea il Documento DOCX sulla posizione scelta (che diventa la nuova posizione di default) secondo i Contenuti
    - CTRL+CLICK - Crea il Documento PDF sul Desktop secondo i Contenuti
    - CTRL+SHIFT+CLICK - Crea il documento PDF sulla posizione scelta (che diventa la nuova posizione di default) secondo i Contenuti";

            MessageBox.Show(testoPopup, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}