using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;

using Huddle.Engine.Data;
using Huddle.Engine.Properties;
using Huddle.Engine.Util;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;

namespace Huddle.Engine.Processor
{

    [ViewTemplate("Image Accumulator (Diff)", "ImageDiffAccumulator")]
    public class ImageDiffAccumulator : UMatProcessor
    {
        private UMat _previousImage = null;
        private UMat _accImage = null;

        #region public properties

        #region Threshold

        /// <summary>
        /// The <see cref="Threshold" /> property's name.
        /// </summary>
        public const string ThresholdPropertyName = "Threshold";

        private int _threshold = 75;

        /// <summary>
        /// Sets and gets the Threshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Threshold
        {
            get
            {
                return _threshold;
            }

            set
            {
                if (_threshold == value)
                {
                    return;
                }

                RaisePropertyChanging(ThresholdPropertyName);
                _threshold = value;
                RaisePropertyChanged(ThresholdPropertyName);
            }
        }

        #endregion

        #region IntermediateImage

        /// <summary>
        /// The <see cref="IntermediateImage" /> property's name.
        /// </summary>
        public const string IntermediateImagePropertyName = "IntermediateImage";

        private BitmapSource _intermediateImage;

        /// <summary>
        /// Sets and gets the IntermediateImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource IntermediateImage
        {
            get
            {
                return _intermediateImage;
            }

            set
            {
                if (_intermediateImage == value)
                {
                    return;
                }

                RaisePropertyChanging(IntermediateImagePropertyName);
                _intermediateImage = value;
                RaisePropertyChanged(IntermediateImagePropertyName);
            }
        }

        #endregion

        #endregion


        public ImageDiffAccumulator()
            : base(false)
        {

        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (_previousImage == null)
            {
                _previousImage = data.Data.Clone();
                return null;
            }
            else
            {
                if (_previousImage.Cols != data.Data.Cols ||
                    _previousImage.Rows != data.Data.Rows ||
                    _previousImage.Depth != data.Data.Depth ||
                    _previousImage.NumberOfChannels != data.Data.NumberOfChannels)
                {
                    return null;
                }
                var imageCopy = data.Data.Clone();

                UMat ret = new UMat(data.Height, data.Width, DepthType.Cv8U, 3);
                CvInvoke.AbsDiff(_previousImage, data.Data, ret);

                if (_accImage == null)
                {
                    _accImage = ret.Clone();
                }
                else
                {
                    CvInvoke.AddWeighted(ret, 0.1, _accImage, 0.8, 0, _accImage);
                }
                // intermediate image
                #region render intermediate image
                var image = _accImage.Clone().ToImage();

                Task.Factory.StartNew(() =>
                {
                    if (image == null) return null;

                    BitmapSource bitmap;
                    if (image is Image<Gray, float>)
                        bitmap = (image as Image<Gray, float>).ToGradientBitmapSource(true, EmguExtensions.LowConfidence, EmguExtensions.Saturation);
                    else
                        bitmap = image.ToBitmapSource(true);

                    image.Dispose();

                    return bitmap;
                }).ContinueWith(s => IntermediateImage = s.Result);
                #endregion

                UMat tmp = new UMat();

                CvInvoke.CvtColor(_accImage, tmp, ColorConversion.Rgb2Gray);
                CvInvoke.Threshold(tmp, data.Data, Threshold, 255, ThresholdType.Binary);

                //data.Data = _accImage.Clone();


                // save image as lastImage
                _previousImage = imageCopy;
                return data;
            }
        }
    }
}
