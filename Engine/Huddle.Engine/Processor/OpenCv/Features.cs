using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.External.Extensions;
using Emgu.CV.External.Structure;

using Huddle.Engine.Data;
using Huddle.Engine.Util;

namespace Huddle.Engine.Processor.OpenCv
{

    [ViewTemplate("Feature Detection and Matching","FeaturesDM")]
    public class Features : UMatProcessor
    {
        public Features()
            : base(false)
        {

        }

        private void detectAndShow(ref UMat img, string name)
        {
            Emgu.CV.Features2D.FastDetector detector = new FastDetector(); ;
            Emgu.CV.Util.VectorOfKeyPoint res = new Emgu.CV.Util.VectorOfKeyPoint();

            detector.DetectRaw(img, res);

            Emgu.CV.Features2D.Features2DToolbox.DrawKeypoints(img,
                res,
                img,
                new Bgr(Rgbs.Red.Blue, Rgbs.Red.Green, Rgbs.Red.Red),
                Features2DToolbox.KeypointDrawType.DrawRichKeypoints);


            CvInvoke.Imshow(name, img);
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            UMat a = data.Data;
    
            if (data.Key == "color")
            {
                CvInvoke.CvtColor(a, a, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
                detectAndShow(ref a, "color");
            }

            if (data.Key == "depth")
            {
                a.ConvertTo(a, Emgu.CV.CvEnum.DepthType.Cv8U);
                detectAndShow(ref a, "depth");
            }

            if (data.Key == "confidence")
            {
                detectAndShow(ref a, "confidence");
            }

            CvInvoke.WaitKey();

            return data;
        }
    }
}
