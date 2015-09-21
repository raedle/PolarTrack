using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huddle.Engine.Util
{
    interface ILog
    {
        void Start();
        void Stop();
        void log(String message);
        void log(String[] message);
    }
}
