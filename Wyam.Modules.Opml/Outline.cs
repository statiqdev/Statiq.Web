using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Opml
{
    public class Outline
    {
        public Dictionary<string, string> Attributes { get; private set; } 
            = new Dictionary<string, string>();

        public List<Outline> Outlines { get; private set; } 
            = new List<Outline>();
    }
}
