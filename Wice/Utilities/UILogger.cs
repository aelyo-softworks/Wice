using System;

namespace Wice.Utilities
{
    public class UILogger : EventProviderLogger
    {
        public static UILogger Instance = new UILogger();

        public UILogger()
            : base(new Guid("964D4572-ADB9-4F3A-8170-FCBECEC27465"))
        {
        }
    }
}

