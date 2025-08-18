using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.DTO;

namespace IO
{
    internal interface IParser
    {
         FsmDto Parse(string rawInput);
    }
}
