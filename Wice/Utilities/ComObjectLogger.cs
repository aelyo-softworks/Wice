using System;

namespace Wice.Utilities
{
    public class ComObjectLogger : EventProviderLogger
    {
        public static ComObjectLogger Instance = new ComObjectLogger();

        public ComObjectLogger()
            : base(new Guid("964D4572-ADB9-4F3A-8170-FCBECEC27467"))
        {
        }
    }
}
