using Common;
using Xunit;

namespace LauncherServer.Tests
{
    public class HonorCalculationTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(999, 0)]
        [InlineData(HonorCalculation.HONOR_RANK_1, 1)]
        [InlineData(HonorCalculation.HONOR_RANK_2, 2)]
        [InlineData(HonorCalculation.HONOR_RANK_3, 3)]
        [InlineData(HonorCalculation.HONOR_RANK_4, 4)]
        public void CalculatesCorrectRank(int points, int expected)
        {
            var calc = new HonorCalculation();
            Assert.Equal(expected, calc.GetHonorLevel(points));
        }
    }
}
