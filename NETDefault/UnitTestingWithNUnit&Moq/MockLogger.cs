using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTestingExample;

internal sealed class MockLogger : Mock<ILogger<UserService>>
{
    private MockLogger(string expectedLogMessage)
    {
        Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == expectedLogMessage),
                It.IsAny<NotFoundException>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!))
            .Verifiable();
    }
    
    public static MockLogger Create(string expectedLogMessage) =>
        new (expectedLogMessage);
}
