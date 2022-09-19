using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyDefinition : System.Attribute
    {
        public bool IsPrimaryKey { get; set; }
        public bool AutoNumeric { get; set; }

        public PrimaryKeyDefinition(bool IsPrimaryKey, bool AutoNumeric = true)
        {
            this.IsPrimaryKey = IsPrimaryKey;
            this.AutoNumeric = AutoNumeric;
        }

    }
}
