using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO
{
    public interface ILoaderFactory
    {
        ILoader CreateLoader();
    }

    public interface ILoader
    {
        string Load(string path);
    }
}
