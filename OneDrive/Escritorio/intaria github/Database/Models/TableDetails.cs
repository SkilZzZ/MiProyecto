using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Models
{
    public class TableDetails
    {
        public string Type;
        public string Name;
        public string Schema;
        public string FileGroup;
        public long RowCount;
        public double IndexSpace;
        public double DiskSpace;
        public double UnusedSpace;
        public double Reserved;
        public bool HasPk;
        //for the schemas
        public int ItemCount;
    }
}
