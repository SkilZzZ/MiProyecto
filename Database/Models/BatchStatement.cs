using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Models
{
    public class BatchStatement
    {
        public string Query;
        public Dictionary<string, object> Parameters;
    }
}
