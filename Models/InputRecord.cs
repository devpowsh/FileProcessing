using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessing.Models
{
    public class InputRecord
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
}
