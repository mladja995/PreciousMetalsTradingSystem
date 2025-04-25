using PreciousMetalsTradingSystem.Application.Common.Behaviors;
using PreciousMetalsTradingSystem.Application.Common.Locking;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Common.Behaviors
{
    public class LockingBehaviorTests
    {
        private readonly Mock<ILockManager> _mockLockManager = new();
        private readonly Mock<ILogger<LockingBehavior<SampleRequest, SampleResponse>>> _mockLogger = new();

        [Fact]
        public async Task Should_AcquireAndReleaseLock_ForLockableRequest()
        {
            var lockKey = "SampleKey";
            var request = new SampleRequest { LockKey = lockKey };
            var response = new SampleResponse();

            _mockLockManager
                .Setup(l => l.AcquireLockAsync(lockKey, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var behavior = new LockingBehavior<SampleRequest, SampleResponse>(_mockLockManager.Object, _mockLogger.Object);
            var result = await behavior.Handle(request, () => Task.FromResult(response), CancellationToken.None);

            Assert.Equal(response, result);

            _mockLockManager.Verify(l => l.AcquireLockAsync(lockKey, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockLockManager.Verify(l => l.ReleaseLock(lockKey), Times.Once);
        }

        [Fact]
        public async Task Should_ThrowException_WhenLockCannotBeAcquired()
        {
            var lockKey = "SampleKey";
            var request = new SampleRequest { LockKey = lockKey };

            _mockLockManager
                .Setup(l => l.AcquireLockAsync(lockKey, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Simulate timeout

            var behavior = new LockingBehavior<SampleRequest, SampleResponse>(_mockLockManager.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(request, () => Task.FromResult(new SampleResponse()), CancellationToken.None));
        }
    }

    public class SampleRequest : ILockable
    {
        public string LockKey { get; set; }

        public string GetLockKey() => LockKey;
    }

    public class SampleResponse { }
}
