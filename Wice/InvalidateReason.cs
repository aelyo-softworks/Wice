using System;

namespace Wice
{
    public class InvalidateReason
    {
        public InvalidateReason(Type type, InvalidateReason innerReason = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
            InnerReason = innerReason;
        }

        public Type Type { get; }
        public InvalidateReason InnerReason { get; }

        protected virtual string GetBaseString()
        {
            var typeName = GetType().Name;
            if (typeName.EndsWith(typeof(InvalidateReason).Name))
            {
                typeName = typeName.Substring(0, typeName.Length - typeof(InvalidateReason).Name.Length);
            }
            return typeName + "(" + Type.Name + ")";
        }

        public override string ToString()
        {
            var s = GetBaseString();
            if (InnerReason != null)
            {
                s += " <= " + InnerReason.ToString();
            }
            return s;
        }
    }
}
