using System;
using System.Collections.Generic;
using Huddle.Engine.Processor;
using Huddle.Engine.Util;

using Huddle.Engine.Processor.Statistics;

namespace Huddle.Engine.Data
{
    public sealed class DataContainer : List<IData>, IDataContainer //SynchronizedCollection<IData>, IDataContainer
    {
        #region private fields

        private object _lock = new object();

        // TODO move to private
        public List<KeyValuePair<Type, long>> _processingTime = new List<KeyValuePair<Type, long>>();

        #endregion

        #region properties

        #region FrameId

        public long FrameId { get; set; }

        #endregion

        #region DataContainerNumber

        private long _dataContainerNumber = -1L;

        public long DataContainerNumber
        {
            get
            {
                return _dataContainerNumber;
            }
        }
        #endregion

        #region Timestamp

        public DateTime Timestamp { get; private set; }

        #endregion

        #endregion

        #region ctor

        public DataContainer(long frameId, DateTime timestamp, long value = -1L)
        {
            FrameId = frameId;
            Timestamp = timestamp;
            if(value == -1L)
            {
                _dataContainerNumber = DataFrameNumberGenerator.Instance.getNextDataFrameNumber();
            }
            else
            {
                _dataContainerNumber = value;
            }
        }

        #endregion

        public List<BaseProcessor> Trail { get; private set; }

        public List<IDataContainer> Siblings { get; private set; }

        public IDataContainer Copy()
        {
            var copy = new DataContainer(FrameId, Timestamp, _dataContainerNumber)
            {
                Siblings = new List<IDataContainer>(),
                _processingTime = new List<KeyValuePair<Type, long>>(_processingTime)
            };

            for (int i = 0; i < _processingTime.Count; i++)
            {
                copy._processingTime.Add(new KeyValuePair<Type, long>(_processingTime[i].Key, _processingTime[i].Value));
            }

            if (Siblings == null)
                Siblings = new List<IDataContainer>();

            lock (_lock)
            {
                Siblings.Add(copy);
            }

            copy.Siblings.Add(this);

            foreach (var data in ToArray())
            {
                var dataCopy = data.Copy();

                if (dataCopy != null)
                    copy.Add(dataCopy);
            }

            return copy;
        }

        public void processedBy(Type node, long ticks)
        {
            _processingTime.Add(new KeyValuePair<Type, long>(node, ticks));
        }
        

        public void Dispose()
        {
            //lock (_lock)
            //{
            //    if (Siblings != null)
            //    {
            //        foreach (var sibling in Siblings.ToArray())
            //            sibling.Siblings.Remove(this);

            //        if (Siblings.Any())
            //            Siblings.Clear();
            //    } 
            //}

            //if (Trail != null && Trail.Any())
            //    Trail.Clear();

            // TODO call tick node!
            if (ProcessingTimeTicks.hopeImAlone != null)
            {
                ProcessingTimeTicks.hopeImAlone.PostProcess(this);
            }

            if (Count > 0)
                foreach (var data in this)
                    data.Dispose();
        }

        // override add function to inject parent data container to all
        // elements
        public new void Add(IData item)
        {
            //if (item.Parent == null)
            //{
            //    item.Parent = this;
            //}
            base.Add(item);
        }
    }
}
