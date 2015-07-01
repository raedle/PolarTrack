using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Runtime.Serialization;

using Huddle.Engine.Data;

using Emgu.CV;
using Emgu.CV.External.Extensions;
using Emgu.CV.Structure;

namespace Huddle.Engine.Processor
{
    public abstract class UMatProcessor : BaseProcessor
    {
        #region static fields

        private static int VideoWriterId = 0;
        public const int VideoWriterWidth = 1280;
        public const int VideoWriterHeight = 720;
        public const int VideoWriterFps = 25;

        #endregion

        #region private members

        private readonly bool _enableVideoWriter = false;
        private readonly VideoWriter _videoWriter;

        #endregion


        #region PreProcessImage

        /// <summary>
        /// The <see cref="PreProcessImage" /> property's name.
        /// </summary>
        public const string PreProcessImagePropertyName = "PreProcessImage";

        private BitmapSource _preProcessImage;

        /// <summary>
        /// Sets and gets the PreProcessImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource PreProcessImage
        {
            get
            {
                return _preProcessImage;
            }

            set
            {
                if (_preProcessImage == value)
                {
                    return;
                }

                RaisePropertyChanging(PreProcessImagePropertyName);
                _preProcessImage = value;
                RaisePropertyChanged(PreProcessImagePropertyName);
            }
        }

        #endregion

        #region PostProcessImage

        /// <summary>
        /// The <see cref="PostProcessImage" /> property's name.
        /// </summary>
        public const string PostProcessImagePropertyName = "PostProcessImage";

        private BitmapSource _postProcessImage;

        /// <summary>
        /// Sets and gets the PostProcessImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource PostProcessImage
        {
            get
            {
                return _postProcessImage;
            }

            set
            {
                if (_postProcessImage == value)
                {
                    return;
                }

                RaisePropertyChanging(PostProcessImagePropertyName);
                _postProcessImage = value;
                RaisePropertyChanged(PostProcessImagePropertyName);
            }
        }

        #endregion

        protected UMatProcessor()
            : this(false)
        {

        }

        protected UMatProcessor(bool enableVideoWriter)
        {
            _enableVideoWriter = enableVideoWriter;

            if (_enableVideoWriter)
                _videoWriter = new VideoWriter(string.Format("{0}_{1}.avi", GetType().Name, VideoWriterId++),
                    Emgu.CV.VideoWriter.Fourcc('D', 'I', 'V', 'X'),
                    VideoWriterFps,
                    new System.Drawing.Size(VideoWriterWidth, VideoWriterHeight),
                    true);
        }

        public virtual UMatData PreProcess(UMatData data)
        {
            return data;
        }

        public override IData Process(IData data)
        {
            var umatData = data as UMatData;

            return umatData != null ? Process(umatData) : data;
        }
        private IData Process(UMatData data)
        {
            if (IsRenderContent)
            {
                // draw debug information on image -> TODO might worth be worth it to bind that information to the data template directly
                var preProcessImage = data.Data.Clone().ToImage();

                Task.Factory.StartNew(() =>
                {
                    if (preProcessImage == null) return null;

                    BitmapSource bitmap;
                    if (preProcessImage is Image<Gray, float>)
                        bitmap = (preProcessImage as Image<Gray, float>).ToGradientBitmapSource(true, EmguExtensions.LowConfidence, EmguExtensions.Saturation);
                    else
                        bitmap = preProcessImage.ToBitmapSource(true);

                    preProcessImage.Dispose();

                    return bitmap;
                }).ContinueWith(s => PreProcessImage = s.Result);
            }

            try
            {
                try
                {
                    var processedImage = ProcessAndView(data);

                    if (processedImage == null) return null;

                    data = processedImage;
                }
                catch (Exception e)
                {
                    LogFormat("Exception occured in ProcessAndView:{0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);
                }
            }
            catch (Exception)
            {
                //DispatcherHelper.RunAsync(() =>
                //{
                //    //if (!Messages.Any(m => Equals(e.Message, m)))
                //    //    Log(e.Message);
                //});
                return data;
            }

            if (_enableVideoWriter && _videoWriter != null)
            {
                WriteImage(data.Data.Clone());
            }

            if (IsRenderContent)
            {
                var postProcessImage = data.Data.Clone();

                Task.Factory.StartNew(() =>
                {
                    if (postProcessImage == null) return null;

                    BitmapSource bitmap;
                    if (postProcessImage is Image<Gray, float>)
                        bitmap = (postProcessImage.ToImage() as Image<Gray, float>).ToGradientBitmapSource(true, EmguExtensions.LowConfidence, EmguExtensions.Saturation);
                    else
                        bitmap = postProcessImage.ToBitmapSource(true);

                    postProcessImage.Dispose();

                    return bitmap;
                }).ContinueWith(s => PostProcessImage = s.Result);
            }
            
            return data;
        }

        public abstract UMatData ProcessAndView(UMatData data);

        private void WriteImage(UMat data)
        {
            //var bgrImage = image.Convert<Bgr, byte>();
            //image.Dispose();

            UMat ret = new UMat();
            CvInvoke.Resize(data,
                ret,
                new System.Drawing.Size(VideoWriterWidth,VideoWriterHeight),
                0,
                0,
                Emgu.CV.CvEnum.Inter.Cubic);


            _videoWriter.Write(ret.ToMat(Emgu.CV.CvEnum.AccessType.Read));
            //resizedImage.Dispose();
        }
    }
}
