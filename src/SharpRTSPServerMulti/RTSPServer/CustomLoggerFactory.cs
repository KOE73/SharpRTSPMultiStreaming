using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace SharpRTSPServerMulti
{
    public class CustomLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        { }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger();
        }

        public void Dispose()
        { }
    }
}
