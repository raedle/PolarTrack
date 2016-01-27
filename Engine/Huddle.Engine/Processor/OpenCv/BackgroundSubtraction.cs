using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Util;
using Huddle.Engine.Data;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Background Subtraction", "BackgroundSubtraction")]
    public class BackgroundSubtraction : UMatProcessor
    {
        #region private fields

        private UMat _backgroundImage = null;

        private int _collectedBackgroundImages;

        #endregion

        #region commands

        public RelayCommand SubtractCommand { get; private set; }

        #endregion

        #region properties

        #region BackgroundSubtractionSamples

        /// <summary>
        /// The <see cref="BackgroundSubtractionSamples" /> property's name.
        /// </summary>
        public const string BackgroundSubtractionSamplesPropertyName = "BackgroundSubtractionSamples";

        private int _backgroundSubtractionSamples = 50;

        /// <summary>
        /// Sets and gets the BackgroundSubtractionSamples property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int BackgroundSubtractionSamples
        {
            get
            {
                return _backgroundSubtractionSamples;
            }

            set
            {
                if (_backgroundSubtractionSamples == value)
                {
                    return;
                }

                RaisePropertyChanging(BackgroundSubtractionSamplesPropertyName);
                _backgroundSubtractionSamples = value;
                RaisePropertyChanged(BackgroundSubtractionSamplesPropertyName);
            }
        }

        #endregion

        #region LowCutOffDepth

        /// <summary>
        /// The <see cref="LowCutOffDepth" /> property's name.
        /// </summary>
        public const string LowCutOffDepthPropertyName = "LowCutOffDepth";

        private float _lowCutOffDepth = 0.0f;

        /// <summary>
        /// Sets and gets the LowCutOffDepth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float LowCutOffDepth
        {
            get
            {
                return _lowCutOffDepth;
            }

            set
            {
                if (_lowCutOffDepth == value)
                {
                    return;
                }

                RaisePropertyChanging(LowCutOffDepthPropertyName);
                _lowCutOffDepth = value;
                RaisePropertyChanged(LowCutOffDepthPropertyName);
            }
        }

        #endregion

        #region HighCutOffDepth

        /// <summary>
        /// The <see cref="HighCutOffDepth" /> property's name.
        /// </summary>
        public const string HighCutOffDepthPropertyName = "HighCutOffDepth";

        private float _highCutOffDepth = 1000.0f;

        /// <summary>
        /// Sets and gets the HighCutOffDepth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float HighCutOffDepth
        {
            get
            {
                return _highCutOffDepth;
            }

            set
            {
                if (_highCutOffDepth == value)
                {
                    return;
                }

                RaisePropertyChanging(HighCutOffDepthPropertyName);
                _highCutOffDepth = value;
                RaisePropertyChanged(HighCutOffDepthPropertyName);
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

        #region ctor

        public BackgroundSubtraction()
        {
            SubtractCommand = new RelayCommand(() =>
            {
                _backgroundImage = null;
            });
        }

        #endregion

        public override void Start()
        {
            _collectedBackgroundImages = 0;

            base.Start();
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (BuildingBackgroundImage(data.Data)) return null;

            var width = data.Data.Cols;
            var height = data.Data.Rows;

            var lowCutOffDepth = LowCutOffDepth;
            var highCutOffDepth = HighCutOffDepth;

            // This image is used to segment object from background
            UMat imageRemovedBackground = new UMat();
            CvInvoke.Subtract(_backgroundImage, data.Data.Clone(), imageRemovedBackground);

            if (IsRenderContent)
            {
                #region Render Debug Image

                var debugImageCopy = imageRemovedBackground.Clone().ToImage();
                Task.Factory.StartNew(() =>
                {
                    if (debugImageCopy == null) return null;

                    BitmapSource bitmapSource = debugImageCopy.ToBitmapSource(true);
                    debugImageCopy.Dispose();
                    return bitmapSource;
                }).ContinueWith(t => DebugImageSource = t.Result);

                #endregion
            }

            // This image is necessary for using FloodFill to avoid filling background
            // (segmented objects are shifted back to original depth location after background subtraction)
            //UMat imageWithOriginalDepth = new UMat();
            var imageWithOriginalDepth = new Image<Rgb, byte>(width, height);

            var imageData = data.Data.Clone().ToImage<Rgb, byte>().Data;
            var imageRemovedBackgroundData = imageRemovedBackground.Clone().ToImage<Rgb, byte>().Data;
            var imageWithOriginalDepthData = imageWithOriginalDepth.Data;

            //Parallel.For(0, height, y =>
            //{
                for (var y = 0; y < height; y++) {
                byte originalDepthValue;
                for (var x = 0; x < width; x++)
                {
                    for (var z = 0; z < 3; z++)
                    {
                        // DON'T REMOVE CAST (it is necessary!!! :) )
                        var depthValue = Math.Abs((byte)imageRemovedBackgroundData[y, x, z]);

                        if (depthValue > lowCutOffDepth && depthValue < highCutOffDepth)
                            originalDepthValue = (byte)imageData[y, x, z];
                        else
                            originalDepthValue = 0;

                        imageWithOriginalDepthData[y, x, z] = originalDepthValue;
                    }
                }
                }
            //});

            UMat ret = new UMat();
            UMat tmp = new UMat();

            CvInvoke.Erode(imageWithOriginalDepth.ToUMat(),
                ret,
                new Mat(),
                new System.Drawing.Point(-1, -1),
                2,
                Emgu.CV.CvEnum.BorderType.Default,
                new MCvScalar());
            CvInvoke.Dilate(ret,
                tmp,
                new Mat(),
                new System.Drawing.Point(-1, -1),
                2,
                Emgu.CV.CvEnum.BorderType.Default,
                new MCvScalar());
            CvInvoke.PyrUp(tmp, ret);
            CvInvoke.PyrDown(ret, data.Data);

            return data;

            //CvInvoke.cvNormalize(imageRemovedBackground.Ptr, imageRemovedBackground.Ptr, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);

            //return imageRemovedBackground.Copy();
        }

        private bool BuildingBackgroundImage(UMat data)
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = data.Clone();
                return true;
            }

            if (++_collectedBackgroundImages < BackgroundSubtractionSamples)
            {
                var alpha = 0.8;
                CvInvoke.AddWeighted(_backgroundImage, 1-alpha, data, alpha, 0, _backgroundImage);
                //_backgroundImage.AccumulateWeighted(data, 0.8);
                return true;
            }

            return false;
        }
    }
}
