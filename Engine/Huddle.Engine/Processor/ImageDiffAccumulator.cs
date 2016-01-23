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

        #region WeightOld

        /// <summary>
        /// The <see cref="WeightOld" /> property's name.
        /// </summary>
        public const string WeightOldPropertyName = "WeightOld";

        private double _weightOld = 0.8;

        /// <summary>
        /// Sets and gets the WeightOld property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double WeightOld
        {
            get
            {
                return _weightOld;
            }

            set
            {
                if (_weightOld == value)
                {
                    return;
                }

                RaisePropertyChanging(WeightOldPropertyName);
                _weightOld = value;
                RaisePropertyChanged(WeightOldPropertyName);
            }
        }

        #endregion

        #region WeightNew

        /// <summary>
        /// The <see cref="WeightNew" /> property's name.
        /// </summary>
        public const string WeightNewPropertyName = "WeightNew";

        private double _weightNew = 0.1;

        /// <summary>
        /// Sets and gets the WeightNew property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double WeightNew
        {
            get
            {
                return _weightNew;
            }

            set
            {
                if (_weightNew == value)
                {
                    return;
                }

                RaisePropertyChanging(WeightNewPropertyName);
                _weightNew = value;
                RaisePropertyChanged(WeightNewPropertyName);
            }
        }

        #endregion

        #region IsUseGrayImages

        /// <summary>
        /// The <see cref="IsUseGrayImages" /> property's name.
        /// </summary>
        public const string IsUseGrayImagesPropertyName = "IsUseGrayImages";

        private bool _IsUseGrayImages = true;

        /// <summary>
        /// Sets and gets the IsUseGrayImages property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUseGrayImages
        {
            get
            {
                return _IsUseGrayImages;
            }

            set
            {
                if (_IsUseGrayImages == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUseGrayImagesPropertyName);
                _IsUseGrayImages = value;
                RaisePropertyChanged(IsUseGrayImagesPropertyName);
            }
        }

        #endregion

        #endregion


        public ImageDiffAccumulator()
            : base(false)
        {

        }

        public override void Start()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case IsUseGrayImagesPropertyName:
                        _previousImage = null;
                        _accImage = null;
                        break;
                }
            };

            base.Start();
        }

        public override void Stop()
        {
            _previousImage = null;
            _accImage = null;

            base.Stop();
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (IsUseGrayImages)
            {
                /* gray images */
                CvInvoke.CvtColor(data.Data, data.Data, ColorConversion.Rgb2Gray);
            }

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

                UMat ret = new UMat();
                CvInvoke.AbsDiff(_previousImage, data.Data, ret);

                if (_accImage == null)
                {
                    _accImage = ret.Clone();
                }
                else
                {
                    CvInvoke.AddWeighted(ret, WeightNew, _accImage, WeightOld, 0, _accImage);
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

                if (IsUseGrayImages)
                {
                    tmp = _accImage;
                }
                else
                {
                    CvInvoke.CvtColor(_accImage, tmp, ColorConversion.Rgb2Gray);
                }

                CvInvoke.Threshold(tmp, data.Data, Threshold, 255, ThresholdType.Binary);

                // save image as lastImage
                _previousImage = imageCopy;
                return data;
            }
        }
    }
}
