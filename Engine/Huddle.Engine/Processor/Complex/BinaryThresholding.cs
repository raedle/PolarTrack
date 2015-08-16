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
            // image to gray
            UMat grayImage = new UMat();

            if (data.Data.NumberOfChannels == 3 && data.Data.Depth == Emgu.CV.CvEnum.DepthType.Cv8U)
            {
                CvInvoke.CvtColor(data.Data, grayImage, ColorConversion.Rgb2Gray);
            }
            else if (data.Data.NumberOfChannels == 1 && data.Data.Depth == Emgu.CV.CvEnum.DepthType.Cv32F)
            {
                data.Data.ConvertTo(grayImage, DepthType.Cv8U);
            }
            else
            {
                //throw new Exception("Unexpected input type");
                return data;
            }


            if (IsBinaryThresholdInv)
            {
                CvInvoke.Threshold(grayImage,
                    data.Data, 
                    BinaryThreshold, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    BinaryThresholdMax, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            } else {
                CvInvoke.Threshold(grayImage,
                    data.Data,
                    BinaryThreshold, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    BinaryThresholdMax, // TODO stimmt der wert? oder ist new Gray(BinaryThreshold) was anderes?
                    Emgu.CV.CvEnum.ThresholdType.Binary);
            }

            return data;
        }
    }
}
