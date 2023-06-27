using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceMaster
{
    public class Content
    {
        public enum Types
        {
            Image,
            Title
        }

        public Types Type { get; set; }
        public string Name { get; set; }
        public string? FilePath { get; set; }

        public Content(Types type, string name, string? filePath = null)
        {
            Type = type;
            Name = name;
            FilePath = filePath;
        }
    }
}
