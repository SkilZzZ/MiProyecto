using System;
using System.Collections.Generic;
using System.Text;

namespace qDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RelatedField : System.Attribute
    {
        public RelatedField()
        {
        }
    }
}
