using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using AForge.Imaging.Filters;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Extensions;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Data;
using Huddle.Engine.Properties;
using Huddle.Engine.Util;
using ZXing;
using Point = System.Windows.Point;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Huddle.Engine.Processor.Sensors;

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Polar Tracker", "PolarTracker")]
    public class PolarTracker : UMatProcessor
    {
        #region private members

        private UMat _prevImage;
        private UMat _prevGpuImage;

        #endregion

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

        #region FPS

        /// <summary>
        /// The <see cref="FPS" /> property's name.
        /// </summary>
        public const string FPSPropertyName = "FPS";

        private double _FPS = 0.0;
        private int _imageCount = 0;

        /// <summary>
        /// Sets and gets the FPS property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public double FPS
        {
            get
            {
                return _FPS;
            }

            set
            {
                if (_FPS == value)
                {
                    return;
                }

                RaisePropertyChanging(FPSPropertyName);
                _FPS = value;
                RaisePropertyChanged(FPSPropertyName);
            }
        }

        #endregion

        #region IsUseDepthImages

        /// <summary>
        /// The <see cref="IsUseDepthImages" /> property's name.
        /// </summary>
        public const string IsUseDepthImagesPropertyName = "IsUseDepthImages";

        private bool _isUseDepthImages = false;

        /// <summary>
        /// Sets and gets the IsUseDepthImages property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUseDepthImages
        {
            get
            {
                return _isUseDepthImages;
            }

            set
            {
                if (_isUseDepthImages == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUseDepthImagesPropertyName);
                _isUseDepthImages = value;
                RaisePropertyChanged(IsUseDepthImagesPropertyName);
            }
        }

        #endregion

        #region Shutter

        /// <summary>
        /// The <see cref="Shutter" /> property's name.
        /// </summary>
        public const string ShutterPropertyName = "Shutter";

        private bool _Shutter = false;

        /// <summary>
        /// Sets and gets the Shutter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Shutter
        {
            get
            {
                return _Shutter;
            }

            set
            {
                if (_Shutter == value)
                {
                    return;
                }

                RaisePropertyChanging(ShutterPropertyName);
                _Shutter = value;
                RaisePropertyChanged(ShutterPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public PolarTracker()
            : base(false)
        {
            CudafyTranslator.Language = eLanguage.OpenCL;

            int i = 0;

            foreach (GPGPUProperties prop in CudafyHost.GetDeviceProperties(eGPUType.OpenCL, false))
            {
                Console.WriteLine("   --- General Information for device {0} ---", i);
                Console.WriteLine("Name:  {0}", prop.Name);
                Console.WriteLine("Platform Name:  {0}", prop.PlatformName);
                Console.WriteLine("Device Id:  {0}", prop.DeviceId);
                Console.WriteLine("Compute capability:  {0}.{1}", prop.Capability.Major, prop.Capability.Minor);
                Console.WriteLine("Clock rate: {0}", prop.ClockRate);
                Console.WriteLine("Simulated: {0}", prop.IsSimulated);
                Console.WriteLine();

                Console.WriteLine("   --- Memory Information for device {0} ---", i);
                Console.WriteLine("Total global mem:  {0}", prop.TotalMemory);
                Console.WriteLine("Total constant Mem:  {0}", prop.TotalConstantMemory);
                Console.WriteLine("Max mem pitch:  {0}", prop.MemoryPitch);
                Console.WriteLine("Texture Alignment:  {0}", prop.TextureAlignment);
                Console.WriteLine();

                Console.WriteLine("   --- MP Information for device {0} ---", i);
                Console.WriteLine("Shared mem per mp: {0}", prop.SharedMemoryPerBlock);
                Console.WriteLine("Registers per mp:  {0}", prop.RegistersPerBlock);
                Console.WriteLine("Threads in warp:  {0}", prop.WarpSize);
                Console.WriteLine("Max threads per block:  {0}", prop.MaxThreadsPerBlock);
                Console.WriteLine("Max thread dimensions:  ({0}, {1}, {2})", prop.MaxThreadsSize.x, prop.MaxThreadsSize.y, prop.MaxThreadsSize.z);
                Console.WriteLine("Max grid dimensions:  ({0}, {1}, {2})", prop.MaxGridSize.x, prop.MaxGridSize.y, prop.MaxGridSize.z);

                Console.WriteLine();

                i++;
            }

            //listen to properties
            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case IsUseDepthImagesPropertyName:
                        //Dispable Depth for now
                        Senz3DSoftKinetic.getInstance().IsUseDepthNode = IsUseDepthImages;
                        break;
                    case ShutterPropertyName:
                        imageTimer.Enabled = Shutter;
                        break;
                }
            };
        }

        #endregion

        private GPGPU _gpu;

        private System.Timers.Timer fpsTimer = new System.Timers.Timer();
        private System.Timers.Timer imageTimer = new System.Timers.Timer();

        public override void Start()
        {
            base.Start();

            var km = CudafyTranslator.Cudafy(typeof(PolarTracker));
            Console.WriteLine(km);
            Console.WriteLine("OpenCL count {0}", CudafyHost.GetDeviceCount(eGPUType.OpenCL));

            _gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            _gpu.LoadModule(km);

            fpsTimer.Elapsed += new System.Timers.ElapsedEventHandler(updatFPS);
            fpsTimer.Interval = 1000;
            fpsTimer.Enabled = true;

            imageTimer.Elapsed += new System.Timers.ElapsedEventHandler(getImage);
            imageTimer.Interval = 250;

            if(Shutter)
                imageTimer.Enabled = true;
        }

        public override void Stop()
        {
            fpsTimer.Enabled = false;
            imageTimer.Enabled = false;
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            if (Shutter)
            {
                Senz3DSoftKinetic.getInstance().IsUseColorNode = false;
                s.Stop();
            }

            _imageCount++;
            var imageCopy = data.Data.Clone();
            if (_prevImage == null)
            {
                _prevImage = imageCopy;
            }

            UMat ret = new UMat(data.Height, data.Width, DepthType.Cv8U, 3);

            CvInvoke.AbsDiff(_prevImage, data.Data, ret);
            CvInvoke.CvtColor(ret, ret, ColorConversion.Rgb2Gray);
            CvInvoke.Threshold(ret, ret, Threshold, 255, ThresholdType.Binary);

            // save image for nxt turn
            _prevImage = imageCopy;

            data.Data = ret;
            return data;


        }

        private void updatFPS(object sender, System.Timers.ElapsedEventArgs e)
        {
            FPS = _imageCount;
            _imageCount = 0;
        }

        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        private void getImage(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Shutter)
            {
                Senz3DSoftKinetic.getInstance().IsUseColorNode = true;
                s.Reset();
                s.Start();
                Senz3DSoftKinetic.getInstance().IsUseDepthNode = IsUseDepthImages;
            }
        }

        [Cudafy]
        public static void RunStuff(GThread gthread, int width, byte threshold, byte[, ,] img1, byte[, ,] img2, byte[, ,] result)
        {
            var x = gthread.blockIdx.x * gthread.blockDim.x + gthread.threadIdx.x;

            for (var y = width - 1; y >= 0; y--)
            {
                var b = Math.Abs(img1[x, y, 0] - img2[x, y, 0]);
                var g = Math.Abs(img1[x, y, 1] - img2[x, y, 1]);
                var r = Math.Abs(img1[x, y, 2] - img2[x, y, 2]);

                if (b > threshold || g > threshold || r > threshold)
                {
                    result[x, y, 0] = 255;
                }
            }


            //if (r >= dimension || g >= dimension || b >= dimension)
            //{
            //    return;
            //}

            //for (var k = 0; k < dimension; k++)
            //{
            //    var diffR = Math.Abs(img1[k, g, b] - img2[k, g, b]);
            //    var diffG = Math.Abs(img1[r, k, b] - img2[r, k, b]);
            //    var diffB = Math.Abs(img1[r, g, k] - img2[r, g, k]);

            //    if (diffR > threshold || diffG > threshold || diffB > threshold)
            //    {
            //        result[k, g, b] = 255;
            //    }
            //    else
            //    {
            //        result[k, g, b] = 0;
            //    }
            //}
        }
    }
}
