using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace SharpRTSPServerMulti
{
    public class CustomLogger : ILogger
    {
        class CustomLoggerScope<TState> : IDisposable
        {
            public CustomLoggerScope(TState state)
            {
                State = state;
            }
            public TState State { get; }
            public void Dispose()
            { }
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return new CustomLoggerScope<TState>(state);
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch(logLevel)
            {
                case LogLevel.Trace:
                    {
                        if(SharpRTSPServerMulti.Log.TraceEnabled)
                        {
                            SharpRTSPServerMulti.Log.Trace(formatter.Invoke(state, exception));
                        }
                    }
                    break;

                case LogLevel.Debug:
                    {
                        if(SharpRTSPServerMulti.Log.DebugEnabled)
                        {
                            SharpRTSPServerMulti.Log.Debug(formatter.Invoke(state, exception));
                        }
                    }
                    break;

                case LogLevel.Information:
                    {
                        if(SharpRTSPServerMulti.Log.InfoEnabled)
                        {
                            SharpRTSPServerMulti.Log.Info(formatter.Invoke(state, exception));
                        }
                    }
                    break;

                case LogLevel.Warning:
                    {
                        if(SharpRTSPServerMulti.Log.WarnEnabled)
                        {
                            SharpRTSPServerMulti.Log.Warn(formatter.Invoke(state, exception));
                        }
                    }
                    break;

                case LogLevel.Error:
                case LogLevel.Critical:
                    {
                        if(SharpRTSPServerMulti.Log.ErrorEnabled)
                        {
                            SharpRTSPServerMulti.Log.Error(formatter.Invoke(state, exception));
                        }
                    }
                    break;

                default:
                    {
                        Debug.WriteLine($"Unknown trace level: {logLevel}");
                    }
                    break;
            }
        }
    }
}
