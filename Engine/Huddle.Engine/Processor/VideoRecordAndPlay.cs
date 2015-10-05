using Emgu.CV;
using Emgu.CV.External.Extensions;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Huddle.Engine.Data;
using Huddle.Engine.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Video Record / Play", "VideoRecordAndPlay", "/Huddle.Engine;component/Resources/film2.png")]
    public class VideoRecordAndPlay : UMatProcessor
    {
        #region private fields

        private /*readonly*/ Dictionary<VideoMetadata, VideoWriter> _recorders = new Dictionary<VideoMetadata, VideoWriter>();
        private readonly Dictionary<VideoMetadata, Capture> _players = new Dictionary<VideoMetadata, Capture>();

        private string _tmpRecordPath;
        private string _tmpPlayPath;

        private bool _isRecording;
        private bool _isPlaying;

        #endregion

        #region commands

        public RelayCommand PlayCommand { get; private set; }

        public RelayCommand StopCommand { get; private set; }

        public RelayCommand RecordCommand { get; private set; }

        #endregion

        #region properties

        #region Fps

        /// <summary>
        /// The <see cref="Fps" /> property's name.
        /// </summary>
        public const string FpsPropertyName = "Fps";

        private int _fps = 30;

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

        #region Mode

        [IgnoreDataMember]
        public static string[] Modes { get { return new[] { "Recorder", "Player" }; } }

        /// <summary>
        /// The <see cref="Mode" /> property's name.
        /// </summary>
        public const string ModePropertyName = "Mode";

        private string _mode = Modes[0];

        /// <summary>
        /// Sets and gets the Mode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (_mode == value)
                {
                    return;
                }

                RaisePropertyChanging(ModePropertyName);
                _mode = value;
                RaisePropertyChanged(ModePropertyName);
            }
        }

        #endregion

        #region ColorImageSource

        /// <summary>
        /// The <see cref="ColorImageSource" /> property's name.
        /// </summary>
        public const string ColorImageSourcePropertyName = "ColorImageSource";

        private BitmapSource _colorImageSource = null;

        /// <summary>
        /// Sets and gets the ColorImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource ColorImageSource
        {
            get
            {
                return _colorImageSource;
            }

            set
            {
                if (_colorImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(ColorImageSourcePropertyName);
                _colorImageSource = value;
                RaisePropertyChanged(ColorImageSourcePropertyName);
            }
        }

        #endregion

        #region DepthImageSource

        /// <summary>
        /// The <see cref="DepthImageSource" /> property's name.
        /// </summary>
        public const string DepthImageSourcePropertyName = "DepthImageSource";

        private BitmapSource _depthImageSource = null;

        /// <summary>
        /// Sets and gets the DepthImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource DepthImageSource
        {
            get
            {
                return _depthImageSource;
            }

            set
            {
                if (_depthImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(DepthImageSourcePropertyName);
                _depthImageSource = value;
                RaisePropertyChanged(DepthImageSourcePropertyName);
            }
        }

        #endregion

        #region ConfidenceImageSource

        /// <summary>
        /// The <see cref="ConfidenceImageSource" /> property's name.
        /// </summary>
        public const string ConfidenceImageSourcePropertyName = "ConfidenceImageSource";

        private BitmapSource _confidenceImageSource = null;

        /// <summary>
        /// Sets and gets the ConfidenceImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public BitmapSource ConfidenceImageSource
        {
            get
            {
                return _confidenceImageSource;
            }

            set
            {
                if (_confidenceImageSource == value)
                {
                    return;
                }

                RaisePropertyChanging(ConfidenceImageSourcePropertyName);
                _confidenceImageSource = value;
                RaisePropertyChanged(ConfidenceImageSourcePropertyName);
            }
        }

        #endregion

        #region IsEndlessLoop

        /// <summary>
        /// The <see cref="IsEndlessLoop" /> property's name.
        /// </summary>
        public const string IsEndlessLoopPropertyName = "IsEndlessLoop";

        private bool _isEndlessLoop = true;

        /// <summary>
        /// Sets and gets the IsEndlessLoop property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEndlessLoop
        {
            get
            {
                return _isEndlessLoop;
            }

            set
            {
                if (_isEndlessLoop == value)
                {
                    return;
                }

                RaisePropertyChanging(IsEndlessLoopPropertyName);
                _isEndlessLoop = value;
                RaisePropertyChanged(IsEndlessLoopPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public VideoRecordAndPlay()
            :base(false)
        {
            #region commands

            RecordCommand = new RelayCommand(OnRecord);
            PlayCommand = new RelayCommand(OnPlay);
            StopCommand = new RelayCommand(OnStop);

            #endregion

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case ModePropertyName:
                        UpdateRecorderAndPlayer();
                        break;
                }
            };
            UpdateRecorderAndPlayer();
        }

        #endregion

        private void UpdateRecorderAndPlayer()
        {
            if (Equals(Mode, "Player"))
            {
                var fileDialog = new OpenFileDialog { Filter = "Huddle Engine Recording|*.rec.huddle" };

                if ((bool)fileDialog.ShowDialog(Application.Current.MainWindow))
                {
                    _tmpPlayPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    ZipFile.ExtractToDirectory(fileDialog.FileName, _tmpPlayPath); // TODO clean up


                    var task = Task.Factory.StartNew(() =>
                    {
                        Metadata metadata;

                        var serializer = new XmlSerializer(typeof(Metadata));
                        using (var stream = new FileStream(GetTempFilePath(_tmpPlayPath, ".metadata"), FileMode.Open))
                            metadata = serializer.Deserialize(stream) as Metadata;

                        if (metadata == null)
                            throw new Exception("Could not load recording metadata");

                        foreach (var item in metadata.Items)
                        {
                            var player = new Capture(GetTempFilePath(_tmpPlayPath, item.FileName));
                            _players.Add(item, player);
                        }

                    });

                    MessageBox.Show("All files loaded.", "Player");
                }
                else
                {
                    return;
                }
            }
        }

        private bool _isRecorderStarted = false;

        public override IData Process(IData data)
        {
            var imageData = data as UMatData;

            if (_isRecording && imageData != null)
                Record(imageData);

            if (IsRenderContent)
                render(imageData.Data, imageData.Key);

            return null; //data;
        }

        public override UMatData ProcessAndView(UMatData data)
        {
            return data;
        }

        public void Record(UMatData imageData)
        {
            if (!_recorders.Any(kvp => Equals(kvp.Key.Key, imageData.Key)))
            {
                var videoMetadata = new VideoMetadata
                {
                    Key = imageData.Key,
                    FileName = string.Format("{0}{1}", imageData.Key, ".avi"),
                    Width = imageData.Data.Cols,
                    Height = imageData.Data.Rows,
                    Fps = Fps
                };

                var videoWriter = new VideoWriter(
                    GetTempFilePath(_tmpRecordPath, videoMetadata.FileName),
                    0, // probably raw https://wiki.videolan.org/YUV/
                    /*Emgu.CV.VideoWriter.Fourcc('D', 'I', 'V', 'X'),*/
                    Fps,
                    new System.Drawing.Size(videoMetadata.Width, videoMetadata.Height),
                    true);

                _recorders.Add(videoMetadata, videoWriter);

                _isRecorderStarted = true;
            }

            if (!_isRecorderStarted) return;

            // TODO: The _recorders.Single may raise an exception if sequence is empty or multiple items in sequence match
            VideoWriter recorder = null;
            try {
                recorder = _recorders.Single(kvp => Equals(kvp.Key.Key, imageData.Key)).Value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            
            if (recorder != null)
            {
                Mat _image = new Mat();
                imageData.Data.CopyTo(_image);

                // convert depth images to an rgb format so i can write the frame
                if (_image.NumberOfChannels == 1)
                {
                    Mat _tmp = new Mat(_image.Rows, _image.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                    CvInvoke.CvtColor(_image, _tmp, Emgu.CV.CvEnum.ColorConversion.Gray2Rgb);
                    _image = _tmp;

                }

                recorder.Write(_image);
            }
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            OnStop(); // Stop players and recorders

            DisposeRecorders();
            DisposePlayers();

            base.Stop();
        }

        #region Record and Play Actions

        private void OnRecord()
        {
            if (_isRecording) return;

            _tmpRecordPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            if (!Directory.Exists(_tmpRecordPath))
                Directory.CreateDirectory(_tmpRecordPath);

            _isRecording = true;

            // Start pipeline if not running yet
            if (!base.IsProcessing)
            {
                base.Start();
            }
        }

        private void OnPlay()
        {
            if (_isPlaying) return;

            _isPlaying = true;

            var task = Task.Factory.StartNew(() =>
            {
                Dictionary<String, int> fckvp = new Dictionary<String, int>();
                foreach (var kvp in _players)
                {
                    fckvp[kvp.Key.Key] = 0;
                }


                while (_isPlaying)
                {
                    foreach (var kvp in _players)
                    {
                        var videoMetadata = kvp.Key;
                        var player = kvp.Value;

                        try
                        {
                            if (IsEndlessLoop)
                            {
                                if (fckvp[videoMetadata.Key] == player.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount))
                                {
                                    player.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, 0);
                                    fckvp[videoMetadata.Key] = 0;
                                }
                            }

                            Mat image = player.QueryFrame();
                            if (IsEndlessLoop)
                            {
                                fckvp[videoMetadata.Key]++;
                            }

                            if (image == null) continue;

                            var imageCopy = image.Clone();
                            image.Dispose();

                            UMat img = imageCopy.ToUMat(Emgu.CV.CvEnum.AccessType.ReadWrite);

                            Stage(new UMatData(this, videoMetadata.Key, img));

                            if (IsRenderContent)
                            {
                                render(img, videoMetadata.Key);
                            }
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                    Push();

                    Thread.Sleep(1000 / Fps);
                }
            });
        }

        private void OnStop()
        {
            //Delete _tmpPlayPath if isPlaying

            if (_isRecording)
            {
                SaveRecording();
            }
            else if (_isPlaying)
            {
                StopPlaying();
            }
        }

        private void SaveRecording()
        {
            var fileDialog = new SaveFileDialog { Filter = "Huddle Engine Recording|*.rec.huddle" };

            var result = fileDialog.ShowDialog(Application.Current.MainWindow);

            if (!result.Value) return;

            // stop recorders, otherwise the video data cannot be stored in zip archive
            var a = _recorders.Keys.ToList();
            foreach (var b in a)
            {
                _recorders[b].Dispose();
                _recorders[b] = null;
            }

            var task = Task.Factory.StartNew(() =>
            {
                var metadata = new Metadata { Items = _recorders.Keys.ToList() };

                var serializer = new XmlSerializer(typeof(Metadata));
                using (var stream = new FileStream(GetTempFilePath(_tmpRecordPath, ".metadata"), FileMode.Create))
                    serializer.Serialize(stream, metadata);

                if (File.Exists(fileDialog.FileName))
                    File.Delete(fileDialog.FileName);

                ZipFile.CreateFromDirectory(_tmpRecordPath, fileDialog.FileName, CompressionLevel.Optimal, false);

                // cleanup resources
                if (Directory.Exists(_tmpRecordPath))
                    Directory.Delete(_tmpRecordPath, true);
            });
            task.ContinueWith(t =>
            {
                _isRecording = false;
                MessageBox.Show("Video recording saved.", "Recordings");
            });
        }

        void render(UMat image, String key) {
            if (IsRenderContent)
            {
                var _image = image.Clone();
                switch (key)
                {
                    case "color":
                        Task.Factory.StartNew(() =>
                        {
                            var bitmap = _image.ToBitmapSource(true);
                            _image.Dispose();
                            return bitmap;
                        }).ContinueWith(s => ColorImageSource = s.Result);
                        break;
                    case "depth":
                        Task.Factory.StartNew(() =>
                        {
                            var bitmap = _image.ToBitmapSource(true);
                            _image.Dispose();
                            return bitmap;
                        }).ContinueWith(s => DepthImageSource = s.Result);
                        break;
                    case "confidence":
                        Task.Factory.StartNew(() =>
                        {
                            var bitmap = _image.ToBitmapSource(true);
                            _image.Dispose();
                            return bitmap;
                        }).ContinueWith(s => ConfidenceImageSource = s.Result);
                        break;
                    default:
                        _image.Dispose();
                        break;
                }
            }
        }

        private void StopPlaying()
        {
            // cleanup resources
            if (Directory.Exists(_tmpPlayPath))
                Directory.Delete(_tmpPlayPath, true);

            _isPlaying = false;
        }

        #endregion

        private void DisposeRecorders()
        {
            foreach (var recorder in _recorders.Values)
            {
                if (recorder != null)
                {
                    recorder.Dispose();
                }
            }

            _recorders.Clear();
        }

        private void DisposePlayers()
        {
            foreach (var player in _players.Values)
                player.Dispose();

            _players.Clear();
        }

        public string GetTempFilePath(string tmpPath, string fileName, string extension = null)
        {
            fileName = string.Format("{0}{1}", fileName, extension ?? "");
            return Path.Combine(tmpPath, fileName);
        }
    }

    [XmlRoot]
    public class Metadata
    {
        #region properties

        #region Items

        [XmlArray]
        [XmlArrayItem]
        public List<VideoMetadata> Items { get; set; }

        #endregion

        #endregion
    }

    [XmlType]
    public class VideoMetadata
    {
        #region properties

        #region Key

        [XmlAttribute]
        public string Key { get; set; }

        #endregion

        #region FileName

        [XmlAttribute]
        public string FileName { get; set; }

        #endregion

        #region Width

        [XmlElement]
        public int Width { get; set; }

        #endregion

        #region Height

        [XmlElement]
        public int Height { get; set; }

        #endregion

        #region Fps

        [XmlElement]
        public int Fps { get; set; }

        #endregion

        #endregion

        #region Override Equals and HashCode

        public override bool Equals(object obj)
        {
            var other = obj as VideoMetadata;
            return other != null && Equals(Key, other.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #endregion
    }
}
