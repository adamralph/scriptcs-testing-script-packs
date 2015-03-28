namespace ScriptCs.Testing.ScriptPacks
{
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    internal static class LogConfigurator
    {
        public static void DoYourWorst()
        {
            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = @"${date} [${level}] ${logger} ${message} ${exception}",
            };

            var fileTarget = new FileTarget
            {
                FileName = "packs.log",
                Layout = @"${date} [${level}] ${logger} ${message} ${exception}",
            };

            var config = new LoggingConfiguration();
            config.AddTarget("console", consoleTarget);
            config.AddTarget("file", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));
            LogManager.Configuration = config;
        }
    }
}
