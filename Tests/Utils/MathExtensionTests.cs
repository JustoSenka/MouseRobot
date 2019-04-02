using NUnit.Framework;
using RobotRuntime;

namespace Tests.Utils
{
    [TestFixture]
    public class MathExtensionTests
    {
        [TestCase(new[] { 5, 2, 4, 3}, ExpectedResult = true)]
        [TestCase(new[] { 9, 8, 5, 6, 7}, ExpectedResult = true)]
        [TestCase(new[] { 0, 1, 2, 3, 4}, ExpectedResult = true)]
        public bool TheseNumbers_ShouldBe_Consecutive(int[] numbers)
        {
            return numbers.AreNumbersConsecutive();
        }

        [TestCase(new[] { 5, 2, 3, 6 }, ExpectedResult = false)]
        [TestCase(new[] { 5, 2, 3, 6, 9 }, ExpectedResult = false)]
        [TestCase(new[] { 2, 3, 4, 3, 2 }, ExpectedResult = false)]
        public bool TheseNumbers_ShouldNotBe_Consecutive(int[] numbers)
        {
            return numbers.AreNumbersConsecutive();
        }
    }
}
