using System;
using Huddle.Engine.Processor;

namespace Huddle.Engine.Data
{
    public interface IData : IDisposable
    {
        IProcessor Source { get; }

        string Key { get; }

        DateTime CreationTime { get; }

        IDataContainer Parent { get; set; }

        IData Copy();
    }
}
