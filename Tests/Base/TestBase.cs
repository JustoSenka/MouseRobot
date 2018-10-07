using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
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

            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            return container;
        }


        public static void CheckThatGuidsAreNotSame(Script s1, Script s2) 
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreNotEqual(s1.Guid, s2.Guid, $"Script guids should not be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreNotEqual(zip.a.value.Guid, zip.b.value.Guid, $"Command guids in script not should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatGuidsAreSame(Script s1, Script s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreEqual(s1.Guid, s2.Guid, $"Script guids should be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreEqual(zip.a.value.Guid, zip.b.value.Guid, $"Command guids in script should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatPtrsAreNotSame(Script s1, Script s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreNotSame(s1, s2, $"Script ptrs should not be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreNotSame(zip.a.value, zip.b.value, $"Command ptrs in script should not be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }

        public static void CheckThatPtrsAreSame(Script s1, Script s2)
        {
            var s1Nodes = s1.Commands.GetAllNodes(false);
            var s2Nodes = s2.Commands.GetAllNodes(false);

            Assert.AreSame(s1, s2, $"Script ptrs should be the same: {s1.Name}, {s2.Name}");

            foreach (var zip in s1Nodes.Zip(s2Nodes, (a, b) => new { a, b }))
                Assert.AreSame(zip.a.value, zip.b.value, $"Command ptrs in script should be the same: {zip.a.value.Name}, {zip.b.value.Name}");
        }


    }
}
