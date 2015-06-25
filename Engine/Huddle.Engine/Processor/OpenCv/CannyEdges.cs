using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Huddle.Engine.Util;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Canny Edges", "CannyEdges")]
    public class CannyEdges : RgbProcessor
    {
        #region properties

        #region Threshold

        /// <summary>
        /// The <see cref="Threshold" /> property's name.
        /// </summary>
        public const string ThreholdPropertyName = "Threshold";

        private double _threshold = 180.0;

        /// <summary>
        /// Sets and gets the Threshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Threshold
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

                RaisePropertyChanging(ThreholdPropertyName);
                _threshold = value;
                RaisePropertyChanged(ThreholdPropertyName);
            }
        }

        #endregion

        #region ThresholdLinking

        /// <summary>
        /// The <see cref="ThresholdLinking" /> property's name.
        /// </summary>
        public const string ThresholdLinkingPropertyName = "ThresholdLinking";

        private double _thresholdLinking = 120.0;

        /// <summary>
        /// Sets and gets the ThresholdLinking property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ThresholdLinking
        {
            get
            {
                return _thresholdLinking;
            }

            set
            {
                if (_thresholdLinking == value)
                {
                    return;
                }

                RaisePropertyChanging(ThresholdLinkingPropertyName);
                _thresholdLinking = value;
                RaisePropertyChanged(ThresholdLinkingPropertyName);
            }
        }

        #endregion

        #region GaussianPyramidDownUpDecomposition

        /// <summary>
        /// The <see cref="GaussianPyramidDownUpDecomposition" /> property's name.
        /// </summary>
        public const string GaussianPyramidDownUpDecompositionPropertyName = "GaussianPyramidDownUpDecomposition";

        private bool _gaussianPyramidDownUpDecomposition = true;

        /// <summary>
        /// Sets and gets the GaussianPyramidDownUpDecomposition property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool GaussianPyramidDownUpDecomposition
        {
            get
            {
                return _gaussianPyramidDownUpDecomposition;
            }

            set
            {
                if (_gaussianPyramidDownUpDecomposition == value)
                {
                    return;
                }

                RaisePropertyChanging(GaussianPyramidDownUpDecompositionPropertyName);
                _gaussianPyramidDownUpDecomposition = value;
                RaisePropertyChanged(GaussianPyramidDownUpDecompositionPropertyName);
            }
        }

        #endregion

        #endregion

        public override Image<Rgb, byte> ProcessAndView(Image<Rgb, byte> image)
        {
            //Convert the image to grayscale and filter out the noise
            var grayImage = image.Convert<Gray, byte>(); //TODO does not work with UMat atm
            UMat u_grayImage = grayImage.ToUMat();
            
            if (GaussianPyramidDownUpDecomposition)
            {
                CvInvoke.PyrDown(u_grayImage,
                    u_grayImage);
                CvInvoke.PyrUp(u_grayImage,
                    u_grayImage);
            }

            UMat ret = new UMat();
            CvInvoke.Canny(u_grayImage,
                ret,
                Threshold,
                ThresholdLinking);

            // Dispose old images
            image.Dispose();
            grayImage.Dispose();

            return ret.ToImage<Gray, byte>().Convert<Rgb, byte>(); //TODO does not work with UMat atm
        }
    }
}
