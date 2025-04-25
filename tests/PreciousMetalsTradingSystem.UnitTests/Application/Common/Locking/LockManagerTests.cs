using PreciousMetalsTradingSystem.Application.Common.Locking;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Common.Locking
{
    public class LockManagerTests
    {
        private readonly ILockManager _lockManager = new LockManager();

        [Fact]
        public async Task Should_AcquireAndReleaseLock_Successfully()
        {
            var lockKey = "TestKey";

            var acquired = await _lockManager.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(1));
            Assert.True(acquired);

            _lockManager.ReleaseLock(lockKey);
        }

        [Fact]
        public async Task Should_TimeoutIfLockNotReleased()
        {
            var lockKey = "TestKey";

            var acquired = await _lockManager.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(1));
            Assert.True(acquired);

            var secondAttempt = await _lockManager.AcquireLockAsync(lockKey, TimeSpan.FromMilliseconds(100));
            Assert.False(secondAttempt); // Should timeout as the first lock is not released

            _lockManager.ReleaseLock(lockKey);
        }

        [Fact]
        public async Task Should_AllowConcurrentLocksForDifferentKeys()
        {
            var lockKey1 = "TestKey1";
            var lockKey2 = "TestKey2";

            var lock1Acquired = await _lockManager.AcquireLockAsync(lockKey1, TimeSpan.FromSeconds(1));
            var lock2Acquired = await _lockManager.AcquireLockAsync(lockKey2, TimeSpan.FromSeconds(1));

            Assert.True(lock1Acquired);
            Assert.True(lock2Acquired);

            _lockManager.ReleaseLock(lockKey1);
            _lockManager.ReleaseLock(lockKey2);
        }
    }
}
