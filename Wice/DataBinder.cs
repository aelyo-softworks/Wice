using System;

namespace Wice
{
    public class DataBinder
    {
        public Action<DataBindContext> ItemVisualCreator { get; set; }
        public Action<DataBindContext> DataItemVisualCreator { get; set; }
        public Action<DataBindContext> DataItemVisualBinder { get; set; }
    }
}
