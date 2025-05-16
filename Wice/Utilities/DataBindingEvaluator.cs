namespace Wice.Utilities;

public static class DataBindingEvaluator
{
    private static readonly char[] _expressionPartSeparator = new char[] { '.' };
    private static readonly char[] _indexExprEndChars = new char[] { ']', ')' };
    private static readonly char[] _indexExprStartChars = new char[] { '[', '(' };

    public static string Eval(object container, string expression, string format, IFormatProvider provider = null)
    {
        if (provider == null)
            return string.Format(format, Eval(container, expression));

        return string.Format(provider, format, Eval(container, expression));
    }

    public static object Eval(object container, string expression, bool throwOnError = true)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        expression = expression.Nullify();
        if (expression == null)
            throw new ArgumentException(null, nameof(expression));

        if (container == null)
            return null;

        var expressionParts = expression.Split(_expressionPartSeparator);
        return Eval(container, expressionParts, throwOnError);
    }

    private static object Eval(object container, string[] expressionParts, bool throwOnError)
    {
        var propertyValue = container;
        for (var i = 0; i < expressionParts.Length && propertyValue != null; i++)
        {
            var propName = expressionParts[i];
            if (propName.IndexOfAny(_indexExprStartChars) < 0)
            {
                propertyValue = GetPropertyValue(propertyValue, propName, throwOnError);
            }
            else
            {
                propertyValue = GetIndexedPropertyValue(propertyValue, propName, throwOnError);
            }
        }
        return propertyValue;
    }

    public static object GetPropertyValue(object container, string propName, bool throwOnError = true)
    {
        if (container == null)
            throw new ArgumentNullException(nameof(container));

        if (propName == null)
            throw new ArgumentNullException(nameof(propName));

        propName = propName.Nullify();
        if (propName == null)
        {
            if (throwOnError)
                throw new ArgumentException(null, nameof(propName));

            return null;
        }

        var props = TypeDescriptor.GetProperties(container);
        var descriptor = props?.Find(propName, true);
        if (descriptor == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + container.GetType().FullName + "' does not contain a property with the name '" + propName + "'.", nameof(propName));

            return null;
        }
        return descriptor.GetValue(container);
    }

    public static object GetIndexedPropertyValue(object container, string expression, bool throwOnError = true)
    {
        if (container == null)
            throw new ArgumentNullException(nameof(container));

        expression = expression.Nullify();
        if (expression == null)
            throw new ArgumentException(null, nameof(expression));

        var isIndex = false;
        var pos1 = expression.IndexOfAny(_indexExprStartChars);
        var pos2 = expression.IndexOfAny(_indexExprEndChars, pos1 + 1);
        if (pos1 < 0 || pos2 < 0 || pos2 == (pos1 + 1))
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + expression + "' is not a valid indexed expression.", nameof(expression));

            return null;
        }

        string propName = null;
        object index = null;
        var str = expression.Substring(pos1 + 1, (pos2 - pos1) - 1).Trim();
        if (pos1 != 0)
        {
            propName = expression.Substring(0, pos1);
        }

        if (str.Length != 0)
        {
            if ((str[0] == '"' && str[str.Length - 1] == '"') || (str[0] == '\'' && str[str.Length - 1] == '\''))
            {
                index = str.Substring(1, str.Length - 2);
            }
            else if (char.IsDigit(str[0]))
            {
                isIndex = int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number);
                if (isIndex)
                {
                    index = number;
                }
                else
                {
                    index = str;
                }
            }
            else
            {
                index = str;

            }
        }

        if (index == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + expression + "' is not a valid indexed expression.", nameof(expression));

            return null;
        }

        object propertyValue;
        if (propName != null && propName.Length != 0)
        {
            propertyValue = GetPropertyValue(container, propName, throwOnError);
        }
        else
        {
            propertyValue = container;
        }

        if (propertyValue == null)
            return null;

        if (isIndex && propertyValue is Array array && array.Rank == 1)
        {
            var idx = (int)index;
            if ((idx < 0 || idx > (array.Length - 1)) && !throwOnError)
                return null;

            return array.GetValue(idx);
        }

        if (isIndex && propertyValue is IList list1)
        {
            var idx = (int)index;
            if ((idx < 0 || idx > (list1.Count - 1)) && !throwOnError)
                return null;

            return list1[idx];
        }

        var info = propertyValue.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new[] { index.GetType() }, null);
        if (info == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + propertyValue.GetType().FullName + "' does not allow indexed access.", nameof(container));

            return null;
        }

        return info.GetValue(propertyValue, new object[] { index });
    }
}
