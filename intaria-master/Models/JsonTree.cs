using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intaria.Models
{
    public class JsonTree
    {
        public string id { get; set; }
        public string text { get; set; }
        public List<JsonTree> nodes { get; set; }
    }
}
