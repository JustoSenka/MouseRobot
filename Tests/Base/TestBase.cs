using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.PropertyUtils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity;
using Unity.Lifetime;

namespace Tests
{
    public static class TestBase
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

            Logger.Instance = container.Resolve<ILogger>();
            container.Resolve<PropertyDependencyProvider>(); // This one is needed, since it provides dependencies to static fields (command Yaml Serialized etc)

            return container;
        }

        internal static void CopyAllTemplateScriptsToProjectFolder(IScriptTemplates ScriptTemplates, IAssetManager AssetManager)
        {
            foreach (var name in ScriptTemplates.TemplateNames)
            {
                var script = ScriptTemplates.GetTemplate(name);
                var fileName = ScriptTemplates.GetTemplateFileName(name);
                var filePath = Path.Combine(Paths.ScriptPath, fileName + ".cs");
                filePath = Paths.GetUniquePath(filePath);
                AssetManager.CreateAsset(script, filePath);
            }
        }

        public static bool TryCleanUp()
        {
            return TryCleanDirectory(TempFolderPath);
        }

        public static bool TryCleanDirectory(string tempProjectPath)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

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

        public static void ModifyScriptAsset(IAssetManager AssetManager, string path)
        {
            var asset = AssetManager.GetAsset(path);
            var value = asset.Importer.Load<string>();
            //asset.Importer.Value = value.Replace("public int SomeInt { get; set; } = 5;", "public int SomeInt { get; set; } = 5; \n public int SomeInt2 { get; set; } = 10;");
            asset.Importer.Value += "// Comment";
            asset.Importer.SaveAsset();
        }

        internal static void ReplaceTextInAsset(IAssetManager AssetManager, string assetPath, string regexFilter, string replacement)
        {
            var asset = AssetManager.GetAsset(assetPath);
            var value = asset.Importer.Load<string>();
            asset.Importer.Value = Regex.Replace(value, regexFilter, replacement);
            asset.Importer.SaveAsset();
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
