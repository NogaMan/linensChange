using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinensChange
{
    class CardReaderException : Exception
    {
        int _code;

        public CardReaderException(string message)
            :base(message)
        {

        }
        
        public CardReaderException(string message, int code)
            : base(message)
        {
            _code = code;
        }

    }
}
