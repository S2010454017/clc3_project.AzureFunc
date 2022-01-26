using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clc3_project.AzureFunc
{
    public class Book
    {
        public string? Id { get; set; }

        public string? ISBN { get; set; }
        public string BookName { get; set; } = null!;

        public HashSet<string> Category { get; set; } = new HashSet<string>();

        public HashSet<string> Authors { get; set; } = new HashSet<string>();

        public string? Cover { get; set; }
    }
}
