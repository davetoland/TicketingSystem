using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aareon.Api.Tests.Utilities
{
    public static class MockLoggerValidator
    {
        public static Mock<ILogger> VerifyLogExact(this Mock<ILogger> logger, LogLevel logLevel, string expectedMessage)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
        
        public static Mock<ILogger<T>> VerifyLogExact<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, string expectedMessage)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
        
        public static Mock<ILogger<T>> VerifyLogStartsWith<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, string expectedMessage)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
        
        public static Mock<ILogger> VerifyLogStartsWith(this Mock<ILogger> logger, LogLevel logLevel, string expectedMessage)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
    }
}