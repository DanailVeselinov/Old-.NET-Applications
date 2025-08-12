using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiefPortDocuments
{
    class Tags
    {

        public string Value { get; set; }
        public string Tag { get; set; }

        public Tags(string value, string tag)
        {
            Value = value;
            Tag = tag;
        }

        public override bool Equals(object obj)
        {
            try
            {
                Tags t = obj as Tags;
                if (t.Tag == this.Tag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
