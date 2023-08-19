using Microsoft.Office.Interop.Word;

namespace EvidenceMaster
{
    public class Content
    {
        public enum Types
        {
            Image,
            Title
        }

        public static Dictionary<Content.Types, string> TypeSymbols = new Dictionary<Content.Types, string>()
        {
            {Content.Types.Image, "-"},
            {Content.Types.Title, "+"},
        };

        private Types _type;
        private string _name;
        private string? _filePath;

        public Types Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string? FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public Content(Types type, string name, string? filePath = null)
        {
            _type = type;
            _name = name;
            _filePath = filePath;
        }
    }

    public class Contents
    {
        const char serializationTitle = 'T';
        const char serializationImage = 'I';
        const char serializationContentSplit = '|';
        const char serializationContentImageSplit = '?';

        private List<Content> _list;

        public Content this[int index] => _list[index];

        public event EventHandler<int>? ProgressUpdated;

        public List<Content> List
        {
            get { return _list; }
            set { _list = value; }
        }

        public Contents()
        {
            _list = new List<Content>();
        }

        public Contents(string serial)
        {
            _list = new List<Content>();

            string[] serialContents = serial.Split(serializationContentSplit);

            foreach (string serialContent in serialContents)
            {
                if (!string.IsNullOrEmpty(serialContent))
                {
                    if (serialContent[0] == serializationTitle)
                    {
                        string serialTitle = serialContent[1..];
                        this.AddTitle(serialTitle);
                    }
                    else if (serialContent[0] == serializationImage)
                    {
                        string serialImage = serialContent[1..];
                        string[] seralImageProperties = serialImage.Split(serializationContentImageSplit);
                        this.AddImage(seralImageProperties[0], seralImageProperties[1]);
                    }
                }
            }
        }

        public string Serialize()
        {
            string serial = "";
            foreach (var content in _list.OrderBy(content => _list.IndexOf(content)))
            {
                if (content.Type == Content.Types.Title)
                {
                    serial += $"{serializationTitle}{content.Name}{serializationContentSplit}";
                }
                else if ((content.Type == Content.Types.Image) && (!string.IsNullOrEmpty(content.FilePath)))
                {
                    serial += $"{serializationImage}{content.Name}{serializationContentImageSplit}{content.FilePath}{serializationContentSplit}";
                }
            }
            return serial;
        }


        public List<string> ListDisplay()
        {
            List<string> listDisplay = new List<string>();
            foreach (var content in _list)
            {
                listDisplay.Add($"{Content.TypeSymbols[content.Type]} {content.Name}");
            }
            return listDisplay;
        }

        public void AddImage(string name, string filePath, int? index = null)
        {
            if (index is null)
            {
                _list.Add(new Content(Content.Types.Image, name, filePath));
            }
            else
            {
                _list.Insert((int)index, new Content(Content.Types.Image, name, filePath));
            }
        }

        public void AddTitle(string name, int? index = null)
        {
            if (index is null)
            {
                _list.Add(new Content(Content.Types.Title, name));
            }
            else
            {
                _list.Insert((int)index, new Content(Content.Types.Title, name));
            }
        }

        public Content RemoveAt(int index)
        {
            var result = _list[index];
            _list.RemoveAt(index);
            return result;
        }

        public bool Move(int index, int offset)
        {
            if ( (index < 0) || (index >= _list.Count) || (index + offset < 0) || (index + offset >= _list.Count) )
            {
                return false;
            }
            Content content = _list[index];
            _list.RemoveAt(index);
            _list.Insert(index + offset, content);
            return true;
        }

        public bool MoveUp(int index)
        {
            return Move(index, -1);
        }

        public bool MoveDown(int index)
        {
            return Move(index, +1);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public async Task<bool> CreateDocx(string filePath, bool saveAsPdf, string? header = null, string? footer = null)
        {
            const int titlesFontSize = 24;
            const int paragrahpsFontSize = 14;
            const int horizontalImagesMaxHeight = 350;
            const int verticalImgesMaxHeight = 300;
            const int imagesBeforeBreak = 2;

            Microsoft.Office.Interop.Word.Application wordApp;
            Microsoft.Office.Interop.Word.Document wordDoc;
            ProgressUpdated?.Invoke(this, 20);
            try
            {
                wordApp = new Microsoft.Office.Interop.Word.Application();
                wordDoc = wordApp.Documents.Add();
                ProgressUpdated?.Invoke(this, 40);

                PageSetup pageSetup = wordDoc.PageSetup;
                pageSetup.TopMargin = wordApp.CentimetersToPoints(2);
                pageSetup.BottomMargin = wordApp.CentimetersToPoints(2);
                pageSetup.LeftMargin = wordApp.CentimetersToPoints(2);
                pageSetup.RightMargin = wordApp.CentimetersToPoints(2);
                ProgressUpdated?.Invoke(this, 60);

                HeaderFooter headerFooter = wordDoc.Sections[1].Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
                Microsoft.Office.Interop.Word.Paragraph headerParagraph = headerFooter.Range.Paragraphs.Add();
                headerParagraph.Range.Text = header;
                headerParagraph.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                ProgressUpdated?.Invoke(this, 80);

                headerFooter = wordDoc.Sections[1].Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
                headerFooter.PageNumbers.Add(0);
                ProgressUpdated?.Invoke(this, 100);

                int sectionImageCount = 0;
                bool firstElement = true;

                foreach (var content in _list.OrderBy(content => _list.IndexOf(content)))
                {
                    if (content.Type == Content.Types.Title)
                    {
                        if (!firstElement)
                        {
                            wordDoc.Words.Last.InsertBreak(WdBreakType.wdPageBreak);
                            sectionImageCount = 0;
                        }

                        Microsoft.Office.Interop.Word.Paragraph titleParagraph = wordDoc.Paragraphs.Add();
                        titleParagraph.Range.Text = content.Name;
                        titleParagraph.Range.Font.Bold = 1;
                        titleParagraph.Range.Font.Size = titlesFontSize;
                        titleParagraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        titleParagraph.Range.InsertParagraphAfter();
                    }
                    else if ((content.Type == Content.Types.Image) && (!string.IsNullOrEmpty(content.FilePath)))
                    {
                        if (sectionImageCount == imagesBeforeBreak)
                        {
                            wordDoc.Words.Last.InsertBreak(WdBreakType.wdPageBreak);
                            sectionImageCount = 0;
                        }
                        sectionImageCount++;

                        Microsoft.Office.Interop.Word.Paragraph imageParagraph = wordDoc.Paragraphs.Add();
                        imageParagraph.Range.Text = content.Name;
                        imageParagraph.Range.Font.Bold = 1;
                        imageParagraph.Range.Font.Size = paragrahpsFontSize;
                        imageParagraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                        imageParagraph.Range.InsertParagraphAfter();

                        InlineShape inlineShape = imageParagraph.Range.InlineShapes.AddPicture(content.FilePath);

                        float aspectRatio = (float)inlineShape.Width / inlineShape.Height;
                        float imagesMaxHeight = aspectRatio > 1 ? horizontalImagesMaxHeight : verticalImgesMaxHeight;
                        float newHeight = imagesMaxHeight;
                        float newWidth = imagesMaxHeight * aspectRatio;

                        if (newWidth > pageSetup.PageWidth - (pageSetup.LeftMargin + pageSetup.RightMargin))
                        {
                            newWidth = pageSetup.PageWidth - (pageSetup.LeftMargin + pageSetup.RightMargin);
                            newHeight = newWidth / aspectRatio;
                        }

                        inlineShape.Width = newWidth;
                        inlineShape.Height = newHeight;

                        if (sectionImageCount != imagesBeforeBreak)
                        {
                            imageParagraph.Range.InsertParagraphAfter();
                        }
                    }
                    firstElement = false;

                    float progressPercentage = (float)(_list.IndexOf(content) + 1) / _list.Count * 100;
                    ProgressUpdated?.Invoke(this, (int)progressPercentage);
                    await System.Threading.Tasks.Task.Delay(1);
                }

                wordDoc.SaveAs2(filePath, saveAsPdf ? WdSaveFormat.wdFormatPDF : WdSaveFormat.wdFormatDocumentDefault);

                wordDoc?.Close(WdSaveOptions.wdDoNotSaveChanges);
                wordApp?.Quit();

                ProgressUpdated?.Invoke(this, 0);

                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                ProgressUpdated?.Invoke(this, 0);
                Console.WriteLine("Si è verificato un errore durante la creazione del documento Word: " + ex.Message);
                return false;
            }
        }
    }
}
