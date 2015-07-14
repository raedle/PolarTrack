using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;

using Huddle.Engine.Data;
using Huddle.Engine.Properties;
using Huddle.Engine.Util;

namespace Huddle.Engine.Processor
{

    [ViewTemplate("Dummy", "Dummy")]
    public class Dummy : UMatProcessor
    {
        public Dummy()
            : base(false)
        {

        }

        public override UMatData ProcessAndView(UMatData data)
        {
            return data;
        }
    }
}
