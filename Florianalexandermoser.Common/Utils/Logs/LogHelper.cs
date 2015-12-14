using System;
using System.Collections.Generic;
using Florianalexandermoser.Common.Patterns.Singleton;

namespace Florianalexandermoser.Common.Utils.Logs
{
    public class LogHelper : SingletonBase<LogHelper>
    {
        private List<LogModel> _logs = new List<LogModel>();

        public void Log(LogLevel level, object from, string message, Exception ex = null)
        {
            var lm = new LogModel
            {
                LogLevel = level,
                Message = message
            };

            if (from != null)
                lm.Location = from.GetType().Namespace + "." + from.GetType().Name;

            if (ex != null)
                lm.Message += ex.ToString();

            _logs.Add(lm);
        }

        public void Log(LogLevel level, string from, string message, Exception ex = null)
        {
            var lm = new LogModel
            {
                LogLevel = level,
                Message = message
            };

            if (from != null)
                lm.Location = from;

            if (ex != null)
                lm.Message += ex.ToString();

            _logs.Add(lm);
        }

        public List<LogModel> GetAllLogs()
        {
            var templogs = new List<LogModel>(_logs);
            _logs = new List<LogModel>();
            return templogs;
        }

        public void LogExeption(Exception ex)
        {
            var lm = new LogModel
            {
                LogLevel = LogLevel.FatalError,
                Message = ex.ToString()
            };
            
            _logs.Add(lm);
        }
    }
}
