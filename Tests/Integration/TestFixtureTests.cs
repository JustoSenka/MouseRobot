﻿using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using Robot.Tests;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class TestFixtureTests : TestWithCleanup
    {
        private string TempProjectPath;

        private const string k_FixtureName = "fixture";
        private const string k_FixturePath = "Assets\\" + k_FixtureName + ".mrt";

        IUnityContainer Container;
        IAssetManager AssetManager;
        private ITestFixtureManager TestFixtureManager;

        private TestFixture TestFixture;

        private LightTestFixture LightTestFixture
        {
            get
            {
                var f = new LightTestFixture();
                f.Name = k_FixtureName;
                f.Setup = new Recording();
                f.TearDown = new Recording();
                f.OneTimeSetup = new Recording();
                f.OneTimeTeardown = new Recording();
                f.Setup.Name = LightTestFixture.k_Setup;
                f.TearDown.Name = LightTestFixture.k_TearDown;
                f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
                f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
                f.Tests = new Recording[] { new Recording(), new Recording() }.ToList();
                return f;
            }
        }

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            TestFixtureManager = Container.Resolve<ITestFixtureManager>();
            TestFixture = Container.Resolve<TestFixture>();

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
        }

        [Test]
        public void AssetIsCreated_For_LightTestFixture()
        {
            var asset = AssetManager.CreateAsset(LightTestFixture, k_FixturePath);
            Assert.IsTrue(File.Exists(asset.Path), "Test fixture asset should have been created at path: " + k_FixturePath);
        }

        [Test]
        public void AssetIsCreated_For_TestFixture()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);
            Assert.IsTrue(File.Exists(asset.Path), "Test fixture asset should have been created at path: " + k_FixturePath);
        }

        [Test]
        public void TestFixture_FromLightTestFixture_ConvertsCorrectlyBack()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var newLightFixture = TestFixture.ToLightTestFixture();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [Test]
        public void LightTestFixture_LoadedFromImporter_IsCorrect()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.ReloadAsset<LightTestFixture>();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [Test]
        public void TestFixture_LoadedFromImporter_AndConverted_IsCorrect()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.ReloadAsset<LightTestFixture>();
            var newTestFixture = Container.Resolve<TestFixture>();
            newTestFixture.ApplyLightFixtureValues(newLightFixture);

            CheckIfTestFixturesAreEqual(TestFixture, newTestFixture);
        }

        [Test]
        public void SavingFixtureVia_TestFixtureManager_ProducesCorrectAsset()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);
            TestFixtureManager.SaveTestFixture(fixture, k_FixturePath);

            var asset = AssetManager.GetAsset(k_FixturePath);
            var newLightFixture = asset.ReloadAsset<LightTestFixture>();
            newLightFixture.Name = lightFixture.Name; // Names are overrided by path they are saved

            CheckIfLightTestFixturesAreEqual(lightFixture, newLightFixture);
        }

        [Test]
        public void SavingFixture_ToPath_WillAdjustTheNameOfFixture()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);

            TestFixtureManager.SaveTestFixture(fixture, k_FixturePath);

            Assert.AreEqual(k_FixtureName, fixture.Name, "Names should be adjusted according to path");
        }

        [Test]
        public void ApplyLightFixtureValues_CorrectlyAppliesFixtureNames()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);

            Assert.AreEqual(lightFixture.Name, fixture.Name, "Names should be the same");
        }

        [Test]
        public void ApplyLightFixtureValues_CorrectlyAppliesFixtureGuid()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            var guid = lightFixture.Guid;

            fixture.ApplyLightFixtureValues(lightFixture);

            Assert.AreEqual(guid, fixture.Guid, "Guids should be the same");
        }

        [Test]
        public void ToLightFixtureValues_CorrectlySetsFixtureGuid()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);

            var guid = fixture.Guid;

            var newLightTestFixture = fixture.ToLightTestFixture();

            Assert.AreEqual(guid, newLightTestFixture.Guid, "Guids should be the same");
        }

        [Test]
        public void TestFixture_EqualsOperator_ForNewFixtures_ReturnsTrue()
        {
            var fixture1 = TestFixtureManager.NewTestFixture();
            var fixture2 = TestFixtureManager.NewTestFixture();

            Assert.IsTrue(fixture1.Similar(fixture2), "Fixture.Equals returned false for similar two new identical fixtures");
        }

        [Test]
        public void TestFixture_EqualsOperator_ForIdenticalFixtures_ReturnsTrue()
        {
            var fixture1 = TestFixtureManager.NewTestFixture();
            var fixture2 = TestFixtureManager.NewTestFixture();

            fixture1.ApplyLightFixtureValues(LightTestFixture);
            fixture2.ApplyLightFixtureValues(LightTestFixture);

            Assert.IsTrue(fixture1.Similar(fixture2), "Fixture.Equals returned false for similar two new identical fixtures");
        }

        [Test]
        public void TestFixture_EqualsOperator_ForFixturesWithDifferentRecordings_ReturnsFalse()
        {
            var fixture1 = TestFixtureManager.NewTestFixture();
            var fixture2 = TestFixtureManager.NewTestFixture();

            fixture1.ApplyLightFixtureValues(LightTestFixture);
            fixture2.ApplyLightFixtureValues(LightTestFixture);

            fixture1.Tests[1].Name = "Some new name";

            Assert.IsFalse(fixture1.Similar(fixture2), "Fixture.Equals returned true for for different fixtures");
        }

        [Test]
        public void TestFixture_EqualsOperator_ForFixturesWithDifferentCommands_ReturnsFalse()
        {
            var fixture1 = TestFixtureManager.NewTestFixture();
            var fixture2 = TestFixtureManager.NewTestFixture();

            fixture1.ApplyLightFixtureValues(LightTestFixture);
            fixture2.ApplyLightFixtureValues(LightTestFixture);

            fixture1.Tests[1].AddCommand(new CommandSleep(5));

            Assert.IsFalse(fixture1.Similar(fixture2), "Fixture.Equals returned true for for different fixtures");
        }

        [Test]
        public void TestFixture_EqualsOperator_ForDifferentFixtures_ReturnsFalse()
        {
            var fixture1 = TestFixtureManager.NewTestFixture();
            var fixture2 = TestFixtureManager.NewTestFixture();

            fixture2.ApplyLightFixtureValues(LightTestFixture);

            Assert.IsFalse(fixture1.Similar(fixture2), "Fixture1.Equals returned true for for different fixtures");
            Assert.IsFalse(fixture2.Similar(fixture1), "Fixture2.Equals returned true for for different fixtures");
        }

        [Test]
        public void TestFixture_FromLightTestFixture_KeepsSameCommandAndRecordingGuids()
        {
            var cachedLightTestFixture = LightTestFixture;

            var fixture1 = TestFixtureManager.NewTestFixture(cachedLightTestFixture);
            var fixture2 = TestFixtureManager.NewTestFixture(cachedLightTestFixture);

            Assert.AreEqual(fixture1.Guid, fixture2.Guid, "Fixtures should have same guids");

            TestUtils.CheckThatGuidsAreSame(fixture1.Setup, fixture2.Setup);
            TestUtils.CheckThatPtrsAreSame(fixture1.Setup, fixture2.Setup);
        }

        private void CheckIfLightTestFixturesAreEqual(LightTestFixture a, LightTestFixture b)
        {
            Assert.AreEqual(a.Name, b.Name, "Names missmatched");
            Assert.AreEqual(a.Setup.Name, b.Setup.Name, "Setup names missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Name, b.OneTimeTeardown.Name, "Teardown names missmatched");
            Assert.AreEqual(a.Setup.Commands.Count(), b.Setup.Commands.Count(), "Setup command count missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Commands.Count(), b.OneTimeTeardown.Commands.Count(), "teardown command count missmatched");
            Assert.AreEqual(a.Tests.Count, b.Tests.Count, "Test count missmatched");
        }

        private void CheckIfTestFixturesAreEqual(TestFixture a, TestFixture b)
        {
            Assert.AreEqual(a.Name, b.Name, "Names missmatched");
            Assert.AreEqual(a.Setup.Name, b.Setup.Name, "Setup names missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Name, b.OneTimeTeardown.Name, "Teardown names missmatched");
            Assert.AreEqual(a.Setup.Commands.Count(), b.Setup.Commands.Count(), "Setup command count missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Commands.Count(), b.OneTimeTeardown.Commands.Count(), "teardown command count missmatched");
            Assert.AreEqual(a.Tests.Count, b.Tests.Count, "Test count missmatched");
        }
    }
}
