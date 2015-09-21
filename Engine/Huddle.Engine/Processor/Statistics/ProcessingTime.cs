using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Diagnostics;
using Huddle.Engine.Data;
using Huddle.Engine.Util;

using System.Data.SQLite;

namespace Huddle.Engine.Processor.Statistics
{
    [ViewTemplate("Processing Time","ProcessingTime")]
    public class ProcessingTime : BaseProcessor
    {
        #region private fields

        private PerformanceCounter _cpuCounter;
        private Process _currentProcess;
        private Timer _timer;

        private System.IO.StreamWriter _file;
        private const string LOG_SEPARATOR = ";";

        private SQLiteConnection connection = null;
        private SQLiteCommand command = null;

        private Timer _fpsTimer;
        private int _dcnt = 0;
        private int _ccnt = 0;
        private int _last_ccnt = 0;
        private int _last_dcnt = 0;

        #endregion

        #region properties

        #region CPU

        /// <summary>
        /// The <see cref="CPU" /> property's name.
        /// </summary>
        public const string CPUPropertyName = "CPU";

        private float _cpu = 0.0f;

        /// <summary>
        /// Sets and gets the CPU property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float CPU
        {
            get
            {
                return _cpu;
            }
            set
            {
                RaisePropertyChanging(CPUPropertyName);
                _cpu = value;
                RaisePropertyChanged(CPUPropertyName);
            }
        }

        #endregion

        #region Memory

        /// <summary>
        /// The <see cref="Memory" /> property's name.
        /// </summary>
        public const string MemoryPropertyName = "Memory";

        private long _memory;

        /// <summary>
        /// Sets and gets the Memory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long Memory
        {
            get
            {
                return _memory;
            }
            set
            {
                RaisePropertyChanging(MemoryPropertyName);
                _memory = value;
                RaisePropertyChanged(MemoryPropertyName);
            }
        }

        #endregion

        #region PagedMemory

        /// <summary>
        /// The <see cref="PagedMemory" /> property's name.
        /// </summary>
        public const string PagedMemoryPropertyName = "PagedMemory";

        private long _pagedMemory;

        /// <summary>
        /// Sets and gets the PagedMemory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long PagedMemory
        {
            get
            {
                return _pagedMemory;
            }
            set
            {
                RaisePropertyChanging(PagedMemoryPropertyName);
                _pagedMemory = value;
                RaisePropertyChanged(PagedMemoryPropertyName);
            }
        }

        #endregion

        #region ColorTime

        /// <summary>
        /// The <see cref="ColorTime" /> property's name.
        /// </summary>
        public const string ColorTimePropertyName = "ColorTime";

        private String _processingColorTime = String.Empty;

        /// <summary>
        /// Sets and gets the ColorTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String ColorTime
        {
            get
            {
                return _processingColorTime;
            }
            set
            {
                RaisePropertyChanging(ColorTimePropertyName);
                _processingColorTime = value;
                RaisePropertyChanged(ColorTimePropertyName);
            }
        }

        #endregion

        #region DepthTime

        /// <summary>
        /// The <see cref="DepthTime" /> property's name.
        /// </summary>
        public const string DepthTimePropertyName = "DepthTime";

        private String _processingDepthTime = String.Empty;

        /// <summary>
        /// Sets and gets the DepthTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String DepthTime
        {
            get
            {
                return _processingDepthTime;
            }
            set
            {
                RaisePropertyChanging(DepthTimePropertyName);
                _processingDepthTime = value;
                RaisePropertyChanged(DepthTimePropertyName);
            }
        }

        #endregion

        /*
         * TODO init connection if status is changed
         */
        #region Log2DB

        /// <summary>
        /// The <see cref="Log2DB" /> property's name.
        /// </summary>
        public const string Log2DBPropertyName = "Log2DB";

        private Boolean _log2db = true;

        /// <summary>
        /// Sets and gets the Log2DB property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Boolean Log2DB
        {
            get
            {
                return _log2db;
            }
            set
            {
                RaisePropertyChanging(Log2DBPropertyName);
                _log2db = value;
                RaisePropertyChanged(Log2DBPropertyName);
            }
        }

        #endregion

        #region Log2File

        /// <summary>
        /// The <see cref="Log2File" /> property's name.
        /// </summary>
        public const string Log2FilePropertyName = "Log2File";

        private Boolean _log2file = false;

        /// <summary>
        /// Sets and gets the Log2File property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Boolean Log2File
        {
            get
            {
                return _log2file;
            }
            set
            {
                RaisePropertyChanging(Log2FilePropertyName);
                _log2file = value;
                RaisePropertyChanged(Log2FilePropertyName);
            }
        }

        #endregion

        #region LogFilePath

        /// <summary>
        /// The <see cref="LogFilePath" /> property's name.
        /// </summary>
        public const string LogFilePathPropertyName = "LogFilePath";

        private String _logFilePath = String.Empty;

        /// <summary>
        /// Sets and gets the LogFilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String LogFilePath
        {
            get
            {
                return _logFilePath;
            }
            set
            {
                RaisePropertyChanging(LogFilePathPropertyName);
                _logFilePath = value;
                RaisePropertyChanged(LogFilePathPropertyName);
            }
        }

        #endregion

        #region DataFrameNumber

        /// <summary>
        /// The <see cref="DataFrameNumber" /> property's name.
        /// </summary>
        public const string DataFrameNumberPropertyName = "DataFrameNumber";

        private long _dataFrameNumber;

        /// <summary>
        /// Sets and gets the DataFrameNumber property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public long DataFrameNumber
        {
            get
            {
                return _dataFrameNumber;
            }
            set
            {
                RaisePropertyChanging(DataFrameNumberPropertyName);
                _dataFrameNumber = value;
                RaisePropertyChanged(DataFrameNumberPropertyName);
            }
        }

        #endregion

        #region FPSColor

        /// <summary>
        /// The <see cref="FPSColor" /> property's name.
        /// </summary>
        public const string FPSColorPropertyName = "FPSColor";

        private int _fpsColor = 0;

        /// <summary>
        /// Sets and gets the FPSColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FPSColor
        {
            get
            {
                return _fpsColor;
            }

            set
            {
                if (_fpsColor == value)
                {
                    return;
                }

                RaisePropertyChanging(FPSColorPropertyName);
                _fpsColor = value;
                RaisePropertyChanged(FPSColorPropertyName);
            }
        }

        #endregion

        #region FPSDepth

        /// <summary>
        /// The <see cref="FPSDepth" /> property's name.
        /// </summary>
        public const string FPSDepthPropertyName = "FPSDepth";

        private int _fpsDepth = 0;

        /// <summary>
        /// Sets and gets the FPSDepth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FPSDepth
        {
            get
            {
                return _fpsDepth;
            }

            set
            {
                if (_fpsDepth == value)
                {
                    return;
                }

                RaisePropertyChanging(FPSDepthPropertyName);
                _fpsDepth = value;
                RaisePropertyChanged(FPSDepthPropertyName);
            }
        }

        #endregion

        #endregion

        #region ctor

        public ProcessingTime()
        {
            _cpuCounter = new PerformanceCounter();
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";

            _currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            _timer = new Timer(1000/3); // 3 times a second
            _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            // FPS Timer
            _fpsTimer = new Timer(1000);
            _fpsTimer.Elapsed += new ElapsedEventHandler(fpsTimer_Elapsed);
        }

        #endregion

        #region overrice methods

        public override Data.IData Process(Data.IData data)
        {
            // DataFrameNumber
            //DataFrameNumber = data.Parent.DataContainerNumber;
            if (DataFrameNumber == 0)
            {
                // TODO smth wrong here!
                // BUG fixme
            }

            String _time = String.Empty;
            // Processing Time
            if ("color".Equals(data.Key))
            {
                _ccnt++;
                _time = ColorTime = (DateTime.Now - data.CreationTime).Milliseconds.ToString();
            }
            if ("depth".Equals(data.Key))
            {
                _dcnt++;
                _time = DepthTime = (DateTime.Now - data.CreationTime).Milliseconds.ToString();
            }

            log(DataFrameNumber, CPU, Memory, data.Key, FPSColor, FPSDepth, _time);

            return data;
        }

        public override void Start()
        {
            _timer.Enabled = true;
            _fpsTimer.Enabled = true;
            openLog();
            connectDB();
            base.Start();
        }

        public override void Stop()
        {
            _timer.Enabled = false;
            _fpsTimer.Enabled = false;
            closeLog();
            disconnectDB();
            base.Stop();
        }

        #endregion

        #region private methods

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // CPU usage
            CPU = _cpuCounter.NextValue();

            // Memory
            Memory = _currentProcess.PrivateMemorySize64;
            PagedMemory = _currentProcess.PagedMemorySize64;
        }

        void fpsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FPSColor = _ccnt - _last_ccnt;
            FPSDepth = _dcnt - _last_dcnt;
            _last_ccnt = _ccnt;
            _last_dcnt = _dcnt;
        }

        void log(long frameNumber, float cpu, float memory, string key, int fpsColor, int fpsDepth, string processingTime)
        {
            double ts;
            ts = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            if (_file != null && Log2File)
            {
                // -> send to logger
                //System.Console.WriteLine("logging: {0}, {1}, {2}, {3}, {4}", frameNumber, cpu, memory, key, processingTime);
                String msg = (ts + LOG_SEPARATOR +
                    frameNumber.ToString() + LOG_SEPARATOR +
                    cpu.ToString() + LOG_SEPARATOR +
                    memory.ToString() + LOG_SEPARATOR +
                    key + LOG_SEPARATOR +
                    fpsColor + LOG_SEPARATOR +
                    fpsDepth + LOG_SEPARATOR +
                    processingTime.ToString());
                _file.WriteLine(msg);
            }

            if (connection != null && Log2DB)
            {
                // TODO check input values!
                command.CommandText = "INSERT INTO log (time, frameNumber, cpu, memory, key, fpsColor, fpsDepth, processingTime) VALUES (@ts, @frameNumber, @cpu, @memory, @key, @fpsColor, @fpsDepth, @processingTime)";
                command.Parameters.Add(new SQLiteParameter("@ts", ts));
                command.Parameters.Add(new SQLiteParameter("@frameNumber", frameNumber));
                command.Parameters.Add(new SQLiteParameter("@cpu", cpu));
                command.Parameters.Add(new SQLiteParameter("@memory", memory));
                command.Parameters.Add(new SQLiteParameter("@key",System.Data.DbType.String ,key));
                command.Parameters.Add(new SQLiteParameter("@fpsColor", System.Data.DbType.Int32, fpsColor));
                command.Parameters.Add(new SQLiteParameter("@fpsDepth", System.Data.DbType.Int32, fpsDepth));
                command.Parameters.Add(new SQLiteParameter("@processingTime", System.Data.DbType.String, processingTime));
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        void openLog()
        {
            DateTime _now = DateTime.Now;
            LogFilePath = Path.GetFullPath(@"huddle_" + _now.Day + "_" +_now.Month + "_" + _now.Year + "_" + _now.Hour + "_" + _now.Minute + "_" + _now.Second + ".huddlelog");

            _file = new StreamWriter(LogFilePath);
            // write header
            _file.WriteLine("Time" + LOG_SEPARATOR +
                "Frame Number" + LOG_SEPARATOR +
                "CPU%" + LOG_SEPARATOR +
                "Memory" + LOG_SEPARATOR +
                "key" + LOG_SEPARATOR +
                "FPS Color" + LOG_SEPARATOR +
                "FPS Depth" + LOG_SEPARATOR +
                "Processing Time");
        }

        /*
         * rename to useful name
         */
        void connectDB()
        {
            // connect
            connection = new SQLiteConnection();
            connection.ConnectionString = "Data Source=" + "Statistics.db";
            connection.Open();

            // init db only once
            command = new SQLiteCommand(connection);
            command.CommandText = "CREATE TABLE IF NOT EXISTS log ( "+
                "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "+
                "time REAL, "+
                "frameNumber INTEGER, " +
                "cpu REAL, "+
                "memory INTEGER, "+
                "key TEXT, "+
                "fpsColor INTEGER, "+
                "fpsDepth INTEGER,"+
                "processingTime TEXT );";
            command.ExecuteNonQuery();
        }

        void disconnectDB()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }

        void closeLog()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        #endregion
    }
}