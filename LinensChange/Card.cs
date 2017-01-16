using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinensChange
{
    public class Card
    {
        public byte[] UId { get; set; }

        public string TextUId
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (UId != null)
                {
                    foreach (var b in UId)
                        sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
