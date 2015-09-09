using Emgu.CV;
using Emgu.CV.Structure;
using Huddle.Engine.Data;
using Huddle.Engine.Extensions;
using Huddle.Engine.Util;
using System;
using System.IO;
using System.Threading;

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Images to Video", "ImagesToVideo")]
    public class ImagesToVideo : BaseProcessor
    {
        #region private members

        private bool _isRunning = false;

        private int _imgNumber = 1;

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

        #region VideoToImages

        /// <summary>
        /// The <see cref="VideoToImages" /> property's name.
        /// </summary>
        public const string VideoToImagesPropertyName = "VideoToImages";

        private bool _videosToImages = false;

        /// <summary>
        /// Sets and gets the VideoToImages property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool VideoToImages
        {
            get
            {
                return _videosToImages;
            }

            set
            {
                if (_videosToImages == value)
                {
                    return;
                }

                RaisePropertyChanging(VideoToImagesPropertyName);
                _videosToImages = value;
                RaisePropertyChanged(VideoToImagesPropertyName);
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

                    Stage(new UMatData(this, "color", image.ToUMat()));
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
            if (VideoToImages)
            {
                String type = "";
                IImage img;

                switch (data.Key)
                {
                    case "color":
                        type = "color";
                        img = (data as UMatData).Data.Clone().ToImage<Rgb,byte>();
                        break;
                    case "depth":
                        img = ((data as UMatData).Data.Clone()).ToImage<Gray,byte>();
                        type = "depth";
                        break;
                    case "confidence":
                        img = (data as UMatData).Data.Clone().ToImage<Gray,float>();
                        type = "confidence";
                        break;
                    default:
                        return null; // return if data is no image 
                }

                using (var m = new MemoryStream())
                {
                    String path = Path.Combine(Path.Combine(ImagesPath, type), _imgNumber++ + ".png"); 
                    img.Save(path);
                }

            }
            return null;
        }
    }
}
