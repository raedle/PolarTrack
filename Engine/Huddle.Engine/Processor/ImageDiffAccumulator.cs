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
            if (__cnt == 1 || __cnt == 3 || __cnt == 4)
            {
                if (q1.Count == 3)
                {
                    q1.Take();
                }

                q1.Add(data.Data.Clone());

                if (__cnt == 4)
                {
                    if (q2.Count == 3)
                    {
                        q2.Take();
                    }

                    q2.Add(data.Data.Clone());
                }
            }
            else
            {
                if (q2.Count == 3)
                {
                    q2.Take();
                }

                q2.Add(data.Data.Clone());
            }
            __cnt++;
            if (__cnt == 6)
            {
                __cnt = 1;
            }

            if (q1.Count == 3 && q2.Count == 3)
            {
                UMat outp = null;
                foreach (var i in q1)
                {
                    if (outp == null)
                    {
                        outp = i.Clone();
                    }
                    else
                    {
                        CvInvoke.AddWeighted(i, WeightNew, outp, WeightOld, 0, outp);
                    }
                }
                UMat outq = null;
                foreach (var i in q2)
                {
                    if (outq == null)
                    {
                        outq = i.Clone();
                    }
                    else
                    {
                        CvInvoke.AddWeighted(i, WeightNew, outq, WeightOld, 0, outq);
                    }
                }

                UMat ret = new UMat();
                CvInvoke.AbsDiff(outp, outq, ret);

                if (IsUseGrayImages)
                {
                    /* gray images */
                    UMat __tmp = new UMat();
                    CvInvoke.CvtColor(ret, __tmp, ColorConversion.Rgb2Gray);
                    CvInvoke.Threshold(__tmp, data.Data, Threshold, 255, ThresholdType.Binary);

                }
                else
                {
                    data.Data = ret;
                }

                return data;
            }


           

            return null;
        }
    }
}
