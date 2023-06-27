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

        private List<Content> contents = new List<Content>();

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
                        contents.Add(new Content(Content.Types.Image, formInputContent.ContentName, imageFilePath));
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
                                contents.Add(new(Content.Types.Image, formInputContent.ContentName, tempFilePath));
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
                contents.RemoveAt(selectedIndex);
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

                    Content selectedContent = contents[selectedIndex];
                    contents.RemoveAt(selectedIndex);
                    contents.Insert(selectedIndex - 1, selectedContent);

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

                    Content selectedContent = contents[selectedIndex];
                    contents.RemoveAt(selectedIndex);
                    contents.Insert(selectedIndex + 1, selectedContent);

                    listViewContents.Items[selectedIndex + 1].Selected = true;
                }
            }
            // R
            else if (e.KeyCode == Keys.R)
            {
                if (listViewContents.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewContents.SelectedItems[0];
                    int selectedIndex = selectedItem.Index;
                    string newName = Interaction.InputBox("Inserisci il nuovo nome:", "Rinomina elemento", contents[selectedIndex].Name);
                    selectedItem.Text = String.Format("{0} {1}", contents[selectedIndex].Type == Content.Types.Title ? SYMBOL_TITLE : SYMBOL_IMAGE, newName);
                    contents[selectedIndex].Name = newName;
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
                        contents.Insert(selectedIndex, new Content(Content.Types.Title, formInputContent.ContentName));
                    }
                    else
                    {
                        listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_TITLE, formInputContent.ContentName));
                        contents.Add(new Content(Content.Types.Title, formInputContent.ContentName));
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
                    contents.Add(new(Content.Types.Image, formInputContent.ContentName, tempFilePath));
                    listViewContents.Items.Add(String.Format("{0} {1}", SYMBOL_IMAGE, formInputContent.ContentName));
                }
            }
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            // Crea un nuovo documento Word
            //string filePath = "output.docx";
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), String.Format("{0}_{1:yyyyMMddHHmmss}.docx", textBoxReference.Text, DateTime.Now));
            var doc = DocX.Create(outputPath);

            // Imposta il contenuto delle pagine
            var contentQuery = contents.OrderBy(content => contents.IndexOf(content));
            int sectionImageCount = 0;
            bool firstElement = true;
            foreach (var content in contentQuery)
            {
                // Aggiungi un titolo
                if (content.Type == Content.Types.Title)
                {
                    if (!firstElement)
                    {
                        doc.InsertSectionPageBreak();
                        sectionImageCount = 0;
                    }
                    doc.InsertParagraph(content.Name).Heading(HeadingType.Heading1).FontSize(24).Alignment = Alignment.center;
                    doc.InsertParagraph();
                }
                // Aggiungi un'immagine
                else if (content.Type == Content.Types.Image && !string.IsNullOrEmpty(content.FilePath))
                {
                    if (sectionImageCount == 2)
                    {
                        doc.InsertSectionPageBreak();
                        sectionImageCount = 0;
                    }
                    sectionImageCount++;

                    doc.InsertParagraph(content.Name).Bold().Alignment = Alignment.left;
                    var image = doc.AddImage(content.FilePath);
                    var picture = image.CreatePicture();

                    // Imposta la larghezza e l'altezza massima desiderate per l'immagine (ad esempio, 400 pixel per entrambe)
                    int maxWidth = 400;
                    int maxHeight = 400;
                    double widthScaleFactor = (double)maxWidth / picture.Width;
                    double heightScaleFactor = (double)maxHeight / picture.Height;
                    double scaleFactor = Math.Min(widthScaleFactor, heightScaleFactor);
                    if (scaleFactor < 1)
                    {
                        picture.Width = (int)(picture.Width * scaleFactor);
                        picture.Height = (int)(picture.Height * scaleFactor);
                    }

                    var paragraph = doc.InsertParagraph();
                    paragraph.Alignment = Alignment.center;
                    paragraph.AppendPicture(picture);
                    doc.InsertParagraph();
                }
                firstElement = false;
            }

            // Aggiungi gli header e i footer
            doc.AddHeaders();
            doc.AddFooters();

            // Indicate that the first page will have independent Headers/Footers
            doc.DifferentFirstPage = false;
            doc.DifferentOddAndEvenPages = false;

            // Insert a Paragraph in the Headers/Footers for the first page
            doc.Headers.Odd.InsertParagraph(String.Format("{0}\t\t\t{1}\t\t\t{2}", comboBoxCI.Text, textBoxReference.Text, Environment.UserName));
            //doc.Footers.Odd.InsertParagraph(String.Format("{0}", Environment.UserName));


            if (isShiftPressed)
            {
                // Crea un'istanza del SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                // Imposta il filtro dei tipi di file
                saveFileDialog.Filter = "Documento Word (*.docx)|*.docx";

                // Imposta il titolo del dialogo
                saveFileDialog.Title = "Salva il file";

                // Mostra la finestra di dialogo e verifica se l'utente ha premuto OK
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Ottieni il percorso del file selezionato dall'utente
                    outputPath = saveFileDialog.FileName;
                }
            }

            // Salva il documento Word
            doc.SaveAs(outputPath);

            // Apri il documento Word
            if (File.Exists(outputPath))
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