namespace Wice.Utilities;

// adapted from http://haacked.com/archive/2009/01/14/named-formats-redux.aspx/
// better graceful error handling, and with format provider support
public static class StringFormatter
{
    public static string FormatWith(this string format, object container) => FormatWith(format, container, true, null, null);
    public static string FormatWith(this string format, object container, bool throwOnError) => FormatWith(format, container, throwOnError, null, null);
    public static string FormatWith(this string format, object container, IFormatProvider provider) => FormatWith(format, container, true, provider, null);
    public static string FormatWith(this string format, object container, Func<object, string, bool, IFormatProvider, string> evaluateExpression) => FormatWith(format, container, true, null, evaluateExpression);
    public static string FormatWith(this string format, object container, bool throwOnError, IFormatProvider provider, Func<object, string, bool, IFormatProvider, string> evaluateExpression)
    {
        if (string.IsNullOrWhiteSpace(format))
            return format;

        var result = new StringBuilder(format.Length * 2);
        using (var reader = new StringReader(format))
        {
            var expression = new StringBuilder();
            var state = State.OutsideExpression;
            do
            {
                int c;
                switch (state)
                {
                    case State.OutsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                                state = State.End;
                                break;

                            case '{':
                                state = State.OnOpenBracket;
                                break;

                            case '}':
                                state = State.OnCloseBracket;
                                break;

                            default:
                                result.Append((char)c);
                                break;
                        }
                        break;

                    case State.OnOpenBracket:
                        c = reader.Read();
                        switch (c)
                        {
                            case '{':
                                result.Append('{');
                                state = State.OutsideExpression;
                                break;

                            default:
                                if (c >= 0)
                                {
                                    expression.Append((char)c);
                                }
                                state = State.InsideExpression;
                                break;
                        }
                        break;

                    case State.InsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                            case '}':
                                result.Append(outputExpression(container, expression.ToString(), throwOnError, provider));
                                expression.Length = 0;
                                state = State.OutsideExpression;
                                break;

                            default:
                                expression.Append((char)c);
                                break;
                        }
                        break;

                    //case State.OnCloseBracket:
                    default:
                        c = reader.Read();
                        switch (c)
                        {
                            case '}':
                                result.Append('}');
                                state = State.OutsideExpression;
                                break;

                            default:
                                result.Append('}');
                                state = State.OutsideExpression;
                                if (c >= 0)
                                {
                                    result.Append((char)c);
                                }
                                break;
                        }
                        break;
                }
            }
            while (state != State.End);
        }
        return result.ToString();

        string outputExpression(object cont, string expr, bool toe, IFormatProvider prov)
        {
            if (evaluateExpression != null)
                return evaluateExpression(cont, expr, toe, prov);

            return EvaluateExpression(cont, expr, toe, prov);
        }
    }

    private enum State
    {
        OutsideExpression,
        OnOpenBracket,
        InsideExpression,
        OnCloseBracket,
        End,
    }

    private static string NormalizeEvalFormat(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return "{0}";

        var pos = format.IndexOf("{0");
        if (pos >= 0)
            return format;

        return "{0:" + format + "}";
    }

    public static string EvaluateExpression(object container, string expression, bool throwOnError, IFormatProvider provider)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        string format = null;
        var pos = expression.IndexOf(':');
        if (pos > 0)
        {
            format = expression.Substring(pos + 1);
            expression = expression.Substring(0, pos);
        }

        if (string.IsNullOrWhiteSpace(expression) || expression == "}")
            return null;

        var obj = container != null ? DataBindingEvaluator.Eval(container, expression, throwOnError) : null;
        format = NormalizeEvalFormat(format);
        return string.Format(provider, format, obj);
    }
}
