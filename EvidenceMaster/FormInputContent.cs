
namespace EvidenceMaster
{
    public partial class FormInputContent : Form
    {
        private const int MaxImageWidth = 1440;
        private const int MaxImageHeight = 810;

        private readonly Content.Types ContentType;
        public string? ContentName { get; set; }
        private string? ImageFilePath;


        public FormInputContent(Content.Types contentType, string? imageFilePath = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ContentType = contentType;
            this.ImageFilePath = imageFilePath;

            if (this.ContentType == Content.Types.Title)
            {
                pictureBoxPreview.Visible = false;
                textBoxName.Width = 500;
                this.Size = new Size(544, 87);
            }
            else if (this.ContentType == Content.Types.Image)
            {
                if (string.IsNullOrEmpty(this.ImageFilePath))
                {
                    MessageBox.Show("Percorso dell'immagine non trovato");
                    this.Close();
                    return;
                }
                pictureBoxPreview.Visible = true;
                if (pictureBoxPreview.Image != null)
                {
                    pictureBoxPreview.Image.Dispose();
                    pictureBoxPreview.Image = null;
                }

                Image image = Image.FromFile(this.ImageFilePath);


                float scaleFactor = image.Width / (float)image.Height >= pictureBoxPreview.Width / (float)pictureBoxPreview.Height ? pictureBoxPreview.Width / (float)image.Width : pictureBoxPreview.Height / (float)image.Height;
                int newWidth = (int)(image.Width * scaleFactor);
                int newHeight = (int)(image.Height * scaleFactor);


                Image scaledImage = new Bitmap(newWidth, newHeight);
                using (Graphics graphics = Graphics.FromImage(scaledImage))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                // Rilascia l'immagine originale
                image.Dispose();

                pictureBoxPreview.Image = scaledImage;
                pictureBoxPreview.Enabled = true;
            }

            // Imposta il focus sul textBoxName all'apertura del form
            textBoxName.Focus();
        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ContentName = textBoxName.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

    }
}
