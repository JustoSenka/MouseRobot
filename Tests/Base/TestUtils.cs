﻿using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using Robot.Analytics;
using Robot.Analytics.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace Tests
{
    [TestFixture]
    public static class TestUtils
    {
        public static string TempFolderPath => Path.Combine(Path.GetTempPath(), "MProjects");
        public static string GenerateProjectPath() => Path.Combine(TempFolderPath, Guid.NewGuid().ToString().Substring(0, 11));

        public static IUnityContainer ConstructContainerForTests(bool passStaticDependencies = true)
        {
            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RobotEditor.Program.RegisterInterfaces(container);

            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReceiveTrackingID, ReceiveTrackingIDFromResourcesForTesting>(new ContainerControlledLifetimeManager());

            Logger.Instance = container.Resolve<ILogger>();

            ContainerUtils.PassStaticDependencies(container, typeof(RobotRuntime.Program).Assembly);
            ContainerUtils.PassStaticDependencies(container, typeof(Robot.Program).Assembly);
            ContainerUtils.PassStaticDependencies(container, typeof(RobotEditor.Program).Assembly);

            var reg = container.Resolve<IRegistryEditor>();
            reg.Put(string.Join("", "Ca", "che", "dK", "ey"), new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 });

            return container;
        }

        internal static void CopyAllTemplateScriptsToProjectFolder(IScriptTemplates ScriptTemplates, IAssetManager AssetManager)
        {
            foreach (var name in ScriptTemplates.TemplateNames)
            {
                var script = ScriptTemplates.GetTemplate(name);
                var fileName = ScriptTemplates.GetTemplateFileName(name);
                var filePath = Path.Combine(Paths.AssetsPath, fileName + ".cs");
                filePath = Paths.GetUniquePath(filePath);
                AssetManager.CreateAsset(script, filePath);
            }
        }

        public static Task<bool> TryCleanDirectory(string tempProjectPath)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            return Task.Run(() =>
            {
                if (!Directory.Exists(tempProjectPath))
                    return true;

                foreach (var path in Directory.GetFiles(tempProjectPath, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }
                    catch { }
                }

                foreach (var dir in Directory.GetDirectories(tempProjectPath))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }
                return true;
            });
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

        public static void ModifyScriptAsset(IAssetManager AssetManager, string path)
        {
            var asset = AssetManager.GetAsset(path);
            var value = asset.Load<string>();
            //asset.Value = value.Replace("public int SomeInt { get; set; } = 5;", "public int SomeInt { get; set; } = 5; \n public int SomeInt2 { get; set; } = 10;");
            asset.Value += "// Comment";
            asset.SaveAsset();
        }

        internal static void ReplaceTextInAsset(IAssetManager AssetManager, string assetPath, string regexFilter, string replacement)
        {
            var asset = AssetManager.GetAsset(assetPath);
            var value = asset.Load<string>();
            asset.Value = Regex.Replace(value, regexFilter, replacement);
            asset.SaveAsset();
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

        public static void CheckThatGuidMapIsCorrect(Recording r)
        {
            var allCommands = r.Commands.GetAllNodes(false).Select(node => node.value);
            var areGuidsRegistered = allCommands.Select(c => (Command: c, HasRegisteredGuid: r.HasRegisteredGuid(c.Guid)));

            foreach (var (Command, IsRegistered) in areGuidsRegistered)
                Assert.IsTrue(IsRegistered, $"Command with name {Command.Name} is not registered in guid map {Command.Guid}");
        }
    }
}
