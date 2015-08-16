using Emgu.CV;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using Huddle.Engine.Data;
using Huddle.Engine.Processor.BarCodes.ZXingHelper;
using Huddle.Engine.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WPoint = System.Windows.Point;

namespace Huddle.Engine.Processor.BarCodes
{
    [ViewTemplate("QRCode Decoder", "QRCodeDecoder", "/Huddle.Engine;component/Resources/qrcode.png")]
    public class QRCodeDecoder : UMatProcessor
    {
        #region private fields

        private readonly IBarcodeReaderImage _barcodeReader;

        private readonly Rgb[] _colors = { Rgbs.Red, Rgbs.Green, Rgbs.Blue, Rgbs.Yellow };

        #endregion

        #region public properties

        #region TryHarder

        /// <summary>
        /// The <see cref="TryHarder" /> property's name.
        /// </summary>
        public const string TryHarderPropertyName = "TryHarder";

        private bool _tryHarder = false;

        /// <summary>
        /// Sets and gets the TryHarder property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool TryHarder
        {
            get
            {
                return _tryHarder;
            }

            set
            {
                if (_tryHarder == value)
                {
                    return;
                }

                RaisePropertyChanging(TryHarderPropertyName);
                _tryHarder = value;
                RaisePropertyChanged(TryHarderPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public QRCodeDecoder()
        {
            _barcodeReader = new BarcodeReaderImage
            {
                AutoRotate = false,
                Options = { PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE } }
            };

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case TryHarderPropertyName:
                        _barcodeReader.Options.TryHarder = TryHarder;
                        break;
                }
            };
        }

        #endregion

        public override UMatData ProcessAndView(UMatData data)
        {
            UMat outputImage = data.Data.Clone();

            // get (hopefully) all visible QR codes
            ZXing.Result[] results;
            try
            {
                results = _barcodeReader.DecodeMultiple(data.Data.ToImage<Rgb,byte>());
            }
            catch (Exception)
            {
                // sometimes there are some less important exceptions about the version of the QR codes
                return data;
            }

            var numQRs = 0;
            if (results != null)
            {
                numQRs = results.Length;
                LogFormat("Found {0} QR tags", numQRs);
            }
            else
            {
                LogFormat("Failed");
                //base.DrawDebug(image);
                return data;
            }

            // Process found QR codes

            if (!results.Any())
                return data;

            for (var i = 0; i < numQRs; i++)
            {
                // Get content of tag from results[i].Text
                var qrText = results[i].Text;

                // Get corner points of tag from results[i].ResultPoints
                var qrPoints = results[i].ResultPoints;

                var minX = qrPoints.Min(p => p.X);
                var minY = qrPoints.Min(p => p.Y);
                var maxX = qrPoints.Max(p => p.X);
                var maxY = qrPoints.Max(p => p.Y);

                var colorEnumerator = _colors.GetEnumerator();

                foreach (var point in qrPoints)
                {
                    if (!colorEnumerator.MoveNext())
                    {
                        colorEnumerator.Reset();
                        colorEnumerator.MoveNext();
                    }


                    CvInvoke.Circle(outputImage,
                        new Point((int)point.X, (int)point.Y),
                        5,
                        ((Rgb)colorEnumerator.Current).MCvScalar,
                        3);
                }

                if (qrPoints.Length >= 2)
                {
                    CvInvoke.Line(outputImage,
                        new Point((int)qrPoints[0].X, (int)qrPoints[0].Y),
                        new Point((int)qrPoints[1].X, (int)qrPoints[1].Y),
                        Rgbs.Red.MCvScalar,
                        5);
                }

                var dx = qrPoints[1].X - qrPoints[0].X;
                var dy = qrPoints[1].Y - qrPoints[0].Y;

                //// Get orientation of tag
                var qrOrientation = Math.Atan2(dy, dx) / Math.PI * 180 + 90;

                LogFormat("Text={0} | Orientation={1}°", qrText, qrOrientation);

                var centerX = (minX + (maxX - minX) / 2);
                var centerY = (minY + (maxY - minY) / 2);

                // center point
                CvInvoke.Circle(outputImage, new Point((int)centerX, (int)centerY), 5, Rgbs.TangerineTango.MCvScalar, 5);

                // Stage data for later push
                Stage(new Marker(this, string.Format("QrCode{0}", results[i].Text))
                {
                    Id = results[i].Text,
                    Center = new WPoint(centerX / data.Width, centerY / data.Height),
                    Angle = qrOrientation
                });
            }

            // Push staged data
            Push();

            //base.DrawDebug(image);
            data.Data = outputImage;
            return data;
        }
    }
}
