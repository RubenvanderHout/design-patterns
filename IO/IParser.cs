using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.DTO;

namespace IO
{
    public interface IParser
    {
        FsmDto Parse(string rawInput);
    }

    public interface IParserFactory
    {
        IParser CreateParser();
    }
}
