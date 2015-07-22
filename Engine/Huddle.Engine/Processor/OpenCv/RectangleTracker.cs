using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using Huddle.Engine.Data;
using Huddle.Engine.Processor.Complex.PolygonIntersection;
using Huddle.Engine.Processor.OpenCv.Struct;
using Huddle.Engine.Util;
using DPoint = System.Drawing.Point;
using WPoint = System.Windows.Point;
using Polygon = Huddle.Engine.Processor.Complex.PolygonIntersection.Polygon;
using Rectangle = System.Drawing.Rectangle;
using Vector = Huddle.Engine.Processor.Complex.PolygonIntersection.Vector;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Rectangle Tracker", "RectangleTracker")]
    public class RectangleTracker : UMatProcessor
    {
        #region private fields

        private readonly List<RectangularObject> _objects = new List<RectangularObject>();

        private UMat _depthImage;

        #endregion

        #region properties

        #region BlobType

        /// <summary>
        /// The <see cref="BlobType" /> property's name.
        /// </summary>
        public const string BlobTypePropertyName = "BlobType";

        private string _blobType = string.Empty;

        /// <summary>
        /// Sets and gets the BlobType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string BlobType
        {
            get
            {
                return _blobType;
            }

            set
            {
                if (_blobType == value)
                {
                    return;
                }

                RaisePropertyChanging(BlobTypePropertyName);
                _blobType = value;
                RaisePropertyChanged(BlobTypePropertyName);
            }
        }

        #endregion

        #region MinAngle

        /// <summary>
        /// The <see cref="MinAngle" /> property's name.
        /// </summary>
        public const string MinAnglePropertyName = "MinAngle";

        private int _minAngle = 80;

        /// <summary>
        /// Sets and gets the MinAngle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MinAngle
        {
            get
            {
                return _minAngle;
            }

            set
            {
                if (_minAngle == value)
                {
                    return;
                }

                RaisePropertyChanging(MinAnglePropertyName);
                _minAngle = value;
                RaisePropertyChanged(MinAnglePropertyName);
            }
        }

        #endregion

        #region MaxAngle

        /// <summary>
        /// The <see cref="MaxAngle" /> property's name.
        /// </summary>
        public const string MaxAnglePropertyName = "MaxAngle";

        private int _maxAngle = 100;

        /// <summary>
        /// Sets and gets the MaxAngle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MaxAngle
        {
            get
            {
                return _maxAngle;
            }

            set
            {
                if (_maxAngle == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxAnglePropertyName);
                _maxAngle = value;
                RaisePropertyChanged(MaxAnglePropertyName);
            }
        }

        #endregion

        #region MinContourArea

        /// <summary>
        /// The <see cref="MinContourArea" /> property's name.
        /// </summary>
        public const string MinContourAreaPropertyName = "MinContourArea";

        private double _minContourArea = 5.0;

        /// <summary>
        /// Sets and gets the MinContourArea property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MinContourArea
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

        #region MaxContourArea

        /// <summary>
        /// The <see cref="MaxContourArea" /> property's name.
        /// </summary>
        public const string MaxContourAreaPropertyName = "MaxContourArea";

        private double _maxContourArea = 10.0;

        /// <summary>
        /// Sets and gets the MaxContourArea property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MaxContourArea
        {
            get
            {
                return _maxContourArea;
            }

            set
            {
                if (_maxContourArea == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxContourAreaPropertyName);
                _maxContourArea = value;
                RaisePropertyChanged(MaxContourAreaPropertyName);
            }
        }

        #endregion

        #region Timeout

        /// <summary>
        /// The <see cref="Timeout" /> property's name.
        /// </summary>
        public const string TimeoutPropertyName = "Timeout";

        private int _timeout = 500;

        /// <summary>
        /// Sets and gets the Timeout property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }

            set
            {
                if (_timeout == value)
                {
                    return;
                }

                RaisePropertyChanging(TimeoutPropertyName);
                _timeout = value;
                RaisePropertyChanged(TimeoutPropertyName);
            }
        }

        #endregion

        #region IsFillContours

        /// <summary>
        /// The <see cref="IsFillContours" /> property's name.
        /// </summary>
        public const string IsFillContoursPropertyName = "IsFillContours";

        private bool _isFillContours;

        /// <summary>
        /// Sets and gets the IsFillContours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool IsFillContours
        {
            get
            {
                return _isFillContours;
            }

            set
            {
                if (_isFillContours == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFillContoursPropertyName);
                _isFillContours = value;
                RaisePropertyChanged(IsFillContoursPropertyName);
            }
        }

        #endregion

        #region IsDrawContours

        /// <summary>
        /// The <see cref="IsDrawContours" /> property's name.
        /// </summary>
        public const string IsDrawContoursPropertyName = "IsDrawContours";

        private bool _isDrawContours = true;

        /// <summary>
        /// Sets and gets the IsDrawContours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDrawContours
        {
            get
            {
                return _isDrawContours;
            }

            set
            {
                if (_isDrawContours == value)
                {
                    return;
                }

                RaisePropertyChanging(IsDrawContoursPropertyName);
                _isDrawContours = value;
                RaisePropertyChanged(IsDrawContoursPropertyName);
            }
        }

        #endregion

        #region IsDrawAllContours

        /// <summary>
        /// The <see cref="IsDrawAllContours" /> property's name.
        /// </summary>
        public const string IsDrawAllContoursPropertyName = "IsDrawAllContours";

        private bool _isDrawAllContours = false;

        /// <summary>
        /// Sets and gets the IsDrawAllContours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDrawAllContours
        {
            get
            {
                return _isDrawAllContours;
            }

            set
            {
                if (_isDrawAllContours == value)
                {
                    return;
                }

                RaisePropertyChanging(IsDrawAllContoursPropertyName);
                _isDrawAllContours = value;
                RaisePropertyChanged(IsDrawAllContoursPropertyName);
            }
        }

        #endregion

        #region IsDrawCenter

        /// <summary>
        /// The <see cref="IsDrawCenter" /> property's name.
        /// </summary>
        public const string IsDrawCenterPropertyName = "IsDrawCenter";

        private bool _isDrawCenter = true;

        /// <summary>
        /// Sets and gets the IsDrawCenter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDrawCenter
        {
            get
            {
                return _isDrawCenter;
            }

            set
            {
                if (_isDrawCenter == value)
                {
                    return;
                }

                RaisePropertyChanging(IsDrawCenterPropertyName);
                _isDrawCenter = value;
                RaisePropertyChanged(IsDrawCenterPropertyName);
            }
        }

        #endregion

        #region MinDetectRightAngles

        /// <summary>
        /// The <see cref="MinDetectRightAngles" /> property's name.
        /// </summary>
        public const string MinDetectRightAnglesPropertyName = "MinDetectRightAngles";

        private int _minDetectRightAngles = 3;

        /// <summary>
        /// Sets and gets the MinDetectRightAngles property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MinDetectRightAngles
        {
            get
            {
                return _minDetectRightAngles;
            }

            set
            {
                if (_minDetectRightAngles == value)
                {
                    return;
                }

                RaisePropertyChanging(MinDetectRightAnglesPropertyName);
                _minDetectRightAngles = value;
                RaisePropertyChanged(MinDetectRightAnglesPropertyName);
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

        #region IsUpdateOccludedRectangles

        /// <summary>
        /// The <see cref="IsUpdateOccludedRectangles" /> property's name.
        /// </summary>
        public const string IsUpdateOccludedRectanglesPropertyName = "IsUpdateOccludedRectangles";

        private bool _isUpdateOccludedRectangles = true;

        /// <summary>
        /// Sets and gets the IsUpdateOccludedRectangles property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUpdateOccludedRectangles
        {
            get
            {
                return _isUpdateOccludedRectangles;
            }

            set
            {
                if (_isUpdateOccludedRectangles == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUpdateOccludedRectanglesPropertyName);
                _isUpdateOccludedRectangles = value;
                RaisePropertyChanged(IsUpdateOccludedRectanglesPropertyName);
            }
        }

        #endregion

        #region MaxRestoreDistance

        /// <summary>
        /// The <see cref="MaxRestoreDistance" /> property's name.
        /// </summary>
        public const string MaxRestoreDistancePropertyName = "MaxRestoreDistance";

        private double _maxRestoreDistance = 5.0;

        /// <summary>
        /// Sets and gets the MaxRestoreDistance property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MaxRestoreDistance
        {
            get
            {
                return _maxRestoreDistance;
            }

            set
            {
                if (_maxRestoreDistance == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxRestoreDistancePropertyName);
                _maxRestoreDistance = value;
                RaisePropertyChanged(MaxRestoreDistancePropertyName);
            }
        }

        #endregion

        #region FixMaskErode

        /// <summary>
        /// The <see cref="FixMaskErode" /> property's name.
        /// </summary>
        public const string FixMaskErodePropertyName = "FixMaskErode";

        private int _fixMaskErode = 0;

        /// <summary>
        /// Sets and gets the FixMaskErode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FixMaskErode
        {
            get
            {
                return _fixMaskErode;
            }

            set
            {
                if (_fixMaskErode == value)
                {
                    return;
                }

                RaisePropertyChanging(FixMaskErodePropertyName);
                _fixMaskErode = value;
                RaisePropertyChanged(FixMaskErodePropertyName);
            }
        }

        #endregion

        #region FixMaskDilate

        /// <summary>
        /// The <see cref="FixMaskDilate" /> property's name.
        /// </summary>
        public const string FixMaskDilatePropertyName = "FixMaskDilate";

        private int _fixMaskDilate = 0;

        /// <summary>
        /// Sets and gets the FixMaskDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FixMaskDilate
        {
            get
            {
                return _fixMaskDilate;
            }

            set
            {
                if (_fixMaskDilate == value)
                {
                    return;
                }

                RaisePropertyChanging(FixMaskDilatePropertyName);
                _fixMaskDilate = value;
                RaisePropertyChanged(FixMaskDilatePropertyName);
            }
        }

        #endregion

        #region IsFirstErodeThenDilateFixMask

        /// <summary>
        /// The <see cref="IsFirstErodeThenDilateFixMask" /> property's name.
        /// </summary>
        public const string IsFirstErodeThenDilateFixMaskPropertyName = "IsFirstErodeThenDilateFixMask";

        private bool _isFirstErodeThenDilateFixMask = true;

        /// <summary>
        /// Sets and gets the IsFirstErodeThenDilateFixMask property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFirstErodeThenDilateFixMask
        {
            get
            {
                return _isFirstErodeThenDilateFixMask;
            }

            set
            {
                if (_isFirstErodeThenDilateFixMask == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFirstErodeThenDilateFixMaskPropertyName);
                _isFirstErodeThenDilateFixMask = value;
                RaisePropertyChanged(IsFirstErodeThenDilateFixMaskPropertyName);
            }
        }

        #endregion

        #region DepthPatchesErode

        /// <summary>
        /// The <see cref="DepthPatchesErode" /> property's name.
        /// </summary>
        public const string DepthPatchesErodePropertyName = "DepthPatchesErode";

        private int _depthPatchesErode = 4;

        /// <summary>
        /// Sets and gets the DepthPatchesErode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int DepthPatchesErode
        {
            get
            {
                return _depthPatchesErode;
            }

            set
            {
                if (_depthPatchesErode == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthPatchesErodePropertyName);
                _depthPatchesErode = value;
                RaisePropertyChanged(DepthPatchesErodePropertyName);
            }
        }

        #endregion

        #region DepthPatchesDilate

        /// <summary>
        /// The <see cref="DepthPatchesDilate" /> property's name.
        /// </summary>
        public const string DepthPatchesDilatePropertyName = "DepthPatchesDilate";

        private int _depthPatchesDilate = 4;

        /// <summary>
        /// Sets and gets the DepthPatchesDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int DepthPatchesDilate
        {
            get
            {
                return _depthPatchesDilate;
            }

            set
            {
                if (_depthPatchesDilate == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthPatchesDilatePropertyName);
                _depthPatchesDilate = value;
                RaisePropertyChanged(DepthPatchesDilatePropertyName);
            }
        }

        #endregion

        #region IsFirstErodeThenDilateDepthPatches

        /// <summary>
        /// The <see cref="IsFirstErodeThenDilateDepthPatches" /> property's name.
        /// </summary>
        public const string IsFirstErodeThenDilateDepthPatchesPropertyName = "IsFirstErodeThenDilateDepthPatches";

        private bool _isFirstErodeThenDilateDepthPatches = true;

        /// <summary>
        /// Sets and gets the IsFirstErodeThenDilateDepthPatches property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFirstErodeThenDilateDepthPatches
        {
            get
            {
                return _isFirstErodeThenDilateDepthPatches;
            }

            set
            {
                if (_isFirstErodeThenDilateDepthPatches == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFirstErodeThenDilateDepthPatchesPropertyName);
                _isFirstErodeThenDilateDepthPatches = value;
                RaisePropertyChanged(IsFirstErodeThenDilateDepthPatchesPropertyName);
            }
        }

        #endregion

        #region SurvivePixelThreshold

        /// <summary>
        /// The <see cref="SurvivePixelThreshold" /> property's name.
        /// </summary>
        public const string SurvivePixelThresholdPropertyName = "SurvivePixelThreshold";

        private int _survivePixelThreshold = 5;

        /// <summary>
        /// Sets and gets the DepthPatchesDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SurvivePixelThreshold
        {
            get
            {
                return _survivePixelThreshold;
            }

            set
            {
                if (_survivePixelThreshold == value)
                {
                    return;
                }

                RaisePropertyChanging(SurvivePixelThresholdPropertyName);
                _survivePixelThreshold = value;
                RaisePropertyChanged(SurvivePixelThresholdPropertyName);
            }
        }

        #endregion

        #region AllowedRepairPixelsRatio

        /// <summary>
        /// The <see cref="AllowedRepairPixelsRatio" /> property's name.
        /// </summary>
        public const string AllowedRepairPixelsRatioPropertyName = "AllowedRepairPixelsRatio";

        private int _allowedRepairPixelsRatio = 95;

        /// <summary>
        /// Sets and gets the DepthPatchesDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int AllowedRepairPixelsRatio
        {
            get
            {
                return _allowedRepairPixelsRatio;
            }

            set
            {
                if (_allowedRepairPixelsRatio == value)
                {
                    return;
                }

                RaisePropertyChanging(AllowedRepairPixelsRatioPropertyName);
                _allowedRepairPixelsRatio = value;
                RaisePropertyChanged(AllowedRepairPixelsRatioPropertyName);
            }
        }

        #endregion

        #region Image Sources

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

        #region DepthPatchesImageSource

        /// <summary>
        /// The <see cref="DepthPatchesImageSource" /> property's name.
        /// </summary>
        public const string DepthPatchesImageSourcePropertyName = "DepthPatchesImageSource";

        private BitmapSource _depthPatchesImageSource;

        /// <summary>
        /// Sets and gets the DepthPatchesImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DepthPatchesImageSource
        {
            get
            {
                return _depthPatchesImageSource;
            }

            set
            {
                if (_depthPatchesImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthPatchesImageSourcePropertyName);
                _depthPatchesImageSource = value;
                RaisePropertyChanged(DepthPatchesImageSourcePropertyName);
            }
        }

        #endregion

        #region DepthFixedImageSource

        /// <summary>
        /// The <see cref="DepthFixedImageSource" /> property's name.
        /// </summary>
        public const string DepthFixedImageSourcePropertyName = "DepthFixedImageSource";

        private BitmapSource _depthFixedImageSource;

        /// <summary>
        /// Sets and gets the DepthFixedImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DepthFixedImageSource
        {
            get
            {
                return _depthFixedImageSource;
            }

            set
            {
                if (_depthFixedImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthFixedImageSourcePropertyName);
                _depthFixedImageSource = value;
                RaisePropertyChanged(DepthFixedImageSourcePropertyName);
            }
        }

        #endregion

        #region FixMaskImageSource

        /// <summary>
        /// The <see cref="FixMaskImageSource" /> property's name.
        /// </summary>
        public const string FixMaskImageSourcePropertyName = "FixMaskImageSource";

        private BitmapSource _fixMaskImageSource;

        /// <summary>
        /// Sets and gets the FixMaskImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource FixMaskImageSource
        {
            get
            {
                return _fixMaskImageSource;
            }

            set
            {
                if (_fixMaskImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(FixMaskImageSourcePropertyName);
                _fixMaskImageSource = value;
                RaisePropertyChanged(FixMaskImageSourcePropertyName);
            }
        }

        #endregion

        #endregion

        #endregion

        #region ctor

        public RectangleTracker()
            : base(false)
        {

        }

        #endregion

        public override void Stop()
        {
            _objects.Clear();

            base.Stop();
        }

        public override IData Process(IData data)
        {
            var depthImageData = data as UMatData;
            if (depthImageData != null && Equals(depthImageData.Key, "depth"))
            {
                if (_depthImage != null)
                    _depthImage.Dispose();

                _depthImage = (depthImageData.Copy() as UMatData).Data;
            }

            var device = data as Device;
            var objects = _objects.ToArray();
            if (device != null && device.IsIdentified)
            {
                var anyBlob = objects.Any(o => o.Id == device.OriginalBlobId);
                if (anyBlob)
                {
                    var objectForDevice = objects.Single(o => o.Id == device.OriginalBlobId);
                    if (!objectForDevice.IsCorrectSize && _depthImage != null)
                    {
                        var width = (float)(_depthImage.Cols * (1 / device.RgbImageToDisplayRatio.X));
                        var height = (float)(_depthImage.Rows * (1 / device.RgbImageToDisplayRatio.Y));

                        var angle = (float)(device.Angle % 90) - 90;
                        var deviceAngle = (float)device.Angle % 360;

                        objectForDevice.OriginDepthShape = objectForDevice.Shape;

                        var oldShape = objectForDevice.Shape;
                        var shapeSize = oldShape.Size;
                        var shapeAngle = oldShape.Angle;

                        objectForDevice.LastAngle = shapeAngle;
                        if (shapeSize.Width > shapeSize.Height)
                        {
                            objectForDevice.SetCorrectSize(width, height);
                            objectForDevice.Shape = new RotatedRect(oldShape.Center, new SizeF(width, height), shapeAngle);
                        }
                        else
                        {
                            objectForDevice.SetCorrectSize(height, width);
                            objectForDevice.Shape = new RotatedRect(oldShape.Center, new SizeF(height, width), shapeAngle);
                        }
                    }
                }
            }

            return base.Process(data);
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (data.Key != "confidence") //TODO
            {
                return data;
            }

            UMat u_image = data.Data;

            var imageWidth = u_image.Cols;
            var imageHeight = u_image.Rows;

            // Get time for current processing.
            var now = DateTime.Now;

            // Remove all objects, which have a last update past the timeout threshold.
            _objects.RemoveAll(o => (now - o.LastUpdate).TotalMilliseconds > Timeout);

            // Reset tracking state of all objects in the previous frame
            foreach (var o in _objects)
                o.State = TrackingState.NotTracked;

            // Needed to be wrapped in closure -> required by Parallel.ForEach below.
            UMat[] outputImage = { new UMat(imageHeight, imageWidth,     DepthType.Cv8U, 3) };
            outputImage[0].SetTo(Rgbs.Black.MCvScalar);

            var threadSafeObjects = _objects.ToArray();

            if (threadSafeObjects.Length > 0)
            {
                // Try to identify objects even if they are connected tightly (without a gap).
                //Parallel.ForEach(threadSafeObjects, obj => FindObjectByBlankingKnownObjects(image, ref outputImage[0], now, threadSafeObjects, obj)); // TODO Parallel.ForEach does not work :(
                foreach (var foundObjects in threadSafeObjects.Select(obj => FindObjectByBlankingKnownObjects(false, ref u_image, ref outputImage[0], now, threadSafeObjects, obj, true)))
                {
                    _objects.AddRange(foundObjects);

                    if (foundObjects.Any())
                        LogFormat("Updated but also found {0} new objects {1}", foundObjects.Length, foundObjects);
                }

                // Update occluded objects. It tries to find not yet identified and maybe occluded objects.
                if (IsUpdateOccludedRectangles)
                    UpdateOccludedObjects(u_image, ref outputImage[0], now, threadSafeObjects);

                // Try to find new objects.
                var foundNewObjects = FindObjectByBlankingKnownObjects(false, ref u_image, ref outputImage[0], now, _objects.ToArray());
                _objects.AddRange(foundNewObjects);

                if (foundNewObjects.Any())
                    LogFormat("Found {0} new objects {1}", foundNewObjects.Length, foundNewObjects);
            }
            else
            {
                // Find yet unidentified objects
                var foundObjects = FindObjectByBlankingKnownObjects(false, ref u_image, ref outputImage[0], now, _objects.ToArray());
                _objects.AddRange(foundObjects);

                if (foundObjects.Any())
                    LogFormat("Found {0} new objects {1}", foundObjects.Length, foundObjects);
            }

            foreach (var obj in _objects.ToArray())
            {
                if (IsRenderContent)
                {
                    if (IsFillContours)
                        CvInvoke.FillConvexPoly(outputImage[0],
                            new Emgu.CV.Util.VectorOfPoint(obj.Points),
                            Rgbs.Yellow.MCvScalar);

                    if (IsDrawContours)
                    {
                        Rgb color;
                        switch (obj.State)
                        {
                            case TrackingState.Tracked:
                                color = Rgbs.Green;
                                break;
                            case TrackingState.Occluded:
                                color = Rgbs.Yellow;
                                break;
                            case TrackingState.NotTracked:
                                color = Rgbs.Red;
                                break;
                            default:
                                color = Rgbs.Cyan;
                                break;
                        }
                        PointF[] vert = obj.Shape.GetVertices();
                        DPoint[] vertices = { new DPoint((int)vert[0].X, (int)vert[0].Y),
                                            new DPoint((int)vert[1].X, (int)vert[1].Y),
                                            new DPoint((int)vert[2].X, (int)vert[2].Y),
                                            new DPoint((int)vert[3].X, (int)vert[3].Y)};
                        CvInvoke.Polylines(outputImage[0],
                            vertices,
                            true,
                            color.MCvScalar,
                            2);
                    }

                    if (IsDrawCenter)
                    {
                        var center = obj.Center;
                        CvInvoke.Circle(outputImage[0], new System.Drawing.Point((int)center.X, (int)center.Y), 2 ,Rgbs.Green.MCvScalar, 3);
                    }

                    if (IsDrawCenter)
                    {
                        var center = obj.SmoothedCenter;
                        CvInvoke.Circle(outputImage[0], new System.Drawing.Point((int)center.X, (int)center.Y), 2 ,Rgbs.Blue.MCvScalar, 3);
                    }

                    Emgu.CV.CvInvoke.PutText(outputImage[0],
                        string.Format("Id {0}", obj.Id),
                        new DPoint((int)obj.Shape.Center.X, (int)obj.Shape.Center.Y),
                        EmguFont.Font,
                        EmguFont.Scale,
                        Rgbs.White.MCvScalar);
                }

                var bounds = obj.Bounds;
                var smoothedCenter = obj.SmoothedCenter;
                //var smoothedAngle = obj.SmoothedAngle;
                Stage(new BlobData(this, obj.Id, BlobType)
                {
                    Id = obj.Id,
                    Center = new WPoint(smoothedCenter.X / imageWidth, smoothedCenter.Y / imageHeight),
                    State = obj.State,
                    Angle = obj.Angle,
                    //Angle = rawObject.SlidingAngle,
                    Shape = obj.Shape,
                    Polygon = obj.Polygon,
                    Area = new Rect
                    {
                        X = bounds.X / (double)imageWidth,
                        Y = bounds.Y / (double)imageHeight,
                        Width = bounds.Width / (double)imageWidth,
                        Height = bounds.Height / (double)imageHeight,
                    }
                });
            }

            Push();

            data.Data = outputImage[0];

            return data;
        }

        private DPoint[] PointFToDPoint(PointF[] input)
        {
            DPoint[] ret = new DPoint[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                ret[i] = new DPoint((int)input[i].X, (int)input[i].Y);
            }

            return ret;
        }

        private Emgu.CV.Util.VectorOfPoint PointFToVOP(PointF[] input)
        {
            return new Emgu.CV.Util.VectorOfPoint(PointFToDPoint(input));
        }

        /// <summary>
        /// Find an object by blanking out known objects except for the parameter object in the
        /// source image. If obj == null it will blank out all objects.
        /// </summary>
        /// <param name="occlusionTracking"></param>
        /// <param name="image"></param>
        /// <param name="outputImage"></param>
        /// <param name="objects"></param>
        /// <param name="obj"></param>
        /// <param name="updateTime"></param>
        /// <param name="useROI"></param>
        private RectangularObject[] FindObjectByBlankingKnownObjects(bool occlusionTracking, ref UMat image, ref UMat outputImage, DateTime updateTime, RectangularObject[] objects, RectangularObject obj = null, bool useROI = false)
        {
            var imageWidth = image.Cols;
            var imageHeight = image.Rows;

            var objectsToBlank = obj != null ? objects.Where(o => o != obj) : objects;

            // Blank previous objects from previous frame
            //var blankedImage = image.Clone();
            //var blankedImageGray = image.Clone();
            UMat blankedImageGray = image.DeepClone();
            UMat blankedImage = image.DeepClone();

            foreach (var otherObject in objectsToBlank)
            {
                //TODO
                (blankedImage.ToImage() as Image<Rgb, byte>).Draw(otherObject.Shape, Rgbs.Black, -1);
            }


            UMat u_blankedImageGray = new UMat();
            CvInvoke.CvtColor(blankedImage, u_blankedImageGray, ColorConversion.Rgb2Gray);
            //var blankedImageGray2 = (blankedImageGray.ToImage() as Image<Rgb, Byte>).Convert<Gray, Byte>();
            //UMat u_blankedImageGray = blankedImageGray2.ToUMat();

            //blankedImageGray = blankedImageGray.Erode(3);

            var roi = (blankedImage.ToImage() as Image<Rgb, byte>).ROI;
            if (useROI)
            {
                const int threshold = 20;
                var b = obj.Bounds;

                roi = new Rectangle(b.X - threshold, b.Y - threshold, b.Width + 2 * threshold, b.Height + 2 * threshold);

                //blankedImageGray.ROI = roi;

                if (IsRenderContent)
                {
                    CvInvoke.Rectangle(outputImage, roi, Rgbs.AquaSky.MCvScalar, 2);
                }

                UMat maskImage = new UMat(imageHeight, imageWidth,DepthType.Cv8U,1);
                CvInvoke.Rectangle(maskImage, roi, new Gray(255).MCvScalar, -1);

                CvInvoke.BitwiseAnd(u_blankedImageGray,
                    maskImage,
                    u_blankedImageGray);
            }

            //blankedImageGray = blankedImageGray.Erode(2).Dilate(1).Erode(1).Dilate(1);
            //blankedImageGray = blankedImageGray.Dilate(6).Erode(8);

            if (IsRenderContent && occlusionTracking)
            {
                #region Render Depth Fixed Image

                var debugImageCopy = u_blankedImageGray.DeepClone();
                Task.Factory.StartNew(() =>
                {
                    var bitmapSource = debugImageCopy.ToImage().ToBitmapSource(true);
                    debugImageCopy.Dispose();
                    return bitmapSource;
                }).ContinueWith(t => DebugImageSource = t.Result);

                #endregion
            }

            //var oldROI = outputImage.ROI;
            //outputImage.ROI = roi;

            var newObjects = FindRectangles(occlusionTracking, u_blankedImageGray, ref outputImage, updateTime, objects, imageWidth, imageHeight);

            //outputImage.ROI = oldROI;

            // Remove objects that intersect with previous objects.
            var filteredObjects = newObjects.ToList();
            foreach (var newObject in newObjects)
            {
                if (objects.Any(o =>
                            {
                                var r = PolygonCollisionUtils.PolygonCollision(o.Polygon, newObject.Polygon, Vector.Empty);
                                return r.WillIntersect;
                            }))
                {
                    filteredObjects.Remove(newObject);
                }
            }

            u_blankedImageGray.Dispose();

            return filteredObjects.ToArray();
        }

        /// <summary>
        /// Tries to find occluded objects based on their previous positions.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="updateTime"></param>
        /// <param name="outputImage"></param>
        /// <param name="objects"></param>
        private void UpdateOccludedObjects(/*Image<Rgb, byte>*/UMat u_image, ref UMat outputImage, DateTime updateTime, RectangularObject[] objects)
        {
            var occludedObjects = objects.Where(o => !Equals(o.LastUpdate, updateTime)).ToArray();

            // ignore if no objects are occluded but continue in case is render content set true to update debug view
            if (occludedObjects.Length < 1 || _depthImage == null)
                return;

            var enclosedOutputImage = outputImage.Clone();
            Parallel.ForEach(occludedObjects, obj => UpdateOccludedObject(u_image, ref enclosedOutputImage, updateTime, objects, obj));
        }

        /// <summary>
        /// Tries to find the occluded object based on its previous position.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="updateTime"></param>
        /// <param name="outputImage"></param>
        /// <param name="objects"></param>
        /// <param name="obj"></param>
        private void UpdateOccludedObject(/*Image<Rgb, byte>*/UMat u_image, ref UMat outputImage, DateTime updateTime, RectangularObject[] objects, RectangularObject obj)
        {
            var imageWidth = u_image.Cols;
            var imageHeight = u_image.Rows;

            UMat mask = new UMat(imageHeight, imageWidth, DepthType.Cv8U, 1);
            UMat depthPatchesImage = new UMat(imageHeight, imageWidth, DepthType.Cv32F, 1);
            //var mask = new Image<Gray, byte>(imageWidth, imageHeight);
            //var depthPatchesImage = new Image<Gray, float>(imageWidth, imageHeight);

            // create mask for objects previousl location
            // TODO
            (mask.ToImage() as Image<Gray, byte>).Draw(obj.Shape, new Gray(1), -1);
            //mask.Draw(obj.Shape, new Gray(1), -1);

            UMat depthMapBinary = new UMat(_depthImage.Rows,_depthImage.Cols,DepthType.Cv32F,1);
            CvInvoke.Threshold(_depthImage, depthMapBinary, 255, 255, ThresholdType.BinaryInv); // 255 == new Gray(255) ???
            var depthMap = depthMapBinary.Clone();

            if (depthMapBinary.Cols != imageWidth || depthMapBinary.Rows != imageHeight)
            {
                CvInvoke.Resize(depthMap,
                    depthMap,
                    new System.Drawing.Size(imageWidth, imageHeight),
                    0,
                    0,
                    Emgu.CV.CvEnum.Inter.Cubic);
            }

            if (IsFirstErodeThenDilateFixMask)
            {
                CvInvoke.Erode(mask,
                    mask,
                    new UMat(),
                    new System.Drawing.Point(-1, -1),
                    FixMaskErode,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                CvInvoke.Dilate(mask,
                    mask,
                    new UMat(),
                    new System.Drawing.Point(-1, -1),
                    FixMaskDilate,
                    BorderType.Default,
                    new MCvScalar());
            }
            else
            {
                CvInvoke.Dilate(mask,
                    mask,
                    new UMat(),
                    new System.Drawing.Point(-1, -1),
                    FixMaskDilate,
                    BorderType.Default,
                    new MCvScalar());
                CvInvoke.Erode(mask,
                   mask,
                   new UMat(),
                   new System.Drawing.Point(-1, -1),
                   FixMaskErode,
                   Emgu.CV.CvEnum.BorderType.Default,
                   new MCvScalar());
            }

            if (IsRenderContent)
            {
                #region Render Fix Mask Image

                var maskCopy = mask.DeepClone().ToImage();
                maskCopy = (maskCopy as Image<Gray,byte>).Mul(255);
                Task.Factory.StartNew(() =>
                {
                    var bitmapSource = maskCopy.ToBitmapSource(true);
                    maskCopy.Dispose();
                    return bitmapSource;
                }).ContinueWith(t => FixMaskImageSource = t.Result);

                #endregion
            }

            // TODO UMat or toimage
            CvInvoke.cvCopy(depthMap,
                depthPatchesImage,
                mask);

            //var _originPixels = new Image<Rgb, byte>(imageWidth, imageHeight);
            UMat originPixels = new UMat();
            u_image.CopyTo(originPixels, mask); // not impl atm
            //UMat u_imageCopy = u_image.DeepClone();
            //CvInvoke.cvCopy(u_imageCopy.ToImage<Rgb, byte>(),
            //    _originPixels,
            //    mask);
            //UMat originPixels = _originPixels.ToUMat();


            Mat cn1 = new Mat(imageWidth, imageHeight, DepthType.Cv8U, 1);
            Emgu.CV.Util.VectorOfMat ret = new Emgu.CV.Util.VectorOfMat(cn1);
            CvInvoke.Split(originPixels,
                ret);
            int pixelsSurvived = CvInvoke.CountNonZero(cn1);

            //int pixelsSurvived = CvInvoke.CountNonZero(originPixels);
            //var pixelsSurvived = originPixels CountNonzero()[0];
            if (pixelsSurvived < SurvivePixelThreshold) return;

            //var repairedPixels = depthPatchesImage.CountNonzero()[0];
            var repairedPixels = CvInvoke.CountNonZero(depthPatchesImage);
            var totalPixels = obj.OriginDepthShape.Size.Width * obj.OriginDepthShape.Size.Height;
            var factorOfRepairedPixels = (double)repairedPixels / totalPixels;
            //Console.WriteLine("{0}% pixels repaired.", factorOfRepairedPixels * 100);

            // Do not account for entire occlusion at this time to avoid phantom objects even if the device is not present anymore.
            if (factorOfRepairedPixels > (AllowedRepairPixelsRatio / 100.0)) return;

            // Erode and dilate depth patches image to remove small pixels around device borders.
            if (IsFirstErodeThenDilateDepthPatches)
            {
                CvInvoke.Erode(depthPatchesImage,
                    depthPatchesImage,
                    null, // or new Mat() or IntPtr.Zero
                    new System.Drawing.Point(-1, -1),
                    DepthPatchesErode,
                    Emgu.CV.CvEnum.BorderType.Default, // TODO gut oder andere methode?
                    new Emgu.CV.Structure.MCvScalar());
                CvInvoke.Dilate(depthPatchesImage,
                    depthPatchesImage,
                    null,
                    new System.Drawing.Point(-1, -1),
                    DepthPatchesDilate,
                    Emgu.CV.CvEnum.BorderType.Default, // TODO gut oder andere methode?
                    new Emgu.CV.Structure.MCvScalar());
            }
            else
            {
                CvInvoke.Dilate(depthPatchesImage,
                    depthPatchesImage,
                    null,
                    new System.Drawing.Point(-1, -1),
                    DepthPatchesDilate,
                    Emgu.CV.CvEnum.BorderType.Default, // TODO gut oder andere methode?
                    new Emgu.CV.Structure.MCvScalar());
                CvInvoke.Erode(depthPatchesImage,
                    depthPatchesImage,
                    null,
                    new System.Drawing.Point(-1, -1),
                    DepthPatchesErode,
                    Emgu.CV.CvEnum.BorderType.Default, // TODO gut oder andere methode?
                    new Emgu.CV.Structure.MCvScalar());
            }

            if (IsRenderContent)
            {
                #region Render Depth Patches Image

                var depthPatchesImageCopy = depthPatchesImage.DeepClone().ToImage();
                Task.Factory.StartNew(() =>
                {
                    var bitmapSource = depthPatchesImageCopy.ToBitmapSource(true);
                    depthPatchesImageCopy.Dispose();
                    return bitmapSource;
                }).ContinueWith(t => DepthPatchesImageSource = t.Result);

                #endregion
            }

            // ??? Clip depth patches image again to avoid depth fixed rectangles to grow.
            //CvInvoke.cvCopy(depthPatchesImage.Ptr, depthPatchesImage.Ptr, mask);

            UMat debugImage3 = new UMat();
            CvInvoke.CvtColor(depthPatchesImage, debugImage3, ColorConversion.Gray2Rgb);
            //var debugImage3 = u_depthPatchesImage.ToImage<Gray, float>().Convert<Rgb, byte>(); // TODO UMat

            UMat depthFixedImage = new UMat();
            CvInvoke.BitwiseOr(u_image,
                debugImage3,
                depthFixedImage);
            //fixedImage = fixedImage.Erode(2);

            if (IsRenderContent)
            {

                #region Render Depth Fixed Image

                UMat depthFixedImageCopy = depthFixedImage.DeepClone();
                Task.Factory.StartNew(() =>
                {
                    var bitmapSource = depthFixedImageCopy.ToBitmapSource(true);
                    depthFixedImageCopy.Dispose();
                    return bitmapSource;
                }).ContinueWith(t => DepthFixedImageSource = t.Result);

                #endregion
            }

            FindObjectByBlankingKnownObjects(true, ref depthFixedImage, ref outputImage, updateTime, objects, obj, true);
        }

        /// <summary>
        /// Find rectangles in image and add possible rectangle candidates as temporary but known objects or updates
        /// existing objects from previous frames.
        /// </summary>
        /// <param name="occlusionTracking"></param>
        /// <param name="grayImage"></param>
        /// <param name="outputImage"></param>
        /// <param name="updateTime"></param>
        /// <param name="objects"></param>
        private RectangularObject[] FindRectangles(bool occlusionTracking, /*Image<Gray, byte>*/UMat grayImage, ref UMat outputImage, DateTime updateTime, RectangularObject[] objects, int imageWidth, int imageHeight)
        {
            var newObjects = new List<RectangularObject>();

            var pixels = imageWidth * imageHeight;

            var diagonal = Math.Sqrt(Math.Pow(imageWidth, 2) + Math.Pow(imageHeight, 2));

            var maxRestoreDistance = (MaxRestoreDistance / 100.0) * diagonal;

            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();

            CvInvoke.FindContours(grayImage,
                contours,
                null, // TODO can we use hierarchy
                IsRetrieveExternal ? RetrType.External : RetrType.List,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                Emgu.CV.Util.VectorOfPoint lowApproxContour = new Emgu.CV.Util.VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours[i],
                    lowApproxContour, 
                    CvInvoke.ArcLength(contours[i], true) * 0.015, 
                    true);

                if (IsRenderContent && IsDrawAllContours)
                {
                    CvInvoke.Polylines(outputImage, lowApproxContour.ToArray(), true, Rgbs.FuchsiaRose.MCvScalar);
                }

                if (CvInvoke.ContourArea(lowApproxContour, false) > ((MinContourArea / 100.0) * pixels) && CvInvoke.ContourArea(lowApproxContour, false) < ((MaxContourArea / 100.0) * pixels)) //only consider contours with area greater than
                {
                    if (IsRenderContent && IsDrawAllContours)
                    {
                        CvInvoke.Polylines(outputImage, lowApproxContour.ToArray(), true, Rgbs.BlueTorquoise.MCvScalar);
                    }
                            
                    //outputImage.Draw(currentContour.GetConvexHull(ORIENTATION.CV_CLOCKWISE), Rgbs.BlueTorquoise, 2);

                    // Continue with next contour if current contour is not a rectangle.
                    List<DPoint> points;
                    if (!IsPlausibleRectangle(lowApproxContour.ToArray(), MinAngle, MaxAngle, MinDetectRightAngles, out points)) continue;

                    Emgu.CV.Util.VectorOfPoint highApproxContour = new Emgu.CV.Util.VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contours[i],
                        highApproxContour,
                        CvInvoke.ArcLength(contours[i], true) * 0.05,
                        true);
                    if (IsRenderContent && IsDrawAllContours)
                        CvInvoke.Polylines(outputImage, highApproxContour.ToArray(), true, Rgbs.Yellow.MCvScalar);



                    var rectangle = CvInvoke.BoundingRectangle(highApproxContour);
                    var minAreaRect = CvInvoke.MinAreaRect(highApproxContour);
                    var polygon = new Polygon(points.ToArray(), imageWidth, imageHeight);
                    var contourPoints = highApproxContour.ToArray();


                    //Contour<DPoint> cont = new Contour<DPoint>(highApproxContour, storage);

                    if (!UpdateObject(occlusionTracking,
                        highApproxContour,
                        maxRestoreDistance,
                        rectangle,
                        minAreaRect,
                        polygon,
                        contourPoints,
                        updateTime,
                        objects))
                    {
                        newObjects.Add(CreateObject(NextId(), rectangle, minAreaRect, polygon, contourPoints, updateTime));
                    }
                }
            }

            return newObjects.ToArray();
        }

        /// <summary>
        /// Determine if all the angles in the contour are within min/max angle.
        /// </summary>
        /// <param name="contour"></param>
        /// <param name="minAngle"></param>
        /// <param name="maxAngle"></param>
        /// <param name="minDetectAngles"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        private bool IsPlausibleRectangle(DPoint[] contour, int minAngle, int maxAngle, int minDetectAngles, out List<DPoint> points)
        {
            points = new List<DPoint>();

            if (contour.Length < minDetectAngles) return false; //The contour has less than 3 vertices.

            var edges = PointCollection.PolyLine(contour, true);

            var rightAngle = 0;
            for (var i = 0; i < edges.Length; i++)
            {
                var edge1 = edges[i];
                var edge2 = edges[(i + 1) % edges.Length];

                var edgeRatio = (edge1.Length / edge2.Length);

                points.Add(edge1.P1);

                var angle = Math.Abs(edge1.GetExteriorAngleDegree(edge2));

                // stop if an angle is not in min/max angle range, no need to continue
                // also stop if connected edges are more than double in ratio
                if ((angle < minAngle || angle > maxAngle) ||
                     (edgeRatio > 3.0 || 1 / edgeRatio > 3.0))
                {
                    continue;
                }

                rightAngle++;
            }

            return rightAngle >= minDetectAngles;
        }

        /// <summary>
        /// Adds a new temporary object that will be used to identify itself in the preceeding frames.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="boundingRectangle"></param>
        /// <param name="minAreaRect"></param>
        /// <param name="polygon"></param>
        /// <param name="points"></param>
        /// <param name="updateTime"></param>
        private static RectangularObject CreateObject(long id, Rectangle boundingRectangle, RotatedRect minAreaRect, Polygon polygon, DPoint[] points, DateTime updateTime)
        {
            return new RectangularObject
            {
                Id = id,
                State = TrackingState.Tracked,
                LastUpdate = updateTime,
                Center = new WPoint(minAreaRect.Center.X, minAreaRect.Center.Y),
                Bounds = boundingRectangle,
                Shape = minAreaRect,
                LastAngle = minAreaRect.Angle,
                Polygon = polygon,
                Points = points,
            };
        }

        /// <summary>
        /// Updates an object if it finds an object from last frame at a max restore distance.
        /// </summary>
        /// <param name="occluded"></param>
        /// <param name="objectContour"></param>
        /// <param name="maxRestoreDistance"></param>
        /// <param name="boundingRectangle"></param>
        /// <param name="minAreaRect"></param>
        /// <param name="polygon"></param>
        /// <param name="points"></param>
        /// <param name="updateTime"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static bool UpdateObject(bool occluded, Emgu.CV.Util.VectorOfPoint objectContour, double maxRestoreDistance, Rectangle boundingRectangle, RotatedRect minAreaRect, Polygon polygon, DPoint[] points, DateTime updateTime, IEnumerable<RectangularObject> objects)
        {
            double distance;
            var candidate = GetObjectCandidate(objects, objectContour, minAreaRect, maxRestoreDistance, out distance);

            if (candidate == null) return false;

            var deltaAngle = minAreaRect.Angle - candidate.LastAngle;

            // this is a hack but it works pretty good
            if (deltaAngle > 45)
            {
                deltaAngle -= 90;
            }
            else if (deltaAngle < -45)
            {
                deltaAngle += 90;
            }

            // create new candidate shape based on its previous shape size and the new center point and orientation.
            // This keeps the objects shape constant and avoids growing shapes when devices are connected closely or
            // an objects occludes the device.
            RotatedRect shape;
            if (candidate.IsCorrectSize)
            {
                var oldAngle = candidate.Shape.Angle;
                var diff = Math.Abs(Math.Abs(minAreaRect.Angle) - Math.Abs(oldAngle));

                var size = candidate.Size;
                if (diff > 45)
                {
                    candidate.SetCorrectSize(size.Height, size.Width);
                    shape = new RotatedRect(minAreaRect.Center, new SizeF(size.Height, size.Width), minAreaRect.Angle);
                }
                else
                {
                    candidate.SetCorrectSize(size.Width, size.Height);
                    shape = new RotatedRect(minAreaRect.Center, new SizeF(size.Width, size.Height), minAreaRect.Angle);
                }
            }
            else
            {
                shape = minAreaRect;
            }

            candidate.State = occluded ? TrackingState.Occluded : TrackingState.Tracked;
            candidate.LastUpdate = updateTime;
            candidate.Center = new WPoint(shape.Center.X, shape.Center.Y);
            candidate.Bounds = boundingRectangle;
            candidate.Shape = shape;
            candidate.Angle = (candidate.Angle + deltaAngle) % 360;
            candidate.LastAngle = minAreaRect.Angle;
            candidate.Polygon = polygon;
            candidate.Points = points;

            return true;
        }

        private static RectangularObject GetObjectCandidate(IEnumerable<RectangularObject> objects, Emgu.CV.Util.VectorOfPoint objectContour, RotatedRect shape, double maxRestoreDistance, out double retDistance)
        {
            RectangularObject candidate = null;
            var leastDistance = double.MaxValue;
            foreach (var obj in objects)
            {
                // check current contour to last center point distance (checking last contour with current center point does not work because of MemStorage
                // which will lead to an inconsistent last contour after last image has been processed completely and after storage is disposed.
                //UMat ret = new UMat();//TODO remove me
                //CvInvoke.cvCopy(objectContour.toma, ret, IntPtr.Zero);//TODO remove me

                var distanceToContour = CvInvoke.PointPolygonTest(objectContour,
                    obj.Shape.Center,
                    true);

                var oCenter = obj.Shape.Center;
                var distance = Math.Sqrt(Math.Pow(oCenter.X - shape.Center.X, 2) + Math.Pow(oCenter.Y - shape.Center.Y, 2));

                // distance < 0 means the point is outside of the contour.
                if (distanceToContour < 0 || leastDistance < distance) continue;

                //if (distanceToContour )

                candidate = obj;
                leastDistance = distance;
            }

            if (leastDistance > maxRestoreDistance || candidate == null)
            {
                retDistance = -1;
                return null;
            }

            retDistance = leastDistance;
            return candidate;
        }
    }
}
