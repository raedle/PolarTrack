using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huddle.Engine.Data
{
    /*
     * Trackable Object
     * each processing node has to call processedBy and add some information
     * The Object then can be evaluated with the data gattered.
     * 
     * TODO is it possible to call processedBy by the object itselfe?
     */
    public interface ITrackable
    {
        void processedBy(Type node, long ticks);
    }
}
