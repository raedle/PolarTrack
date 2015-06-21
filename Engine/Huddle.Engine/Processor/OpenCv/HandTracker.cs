﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Data;
using Huddle.Engine.Extensions;
using Huddle.Engine.Util;
using WPoint = System.Windows.Point;
using DPoint = System.Drawing.Point;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Hand Tracker", "HandTracker")]
    public class HandTracker : BaseImageProcessor<Gray, float>
    {
        #region private fields

        private Image<Gray, float> _backgroundImage;

        private int _collectedBackgroundImages = 0;

        private readonly List<Hand> _hands = new List<Hand>();

        private static long _id = -1;

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

        private float _lowCutOffDepth = 5.0f;

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

        #region IsSmoothGaussianEnabled

        /// <summary>
        /// The <see cref="IsSmoothGaussianEnabled" /> property's name.
        /// </summary>
        public const string IsSmoothGaussianEnabledPropertyName = "IsSmoothGaussianEnabled";

        private bool _isSmoothGaussianEnabled = false;

        /// <summary>
        /// Sets and gets the IsSmoothGaussianEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSmoothGaussianEnabled
        {
            get
            {
                return _isSmoothGaussianEnabled;
            }

            set
            {
                if (_isSmoothGaussianEnabled == value)
                {
                    return;
                }

                RaisePropertyChanging(IsSmoothGaussianEnabledPropertyName);
                _isSmoothGaussianEnabled = value;
                RaisePropertyChanged(IsSmoothGaussianEnabledPropertyName);
            }
        }

        #endregion

        #region NumDilate

        /// <summary>
        /// The <see cref="NumDilate" /> property's name.
        /// </summary>
        public const string NumDilatePropertyName = "NumDilate";

        private int _numDilate = 2;

        /// <summary>
        /// Sets and gets the NumDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int NumDilate
        {
            get
            {
                return _numDilate;
            }

            set
            {
                if (_numDilate == value)
                {
                    return;
                }

                RaisePropertyChanging(NumDilatePropertyName);
                _numDilate = value;
                RaisePropertyChanged(NumDilatePropertyName);
            }
        }

        #endregion

        #region NumErode

        /// <summary>
        /// The <see cref="NumErode" /> property's name.
        /// </summary>
        public const string NumErodePropertyName = "NumErode";

        private int _numErode = 2;

        /// <summary>
        /// Sets and gets the NumErode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int NumErode
        {
            get
            {
                return _numErode;
            }

            set
            {
                if (_numErode == value)
                {
                    return;
                }

                RaisePropertyChanging(NumErodePropertyName);
                _numErode = value;
                RaisePropertyChanged(NumErodePropertyName);
            }
        }

        #endregion

        #region MinHandArmArea

        /// <summary>
        /// The <see cref="MinHandArmArea" /> property's name.
        /// </summary>
        public const string MinHandArmAreaPropertyName = "MinHandArmArea";

        private int _minHandArmArea = 1500;

        /// <summary>
        /// Sets and gets the MinHandArmArea property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MinHandArmArea
        {
            get
            {
                return _minHandArmArea;
            }

            set
            {
                if (_minHandArmArea == value)
                {
                    return;
                }

                RaisePropertyChanging(MinHandArmAreaPropertyName);
                _minHandArmArea = value;
                RaisePropertyChanged(MinHandArmAreaPropertyName);
            }
        }

        #endregion

        #region HandLocationSamples

        /// <summary>
        /// The <see cref="HandLocationSamples" /> property's name.
        /// </summary>
        public const string HandLocationSamplesPropertyName = "HandLocationSamples";

        private int _handLocationSamples = 30;

        /// <summary>
        /// Sets and gets the HandLocationSamples property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int HandLocationSamples
        {
            get
            {
                return _handLocationSamples;
            }

            set
            {
                if (_handLocationSamples == value)
                {
                    return;
                }

                RaisePropertyChanging(HandLocationSamplesPropertyName);
                _handLocationSamples = value;
                RaisePropertyChanged(HandLocationSamplesPropertyName);
            }
        }

        #endregion

        #region IsDrawHandLocationSamples

        /// <summary>
        /// The <see cref="IsDrawHandLocationSamples" /> property's name.
        /// </summary>
        public const string IsDrawHandLocationSamplesPropertyName = "IsDrawHandLocationSamples";

        private bool _isDrawHandLocationSamples = true;

        /// <summary>
        /// Sets and gets the IsDrawHandLocationSamples property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDrawHandLocationSamples
        {
            get
            {
                return _isDrawHandLocationSamples;
            }

            set
            {
                if (_isDrawHandLocationSamples == value)
                {
                    return;
                }

                RaisePropertyChanging(IsDrawHandLocationSamplesPropertyName);
                _isDrawHandLocationSamples = value;
                RaisePropertyChanged(IsDrawHandLocationSamplesPropertyName);
            }
        }

        #endregion

        #region IntegrationDistance

        /// <summary>
        /// The <see cref="IntegrationDistance" /> property's name.
        /// </summary>
        public const string IntegrationDistancePropertyName = "IntegrationDistance";

        private int _integrationDistance = 25;

        /// <summary>
        /// Sets and gets the IntegrationDistance property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int IntegrationDistance
        {
            get
            {
                return _integrationDistance;
            }

            set
            {
                if (_integrationDistance == value)
                {
                    return;
                }

                RaisePropertyChanging(IntegrationDistancePropertyName);
                _integrationDistance = value;
                RaisePropertyChanged(IntegrationDistancePropertyName);
            }
        }

        #endregion

        #region FloodFillMaskImageSource

        /// <summary>
        /// The <see cref="FloodFillMaskImageSource" /> property's name.
        /// </summary>
        public const string FloodFillMaskPropertyName = "FloodFillMaskImageSource";

        private BitmapSource _floodFillMaskImageSource = null;

        /// <summary>
        /// Sets and gets the FloodFillMaskImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource FloodFillMaskImageSource
        {
            get
            {
                return _floodFillMaskImageSource;
            }

            set
            {
                if (_floodFillMaskImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(FloodFillMaskPropertyName);
                _floodFillMaskImageSource = value;
                RaisePropertyChanged(FloodFillMaskPropertyName);
            }
        }

        #endregion

        #region FloodFillDifference

        /// <summary>
        /// The <see cref="FloodFillDifference" /> property's name.
        /// </summary>
        public const string FloodFillDifferencePropertyName = "FloodFillDifference";

        private float _floodFillDifference = 25.0f;

        /// <summary>
        /// Sets and gets the FloodFillDifference property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float FloodFillDifference
        {
            get
            {
                return _floodFillDifference;
            }

            set
            {
                if (_floodFillDifference == value)
                {
                    return;
                }

                RaisePropertyChanging(FloodFillDifferencePropertyName);
                _floodFillDifference = value;
                RaisePropertyChanged(FloodFillDifferencePropertyName);
            }
        }

        #endregion

        #region MaxFloodFillLoops

        /// <summary>
        /// The <see cref="MaxFloodFillLoops" /> property's name.
        /// </summary>
        public const string MaxFloodFillLoopsPropertyName = "MaxFloodFillLoops";

        private int _maxFloodFillLoops = 100;

        /// <summary>
        /// Sets and gets the MaxFloodFillLoops property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MaxFloodFillLoops
        {
            get
            {
                return _maxFloodFillLoops;
            }

            set
            {
                if (_maxFloodFillLoops == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxFloodFillLoopsPropertyName);
                _maxFloodFillLoops = value;
                RaisePropertyChanged(MaxFloodFillLoopsPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public HandTracker()
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

        public override Image<Gray, float> ProcessAndView(Image<Gray, float> image)
        {
            if (BuildingBackgroundImage(image)) return null;

            var now = DateTime.Now;

            _hands.RemoveAll(p => (now - p.LastUpdate).TotalMilliseconds > 500);

            var width = image.Width;
            var height = image.Height;

            var lowCutOffDepth = LowCutOffDepth;
            var highCutOffDepth = HighCutOffDepth;

            var maxFloodFillLoops = MaxFloodFillLoops;

            // This image is used to segment object from background
            var imageRemovedBackground = _backgroundImage.Sub(image.Copy());

            // This image is necessary for using FloodFill to avoid filling background
            // (segmented objects are shifted back to original depth location after background subtraction)
            var imageWithOriginalDepth = new Image<Gray, byte>(width, height);

            var imageData = image.Data;
            var imageRemovedBackgroundData = imageRemovedBackground.Data;
            var imageWithOriginalDepthData = imageWithOriginalDepth.Data;

            Parallel.For(0, height, y =>
            {
                byte originalDepthValue;
                for (var x = 0; x < width; x++)
                {
                    // DON'T REMOVE CAST (it is necessary!!! :) )
                    var depthValue = Math.Abs((byte)imageRemovedBackgroundData[y, x, 0]);

                    if (depthValue > lowCutOffDepth && depthValue < highCutOffDepth)
                        originalDepthValue = (byte)imageData[y, x, 0];
                    else
                        originalDepthValue = 0;

                    imageWithOriginalDepthData[y, x, 0] = originalDepthValue;
                }
            });

            var imageWithOriginalDepthCopy = imageWithOriginalDepth.Copy();

            // Remove noise (background noise)
            imageWithOriginalDepth = imageWithOriginalDepth
                .Erode(NumErode)
                .Dilate(NumDilate)
                .PyrUp()
                .PyrDown();

            var postProcessImage = imageWithOriginalDepth.Copy();

            double[] minValues;
            double[] maxValues;
            DPoint[] minLocations;
            DPoint[] maxLocations;
            imageWithOriginalDepth.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            var minHandArmArea = MinHandArmArea;
            var maxHandArmArea = width * height - 1000;
            var shrinkMaskROI = new Rectangle(1, 1, width, height);
            var color = 0.0f;

            var debugOutput = new Image<Rgb, byte>(width, height);

            MCvConnectedComp comp = new MCvConnectedComp();
            for (var i = 0; !minValues[0].Equals(maxValues[0]) && i < maxFloodFillLoops;
                imageWithOriginalDepth.MinMax(out minValues, out maxValues, out minLocations, out maxLocations), i++)
            {
                // Mask need to be two pixels bigger than the source image.
                var mask = new Image<Gray, byte>(width + 2, height + 2);

                // Flood fill segment with lowest pixel value to allow for next segment on next iteration.
                CvInvoke.FloodFill(imageWithOriginalDepth,
                    mask,
                    maxLocations[0],
                    new MCvScalar(0.0f),
                    out comp.Rect,
                    new MCvScalar(FloodFillDifference),
                    new MCvScalar(FloodFillDifference),
                    Emgu.CV.CvEnum.Connectivity.EightConnected);

                // Only process segments that are of a certain size (and are not the entire image).
                if (!( comp.Area > minHandArmArea) || !(comp.Area < maxHandArmArea)) continue;

                mask.ROI = shrinkMaskROI;

                var segment = mask.Mul(imageWithOriginalDepthCopy);// .Dilate(2);

                int x;
                int y;
                float depth;
                FindHandLocation(ref segment, ref imageRemovedBackground, ref debugOutput, ref mask, out x, out y, out depth);

                if (depth < 0) continue;

                var nx = x / (double)width;
                var ny = y / (double)height;
                UpdateHand(x, y, nx, ny, width, height, depth, mask);

                segment.Dispose();

                if (IsRenderContent)
                {
                    // This does not work therefore the following manual approach is used.
                    //targetImage = targetImage.And(coloredMask);

                    var coloredMask = mask.Mul(color += 25);

                    var colorMaskData = coloredMask.Data;
                    var targetImageData = postProcessImage.Data;

                    Parallel.For(0, height, y0 =>
                    {
                        for (var x0 = 0; x0 < width; x0++)
                        {
                            var maskValue = colorMaskData[y0, x0, 0];
                            if (maskValue > 0)
                                targetImageData[y0, x0, 0] = maskValue;
                        }
                    });

                    debugOutput += mask.Mul(255).Convert<Rgb, byte>();

                    coloredMask.Dispose();
                }
            }

            imageRemovedBackground.Dispose();

            if (IsRenderContent)
            {
                foreach (var hand in _hands)
                {
                    debugOutput.Draw(new CircleF(new PointF((float)hand.Center.X, (float)hand.Center.Y), 5), Rgbs.Red, 3);
                    debugOutput.Draw(new CircleF(new PointF((float)hand.SmoothedCenter.X, (float)hand.SmoothedCenter.Y), 5), Rgbs.Green, 3);
                    Emgu.CV.CvInvoke.PutText(debugOutput,
                        string.Format("Id {0} ({1:F1})", hand.Id, hand.Depth),
                        new DPoint((int)hand.SmoothedCenter.X, (int)hand.SmoothedCenter.Y),
                        EmguFont.Font,
                        EmguFont.Scale,
                        Rgbs.TrueRed.MCvScalar);

                }

                var debugOutputCopy = debugOutput.Copy();
                Task.Factory.StartNew(() =>
                {
                    var bitmap = debugOutputCopy.ToBitmapSource(true);
                    debugOutputCopy.Dispose();
                    return bitmap;
                }).ContinueWith(s => FloodFillMaskImageSource = s.Result);
            }

            foreach (var hand in _hands)
            {
                Stage(hand.Copy());
            }
            Push();

            return postProcessImage.Convert<Gray, float>();
        }

        private bool BuildingBackgroundImage(Image<Gray, float> image)
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = image.Copy();
                return true;
            }

            if (++_collectedBackgroundImages < BackgroundSubtractionSamples)
            {
                CvInvoke.AccumulateWeighted(image,
                    _backgroundImage,
                    0.8);
                return true;
            }

            return false;
        }

        #region Hand Processing

        /// <summary>
        /// Finds hand location in a segmented image. It assumes that the pixel with the highest depth value
        /// is the tip of the hand (highest value means closest to the static surface). It uses a sample of
        /// 30 highest depth values to compute the average x/y coordinate, which will be returned.
        /// </summary>
        /// <param name="handSegment"></param>
        /// <param name="imageRemoveBackground"></param>
        /// <param name="handMask"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="depth"></param>
        private void FindHandLocation(ref Image<Gray, byte> handSegment, ref Image<Gray, float> imageRemovedBackground, ref Image<Rgb, byte> debugOutput, ref Image<Gray, byte> handMask, out int x, out int y, out float depth)
        {
            var samples = HandLocationSamples;
            var lowCutOffDepth = LowCutOffDepth;
            var highCutOffDepth = HighCutOffDepth;

            var xs = new List<int>(samples);
            var ys = new List<int>(samples);
            var ds = new List<float>(samples);

            var minVal = double.MaxValue;
            var maxVal = 0.0;
            var minLoc = new DPoint();
            var maxLoc = new DPoint();
            var handSegmentData = handSegment.Data;
            var imageRemovedBackgroundData = imageRemovedBackground.Data;

            for (var j = 0; j < samples; j++)
            {
                CvInvoke.MinMaxLoc(handSegment,
                    ref minVal,
                    ref maxVal,
                    ref minLoc,
                    ref maxLoc,
                    handMask);

                var maxX = maxLoc.X;
                var maxY = maxLoc.Y;

                handSegmentData[maxY, maxX, 0] = 0;

                var depthValue = imageRemovedBackgroundData[maxY, maxX, 0];

                if (!(depthValue > lowCutOffDepth && depthValue < highCutOffDepth))
                    continue;

                xs.Add(maxX);
                ys.Add(maxY);
                ds.Add(depthValue);

                if (IsRenderContent && IsDrawHandLocationSamples)
                    debugOutput.Draw(new CircleF(new PointF(maxX, maxY), 2.0f), Rgbs.Cyan, 2);
            }

            if (xs.Count == 0)
            {
                x = -1;
                y = -1;
                depth = -1.0f;
                return;
            }

            x = (int)xs.Average();
            y = (int)ys.Average();
            depth = imageRemovedBackgroundData[y, x, 0]; // ds.Average();

            if (depth > highCutOffDepth)
                depth = ds.Median();
        }

        /// <summary>
        /// Updates a hand from previous frames or adds a new hand if not found in history.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ny"></param>
        /// <param name="nx"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        private void UpdateHand(int x, int y, double nx, double ny, double width, double height, float depth, Image<Gray, byte> segment)
        {
            var now = DateTime.Now;
            //depth = 255 - depth;

            Hand hand;
            var point = new WPoint(x, y);

            // TODO optimize code below
            if (_hands.Count > 0)
            {
                hand = _hands.Aggregate((curmin, p) => p.SmoothedCenter.Length(point) < curmin.SmoothedCenter.Length(point) ? p : curmin);
            }
            else
            {
                var id = NextId();
                hand = new Hand(this, string.Format("Hand{0}", id), id, point)
                {
                    Center = new WPoint(nx, ny),
                    Depth = depth,
                    LastUpdate = now,
                    Segment = segment.Copy()
                };
                hand.RelativeCenter = new WPoint(hand.SmoothedCenter.X / width, hand.SmoothedCenter.Y / height);

                _hands.Add(hand);
            }

            if (hand.SmoothedCenter.Length(point) < IntegrationDistance)
            {
                hand.Center = new WPoint(nx, ny);
                hand.Center = point;
                hand.Depth = depth;
                hand.LastUpdate = now;
                hand.RelativeCenter = new WPoint(hand.SmoothedCenter.X / width, hand.SmoothedCenter.Y / height);
                hand.Segment = segment.Copy();
            }
            else
            {
                var id = NextId();
                hand = new Hand(this, string.Format("Hand{0}", id), id, point)
                {
                    Center = new WPoint(nx, ny),
                    Depth = depth,
                    LastUpdate = now,
                    Segment = segment.Copy()
                };
                hand.RelativeCenter = new WPoint(hand.SmoothedCenter.X / width, hand.SmoothedCenter.Y / height);
                _hands.Add(hand);
            }
        }

        #endregion
    }

    internal class SamplingException : Exception
    {
        #region ctor

        internal SamplingException(string message)
            : base(message)
        {

        }

        #endregion
    }
}
