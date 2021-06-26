using System;

namespace Wice
{
    public class StateButtonState : IEquatable<StateButtonState>
    {
        public StateButtonState(object value, Func<StateButton, EventArgs, StateButtonState, Visual> createChildFunc)
        {
            if (createChildFunc == null)
                throw new ArgumentNullException(nameof(createChildFunc));

            Value = value;
            CreateChildFunc = createChildFunc;
        }

        public object Value { get; }
        public Func<StateButton, EventArgs, StateButtonState, Visual> CreateChildFunc { get; }
        public Func<StateButtonState, object, bool?> EqualsFunc { get; set; }

        internal Visual CreateChild(StateButton box, EventArgs e)
        {
            var visual = CreateChildFunc(box, e, this);
            if (visual == null)
                throw new InvalidOperationException();

            return visual;
        }

        public override string ToString() => Value?.ToString();

        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override bool Equals(object obj)
        {
            var func = EqualsFunc;
            if (func != null)
            {
                var ret = func(this, obj);
                if (ret.HasValue)
                    return ret.Value;
            }

            if (obj is StateButtonState state)
                return Equals(state);

            if (Value == null)
                return false;

            return Value.Equals(obj);
        }

        public virtual bool Equals(StateButtonState other)
        {
            if (other == null)
                return false;

            return Equals(other.Value);
        }
    }
}
