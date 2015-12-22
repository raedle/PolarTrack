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

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;


namespace Huddle.Engine.Processor
{

    [ViewTemplate("Image Accumulator", "ImageAccumulator")]
    public class ImageAccumulator : UMatProcessor
    {
        private UMat _accImage = null;

        public ImageAccumulator()
            : base(false)
        {

        }

        #region override functions

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            _accImage = null;
        }

        int cnt = 0;
        public override UMatData ProcessAndView(UMatData data)
        {
            if (_accImage == null)
            {
                _accImage = data.Data.Clone();
                return null;
            }
            else
            {
                CvInvoke.AddWeighted(data.Data, 0.5, _accImage, 0.5, 0, _accImage);
                cnt++;
                if (cnt >= 3)
                {
                    //_accImage.Clone().ConvertTo(data.Data, Emgu.CV.CvEnum.DepthType.Cv8U);
                    data.Data = _accImage;
                    cnt = 0;
                    _accImage = null;
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
