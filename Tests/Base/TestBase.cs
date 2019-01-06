using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace Tests
{
    public static class TestBase
    {
        public static IUnityContainer ConstructContainerForTests()
        {
            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RobotEditor.Program.RegisterInterfaces(container);

            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            return container;
        }

        public static bool TryCleanDirectory(string tempProjectPath)
        {
            if (!Directory.Exists(tempProjectPath))
                return true;

            try
            {
                Directory.Delete(tempProjectPath, true);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void CheckThatGuidsAreNotSame(Recording s1, Recording s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreNotEqual(s1.Guid, s2.Guid, $"Recording guids should not be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreNotEqual(zip.a.value.Guid, zip.b.value.Guid, $"Command guids in recording not should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatGuidsAreSame(Recording s1, Recording s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreEqual(s1.Guid, s2.Guid, $"Recording guids should be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreEqual(zip.a.value.Guid, zip.b.value.Guid, $"Command guids in recording should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatPtrsAreNotSame(Recording s1, Recording s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreNotSame(s1, s2, $"Recording ptrs should not be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreNotSame(zip.a.value, zip.b.value, $"Command ptrs in recording should not be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatPtrsAreSame(Recording s1, Recording s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreSame(s1, s2, $"Recording ptrs should be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreSame(zip.a.value, zip.b.value, $"Command ptrs in recording should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }
    }
}
