using System;
using System.Collections.Specialized;

namespace Wice
{
    public class CollectionChangedInvalidateReason : InvalidateReason
    {
        public CollectionChangedInvalidateReason(Type type, Type childType, NotifyCollectionChangedAction action, InvalidateReason innerReason = null)
            : base(type, innerReason)
        {
            if (childType == null)
                throw new ArgumentNullException(nameof(childType));

            ChildType = childType;
            Action = action;
        }

        public Type ChildType { get; }
        public NotifyCollectionChangedAction Action { get; }

        protected override string GetBaseString() => base.GetBaseString() + "[" + Action + "](" + ChildType.Name + ")";
    }
}
