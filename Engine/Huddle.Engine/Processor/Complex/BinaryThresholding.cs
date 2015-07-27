using Emgu.CV;
using Emgu.CV.Structure;
using Huddle.Engine.Util;
using Huddle.Engine.Data;

using Emgu.CV.External.Extensions;
using Emgu.CV.CvEnum;

namespace Huddle.Engine.Processor.Complex
{
    [ViewTemplate("Binary Thresholding", "BinaryThresholding")]
    public class BinaryThresholding : UMatProcessor
    {
        #region properties

        #region BinaryThreshold

        /// <summary>
        /// The <see cref="BinaryThreshold" /> property's name.
        /// </summary>
        public const string BinaryThresholdPropertyName = "BinaryThreshold";

        private byte _binaryThreshold = 127;

        /// <summary>
        /// Sets and gets the BinaryThreshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public byte BinaryThreshold
        {
            get
            {
                return _binaryThreshold;
            }

            set
            {
                if (_binaryThreshold == value)
                {
                    return;
                }

                RaisePropertyChanging(BinaryThresholdPropertyName);
                _binaryThreshold = value;
                RaisePropertyChanged(BinaryThresholdPropertyName);
            }
        }

        #endregion

        #region BinaryThresholdMaxValue

        /// <summary>
        /// The <see cref="BinaryThresholdMax" /> property's name.
        /// </summary>
        public const string BinaryThresholdMaxPropertyName = "BinaryThresholdMax";

        private byte _binaryThresholdMax = 255;

        /// <summary>
        /// Sets and gets the BinaryThresholdMax property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public byte BinaryThresholdMax
        {
            get
            {
                return _binaryThresholdMax;
            }

            set
            {
                if (_binaryThresholdMax == value)
                {
                    return;
                }

                RaisePropertyChanging(BinaryThresholdMaxPropertyName);
                _binaryThresholdMax = value;
                RaisePropertyChanged(BinaryThresholdMaxPropertyName);
            }
        }

        #endregion

        #region IsBinaryThresholdInv

        /// <summary>
        /// The <see cref="IsBinaryThresholdInv" /> property's name.
        /// </summary>
        public const string IsBinaryThresholdInvPropertyName = "IsBinaryThresholdInv";

        private bool _isBinaryThresholdInv = true;

        /// <summary>
        /// Sets and gets the IsBinaryThresholdInv property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsBinaryThresholdInv
        {
            get
            {
                return _isBinaryThresholdInv;
            }

            set
            {
                if (_isBinaryThresholdInv == value)
                {
                    return;
                }

                RaisePropertyChanging(IsBinaryThresholdInvPropertyName);
                _isBinaryThresholdInv = value;
                RaisePropertyChanged(IsBinaryThresholdInvPropertyName);
            }
        }

        #endregion

        #endregion

        public override UMatData ProcessAndView(UMatData data)
        {
            var binaryThreshold = new Gray(BinaryThreshold);
            var binaryThresholdMax = new Gray(BinaryThresholdMax);
            var isBinaryThresholdInv = IsBinaryThresholdInv;

            //var imageCopy = data.Data.Clone();

            //var grayImage = imageCopy.ToImage<Rgb,byte>().Convert<Gray, byte>().ToUMat();
            UMat grayImage = new UMat();
            grayImage = data.Data.ToImage<Rgb, byte>().Convert<Gray, float>().ToUMat();
            //CvInvoke.CvtColor(grayImage,
            //    grayImage,
            //    ColorConversion.Rgb2Gray,
            //    1);
            //grayImage = data.Data.Clone();//.ToImage<Rgb, byte>().Convert<Gray, byte>().ToUMat(); // TODO remove me when cvtcolor works

            UMat ret = new UMat();

            if(isBinaryThresholdInv)
            {
                CvInvoke.Threshold(grayImage,
                    ret, 
                    BinaryThreshold, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    BinaryThresholdMax, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            } else {
                CvInvoke.Threshold(grayImage,
                    ret,
                    BinaryThreshold, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    BinaryThresholdMax, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    Emgu.CV.CvEnum.ThresholdType.Binary);
            }

            //TODO convert back??? thresholdImage.Convert<Rgb, byte>()
            data.Data = ret;

            return data;
        }
    }
}
