using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Models
{
    public class ColumnDetails
    {
        public string Name;
        public string DataType;
        public bool isNullable;
        public bool autoincrement;
        public bool Key;
        public bool hasDuplicates;
        public bool IsPk;
    }
}
