using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LookupField : System.Attribute
    {
        public string KeyField { get; set; }
        public string TableName { get; set; }
        public string TableKey { get; set; }
        public string LookupFieldName { get; set; }
        public bool ignoreIfChild { get; set; }
        public LookupField(string KeyField, string TableName = null, string TableKey = null, string LookupFieldName = null, bool ignoreIfChild = false)
        {
            this.KeyField = KeyField;
            this.TableName = TableName;
            this.TableKey = TableKey;
            this.LookupFieldName = LookupFieldName;
            this.ignoreIfChild = ignoreIfChild;
        }
    }
}
