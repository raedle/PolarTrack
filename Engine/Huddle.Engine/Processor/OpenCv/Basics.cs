using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using Emgu.CV.External.Extensions;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Data;
using Huddle.Engine.Properties;
using Huddle.Engine.Util;
using Point = System.Windows.Point;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Basics", "Basics")]
    public class Basics : UMatProcessor
    {
        #region private fields

        private bool _mouseDown;
        private Point _mousePoint;

        #endregion

        #region commands

        public RelayCommand<SenderAwareEventArgs> MouseDownCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> MouseMoveCommand { get; private set; }
        public RelayCommand<SenderAwareEventArgs> MouseUpCommand { get; private set; }

        #endregion

        #region public properties

        #region IsInitialized

        // IsInitialized is used to set ROI if filter is used the first time.
        public bool IsInitialized { get; set; }

        #endregion

        #region IsUseROI

        /// <summary>
        /// The <see cref="IsUseROI" /> property's name.
        /// </summary>
        public const string IsUseROIPropertyName = "IsUseROI";

        private bool _isUseROI = true;

        /// <summary>
        /// Sets and gets the IsUseROI property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUseROI
        {
            get
            {
                return _isUseROI;
            }

            set
            {
                if (_isUseROI == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUseROIPropertyName);
                _isUseROI = value;
                RaisePropertyChanged(IsUseROIPropertyName);
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

        #region Scale

        /// <summary>
        /// The <see cref="Scale" /> property's name.
        /// </summary>
        public const string ScalePropertyName = "Scale";

        private double _scale = 1.0;

        /// <summary>
        /// Sets and gets the Scale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                if (_scale == value)
                {
                    return;
                }

                RaisePropertyChanging(ScalePropertyName);
                _scale = value;
                RaisePropertyChanged(ScalePropertyName);
            }
        }

        #endregion

        #region FlipVertical

        /// <summary>
        /// The <see cref="FlipVertical" /> property's name.
        /// </summary>
        public const string FlipVerticalPropertyName = "FlipVertical";

        private bool _flipVertical = false;

        /// <summary>
        /// Sets and gets the FlipVertical property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool FlipVertical
        {
            get
            {
                return _flipVertical;
            }

            set
            {
                if (_flipVertical == value)
                {
                    return;
                }

                RaisePropertyChanging(FlipVerticalPropertyName);
                _flipVertical = value;
                RaisePropertyChanged(FlipVerticalPropertyName);
            }
        }

        #endregion

        #region FlipHorizontal

        /// <summary>
        /// The <see cref="FlipHorizontal" /> property's name.
        /// </summary>
        public const string FlipHorizontalPropertyName = "FlipHorizontal";

        private bool _flipHorizontal = false;

        /// <summary>
        /// Sets and gets the FlipHorizontal property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool FlipHorizontal
        {
            get
            {
                return _flipHorizontal;
            }

            set
            {
                if (_flipHorizontal == value)
                {
                    return;
                }

                RaisePropertyChanging(FlipHorizontalPropertyName);
                _flipHorizontal = value;
                RaisePropertyChanged(FlipHorizontalPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public Basics()
            : base(false)
        {
            IsInitialized = false;
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

                ROI = ROITemp;
                ROITemp = Rectangle.Empty;

                sender.ReleaseMouseCapture();

                _mouseDown = false;
                e.Handled = true;
            });
        }

        #endregion

        public override IData Process(IData data)
        {
            var roi = data as ROI;
            if (roi != null)
                ROI = roi.RoiRectangle;

            return base.Process(data);
        }

        public override UMatData PreProcess(UMatData data)
        {
            if (data.Key != "color" || data.Key != "confidence") // TODO can i check the type earlier or how can i avoid unecesary calls 
            {
                return data;
            }

            if (!IsInitialized)
            {
                ROI = new Rectangle(0, 0, data.Width, data.Height);

                IsInitialized = true;
            }

            var _data = base.PreProcess(data);

            if (data.Key == "confidence")
            {
                _data.Data.ToImage<Rgb,byte>().Draw(ROI, Rgbs.Red, 1);
            }
            else if (data.Key == "color")
            {
                _data.Data.ToImage<Rgb, byte>().Draw(ROI, Rgbs.Red, 1);
            }
            //TODO leak?
            //var img = _data.Data.Clone().ToImage<Rgb, byte>();
            //img.Draw(ROI, Rgbs.Red, 1);
            //_data.Data.Dispose();
            //_data.Data = img.ToUMat();

            return _data;
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (data.Key != "color" || data.Key != "confidence")
            {
                return data;
            }

            // mirror image
            try
            {
                UMat imageCopy = new UMat();
                if (IsUseROI)
                {
                    imageCopy.Dispose();
                    imageCopy = new UMat(data.Data, ROI); //TODO does this work?
                    //data.Data.CopyTo(imageCopy, ROI);
                }
                else
                {
                    imageCopy = data.Data.Clone();
                }

                // TODO Revise code.
                if (Scale != 1.0)
                {
                    UMat imageCopy2 = new UMat();

                    CvInvoke.Resize(imageCopy,
                        imageCopy2,
                        new System.Drawing.Size((int)(data.Width * Scale), (int)(data.Height * Scale)),
                        0,
                        0,
                        Emgu.CV.CvEnum.Inter.Cubic);

                    imageCopy.Dispose();
                    imageCopy = imageCopy2;
                }

                var flipCode = Emgu.CV.CvEnum.FlipType.None;

                if (FlipHorizontal)
                    flipCode |= FlipType.Horizontal;
                if (FlipVertical)
                    flipCode |= FlipType.Vertical;

                if (flipCode != FlipType.None)
                {
                    CvInvoke.Flip(imageCopy, imageCopy, flipCode);
                }

                data.Data = imageCopy;
                return data;
            }
            catch (Exception e)
            {
                LogFormat("{0}", e.StackTrace);
                return null;
            }
        }
    }
}
