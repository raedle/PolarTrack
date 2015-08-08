using System.Xml.Serialization;


using Huddle.Engine.Data;
using Huddle.Engine.Util;

using Emgu.CV;
using Emgu.CV.CvEnum;


namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Flip","Flip")]
    public class Flip : UMatProcessor
    {
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


        public Flip()
            : base(false)
        {

        }

        public override UMatData ProcessAndView(UMatData data)
        {
            // flip
            var flipCode = FlipType.None;

            if (FlipHorizontal)
                flipCode |= FlipType.Horizontal;
            if (FlipVertical)
                flipCode |= FlipType.Vertical;

            if (flipCode != FlipType.None)
            {
                CvInvoke.Flip(data.Data,
                    data.Data,
                    flipCode);
            }

            return data;
        }
    }
}
