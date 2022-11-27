using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Logging
{
    public interface IXDcLogger
    {
        void LogError(Exception error);
        void LogError(string error);
        void LogInfo(string info);
    }
}
