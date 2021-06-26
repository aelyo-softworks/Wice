using System;
using System.Collections.Generic;

namespace Wice
{
    public class DataBindContext
    {
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public DataBindContext(object data)
        {
            Data = data;
        }

        public object Data { get; }
        public virtual ItemVisual ItemVisual { get; set; }
        public virtual Visual DataVisual { get; set; }

        public virtual IDictionary<string, object> Properties => _properties;

        public virtual string GetDisplayName(object context = null)
        {
            context = context ?? this;
            if (Data is string s)
                return s;

            if (Data is IBindingDisplayName name)
            {
                s = name.GetName(context);
                if (s != null)
                    return s;
            }

            return string.Format("{0}", Data);
        }
    }
}
