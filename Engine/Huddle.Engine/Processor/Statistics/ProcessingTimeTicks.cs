using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Huddle.Engine.Data;
using Huddle.Engine.Util;

using System.Collections.ObjectModel;

namespace Huddle.Engine.Processor.Statistics
{
    [ViewTemplate("Processing Time Ticks", "ProcessingTimeTicks")]
    public class ProcessingTimeTicks : BaseProcessor
    {
        #region private fields

        private Dictionary<Type, long> _nodesAverage = new Dictionary<Type, long>();
        private Dictionary<Type, uint> _nodesAverageN = new Dictionary<Type, uint>();

        private ILog logger = new Log2File("ID" + Log2File.LOG_SEPARATOR +
            "data" + Log2File.LOG_SEPARATOR);

        #endregion

        public static ProcessingTimeTicks hopeImAlone = null;

        #region properties

        public class Node
        {
            public String Key {get; set;}
            public String Value {get; set;}
        }

        private ObservableCollection<Node> _nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes
        {
            get
            {
                return _nodes;
            }

            private set
            {
                if (value == _nodes)
                {
                    return;
                }
                _nodes = value;
            }
        }

        #endregion

        #region ctor

        public ProcessingTimeTicks()
        {
            hopeImAlone = this;
        }

        #endregion

        #region private methods

        private long avg(long curAvg, long n, long value) {
            curAvg = curAvg + (value-curAvg)/n;
            return curAvg;
        }

        #endregion

        #region overrice methods

        public override Data.IData Process(Data.IData data)
        {
            return data;
        }

        // TODO or pre??
        public override Data.IDataContainer PostProcess(Data.IDataContainer dataContainer)
        {
            //TODO fix me i only consume power
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                _nodes.Clear();
            }));

            string message = dataContainer.FrameId.ToString();
            var _dataContainer = dataContainer as DataContainer;
            for (int i = 0; i < _dataContainer._processingTime.Count; i++)
            {
                var n = new Node();

                if (_nodesAverage.ContainsKey(_dataContainer._processingTime[i].Key)) //update
                {
                    long value;
                    if (_nodesAverage.TryGetValue(_dataContainer._processingTime[i].Key, out value))
                    {
                        _nodesAverageN[_dataContainer._processingTime[i].Key]++;
                        _nodesAverage[_dataContainer._processingTime[i].Key] = avg(value, _nodesAverageN[_dataContainer._processingTime[i].Key], _dataContainer._processingTime[i].Value);
                    }

                    n.Key = _dataContainer._processingTime[i].Key.Name;
                    _nodesAverage.TryGetValue(_dataContainer._processingTime[i].Key, out value);
                    n.Value = "" + value;
                }
                else //add
                {
                    _nodesAverage.Add(_dataContainer._processingTime[i].Key, _dataContainer._processingTime[i].Value);
                    _nodesAverageN.Add(_dataContainer._processingTime[i].Key, 0);

                    n.Key = _dataContainer._processingTime[i].Key.Name;
                    n.Value = "" + _dataContainer._processingTime[i].Value;
                }

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _nodes.Add(n);
                }));
                //System.Console.WriteLine(dc._list[i].Key.Name + ": " + dc._list[i].Value);

                message += Log2File.LOG_SEPARATOR + _dataContainer._processingTime[i].Key + Log2File.LOG_SEPARATOR + _nodesAverage[_dataContainer._processingTime[i].Key];
            }
            logger.log(message);

            return base.PostProcess(dataContainer);
        }

        public override void Start()
        {
            logger.Start();
            base.Start();
        }

        public override void Stop()
        {
            logger.Stop();
            base.Stop();
        }

        #endregion

    }
}
