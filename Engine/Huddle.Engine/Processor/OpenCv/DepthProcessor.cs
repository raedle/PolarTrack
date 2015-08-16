﻿using Emgu.CV;
using Emgu.CV.Structure;
using Huddle.Engine.Data;
using Huddle.Engine.Util;
using System.Drawing;
using System.Xml.Serialization;

namespace Huddle.Engine.Processor.OpenCv
{
    [ViewTemplate("Depth Processor", "DepthProcessor")]
    public class DepthProcessor : UMatProcessor
    {
        #region properties

        #region MinReproducedDepth

        /// <summary>
        /// The <see cref="MinReproducedDepth" /> property's name.
        /// </summary>
        public const string MinReproducedDepthPropertyName = "MinReproducedDepth";

        private double _minReproducedDepth = 457;

        /// <summary>
        /// Sets and gets the MinReproducedDepth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MinReproducedDepth
        {
            get
            {
                return _minReproducedDepth;
            }

            set
            {
                if (_minReproducedDepth == value)
                {
                    return;
                }

                RaisePropertyChanging(MinReproducedDepthPropertyName);
                _minReproducedDepth = value;
                RaisePropertyChanged(MinReproducedDepthPropertyName);
            }
        }

        #endregion

        #region MaxReproducedDepth

        /// <summary>
        /// The <see cref="MaxReproducedDepth" /> property's name.
        /// </summary>
        public const string MaxReproducedDepthPropertyName = "MaxReproducedDepth";

        private double _maxReproducedDepth = 617;

        /// <summary>
        /// Sets and gets the MaxReproducedDepth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MaxReproducedDepth
        {
            get
            {
                return _maxReproducedDepth;
            }

            set
            {
                if (_maxReproducedDepth == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxReproducedDepthPropertyName);
                _maxReproducedDepth = value;
                RaisePropertyChanged(MaxReproducedDepthPropertyName);
            }
        }

        #endregion

        #endregion

        public DepthProcessor()
        {
            MinReproducedDepth = 0;
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            var outputImage = new Image<Hsv, double>(data.Width, data.Height);

            // draw gradient legend
            for (var cx = 0; cx < outputImage.Width; cx++)
            {
                var c = new Hsv(120.0 - (cx / (double)outputImage.Width * 120.0), 255.0, 255.0);
                outputImage[0, cx] = c;
                outputImage[1, cx] = c;
                outputImage[2, cx] = c;
            }

            // draw depth values as HSV
            for (var y = 3; y < outputImage.Height; y++)
            {
                for (var x = 0; x < outputImage.Width; x++)
                {
                    // TODO fix me!!
                    var cin = 1000; //image[y, x].Intensity;
            
                    if (cin >= MinReproducedDepth && cin <= MaxReproducedDepth)
                    {
                        var h = 120.0 - ((cin - MinReproducedDepth) / (MaxReproducedDepth - MinReproducedDepth) * 120.0);
                        outputImage[y, x] = new Hsv(h, 255.0, 255.0);
                    }
                    else
                        outputImage[y, x] = new Hsv(0.0, 0.0, 0.0);
                }

            }
            
            var message = outputImage.Width + " x " + outputImage.Height + " [rd: " + MinReproducedDepth + " ," + MaxReproducedDepth + "]";
            Emgu.CV.CvInvoke.PutText(outputImage,
                message,
                new Point(5, 15),
                EmguFont.Font,
                EmguFont.Scale,
                new MCvScalar(0, 0, 0));

            CvInvoke.CvtColor(outputImage,data.Data,Emgu.CV.CvEnum.ColorConversion.Hsv2Rgb);

            return data;
        }
    }
}
