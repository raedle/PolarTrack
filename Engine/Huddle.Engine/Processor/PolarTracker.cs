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

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Polar Tracker", "PolarTracker")]
    public class PolarTracker : RgbProcessor
    {
        #region private members

        private Image<Rgb, byte> _prevImage;
        private Image<Rgb, byte> _prevGpuImage;

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

        #region IsUseGpu

        /// <summary>
        /// The <see cref="IsUseGpu" /> property's name.
        /// </summary>
        public const string IsUseGpuPropertyName = "IsUseGpu";

        private bool _isUseGpu = false;

        /// <summary>
        /// Sets and gets the IsUseGpu property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUseGpu
        {
            get
            {
                return _isUseGpu;
            }

            set
            {
                if (_isUseGpu == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUseGpuPropertyName);
                _isUseGpu = value;
                RaisePropertyChanged(IsUseGpuPropertyName);
            }
        }

        #endregion

        #region IsUseOpenCL

        /// <summary>
        /// The <see cref="IsUseOpenCL" /> property's name.
        /// </summary>
        public const string IsUseOpenCLPropertyName = "IsUseOpenCL";

        private bool _isUseOpenCL = false;

        /// <summary>
        /// Sets and gets the IsUseOpenCL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUseOpenCL
        {
            get
            {
                return _isUseOpenCL;
            }

            set
            {
                if (_isUseOpenCL == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUseOpenCLPropertyName);
                _isUseOpenCL = value;
                RaisePropertyChanged(IsUseOpenCLPropertyName);
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
        }

        #endregion

        private GPGPU _gpu;

        public override void Start()
        {
            base.Start();

            var km = CudafyTranslator.Cudafy(typeof(PolarTracker));
            Console.WriteLine(km);
            Console.WriteLine("OpenCL count {0}", CudafyHost.GetDeviceCount(eGPUType.OpenCL));

            _gpu = CudafyHost.GetDevice(eGPUType.OpenCL, CudafyModes.DeviceId);
            _gpu.LoadModule(km);
        }

        public override Image<Rgb, byte> ProcessAndView(Image<Rgb, byte> image)
        {
            var imageCopy = image.Copy();

            if (IsUseOpenCL)
            {
                if (_prevImage == null || _gpu == null)
                {
                    _prevImage = imageCopy;
                    return image;
                }

                const int gridSize = 320;
                //var dim3 = new dim3(gridSize, gridSize, 1);//, Math.Min(gridSize, image.Width), 3);
                //var dim3 = new dim3(Math.Min(gridSize, image.Height), Math.Min(gridSize, image.Width), 3);
                //var dim3 = new dim3(image.Height, image.Width, 3);
                var dim3 = new dim3(image.Height);

                var img1 = _gpu.CopyToDevice(_prevImage.Data);
                var img2 = _gpu.CopyToDevice(image.Data);

                var resultImage = new Image<Rgb, byte>(image.Width, image.Height);
                var resultGpu = _gpu.Allocate(resultImage.Data);

                _gpu.Launch(dim3, 1).RunStuff(image.Width, (byte)Threshold, img1, img2, resultGpu);
                _gpu.CopyFromDevice(resultGpu, resultImage.Data);
                _gpu.Free(img1);
                _gpu.Free(img2);
                _gpu.Free(resultGpu);

                _prevImage = imageCopy;

                return resultImage;
            }

            var isUseGpu = IsUseGpu;

            Image<Rgb, byte> gpuImage = null;
            if (IsCudaSupported && isUseGpu)
            {
                gpuImage = imageCopy.Copy();
            }
                
            if (_prevImage == null)
            {
                _prevImage = imageCopy;

                if (IsCudaSupported && isUseGpu)
                    _prevGpuImage = gpuImage;

                return image;
            }

            //var imageCopy = image.Copy();
            //var absDiff = _prevImage.AbsDiff(imageCopy);
            //_prevImage = imageCopy;

            //return absDiff;

            Image<Rgb, byte> output;
            if (IsCudaSupported && isUseGpu)
            {
                var gpuDiffImage = gpuImage.Copy();
                CvInvoke.AbsDiff(gpuImage, _prevGpuImage, gpuDiffImage);
                var grayScaleGpuImage = gpuDiffImage.Convert<Gray, byte>();
                CvInvoke.Threshold(grayScaleGpuImage, grayScaleGpuImage, Threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                output = grayScaleGpuImage.Convert<Rgb, byte>();
            }
            else
            {
                output = new Image<Rgb, byte>(image.Width, image.Height);

                var data1 = _prevImage.Data;
                var data2 = image.Data;
                var outputData = output.Data;

                var threshold = Threshold;

                var width = _prevImage.Width;
                var height = _prevImage.Height;
                Parallel.For(0, width, j =>
                {
                    for (var i = 0; i < height; i++)
                    {
                        var b = Math.Abs(data1[i, j, 0] - data2[i, j, 0]);
                        var g = Math.Abs(data1[i, j, 1] - data2[i, j, 1]);
                        var r = Math.Abs(data1[i, j, 2] - data2[i, j, 2]);

                        if (b > threshold || g > threshold || r > threshold)
                        {
                            outputData[i, j, 0] = 255;
                        }
                    }
                });
            }

            //_prevImage.Dispose();
            _prevImage = imageCopy;

            if (IsCudaSupported && isUseGpu)
            {
                _prevGpuImage.Dispose();
                _prevGpuImage = gpuImage;
            }

            return output;
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
