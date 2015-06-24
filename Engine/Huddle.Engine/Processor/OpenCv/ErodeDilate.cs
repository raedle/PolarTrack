using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using Huddle.Engine.Util;
using Huddle.Engine.Properties;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Erode Dilate", "ErodeDilate")]
    public class ErodeDilate : RgbProcessor
    {
        #region properties

        #region IsFirstErodeThenDilate

        /// <summary>
        /// The <see cref="IsFirstErodeThenDilate" /> property's name.
        /// </summary>
        public const string IsFirstErodeThenDilatePropertyName = "IsFirstErodeThenDilate";

        private bool _isFirstErodeThenDilate = true;

        /// <summary>
        /// Sets and gets the IsFirstErodeThenDilate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFirstErodeThenDilate
        {
            get
            {
                return _isFirstErodeThenDilate;
            }

            set
            {
                if (_isFirstErodeThenDilate == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFirstErodeThenDilatePropertyName);
                _isFirstErodeThenDilate = value;
                RaisePropertyChanged(IsFirstErodeThenDilatePropertyName);
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

        #endregion

        public override Image<Rgb, byte> ProcessAndView(Image<Rgb, byte> image)
        {
            UMat img = image.ToUMat();
            UMat ret = new UMat();

            if (IsFirstErodeThenDilate)
            {
                CvInvoke.Erode(img,
                    ret,
                    new Mat(),
                    new System.Drawing.Point(-1, -1),
                    NumErode,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                CvInvoke.Dilate(ret,
                    ret,
                    new Mat(),
                    new System.Drawing.Point(-1, -1),
                    NumDilate,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
            }
            else
            {
                CvInvoke.Dilate(img,
                    ret,
                    new Mat(),
                    new System.Drawing.Point(-1, -1),
                    NumDilate,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                CvInvoke.Erode(ret,
                    ret,
                    new Mat(),
                    new System.Drawing.Point(-1, -1),
                    NumErode,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
            }

            return ret.ToImage<Rgb, byte>();
        }
    }
}
