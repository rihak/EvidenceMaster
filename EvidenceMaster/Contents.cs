using Microsoft.VisualBasic;
using Xceed.Document.NET;
using Xceed.Words.NET;

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
        private List<Content> _list;
        public Content this[int index] => _list[index];

        public List<Content> List
        {
            get { return _list; }
            set { _list = value; }
        }

        public Contents()
        {
            _list = new List<Content>();
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

        public bool CreateDocx(string filePath, string? header = null, string? footer = null)
        {
            const int titlesFontSize = 24;
            const int imagesMaxWidth = 400;
            const int imagesMaxHeight = 400;
            const int imagesBeforeBreak = 2;

            var document = DocX.Create(filePath);

            int sectionImageCount = 0;
            bool firstElement = true;

            foreach (var content in _list.OrderBy(content => _list.IndexOf(content)))
            {
                if (content.Type == Content.Types.Title)
                {
                    if (!firstElement)
                    {
                        document.InsertSectionPageBreak();
                        sectionImageCount = 0;
                    }
                    document.InsertParagraph(content.Name).Heading(HeadingType.Heading1).FontSize(titlesFontSize).Alignment = Alignment.center;
                    document.InsertParagraph();
                }
                else if ((content.Type == Content.Types.Image) && (!string.IsNullOrEmpty(content.FilePath)))
                {
                    if (sectionImageCount == imagesBeforeBreak)
                    {
                        document.InsertSectionPageBreak();
                        sectionImageCount = 0;
                    }
                    sectionImageCount++;

                    document.InsertParagraph(content.Name).Bold().Alignment = Alignment.left;
                    var image = document.AddImage(content.FilePath);
                    var picture = image.CreatePicture();

                    float scaleFactor = Math.Min((float)imagesMaxWidth / picture.Width, (float)imagesMaxHeight / picture.Height);
                    if (scaleFactor < 1)
                    {
                        picture.Width = (int)(picture.Width * scaleFactor);
                        picture.Height = (int)(picture.Height * scaleFactor);
                    }

                    var paragraph = document.InsertParagraph();
                    paragraph.Alignment = Alignment.center;
                    paragraph.AppendPicture(picture);
                    document.InsertParagraph();
                }
                firstElement = false;
            }

            document.AddHeaders();
            document.AddFooters();

            document.DifferentFirstPage = false;
            document.DifferentOddAndEvenPages = false;

            document.Headers.Odd.Paragraphs[0].Append(header);
            document.Footers.Odd.Paragraphs[0].Append(footer);

            document.SaveAs(filePath);

            return File.Exists(filePath);
        }
    }
}
