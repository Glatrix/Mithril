using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLanguage.Language.Manage
{
    public class TokenizerException : Exception
    {
        public TokenizerException(string? message) : base(message)
        {
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string? message) : base(message)
        {
        }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException(string? message) : base(message)
        {
            
        }
    }
}
