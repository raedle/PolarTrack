using Emgu.CV;
using Emgu.CV.External.Extensions;
using Huddle.Engine.Processor;

namespace Huddle.Engine.Data
{
    public sealed class UMatData : BaseData
    {

        #region properties

        #region Image

        private UMat _data;

        public UMat Data // TODO rename
        {
            get
            {
                return _data;
            }

            set
            {
                if (_data != null)
                {
                    _data.Dispose();
                    _data = null;
                }
                // TODO revise!!! is this good?
                _data = value;
            }
        }

        public UMat Ptr
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        #endregion

        #region Width

        public int Width
        {
            get
            {
                return Data.Cols;
            }
        }

        #endregion

        #region Height

        public int Height
        {
            get
            {
                return Data.Rows;
            }
        }

        #endregion

        #endregion

        #region ctor

        public UMatData(IProcessor source, string key, UMat data)
            : base(source, key)
        {
            //CvInvoke.UseOpenCL = false;
            Data = data;
            
            //Data = data;
        }

        #endregion

        public override IData Copy()
        {
            // this is the magic copy which soiuld be used by all clone() calls
            // this returns a deep copy!
            return new UMatData(Source,
                Key,
                new UMat(Data, new System.Drawing.Rectangle(0, 0, Data.Cols, Data.Rows)));
        }
     
        public override void Dispose()
        {
            Data.Dispose();
        }

    }
}
