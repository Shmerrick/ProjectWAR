using Common;
using Xunit;

namespace LauncherServer.Tests
{
    public class FastRandomTests
    {
        [Fact]
        public void FastAbs_HandlesIntMinValue()
        {
            int result = FastRandom.fastAbs(int.MinValue);
            Assert.True(result >= 0);
        }

        [Fact]
        public void RandomIntAbs_NonNegative()
        {
            var rand = new FastRandom(12345);
            for (int i = 0; i < 1000; i++)
                Assert.True(rand.randomIntAbs() >= 0);
        }

        [Fact]
        public void RandomIntAbsRange_NonNegative()
        {
            var rand = new FastRandom(54321);
            for (int i = 0; i < 1000; i++)
                Assert.True(rand.randomIntAbs(100) >= 0);
        }
    }
}
