using NUnit.Framework;

namespace Tests
{
    public class TestWithCleanup
    {
        [OneTimeTearDown]
        public static void TryCleanUp()
        {
            TestUtils.TryCleanDirectory(TestUtils.TempFolderPath).Wait();
        }
    }
}
