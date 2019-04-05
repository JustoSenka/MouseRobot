using RobotRuntime.Tests;

namespace RobotRuntime.Recordings
{
    /// <summary>
    /// Extension methods for Recordings.
    /// Has some FLUENT API
    /// </summary>
    public static class RecordingExtension
    {
        public static Recording With(this Recording r, params Command[] commands)
        {
            foreach (var c in commands)
                r.AddCommand(c);

            return r;
        }

        public static LightTestFixture With(this LightTestFixture f, params Recording[] recs)
        {
            foreach (var r in recs)
                f.AddRecording(r);

            return f;
        }
    }
}
