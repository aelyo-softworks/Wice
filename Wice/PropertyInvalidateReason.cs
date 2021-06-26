﻿using System;

namespace Wice
{
    public class PropertyInvalidateReason : InvalidateReason
    {
        public PropertyInvalidateReason(BaseObjectProperty property, InvalidateReason innerReason = null)
            : base(property?.DeclaringType, innerReason)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            Property = property;
        }

        public BaseObjectProperty Property { get; }

        protected override string GetBaseString() => base.GetBaseString() + "[" + Property.Name + "]";
    }
}
