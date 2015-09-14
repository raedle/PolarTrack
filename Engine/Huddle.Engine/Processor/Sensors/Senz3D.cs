﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.Structure;
using Huddle.Engine.Data;
using Huddle.Engine.Processor.Sensors.Utils;
using Huddle.Engine.Util;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace Huddle.Engine.Processor.Sensors
{
    [ViewTemplate("Senz3D", "Senz3D", "/Huddle.Engine;component/Resources/kinect.png")]
    public class Senz3D : BaseProcessor
    {
        #region private fields
        
        private UtilMPipeline _pp;

        private PXCMCapture.Device _device;

        private bool _isRunning;

        private long _frameId = -1;

        private Rectangle _rgbInDepthROI = new Rectangle(0, 0, 0, 0);

        #endregion

        #region properties

        #region ColorImageProfile

        [IgnoreDataMember]
        public static string[] ColorImageProfiles
        {
            get
            {
                return new[]
                {
                    "1280 x 720",
                    "640 x 480"
                };
            }
        }

        /// <summary>
        /// The <see cref="ColorImageProfile" /> property's name.
        /// </summary>
        public const string ColorImageProfilePropertyName = "ColorImageProfile";

        private string _colorImageProfile = ColorImageProfiles.First();

        /// <summary>
        /// Sets and gets the ColorImageProfile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ColorImageProfile
        {
            get
            {
                return _colorImageProfile;
            }

            set
            {
                if (_colorImageProfile == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorImageProfilePropertyName);
                _colorImageProfile = value;
                RaisePropertyChanged(ColorImageProfilePropertyName);
            }
        }

        #endregion

        #region ColorImageSource

        /// <summary>
        /// The <see cref="ColorImageSource" /> property's name.
        /// </summary>
        public const string ColorImageSourcePropertyName = "ColorImageSource";

        private BitmapSource _colorImageSource = null;

        /// <summary>
        /// Sets and gets the ColorImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource ColorImageSource
        {
            get
            {
                return _colorImageSource;
            }

            set
            {
                if (_colorImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorImageSourcePropertyName);
                _colorImageSource = value;
                RaisePropertyChanged(ColorImageSourcePropertyName);
            }
        }

        #endregion

        #region DepthImageSource

        /// <summary>
        /// The <see cref="DepthImageSource" /> property's name.
        /// </summary>
        public const string DepthImageSourcePropertyName = "DepthImageSource";

        private BitmapSource _depthImageSource = null;

        /// <summary>
        /// Sets and gets the DepthImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DepthImageSource
        {
            get
            {
                return _depthImageSource;
            }

            set
            {
                if (_depthImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthImageSourcePropertyName);
                _depthImageSource = value;
                RaisePropertyChanged(DepthImageSourcePropertyName);
            }
        }

        #endregion

        #region ConfidenceMapImageSource

        /// <summary>
        /// The <see cref="ConfidenceMapImageSource" /> property's name.
        /// </summary>
        public const string ConfidenceMapImageSourcePropertyName = "ConfidenceMapImageSource";

        private BitmapSource _confidenceMapImageSource = null;

        /// <summary>
        /// Sets and gets the ConfidenceMapImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource ConfidenceMapImageSource
        {
            get
            {
                return _confidenceMapImageSource;
            }

            set
            {
                if (_confidenceMapImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(ConfidenceMapImageSourcePropertyName);
                _confidenceMapImageSource = value;
                RaisePropertyChanged(ConfidenceMapImageSourcePropertyName);
            }
        }

        #endregion

        #region UVMapImageSource

        /// <summary>
        /// The <see cref="UVMapImageSource" /> property's name.
        /// </summary>
        public const string UVMapImageSourcePropertyName = "UVMapImageSource";

        private BitmapSource _uvMapImageSource = null;

        /// <summary>
        /// Sets and gets the ConfidenceMapImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource UVMapImageSource
        {
            get
            {
                return _uvMapImageSource;
            }

            set
            {
                if (_uvMapImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(UVMapImageSourcePropertyName);
                _uvMapImageSource = value;
                RaisePropertyChanged(UVMapImageSourcePropertyName);
            }
        }

        #endregion

        #region RgbOfDepthImageSource

        /// <summary>
        /// The <see cref="RgbOfDepthImageSource" /> property's name.
        /// </summary>
        public const string RgbOfDepthImageSourcePropertyName = "RgbOfDepthImageSource";

        private BitmapSource _RgbOfDepthImageSource = null;

        /// <summary>
        /// Sets and gets the ConfidenceMapImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource RgbOfDepthImageSource
        {
            get
            {
                return _RgbOfDepthImageSource;
            }

            set
            {
                if (_RgbOfDepthImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(RgbOfDepthImageSourcePropertyName);
                _RgbOfDepthImageSource = value;
                RaisePropertyChanged(RgbOfDepthImageSourcePropertyName);
            }
        }

        #endregion

        #region DepthOfRgbImageSource

        /// <summary>
        /// The <see cref="DepthOfRgbImageSource" /> property's name.
        /// </summary>
        public const string DepthOfRgbImageSourcePropertyName = "DepthOfRgbImageSource";

        private BitmapSource _DepthOfRgbImageSource = null;

        /// <summary>
        /// Sets and gets the ConfidenceMapImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DepthOfRgbImageSource
        {
            get
            {
                return _DepthOfRgbImageSource;
            }

            set
            {
                if (_DepthOfRgbImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthOfRgbImageSourcePropertyName);
                _DepthOfRgbImageSource = value;
                RaisePropertyChanged(DepthOfRgbImageSourcePropertyName);
            }
        }

        #endregion

        #region DepthConfidenceThreshold

        /// <summary>
        /// The <see cref="DepthConfidenceThreshold" /> property's name.
        /// </summary>
        public const string DepthConfidenceThresholdPropertyName = "DepthConfidenceThreshold";

        private float _depthConfidenceThreshold = 0;

        /// <summary>
        /// Sets and gets the DepthConfidenceThreshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public float DepthConfidenceThreshold
        {
            get
            {
                return _depthConfidenceThreshold;
            }

            set
            {
                if (_depthConfidenceThreshold == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthConfidenceThresholdPropertyName);
                _depthConfidenceThreshold = value;
                RaisePropertyChanged(DepthConfidenceThresholdPropertyName);
            }
        }

        #endregion

        #region DepthSmoothing

        /// <summary>
        /// The <see cref="DepthSmoothing" /> property's name.
        /// </summary>
        public const string DepthSmoothingPropertyName = "DepthSmoothing";

        private bool _depthSmoothing = false;

        /// <summary>
        /// Sets and gets the DepthSmoothing property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool DepthSmoothing
        {
            get
            {
                return _depthSmoothing;
            }

            set
            {
                if (_depthSmoothing == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthSmoothingPropertyName);
                _depthSmoothing = value;
                RaisePropertyChanged(DepthSmoothingPropertyName);
            }
        }

        #endregion

        #region UvMapChecked

        /// <summary>
        /// The <see cref="UvMapChecked" /> property's name.
        /// </summary>
        public const string UvMapCheckedPropertyName = "UvMapChecked";

        private bool _uvMapCheckedProperty = false;

        /// <summary>
        /// Sets and gets the UvMapChecked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UvMapChecked
        {
            get
            {
                return _uvMapCheckedProperty;
            }

            set
            {
                if (_uvMapCheckedProperty == value)
                {
                    return;
                }

                RaisePropertyChanging(UvMapCheckedPropertyName);
                _uvMapCheckedProperty = value;
                RaisePropertyChanged(UvMapCheckedPropertyName);
            }
        }

        #endregion

        #region RgbOfDepthChecked

        /// <summary>
        /// The <see cref="RgbOfDepthCheckedProperty" /> property's name.
        /// </summary>
        public const string RgbOfDepthCheckedPropertyName = "RgbOfDepthCheckedProperty";

        private bool _rgbOfDepthCheckedProperty = false;

        /// <summary>
        /// Sets and gets the RgbOfDepthCheckedProperty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool RgbOfDepthChecked
        {
            get
            {
                return _rgbOfDepthCheckedProperty;
            }

            set
            {
                if (_rgbOfDepthCheckedProperty == value)
                {
                    return;
                }

                RaisePropertyChanging(RgbOfDepthCheckedPropertyName);
                _rgbOfDepthCheckedProperty = value;
                RaisePropertyChanged(RgbOfDepthCheckedPropertyName);
            }
        }

        #endregion

        #region DepthOfRgbChecked

        /// <summary>
        /// The <see cref="DepthOfRgbChecked" /> property's name.
        /// </summary>
        public const string DepthOfRgbCheckedPropertyName = "DepthOfRgbChecked";

        private bool _depethOfRgbChecked = false;

        /// <summary>
        /// Sets and gets the DepthOfRgbChecked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool DepthOfRgbChecked
        {
            get
            {
                return _depethOfRgbChecked;
            }

            set
            {
                if (_depethOfRgbChecked == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthOfRgbCheckedPropertyName);
                _depethOfRgbChecked = value;
                RaisePropertyChanged(DepthOfRgbCheckedPropertyName);
            }
        }

        #endregion

        #region ColorImageFrameTime
        /// <summary>
        /// The <see cref="ColorImageFrameTime" /> property's name.
        /// </summary>
        public const string ColorImageFrameTimePropertyName = "ColorImageFrameTime";

        private long _ColorImageFrameTime = 0;

        /// <summary>
        /// Sets and gets the ColorImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long ColorImageFrameTime
        {
            get
            {
                return _ColorImageFrameTime;
            }

            set
            {
                if (_ColorImageFrameTime == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorImageFrameTimePropertyName);
                _ColorImageFrameTime = value;
                RaisePropertyChanged(ColorImageFrameTimePropertyName);
            }
        }
        #endregion

        #region DepthImageFrameTime
        /// <summary>
        /// The <see cref="DepthImageFrameTime" /> property's name.
        /// </summary>
        public const string DepthImageFrameTimePropertyName = "DepthImageFrameTime";

        private long _DepthImageFrameTime = 0;

        /// <summary>
        /// Sets and gets the DepthImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long DepthImageFrameTime
        {
            get
            {
                return _DepthImageFrameTime;
            }

            set
            {
                if (_DepthImageFrameTime == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthImageFrameTimePropertyName);
                _DepthImageFrameTime = value;
                RaisePropertyChanged(DepthImageFrameTimePropertyName);
            }
        }
        #endregion

        #region ConfidenceMapImageFrameTime
        /// <summary>
        /// The <see cref="ConfidenceMapImageFrameTime" /> property's name.
        /// </summary>
        public const string ConfidenceMapImageFrameTimePropertyName = "ConfidenceMapImageFrameTime";

        private long _ConfidenceMapImageFrameTime = 0;

        /// <summary>
        /// Sets and gets the ConfidenceMapImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long ConfidenceMapImageFrameTime
        {
            get
            {
                return _ConfidenceMapImageFrameTime;
            }

            set
            {
                if (_ConfidenceMapImageFrameTime == value)
                {
                    return;
                }

                RaisePropertyChanging(ConfidenceMapImageFrameTimePropertyName);
                _ConfidenceMapImageFrameTime = value;
                RaisePropertyChanged(ConfidenceMapImageFrameTimePropertyName);
            }
        }
        #endregion

        #region UVMapImageFrameTime
        /// <summary>
        /// The <see cref="UVMapImageFrameTime" /> property's name.
        /// </summary>
        public const string UVMapImageFrameTimePropertyName = "UVMapImageFrameTime";

        private long _UVMapImageFrameTime = 0;

        /// <summary>
        /// Sets and gets the UVMapImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long UVMapImageFrameTime
        {
            get
            {
                return _UVMapImageFrameTime;
            }

            set
            {
                if (_UVMapImageFrameTime == value)
                {
                    return;
                }

                RaisePropertyChanging(UVMapImageFrameTimePropertyName);
                _UVMapImageFrameTime = value;
                RaisePropertyChanged(UVMapImageFrameTimePropertyName);
            }
        }

        #endregion

        #region RgbOfDepthImageFrameTime
        /// <summary>
        /// The <see cref="RgbOfDepthImageFrameTime" /> property's name.
        /// </summary>
        public const string RgbOfDepthImageFrameTimePropertyName = "RgbOfDepthImageFrameTime";

        private long _myProperty = 0;

        /// <summary>
        /// Sets and gets the RgbOfDepthImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long RgbOfDepthImageFrameTime
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                RaisePropertyChanging(RgbOfDepthImageFrameTimePropertyName);
                _myProperty = value;
                RaisePropertyChanged(RgbOfDepthImageFrameTimePropertyName);
            }
        }
        #endregion

        #region DepthOfRgbImageFrameTime
        /// <summary>
        /// The <see cref="DepthOfRgbImageFrameTime" /> property's name.
        /// </summary>
        public const string DepthOfRgbImageFrameTimePropertyName = "DepthOfRgbImageFrameTime";

        private long _DepthOfRgbImageFrameTime = 0;

        /// <summary>
        /// Sets and gets the DepthOfRgbImageFrameTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long DepthOfRgbImageFrameTime
        {
            get
            {
                return _DepthOfRgbImageFrameTime;
            }

            set
            {
                if (_DepthOfRgbImageFrameTime == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthOfRgbImageFrameTimePropertyName);
                _DepthOfRgbImageFrameTime = value;
                RaisePropertyChanged(DepthOfRgbImageFrameTimePropertyName);
            }
        }
        #endregion

        #region MinDepthValue

        /// <summary>
        /// The <see cref="MinDepthValue" /> property's name.
        /// </summary>
        public const string MinDepthValuePropertyName = "MinDepthValue";

        private float _minDepthThreshold = 0.0f;

        /// <summary>
        /// Sets and gets the MinDepthValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float MinDepthValue
        {
            get
            {
                return _minDepthThreshold;
            }

            set
            {
                if (_minDepthThreshold == value)
                {
                    return;
                }

                RaisePropertyChanging(MinDepthValuePropertyName);
                _minDepthThreshold = value;
                RaisePropertyChanged(MinDepthValuePropertyName);
            }
        }

        #endregion

        #region MaxDepthValue

        /// <summary>
        /// The <see cref="MaxDepthValue" /> property's name.
        /// </summary>
        public const string MaxDepthValuePropertyName = "MaxDepthValue";

        private float _maxDepthValue = 5000.0f;

        /// <summary>
        /// Sets and gets the MaxDepthValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float MaxDepthValue
        {
            get
            {
                return _maxDepthValue;
            }

            set
            {
                if (_maxDepthValue == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxDepthValuePropertyName);
                _maxDepthValue = value;
                RaisePropertyChanged(MaxDepthValuePropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public Senz3D()
        {
            PXCMSession session;
            var sts = PXCMSession.CreateInstance(out session);

            Debug.Assert(sts >= pxcmStatus.PXCM_STATUS_NO_ERROR, "could not create session instance");

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case DepthConfidenceThresholdPropertyName:
                        if (_device != null)
                            _device.SetProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_CONFIDENCE_THRESHOLD, _depthConfidenceThreshold);
                        break;

                    case ColorImageProfilePropertyName:
                        //Stop();
                        _rgbInDepthROI = new Rectangle(0, 0, 0, 0);
                        //Thread.Sleep(2000);
                        //Start();
                        break;
                }
            };
        }

        #endregion

        public override IData Process(IData data)
        {
            return null;
        }

        #region override methods

        public override void Start()
        {
            var thread = new Thread(DoRendering);
            thread.Start();
            Thread.Sleep(5);
        }

        public override void Stop()
        {
            _isRunning = false;
        }

        #endregion

        #region private methods

        private DateTime timestamp;

        private void DoRendering()
        {
            _isRunning = true;

            /* UtilMPipeline works best for synchronous color and depth streaming */
            _pp = new UtilMPipeline();

            /* Set Input Source */
            _pp.capture.SetFilter("DepthSense Device 325V2");

            /* Set Color & Depth Resolution */
            PXCMCapture.VideoStream.ProfileInfo cinfo = GetConfiguration(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32);
            _pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, cinfo.imageInfo.width, cinfo.imageInfo.height);
            _pp.capture.SetFilter(ref cinfo); // only needed to set FPS

            PXCMCapture.VideoStream.ProfileInfo dinfo2 = GetConfiguration(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
            _pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, dinfo2.imageInfo.width, dinfo2.imageInfo.height);
            _pp.capture.SetFilter(ref dinfo2); // only needed to set FPS

            /* Initialization */
            if (!_pp.Init())
            {
                LogFormat("Could not initialize Senz3D hardware");
                HasErrorState = true;
                return;
            }

            var capture = _pp.capture;
            _device = capture.device;
            _device.SetProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_CONFIDENCE_THRESHOLD, DepthConfidenceThreshold);
            _device.QueryProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_LOW_CONFIDENCE_VALUE, out EmguExtensions.LowConfidence);
            _device.QueryProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_SATURATION_VALUE, out EmguExtensions.Saturation);

            while (_isRunning)
            {
                /* If raw depth is needed, disable smoothing */
                _pp.capture.device.SetProperty(PXCMCapture.Device.Property.PROPERTY_DEPTH_SMOOTHING, DepthSmoothing ? 1 : 0);

                /* Wait until a frame is ready */
                if (!_pp.AcquireFrame(true)) break;
                if (_pp.IsDisconnected()) break;

                /* Get RGB color image */
                Stopwatch sw = Stopwatch.StartNew();
                var color = _pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_COLOR);
                var colorBitmap = Senz3DUtils.GetRgb32Pixels(color);
                var colorImage = new Image<Rgb, byte>(colorBitmap);
                var colorImageCopy = colorImage.Copy();
                ColorImageFrameTime = sw.ElapsedMilliseconds;

                /* Get depth image */
                sw.Restart();
                var depth = _pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
                var depthImageAndConfidence = Senz3DUtils.GetHighPrecisionDepthImage(depth, MinDepthValue, MaxDepthValue);
                var depthImage = (Image<Gray, float>)depthImageAndConfidence[0];
                var depthImageCopy = depthImage.Copy();
                var confidenceMapImage = (Image<Rgb, Byte>)depthImageAndConfidence[1];
                var confidenceMapImageCopy = confidenceMapImage.Copy();
                DepthImageFrameTime = sw.ElapsedMilliseconds;
                ConfidenceMapImageFrameTime = 0;

                bool getRgbInDepthROI = false;
                /* if rgbInDepthROI is undefined get uvmap and rgbofdepth and rgbInDepthROI */
                if (_rgbInDepthROI.Left == 0 && _rgbInDepthROI.Right == 0 && _rgbInDepthROI.Width == 0 &&
                    _rgbInDepthROI.Height == 0)
                {
                    getRgbInDepthROI = true;
                }


                /* Get UV map */
                Image<Rgb, float> uvMapImage, uvMapImageCopy;
                if (UvMapChecked || getRgbInDepthROI)
                {
                    sw.Restart();
                    uvMapImage = Senz3DUtils.GetDepthUvMap(depth);
                    uvMapImageCopy = uvMapImage.Copy();
                    UVMapImageFrameTime = sw.ElapsedMilliseconds;
                }
                else
                {
                    uvMapImage = null;
                    uvMapImageCopy = null;
                    UVMapImageFrameTime = -1;
                }

                /* Get RgbOfDepth */
                Image<Rgb, byte> rgbOfDepthImage, rgbOfDepthImageCopy;
                if ((RgbOfDepthChecked && uvMapImage != null) || getRgbInDepthROI)
                {
                    sw.Restart();
                    if (getRgbInDepthROI)
                    {
                        rgbOfDepthImage = Senz3DUtils.GetRgbOfDepthPixels(depthImage.ToUMat(),
                            colorImage.ToUMat(),
                            uvMapImage.ToUMat(),
                            true,
                            ref _rgbInDepthROI); //TODO may break here
                        Stage(new ROI(this, "rgbInDepthROI")
                        {
                            RoiRectangle = _rgbInDepthROI
                        });
                        Push();

                        LogFormat("Identified rgbInDepthROI as {0}", _rgbInDepthROI);
                    }
                    else
                    {
                        rgbOfDepthImage = Senz3DUtils.GetRgbOfDepthPixels(depthImage.ToUMat(),
                            colorImage.ToUMat(),
                            uvMapImage.ToUMat()); //TODO may break here
                    }

                    rgbOfDepthImageCopy = rgbOfDepthImage.Copy();
                    RgbOfDepthImageFrameTime = sw.ElapsedMilliseconds;
                }
                else
                {
                    rgbOfDepthImage = null;
                    rgbOfDepthImageCopy = null;
                    RgbOfDepthImageFrameTime = -1;
                }

                /* Get DepthOfRGB */
                Image<Gray, float> depthOfRgbImage, depthOfRgbImageCopy;
                if (DepthOfRgbChecked && uvMapImage != null)
                {
                    sw.Restart();
                    depthOfRgbImage = Senz3DUtils.GetDepthOfRGBPixels(depthImage, colorImage, uvMapImage);
                    depthOfRgbImageCopy = depthOfRgbImage.Copy();
                    DepthOfRgbImageFrameTime = sw.ElapsedMilliseconds;
                }
                else
                {
                    depthOfRgbImage = null;
                    depthOfRgbImageCopy = null;
                    DepthOfRgbImageFrameTime = -1;
                }

                _pp.ReleaseFrame();

                if (IsRenderContent)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var bitmap = colorImageCopy.ToBitmapSource(true);
                        colorImageCopy.Dispose();
                        return bitmap;
                    }).ContinueWith(s => ColorImageSource = s.Result);

                    Task.Factory.StartNew(() =>
                    {
                        var bitmap = depthImageCopy.ToGradientBitmapSource(true, EmguExtensions.LowConfidence, EmguExtensions.Saturation);
                        depthImageCopy.Dispose();
                        return bitmap;
                    }).ContinueWith(s => DepthImageSource = s.Result);

                    Task.Factory.StartNew(() =>
                    {
                        var bitmap = confidenceMapImageCopy.ToBitmapSource(true);
                        confidenceMapImageCopy.Dispose();
                        return bitmap;
                    }).ContinueWith(s => ConfidenceMapImageSource = s.Result);

                    /* draw uvmap */
                    if (uvMapImage != null)
                        Task.Factory.StartNew(() =>
                            {
                                var bitmap = uvMapImageCopy.ToBitmapSource(true);
                                uvMapImageCopy.Dispose();
                                return bitmap;
                            }).ContinueWith(s => UVMapImageSource = s.Result);

                    /* draw rgbofdepth */
                    if (rgbOfDepthImage != null)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            var bitmap = rgbOfDepthImageCopy.ToBitmapSource(true);
                            rgbOfDepthImageCopy.Dispose();
                            return bitmap;
                        }).ContinueWith(s => RgbOfDepthImageSource = s.Result);
                    }

                    /* draw depthofrgb */
                    if (depthOfRgbImage != null)
                        Task.Factory.StartNew(() =>
                        {
                            var bitmap = depthOfRgbImageCopy.ToGradientBitmapSource(true, EmguExtensions.LowConfidence, EmguExtensions.Saturation);
                            depthOfRgbImageCopy.Dispose();
                            return bitmap;
                        }).ContinueWith(s => DepthOfRgbImageSource = s.Result);
                }

                var dc = new DataContainer(++_frameId, DateTime.Now)
                    {
                        new RgbImageData(this, "color", colorImage),
                        new GrayFloatImage(this, "depth", depthImage),
                        new RgbImageData(this, "confidence", confidenceMapImage),
                    };

                if (uvMapImage != null) dc.Add(new RgbFloatImage(this, "uvmap", uvMapImage));
                if (rgbOfDepthImage != null) dc.Add(new RgbImageData(this, "rgbofdepth", rgbOfDepthImage));
                if (depthOfRgbImage != null) dc.Add(new GrayFloatImage(this, "depthofrgb", depthOfRgbImage));
                Publish(dc);
            }

            _pp.Close();
            _pp.Dispose();
        }

        private PXCMCapture.VideoStream.ProfileInfo GetConfiguration(PXCMImage.ColorFormat format)
        {
            var pinfo = new PXCMCapture.VideoStream.ProfileInfo { imageInfo = { format = format } };

            if (((int)format & (int)PXCMImage.ImageType.IMAGE_TYPE_COLOR) != 0)
            {
                if (ColorImageProfile.Equals("1280 x 720"))
                {
                    pinfo.imageInfo.width = 1280;
                    pinfo.imageInfo.height = 720;
                }
                else if (ColorImageProfile.Equals("640 x 480"))
                {
                    pinfo.imageInfo.width = 640;
                    pinfo.imageInfo.height = 480;
                }

                pinfo.frameRateMin.numerator = 1;
                pinfo.frameRateMax.numerator = 25;
                pinfo.frameRateMin.denominator = pinfo.frameRateMax.denominator = 1;
            }
            else
            {
                pinfo.imageInfo.width = 320;
                pinfo.imageInfo.height = 240;

                pinfo.frameRateMin.numerator = 25;
                pinfo.frameRateMin.denominator = 1;
            }

            return pinfo;
        }

        #endregion
    }
}
