using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Logging
{
    public sealed class LogManagerImpl : LogManager
    {
        readonly ILog _logger;

        static LogManagerImpl()
        {
            log4net.GlobalContext.Properties["LogFolderPath"] = "HeroVirtual_Log";
            // Gets directory path of the calling application
            // RelativeSearchPath is null if the executing assembly i.e. calling assembly is a
            // stand alone exe file (Console, WinForm, etc). 
            // RelativeSearchPath is not null if the calling assembly is a web hosted application i.e. a web site
            var log4NetConfigDirectory = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;

            var log4NetConfigFilePath = Path.Combine(log4NetConfigDirectory, "log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigFilePath));
        }
        /// <summary>
        /// Hack method to forcefully write log from anywhere
        /// </summary>
        /// <param name="forematString"></param>
        public static void ForceLog(string formatString, params object[] arguments)
        {
            var logger = log4net.LogManager.GetLogger("");
            logger.Info(string.Format(formatString, arguments));
        }

        public LogManagerImpl(Type logClass)
        {
            _logger = log4net.LogManager.GetLogger(logClass);
        }


        public void Fatal(string formatString, params object[] arguments)
        {
            string errorMessage = string.Format(formatString, arguments);
            if (_logger.IsFatalEnabled)
                _logger.Fatal(errorMessage);
        }

        public void Error(string formatString, params object[] arguments)
        {
            string errorMessage = string.Format(formatString, arguments);
            if (_logger.IsErrorEnabled)
                _logger.Error(errorMessage);
        }

        public void Warn(string formatString, params object[] arguments)
        {
            string message = string.Format(formatString, arguments);
            if (_logger.IsWarnEnabled)
                _logger.Warn(message);
        }

        public void Info(string formatString, params object[] arguments)
        {
            string message = string.Format(formatString, arguments);
            if (_logger.IsInfoEnabled)
                _logger.Info(message);
        }

        public void Debug(string formatString, params object[] arguments)
        {
            string message = string.Format(formatString, arguments);
            if (_logger.IsDebugEnabled)
                _logger.Debug(message);
        }
    }
}
