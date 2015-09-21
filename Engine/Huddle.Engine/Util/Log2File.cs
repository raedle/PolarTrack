using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Huddle.Engine.Util
{
    /// <summary>
    /// Logs messages to file. Use <see cref="LOG_SEPARATOR"/> to separtae messages
    /// and set header with equal numbers of columns as messages.
    /// </summary>
    class Log2File : ILog
    {
        public const string LOG_SEPARATOR = ";";

        #region private fields

        private StreamWriter file = null;
        private string header = null;

        #endregion

        #region properties

        #region BaseDirectory

        private string baseDirectory = null;

        public string BaseDirectory
        {
            get
            {
                return baseDirectory;
            }
            set
            {
                baseDirectory = Path.GetFullPath(value);
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
        /// </summary>
        public String LogFilePath
        {
            get
            {
                return _logFilePath;
            }
            set
            {
                _logFilePath = value;
            }
        }

        #endregion

        #region Append

        private Boolean append = false;

        public Boolean Append
        {
            get
            {
                return append;
            }
            set
            {
                append = value;
            }
        }

        #endregion

        #endregion

        #region ctor

        /// <summary>
        /// default ctor is hidden to avoid logging wrong formated messages
        /// </summary>
        private Log2File()
        {
            
        }

        public Log2File(string header)
        {
            setHeader(header);
        }

        public Log2File(string baseDirectory, string header)
        {
            BaseDirectory = baseDirectory;
            setHeader(header);
        }

        #endregion

        #region private methods

        void open()
        {
            if (BaseDirectory == null)
            {
                BaseDirectory = Directory.GetCurrentDirectory();
            }

            // build filename
            DateTime _now = DateTime.Now;
            string fileName = @"huddle_" +
                _now.Day + "_" +
                _now.Month + "_" +
                _now.Year + "_" +
                _now.Hour + "_" +
                _now.Minute + "_" +
                _now.Second + ".log";
            LogFilePath = Path.Combine(BaseDirectory, fileName);

            file = new StreamWriter(LogFilePath);

            if (!Append)
            {
                if (header != null)
                {
                    file.WriteLine(header);
                }
                else
                {
                    // TODO default header?
                    // atm not needed because default ctor is hidden
                }
            }
        }

        void close()
        {
            if (file != null)
            {
                file.Close();
                file = null;
            }
        }

        #endregion

        #region override methods


        #endregion

        public void Start()
        {
            open();
        }

        public void Stop()
        {
            close();
        }

        public void log(string message)
        {
            if (file != null)
            {
                file.WriteLine(message);
            }
        }

        public void log(string[] message)
        {
            string _message = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                _message += message[i];
                if (i < (message.Length - 1))
                {
                    _message += LOG_SEPARATOR + " ";
                }
            }
            log(_message);
        }

        /// <summary>
        /// must be called once bevor start!
        /// </summary>
        public void setHeader(string header)
        {
            this.header = header;
        }

    }
}
