using System;

namespace Wice
{
    public interface IDataSourceVisual
    {
        event EventHandler<EventArgs> DataBound;

        object DataSource { get; set; }
        string DataItemMember { get; set; }
        string DataItemFormat { get; set; }
        DataBinder DataBinder { get; set; }

        void BindDataSource();
    }
}
