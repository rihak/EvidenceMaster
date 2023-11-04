
namespace EvidenceMaster
{
    public partial class FormInputContent : Form
    {
        private readonly Content.Types ContentType;
        public string ContentName { get; set; }
        public string? ImageFilePath;

        private Rectangle _rectangle;
        private List<Rectangle>? _rectangles;
        private bool _rectangleIsDrawing;
        private float _scaleFactor;
        private int _borderWidth = 3;

        public FormInputContent(Content.Types contentType, string? imageFilePath = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ContentType = contentType;
            this.ImageFilePath = imageFilePath;
            this._rectangleIsDrawing = false;

            if (this.ContentType == Content.Types.Title)
            {
                pictureBoxPreview.Visible = false;
                textBoxName.Width = 500;
                this.Size = new Size(544, 87);
            }
            else if (this.ContentType == Content.Types.Image)
            {
                this.loadImage();
                _rectangles = new List<Rectangle>();
            }

            textBoxName.Focus();
            ContentName = textBoxName.Text;
        }

        public void setDefaultName(string name)
        {
            this.Text = $"Definizione Contenuto \"{name}\"";
            textBoxName.Text = name;
            textBoxName.SelectAll();
            textBoxName.Focus();
        }

        public void loadImage()
        {
            if (string.IsNullOrEmpty(this.ImageFilePath))
            {
                MessageBox.Show("Percorso immagine non trovato");
                ContentName = "";
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


            _scaleFactor = image.Width / (float)image.Height >= pictureBoxPreview.Width / (float)pictureBoxPreview.Height ? pictureBoxPreview.Width / (float)image.Width : pictureBoxPreview.Height / (float)image.Height;
            int newWidth = (int)(image.Width * _scaleFactor);
            int newHeight = (int)(image.Height * _scaleFactor);


            Image scaledImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(scaledImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            image.Dispose();

            pictureBoxPreview.Image = scaledImage;
            pictureBoxPreview.Enabled = true;
        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ContentName = textBoxName.Text;
                if (this.ContentType == Content.Types.Image)
                {
                    pictureBoxPreview.Dispose();
                    pictureBoxPreview = null;
                    saveRectangles();
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void pictureBoxPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_rectangleIsDrawing)
            {
                _rectangleIsDrawing = true;
                _rectangle = new Rectangle(e.Location, new Size(0, 0));
            }
        }

        private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rectangleIsDrawing)
            {
                _rectangle.Width = e.X - _rectangle.X;
                _rectangle.Height = e.Y - _rectangle.Y;
            }
        }

        private void pictureBoxPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (_rectangleIsDrawing)
            {
                _rectangleIsDrawing = false;

                using (Graphics graphics = Graphics.FromImage(pictureBoxPreview.Image))
                {
                    Pen pen = new Pen(Color.Red, _borderWidth);
                    graphics.DrawRectangle(pen, _rectangle);
                }
                if (_rectangles != null)
                {
                    _rectangles.Add(_rectangle);
                }
                _rectangle = Rectangle.Empty;
                pictureBoxPreview.Invalidate();
            }
        }

        private void pictureBoxPreview_DoubleClick(object sender, EventArgs e)
        {
            if (_rectangles != null && _rectangles.Count > 0)
            {
                _rectangles.Clear();
            }
            this.loadImage();
        }

        private void saveRectangles()
        {
            if (_rectangles != null && _rectangles.Count > 0 && this.ImageFilePath != null)
            {
                using (Bitmap originalImage = new Bitmap(Image.FromFile(this.ImageFilePath)))
                {
                    using (Graphics graphics = Graphics.FromImage(originalImage))
                    {
                        using (Pen pen = new Pen(Color.Red, (int)(_borderWidth / _scaleFactor) + 1))
                        {
                            foreach (Rectangle rectangle in _rectangles)
                            {
                                int scaledX = (int)(rectangle.X / _scaleFactor);
                                int scaledY = (int)(rectangle.Y / _scaleFactor);
                                int scaledWidth = (int)(rectangle.Width / _scaleFactor);
                                int scaledHeight = (int)(rectangle.Height / _scaleFactor);
                                graphics.DrawRectangle(pen, scaledX, scaledY, scaledWidth, scaledHeight);
                            }
                        }
                    }

                    ImageFilePath = this.ImageFilePath + ".png";
                    originalImage.Save(this.ImageFilePath);
                }
            }
        }

    }
}
