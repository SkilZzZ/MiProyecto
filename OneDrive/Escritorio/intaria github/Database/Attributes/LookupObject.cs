using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LookupObject : System.Attribute
    {
        public string KeyField { get; set; }
        public bool ignoreIfChild { get; set; }
        public LookupObject(string KeyField, bool ignoreIfChild = false)
        {
            this.KeyField = KeyField;
            this.ignoreIfChild = ignoreIfChild;
        }
    }
}
