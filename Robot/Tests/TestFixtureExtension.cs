using RobotRuntime.Recordings;
using RobotRuntime.Tests;

namespace Robot.Tests
{
    /// <summary>
    /// Extension methods for TestFixtures.
    /// Has some FLUENT API
    /// </summary>
    public static class TestFixtureExtension
    {
        public static TestFixture With(this TestFixture f, params Recording[] recordings)
        {
            foreach (var r in recordings)
                f.AddRecording(r);

            return f;
        }
    }
}
