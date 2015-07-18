using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using Huddle.Engine.Util;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Fill Convex Hulls", "FillConvexHulls")]
    public class FillConvexHulls : RgbProcessor
    {
        #region properties

        #region MinContourArea

        /// <summary>
        /// The <see cref="MinContourArea" /> property's name.
        /// </summary>
        public const string MinContourAreaPropertyName = "MinContourArea";

        private int _minContourArea = 200;

        /// <summary>
        /// Sets and gets the MinContourArea property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MinContourArea
        {
            get
            {
                return _minContourArea;
            }

            set
            {
                if (_minContourArea == value)
                {
                    return;
                }

                RaisePropertyChanging(MinContourAreaPropertyName);
                _minContourArea = value;
                RaisePropertyChanged(MinContourAreaPropertyName);
            }
        }

        #endregion

        #region IsRetrieveExternal

        /// <summary>
        /// The <see cref="IsRetrieveExternal" /> property's name.
        /// </summary>
        public const string IsRetrieveExternalPropertyName = "IsRetrieveExternal";

        private bool _isRetrieveExternal = false;

        /// <summary>
        /// Sets and gets the IsRetrieveExternal property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsRetrieveExternal
        {
            get
            {
                return _isRetrieveExternal;
            }

            set
            {
                if (_isRetrieveExternal == value)
                {
                    return;
                }

                RaisePropertyChanging(IsRetrieveExternalPropertyName);
                _isRetrieveExternal = value;
                RaisePropertyChanged(IsRetrieveExternalPropertyName);
            }
        }

        #endregion

        #region DebugImageSource

        /// <summary>
        /// The <see cref="DebugImageSource" /> property's name.
        /// </summary>
        public const string DebugImageSourcePropertyName = "DebugImageSource";

        private BitmapSource _debugImageSource;

        /// <summary>
        /// Sets and gets the DebugImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DebugImageSource
        {
            get
            {
                return _debugImageSource;
            }

            set
            {
                if (_debugImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DebugImageSourcePropertyName);
                _debugImageSource = value;
                RaisePropertyChanged(DebugImageSourcePropertyName);
            }
        }

        #endregion

        #endregion

        public override Image<Rgb, byte> ProcessAndView(Image<Rgb, byte> image)
        {
            var outputImage = new Image<Rgb, byte>(image.Size.Width, image.Size.Height, Rgbs.Black);
            var debugImage = outputImage.Copy();

            //Convert the image to grayscale and filter out the noise
            var grayImage = image.Convert<Gray, Byte>();

            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            CvInvoke.FindContours(grayImage,
                contours,
                null,
                IsRetrieveExternal ? RetrType.External : RetrType.List,
                ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++ )
            {
                Emgu.CV.Util.VectorOfPointF currentContour = new Emgu.CV.Util.VectorOfPointF(); // TODO move me and my siblings
                CvInvoke.ApproxPolyDP(contours[i],
                    currentContour,
                    CvInvoke.ArcLength(contours[i], true) * 0.05,
                    true);

                //Console.WriteLine("AREA {0}", currentContour.Area);

                //if (currentContour.Area > MinContourArea) //only consider contours with area greater than 250
                //{
                //outputImage.Draw(currentContour.GetConvexHull(ORIENTATION.CV_CLOCKWISE), Rgbs.White, 2);
                Emgu.CV.Util.VectorOfPoint ret = null;
                CvInvoke.ConvexHull(currentContour,
                    ret,
                    true,
                    true);

                outputImage.FillConvexPoly(ret.ToArray(), Rgbs.White);

                if (IsRenderContent)
                {
                    debugImage.FillConvexPoly(ret.ToArray(), Rgbs.White);
                }
                //}
                //else
                //{
                //    if (IsRenderContent)
                //        debugImage.FillConvexPoly(currentContour.GetConvexHull(ORIENTATION.CV_CLOCKWISE).ToArray(), Rgbs.Red);
                //}
            }

            Task.Factory.StartNew(() =>
            {
                var bitmapSource = debugImage.ToBitmapSource(true);
                debugImage.Dispose();
                return bitmapSource;
            }).ContinueWith(t => DebugImageSource = t.Result);

            grayImage.Dispose();

            return outputImage;
        }
    }
}
