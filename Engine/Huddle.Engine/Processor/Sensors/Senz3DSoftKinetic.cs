using DepthSenseWrapper;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Data;
using Huddle.Engine.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Point = System.Windows.Point;

namespace Huddle.Engine.Processor.Sensors
{
    [ViewTemplate("Senz3D (SoftKinetic)", "Senz3DSoftKinetic", "/Huddle.Engine;component/Resources/kinect.png")]
    public class Senz3DSoftKinetic : BaseProcessor
    {
        #region commands

        public RelayCommand<SenderAwareEventArgs> MouseDownCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> MouseMoveCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> MouseUpCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> ColorMouseDownCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> ColorMouseMoveCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> ColorMouseUpCommand { get; private set; }

        #endregion

        #region private fields

        private bool _isRunning;

        private long _frameId = -1;

        private Wrapper DSW;
        private ColorSampleCallBack _colorSampleCallback = null;
        private DepthSampleCallBack _depthSampleCallback = null;

        private bool _mouseDown;
        private Point _mousePoint;
        private bool _colorMouseDown;
        private Point _colorMousePoint;

        private System.Timers.Timer timer = null;

        private static Senz3DSoftKinetic instance = null;

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
                    "640 x 480"/*,1280 x 720"*/
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

        #region DepthFrameRate

        /// <summary>
        /// The <see cref="DepthFrameRate" /> property's name.
        /// </summary>
        public const string DepthFrameRatePropertyName = "Depth Camera Framerate";

        private int _depthFrameRate = 60;

        /// <summary>
        /// Sets and gets the DepthFrameRate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int DepthFrameRate
        {
            get
            {
                return _depthFrameRate;
            }

            set
            {
                if (_depthFrameRate == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthFrameRatePropertyName);
                _depthFrameRate = value;
                RaisePropertyChanged(DepthFrameRatePropertyName);
            }
        }

        #endregion

        #region ColorFrameRate

        /// <summary>
        /// The <see cref="ColorFrameRate" /> property's name.
        /// </summary>
        public const string ColorFrameRatePropertyName = "Color Camera Framerate";

        private int _colorFrameRate = 30;

        /// <summary>
        /// Sets and gets the ColorFrameRate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ColorFrameRate
        {
            get
            {
                return _colorFrameRate;
            }

            set
            {
                if (_colorFrameRate == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorFrameRatePropertyName);
                _colorFrameRate = value;
                RaisePropertyChanged(ColorFrameRatePropertyName);
            }
        }

        #endregion

        #region DepthFPS

        public const string DepthFPSPropertyName = "DepthFPS";
        private int _depthFPS = 0;

        public int DepthFPS
        {
            get
            {
                return _depthFPS;
            }

            set
            {
                RaisePropertyChanging(DepthFPSPropertyName);
                _depthFPS = value;
                RaisePropertyChanged(DepthFPSPropertyName);
            }
        }

        #endregion

        #region ROITemp

        /// <summary>
        /// The <see cref="ROITemp" /> property's name.
        /// </summary>
        public const string ROITempPropertyName = "ROITemp";

        private Rectangle _roiTemp = Rectangle.Empty;

        /// <summary>
        /// Sets and gets the ROITemp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Rectangle ROITemp
        {
            get
            {
                return _roiTemp;
            }

            set
            {
                if (_roiTemp == value)
                {
                    return;
                }

                RaisePropertyChanging(ROITempPropertyName);
                _roiTemp = value;
                RaisePropertyChanged(ROITempPropertyName);
            }
        }

        #endregion

        #region ROI

        /// <summary>
        /// The <see cref="ROI" /> property's name.
        /// </summary>
        public const string ROIPropertyName = "ROI";

        private Rectangle _roi = new Rectangle(0, 0, 1, 1);

        /// <summary>
        /// Sets and gets the ROI property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Rectangle ROI
        {
            get
            {
                return _roi;
            }

            set
            {
                if (_roi == value)
                {
                    return;
                }

                RaisePropertyChanging(ROIPropertyName);
                _roi = value;
                RaisePropertyChanged(ROIPropertyName);
            }
        }

        #endregion

        #region EmitROI

        /// <summary>
        /// The <see cref="EmitROI" /> property's name.
        /// </summary>
        public const string EmitROIPropertyName = "EmitROI";

        private bool _emitROI = false;

        /// <summary>
        /// Sets and gets the EmitROI property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EmitROI
        {
            get
            {
                return _emitROI;
            }

            set
            {
                if (_emitROI == value)
                {
                    return;
                }

                RaisePropertyChanging(EmitROIPropertyName);
                _emitROI = value;
                RaisePropertyChanged(EmitROIPropertyName);

                if (_isRunning == true && EmitROI == true)
                {
                    startEmitROITimer();
                }
                if (_isRunning == true && EmitROI == false && EmitColorROI == false)
                {
                    stopEmitROITimer();
                }
            }
        }

        #endregion

        #region ColorROITemp

        /// <summary>
        /// The <see cref="ColorROITemp" /> property's name.
        /// </summary>
        public const string ColorROITempPropertyName = "ColorROITemp";

        private Rectangle _colorRoiTemp = Rectangle.Empty;

        /// <summary>
        /// Sets and gets the ColorROITemp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Rectangle ColorROITemp
        {
            get
            {
                return _colorRoiTemp;
            }

            set
            {
                if (_colorRoiTemp == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorROITempPropertyName);
                _colorRoiTemp = value;
                RaisePropertyChanged(ColorROITempPropertyName);
            }
        }

        #endregion

        #region ColorROI

        /// <summary>
        /// The <see cref="ColorROI" /> property's name.
        /// </summary>
        public const string ColorROIPropertyName = "ColorROI";

        private Rectangle _colorRoi = new Rectangle(0, 0, 1, 1);

        /// <summary>
        /// Sets and gets the ColorROI property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Rectangle ColorROI
        {
            get
            {
                return _colorRoi;
            }

            set
            {
                if (_colorRoi == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorROIPropertyName);
                _colorRoi = value;
                RaisePropertyChanged(ColorROIPropertyName);
            }
        }

        #endregion

        #region EmitColorROI

        /// <summary>
        /// The <see cref="EmitColorROI" /> property's name.
        /// </summary>
        public const string EmitColorROIPropertyName = "EmitColorROI";

        private bool _emitColorROI = false;

        /// <summary>
        /// Sets and gets the EmitColorROI property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EmitColorROI
        {
            get
            {
                return _emitColorROI;
            }

            set
            {
                if (_emitColorROI == value)
                {
                    return;
                }

                RaisePropertyChanging(EmitColorROIPropertyName);
                _emitColorROI = value;
                RaisePropertyChanged(EmitColorROIPropertyName);

                if (_isRunning == true && EmitColorROI == true)
                {
                    startEmitROITimer();
                }
                if (_isRunning == true && EmitColorROI == false && EmitROI == false)
                {
                    stopEmitROITimer();
                }
            }
        }

        #endregion

        #endregion

        #region ctor

        public Senz3DSoftKinetic()
        {
            DSW = new Wrapper();
            bool init = DSW.init();

            // load the values from driver OR
            // TODO load settings and push them to driver
            //_depthFrameRate = DSW.m_depthFrameRate;
            //_colorFrameRate = DSW.m_colorFrameRate;
            DSW.setDepthFrameRate(30);
            DepthFrameRate = 30;
            DSW.setColorFrameRate(25);
            ColorFrameRate = 25;

            // emitROI Timer
            if (timer == null)
            {
                timer = new System.Timers.Timer(1000);
                timer.BeginInit();
                timer.AutoReset = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(emitROITimerElapsed);
                timer.EndInit();
            }

            // ROI
            MouseDownCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as IInputElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null) return;

                _mouseDown = true;

                sender.CaptureMouse();

                _mousePoint = e.GetPosition(sender);

                e.Handled = true;
            });

            MouseMoveCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as FrameworkElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null || !_mouseDown) return;

                var position = e.GetPosition(sender);
                var diff = position - _mousePoint;

                var x = Math.Min(_mousePoint.X, position.X);
                var y = Math.Min(_mousePoint.Y, position.Y);
                var width = Math.Abs(diff.X);
                var height = Math.Abs(diff.Y);

                ROITemp = new Rectangle((int)x, (int)y, (int)width, (int)height);

                e.Handled = true;
            });

            MouseUpCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as IInputElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null || !_mouseDown) return;

                // check if ROI is valid
                var newx = Math.Max(0, Math.Min(ROITemp.X, DepthImageSource.PixelWidth));
                var newy = Math.Max(0, Math.Min(ROITemp.Y, DepthImageSource.PixelHeight));
                var neww = ROITemp.Width;
                var newh = ROITemp.Height;
                if (ROITemp.X + ROITemp.Width > DepthImageSource.PixelWidth)
                {
                    neww = DepthImageSource.PixelWidth - ROITemp.X;
                }
                if (ROITemp.Y + ROITemp.Height > DepthImageSource.PixelHeight)
                {
                    newh = DepthImageSource.PixelHeight - ROITemp.Y;
                }


                ROI = new Rectangle(newx, newy, neww, newh);
                ROITemp = Rectangle.Empty;

                sender.ReleaseMouseCapture();

                _mouseDown = false;
                e.Handled = true;

                emitROI();
            });

            // color ROI
            ColorMouseDownCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as IInputElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null) return;

                _colorMouseDown = true;

                sender.CaptureMouse();

                _colorMousePoint = e.GetPosition(sender);

                e.Handled = true;
            });

            ColorMouseMoveCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as FrameworkElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null || !_colorMouseDown) return;

                var position = e.GetPosition(sender);
                var diff = position - _colorMousePoint;

                var x = Math.Min(_colorMousePoint.X, position.X);
                var y = Math.Min(_colorMousePoint.Y, position.Y);
                var width = Math.Abs(diff.X);
                var height = Math.Abs(diff.Y);

                ColorROITemp = new Rectangle((int)x, (int)y, (int)width, (int)height);

                e.Handled = true;
            });

            ColorMouseUpCommand = new RelayCommand<SenderAwareEventArgs>(args =>
            {
                var sender = args.Sender as IInputElement;
                var e = args.OriginalEventArgs as MouseEventArgs;

                if (sender == null || e == null || !_colorMouseDown) return;

                // check if ROI is valid
                var newx = Math.Max(0, Math.Min(ColorROITemp.X * 4, ColorImageSource.PixelWidth));
                var newy = Math.Max(0, Math.Min(ColorROITemp.Y * 4, ColorImageSource.PixelHeight));
                var neww = ColorROITemp.Width * 4;
                var newh = ColorROITemp.Height * 4;
                if (ColorROITemp.X + ColorROITemp.Width > ColorImageSource.PixelWidth)
                {
                    neww = ColorImageSource.PixelWidth - ColorROITemp.X;
                }
                if (ColorROITemp.Y + ColorROITemp.Height > ColorImageSource.PixelHeight)
                {
                    newh = ColorImageSource.PixelHeight - ColorROITemp.Y;
                }


                ColorROI = new Rectangle(newx, newy, neww, newh);
                ColorROITemp = Rectangle.Empty;

                sender.ReleaseMouseCapture();

                _colorMouseDown = false;
                e.Handled = true;

                emitROI();
            });

            instance = this;
        }

        #endregion

        #region dtor

        ~Senz3DSoftKinetic()
        {
            try
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
                //TODO
                //DSW.Dispose();
                DSW = null;
            }
            finally
            {
                //base.Finalize();
            }
        }

        #endregion

        public static Senz3DSoftKinetic getInstance()
        {
            return instance;
        }

        public override IData Process(IData data)
        {
            return null;
        }

        public bool TriggerColorNode(bool value)
        {
            if (DSW == null)
                throw new Exception("Depth Sense Wrapper not initialized");

            if (DSW.GetColorNodeEnabled() != value)
            {
                return DSW.EnableColorNode(value);
            } else {
                return DSW.GetColorNodeEnabled();
            }
        }

        public bool TriggerDepthNode(bool value)
        {
            if (DSW == null)
                throw new Exception("Depth Sense Wrapper not initialized");

            if (DSW.GetDepthNodeEnabled() != value)
            {
                return DSW.EnableDepthNode(value);
            }
            else
            {
                return DSW.GetDepthNodeEnabled();
            }
        }

        #region override methods

        public override void Start()
        {
            DSW.start();

            // register CallBacks
            if (_colorSampleCallback == null)
                _colorSampleCallback = new ColorSampleCallBack(ColorSampleCallBackFunc);
            DSW.regColorSampleCallBack(_colorSampleCallback);
            if (_depthSampleCallback == null)
                _depthSampleCallback = new DepthSampleCallBack(DepthSampleCallBackFunc);
            DSW.regDepthSampleCallBack(_depthSampleCallback);

            if (EmitROI == true || EmitColorROI == true)
            {
                emitROI();
                startEmitROITimer();
            }

            _isRunning = true;
            // disable grabbing of color iamges per default?
            //TriggerColorNode(false);
        }

        public override void Stop()
        {
            stopEmitROITimer();

            DSW.unregColorSampleCallBack();
            DSW.unregDepthSampleCallBack();

            if (_isRunning)
            {
                _isRunning = false;
                DSW.stop();
            }
        }

        #endregion

        #region private methods

        private void ColorSampleCallBackFunc(ColorSample sample)
        {
            ProcessColorSample(sample);
        }

        private void DepthSampleCallBackFunc(DepthSample sample)
        {
             ProcessDepthSample(sample);
        }

        private void ProcessColorSample(ColorSample sample)
        {
            /* Get RGB color image */
            Stopwatch sw = Stopwatch.StartNew();
            var _colorImage = new Image<Bgr, byte>(sample.Width, sample.Height);
            _colorImage.Data = sample.Data;

            var colorImage = new Image<Rgb, byte>(sample.Width, sample.Height);
            CvInvoke.CvtColor(_colorImage, colorImage, ColorConversion.Bgr2Rgb);

            ColorImageFrameTime = sw.ElapsedMilliseconds;

            if (IsRenderContent)
            {
                var colorImageCopy = colorImage.Copy(); ;
                Task.Factory.StartNew(() =>
                {
                    var bitmap = colorImageCopy.ToBitmapSource(true);
                    colorImageCopy.Dispose();
                    return bitmap;
                }).ContinueWith(s => ColorImageSource = s.Result);
            }

            var dc = new Huddle.Engine.Data.DataContainer(++_frameId, DateTime.Now)
                    {
                        new UMatData(this, "color", colorImage.ToUMat())
                    };

            Publish(dc);

            _colorImage.Dispose();
            sample.Dispose();
        }

        private void ProcessDepthSample(DepthSample sample)
        {
            /* Get depth image */
            Stopwatch sw = Stopwatch.StartNew();
            var depthImage = new Image<Gray, byte>(sample.Width, sample.Height);
            var confidenceImage = new Image<Rgb, byte>(sample.Width, sample.Height);
            var minValue = MinDepthValue;
            var maxValue = MaxDepthValue;

            /*Parallel.For(0, sample.Height, (i) => {*/
            for (int i=0; i < sample.Height; i++) {
                for (int j = 0; j < sample.Width; j++)
                {
                    //var depth = sample.Ptr[i * sample.Width + j];
                    var depth = sample.Data[i,j,0];

                    if (depth != EmguExtensions.LowConfidence && depth != EmguExtensions.Saturation)
                    {
                        var test = (depth - minValue) / (maxValue - minValue);

                        if (test < 0)
                            test = 0.0f;
                        else if (test > 1.0)
                            test = 1.0f;

                        test *= 255.0f;

                        depthImage.Data[i, j, 0] = (byte)test;
                    }
                    else
                    {
                        depthImage.Data[i, j, 0] = (byte)(depth * 255.0f);

                        if (depth == EmguExtensions.LowConfidence)
                        {
                            confidenceImage.Data[i, j, 0] = 255;
                        }
                        else if (depth == EmguExtensions.Saturation)
                        {
                            confidenceImage.Data[i, j, 0] = 0;
                        }
                    }
                }
            }/*);*/
            var depthImageCopy = depthImage.Copy();
            var confidenceImageCopy = confidenceImage.Copy();

            DepthImageFrameTime = sw.ElapsedMilliseconds;
            ConfidenceMapImageFrameTime = 0;

            if (IsRenderContent)
            {
                Task.Factory.StartNew(() =>
                {
                    var bitmap = depthImageCopy.ToBitmapSource(true);
                    depthImageCopy.Dispose();
                    return bitmap;
                }).ContinueWith(s => DepthImageSource = s.Result);

                Task.Factory.StartNew(() =>
                {
                    var bitmap = confidenceImageCopy.ToBitmapSource(true);
                    confidenceImageCopy.Dispose();
                    return bitmap;
                }).ContinueWith(s => ConfidenceMapImageSource = s.Result);
            }

            var dc = new Huddle.Engine.Data.DataContainer(++_frameId, DateTime.Now)
                    {
                        new UMatData(this, "depth", depthImage.ToUMat()),
                        new UMatData(this, "confidence", confidenceImage.ToUMat())
                    };

            Publish(dc);

            sample.Dispose();
        }

        //public delegate emitRoi(Object state, System.Timers.ElapsedEventArgs e) {
        //    emitROI();
        //}

        // Publish ROI for depth and confidence
        private void emitROI()
        {
            var dc = new Huddle.Engine.Data.DataContainer(++_frameId, DateTime.Now);
            if (EmitROI)
            {
                var roi = new ROI(this, "confidenceDepthROI");
                roi.RoiRectangle = ROI;
                dc.Add(roi);
            }
            if (EmitColorROI)
            {
                var roi = new ROI(this, "colorROI");
                roi.RoiRectangle = ColorROI;
                dc.Add(roi);
            }
            Publish(dc);
        }

        private void startEmitROITimer()
        {
            if (timer != null)
            {
                timer.Start();
            }
        }
        
        private void stopEmitROITimer()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private void emitROITimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            emitROI();
        }

        #endregion
    }
}
