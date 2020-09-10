using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace IronAssembler
{
    internal static class LogConfigurer
    {
        internal static void ConfigureLog()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ConsoleTarget();
            config.AddTarget("console", consoleTarget);

            consoleTarget.Layout = @"${time} | ${level:uppercase=true} | ${logger} | ${message} ";

            var rule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }
    }
}
