namespace Wice.Utilities;

public static class CommandLine
{
    private static readonly Dictionary<string, string> _namedArguments;
    private static readonly Dictionary<int, string> _positionArguments;

    static CommandLine()
    {
        _namedArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _positionArguments = new Dictionary<int, string>();

        var args = Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; i++)
        {
            if (i == 0)
                continue;

            var arg = args[i].Nullify();
            if (arg == null)
                continue;

            var upper = arg.ToUpperInvariant();
            if (arg == "/?" || arg == "-?" || upper == "/HELP" || upper == "-HELP")
            {
                HelpRequested = true;
            }

            var named = false;
            if (arg[0] == '-' || arg[0] == '/')
            {
                arg = arg.Substring(1);
                named = true;
            }

            string name;
            string value;
            var pos = arg.IndexOf(':');
            if (pos < 0)
            {
                name = arg;
                value = null;
            }
            else
            {
                name = arg.Substring(0, pos).Trim();
                value = arg.Substring(pos + 1).Trim();
            }

            _positionArguments[i - 1] = arg;
            if (named)
            {
                _namedArguments[name] = value;
            }
        }
    }

    public static IReadOnlyDictionary<string, string> NamedArguments => _namedArguments;
    public static IReadOnlyDictionary<int, string> PositionArguments => _positionArguments;
    public static bool HelpRequested { get; }

    public static string CommandLineWithoutExe
    {
        get
        {
            var line = Environment.CommandLine;
            var inParens = false;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ' && !inParens)
                    return line.Substring(i + 1).TrimStart();

                if (line[i] == '"')
                {
                    inParens = !inParens;
                }
            }
            return line;
        }
    }

    public static T GetArgument<T>(IEnumerable<string> arguments, string name, T defaultValue = default)
    {
        if (arguments == null)
            return defaultValue;

        foreach (var arg in arguments)
        {
            if (arg.StartsWith("-") || arg.StartsWith("/"))
            {
                var pos = arg.IndexOfAny(new[] { '=', ':' }, 1);
                var argName = pos < 0 ? arg.Substring(1) : arg.Substring(1, pos - 1);
                if (string.Compare(name, argName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var value = pos < 0 ? string.Empty : arg.Substring(pos + 1).Trim();
                    if (value.Length == 0)
                    {
                        if (typeof(T) == typeof(bool)) // special case for bool args: if it's there, return true
                            return (T)(object)true;

                        return defaultValue;
                    }
                    return Conversions.ChangeType(value, defaultValue);
                }
            }
        }
        return defaultValue;
    }

    public static string GetNullifiedArgument(string name, string defaultValue = null)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (!_namedArguments.TryGetValue(name, out string s))
            return defaultValue.Nullify();

        if (string.IsNullOrWhiteSpace(s))
            return defaultValue.Nullify();

        return s.Nullify();
    }

    public static string GetNullifiedArgument(int index, string defaultValue = null) => GetArgument(index, defaultValue).Nullify();
    public static T GetArgument<T>(int index, T defaultValue) => GetArgument(index, defaultValue, null);
    public static T GetArgument<T>(int index, T defaultValue, IFormatProvider provider)
    {
        if (!_positionArguments.TryGetValue(index, out string s))
            return defaultValue;

        return Conversions.ChangeType(s, defaultValue, provider);
    }

    public static object GetArgument(int index, object defaultValue, Type conversionType) => GetArgument(index, defaultValue, conversionType, null);
    public static object GetArgument(int index, object defaultValue, Type conversionType, IFormatProvider provider)
    {
        if (!_positionArguments.TryGetValue(index, out string s))
            return defaultValue;

        return Conversions.ChangeType(s, conversionType, defaultValue, provider);
    }

    public static T GetArgument<T>(string name, T defaultValue = default) => GetArgument(name, defaultValue, null);

    public static T GetArgument<T>(string name, T defaultValue, IFormatProvider provider)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (!_namedArguments.TryGetValue(name, out string s))
            return defaultValue;

        if (typeof(T) == typeof(bool) && string.IsNullOrEmpty(s))
            return (T)(object)true;

        return Conversions.ChangeType(s, defaultValue, provider);
    }

    public static bool HasArgument(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return _namedArguments.TryGetValue(name, out _);
    }

    public static object GetArgument(string name, object defaultValue, Type conversionType) => GetArgument(name, defaultValue, conversionType, null);

    public static object GetArgument(string name, object defaultValue, Type conversionType, IFormatProvider provider)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (conversionType == null)
            throw new ArgumentNullException(nameof(conversionType));

        if (!_namedArguments.TryGetValue(name, out string s))
            return defaultValue;

        if (conversionType == typeof(bool) && string.IsNullOrEmpty(s))
            return true;

        return Conversions.ChangeType(s, conversionType, defaultValue, provider);
    }
}
