using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Logging
{
    public interface LogManager
    {
        void Fatal(string formatString, params object[] arguments);
        void Error(string formatString, params object[] arguments);
        void Warn(string formatString, params object[] arguments);
        void Info(string formatString, params object[] arguments);
        void Debug(string formatString, params object[] arguments);
    }
}
