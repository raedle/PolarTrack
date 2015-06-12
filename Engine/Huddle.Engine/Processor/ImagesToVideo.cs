using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.External.Structure;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using Huddle.Engine.Data;
using Huddle.Engine.Util;
using Point = System.Windows.Point;

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Images to Video", "ImagesToVideo")]
    public class ImagesToVideo : BaseProcessor
    {
        #region private members

        private bool _isRunning = false;

        #endregion

        #region public properties

        #region Fps

        /// <summary>
        /// The <see cref="Fps" /> property's name.
        /// </summary>
        public const string FpsPropertyName = "Fps";

        private int _fps = 1;

        /// <summary>
        /// Sets and gets the Fps property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Fps
        {
            get
            {
                return _fps;
            }

            set
            {
                if (_fps == value)
                {
                    return;
                }

                RaisePropertyChanging(FpsPropertyName);
                _fps = value;
                RaisePropertyChanged(FpsPropertyName);
            }
        }

        #endregion

        #region ImagesPath

        /// <summary>
        /// The <see cref="ImagesPath" /> property's name.
        /// </summary>
        public const string ImagesPathPropertyName = "ImagesPath";

        private string _imagesPath = @"C:\Test\Polarization";

        /// <summary>
        /// Sets and gets the ROITemp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ImagesPath
        {
            get
            {
                return _imagesPath;
            }

            set
            {
                if (_imagesPath == value)
                {
                    return;
                }

                RaisePropertyChanging(ImagesPathPropertyName);
                _imagesPath = value;
                RaisePropertyChanged(ImagesPathPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public ImagesToVideo()
        {
        }

        #endregion

        public override void Start()
        {
            base.Start();

            _isRunning = true;

            var files = Directory.GetFiles(ImagesPath, "*.png", SearchOption.TopDirectoryOnly);
            var index = 0;

            if (files.Length == 0)
                return;

            new Thread(() =>
            {
                while (_isRunning)
                {
                    var file = files[index];

                    // reset index
                    if (++index >= files.Length)
                        index = 0;

                    var image = new Image<Rgb, byte>(file);

                    Stage(new RgbImageData(this, "Image", image));
                    Push();

                    Thread.Sleep(1000 / Fps);
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        public override void Stop()
        {
            _isRunning = false;

            base.Stop();
        }

        public override IData Process(IData data)
        {
            throw new NotImplementedException();
        }
    }
}
