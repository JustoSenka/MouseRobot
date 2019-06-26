using NUnit.Framework;
using RobotRuntime;

namespace Tests.Utils
{
    [TestFixture]
    public class TesseractStringComparerTests
    {
        [TestCase("testing", "TESTING", 1f, true)]
        [TestCase("testing", "tesking", 0.9f, false)]
        [TestCase("testing", "tesking", 0.8f, true)]
        [TestCase("some strings with multiple spelling mistakes", "some sting wiih multiple speiing mistkaes", 0.9f, false)]
        [TestCase("some strings with multiple spelling mistakes", "some sting wiih multiple speiing mistkaes", 0.8f, true)]
        [TestCase("some string", "different integer", 0.3f, false)]
        [TestCase("some string", "different integer", 0.2f, true)] // 20% of letters are the same apparently
        [TestCase("Sleep Options", "SleepOptiors", 1f, false)] 
        [TestCase("Sleep Options", "SleepOptiors", 0.9f, true)]
        public void ComparingStrings_WithSpellingMistakes_StillRecognizesThem(string x, string y, float threshold, bool expectedResult)
        {
            var comparer = new TesseractStringEqualityComparer(threshold);
            var actualResult = comparer.Equals(x, y);
            Assert.AreEqual(expectedResult, actualResult, "String comparer failed");
        }
    }
}
