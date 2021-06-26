using System;

namespace Wice.Utilities
{
    public class DirectNLogger : EventProviderLogger
    {
        public static DirectNLogger Instance = new DirectNLogger();

        public DirectNLogger()
            : base(new Guid("964D4572-ADB9-4F3A-8170-FCBECEC27466"))
        {
        }
    }
}
