using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTales.Model
{
    public class Story
    {
        public string Title { get; set; }
        public List<string> Images { get; set; } = new();
    }
}
