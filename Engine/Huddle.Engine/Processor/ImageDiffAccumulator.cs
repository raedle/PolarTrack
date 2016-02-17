using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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

        #region ThresholdOut

        /// <summary>
        /// The <see cref="ThresholdOut" /> property's name.
        /// </summary>
        public const string ThresholdOutPropertyName = "ThresholdOut";

        private int _thresholdOld = 125;

        /// <summary>
        /// Sets and gets the ThresholdOut property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ThresholdOut
        {
            get
            {
                return _thresholdOld;
            }

            set
            {
                if (_thresholdOld == value)
                {
                    return;
                }

                RaisePropertyChanging(ThresholdOutPropertyName);
                _thresholdOld = value;
                RaisePropertyChanged(ThresholdOutPropertyName);
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

        #region Gamma

        /// <summary>
        /// The <see cref="Gamma" /> property's name.
        /// </summary>
        public const string GammaPropertyName = "Gamma";

        private double _gamma = 0.3;

        /// <summary>
        /// Sets and gets the Gamma property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Gamma
        {
            get
            {
                return _gamma;
            }

            set
            {
                if (_gamma == value)
                {
                    return;
                }

                RaisePropertyChanging(GammaPropertyName);
                _gamma = value;
                RaisePropertyChanged(GammaPropertyName);
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

        /**
         * input image
         * diff image
         * accumulate image
         * threshold image
         * output image
         */
        BlockingCollection<UMat> images = new BlockingCollection<UMat>();
        BlockingCollection<UMat> diff = new BlockingCollection<UMat>();
        BlockingCollection<UMat> q1 = new BlockingCollection<UMat>();
        BlockingCollection<UMat> q2= new BlockingCollection<UMat>();
        int __cnt = 1;
        public override UMatData ProcessAndView(UMatData data)
        {
            if (_previousImage == null)
            {
                _previousImage = data.Data.Clone();

                return null;
            }
            if (_previousImage.Cols != data.Data.Cols ||
                _previousImage.Rows != data.Data.Rows ||
                _previousImage.Depth != data.Data.Depth ||
                _previousImage.NumberOfChannels != data.Data.NumberOfChannels)
            {
                return null;
            }

            UMat imageCopy = data.Data.Clone();


            UMat ret = new UMat();
            UMat tmp = new UMat();
            UMat outp = null;

            CvInvoke.AbsDiff(_previousImage, data.Data, ret); // A-B
            CvInvoke.Blur(ret, ret, new System.Drawing.Size(10, 10), new System.Drawing.Point(-1, -1));
            CvInvoke.CvtColor(ret, tmp, ColorConversion.Rgb2Gray); // Gray
            CvInvoke.Threshold(tmp, ret, Threshold, 255, ThresholdType.Binary); // Thresh

            // intermediate image
            #region render intermediate image
            var image = ret.Clone().ToImage();

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

            if (images.Count >= 8)
            {
                var devnull = images.Take();
            }
            images.Add(ret);

            foreach (var i in images)
            {
                if (outp == null)
                {
                    outp = i.Clone();
                }
                else
                {
                    //CvInvoke.BitwiseOr(i, outp, outp);
                    CvInvoke.AddWeighted(i, WeightNew, outp, WeightOld, Gamma, outp);
                }
            }

            CvInvoke.Threshold(outp, data.Data, ThresholdOut, 255, ThresholdType.Binary);

            // clean up and stuff
            _previousImage = imageCopy;
            return data;
        }
    }
}
