using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Huddle.Engine.Data;
using Huddle.Engine.Util;

using Emgu.CV;

namespace Huddle.Engine.Processor
{
    [ViewTemplate("Merge RgbImage And Device", "MergeRgbImageAndDevice")]
    public class MergeRgbImageAndDevice : BaseProcessor
    {
        #region private members

        private UMatData _rgbImageData;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataContainer"></param>
        /// <returns></returns>
        public override IDataContainer PreProcess(IDataContainer dataContainer)
        {
            var rgbImages = dataContainer.OfType<UMatData>().ToArray();
            if (rgbImages.Any() && rgbImages[0].Key == "color")
            {
                if (_rgbImageData != null)
                    _rgbImageData.Dispose();

                _rgbImageData = rgbImages.First().Copy() as UMatData;
                return null;
            }

            if (_rgbImageData != null)
            {
                dataContainer.Add(_rgbImageData.Copy());
                _rgbImageData.Dispose();
                _rgbImageData = null;
            }

            return dataContainer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override IData Process(IData data)
        {
            return data;
        }
    }
}
