using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth
{
    public static class Exceptions
    {
        public class DataAttributeNotFoundException : Exception
        {
            public DataAttributeNotFoundException()
            {
            }

            public DataAttributeNotFoundException(string message)
                : base(message)
            {
            }

            public DataAttributeNotFoundException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidMFTMagicNumberException : Exception
        {
            public InvalidMFTMagicNumberException()
            {
            }

            public InvalidMFTMagicNumberException(string message)
                : base(message)
            {
            }

            public InvalidMFTMagicNumberException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
}
