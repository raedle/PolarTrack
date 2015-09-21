using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huddle.Engine.Util
{
    public class DataFrameNumberGenerator
    {
        static private DataFrameNumberGenerator _instance = null;
        static private long _dataFrameNumber;
        static private Object _lockObject = new Object();

        #region ctor

        // hide ctor
        private DataFrameNumberGenerator()
        {
            _dataFrameNumber = 0;
        }

        #endregion

        static public DataFrameNumberGenerator Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new DataFrameNumberGenerator();
                    }
                }
                return _instance;
            }
        }
        public long getNextDataFrameNumber()
        {
            long ret;
            lock (_lockObject)
            {
                ret = _dataFrameNumber++;
            }
            return ret;
        }
    }
}
