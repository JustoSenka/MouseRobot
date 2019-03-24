# MouseRobot

| Appveyor      | 
| ------------- |
|[![Build status](https://ci.appveyor.com/api/projects/status/uvt7pre02lvo4uh7/branch/master?svg=true)](https://ci.appveyor.com/project/JustoSenka/mouserobot/branch/master)|

Mouse Robot is a desktop application for building automated UI tests. It can also be used to create automated bots for doing various menial tasks, playing browser games, launching builds, clicking on buttons or links. Its functionality is based on simulating mouse and keyboard input.

![Main window with visible Hierarhcy, Test Runner, Assets window, Inspector window, Settings window and Console window](https://github.com/JustoSenka/MouseRobot/blob/master/Resources/Screenshot_01.png?raw=true)

## Feature List

+ Record, Replay and Save tests using fully featured editor with GUI.
+ C# scripting support for custom solutions and extensibility.
+ Integration with Microsoft Visual Studio IDE.
+ Image recognition based on Deep Learning Algorithms.
+ Full control over mouse and keyboard input.
+ Command line integration to run tests from CI.
+ Currently supported only on MS Windows OS.

## Windows

### Hierarchy

Main window where recorded commands appear. Commands from Hierarchy can be saved into into file, called Recording (.mrb) which is a list of commands used as a template or prefab for common operations. Used to prototype tests, run them and serialize. Supports several recordings being open. Commands can be draged and droped from one recording to another.

Right click has options to create new recordings, save existing, change active recording or to create commands.

Commands do one particular action:

+ Command Move: Will move mouse cursor to a specific coordinates or image if it is nested under image command.
+ Command For Image: Will find an image by reference, and will force all nested commands under it to use image coordinates for their actions.
+ Command Write Text: Will simulate keyboard presses to write specified text onto whatever is selected. etc..

### Test Fixture Window

This windows is created once Test Fixture (.mrt) is opened or new one is created (ctrl + T).
+ A Test Fixture is a collection of tests with Setup and Teardown methods.
+ A Test is basically a collection of commands. A command in a test can reference and execute a serialized Recording. The same Recording can be reused in many different tests or setup methods.

Test Fixture Window can load, save test fixture. Via right click context menu new tests and new commands can be created. Test Fixture can be saved with Ctrl + S if the window currently has the focus. Asterisk * sign is shown for dirty tests. Closing the window will not save Test Fixtures automatically.

After Test Fixture is saved, Test Runner window will be updated automatically.

### Assets

Assets window represents project root folder from file system. Files can be renamed and moved around freely in the window. This window is also representation of AssetManager, which is responsible of collecting all files in the project and importing them into application understandable format. Imported assets can be referenced by commands in Hierarchy and Tests. References are based on Asset Guids and will not break when base file is renamed or moved to different position.

Right click has options to create folders, create template scripts, show file in explorer

### Inspector

Inspector is used to inspect and change values on selected object. Values are saved automatically once edit is finished or focus is given away to other window. It is based on standard MS PropertyGridView. Objects can have custom representation if they have attribute [PropertyDesignerType("DesignerClassName")] next to their class name.

### Test Runner

Contains a list of all tests in the project. Tests can be run from here using double click, right click or menu items. Shows status of test run withing application lifetime:
+ Blue: Test was not run.
+ Red: Test failed.
+ Green: Test passed

Before each test run, test Setup and Teardowns are executed. Before each Test Fixture run, TestFixtureSetup and TestFixtureTeardown are executed.

### Settings window

Shows various settings, settings type can be changed from right click context menu or via main menu -> Windows. Currently has: 
+ Recording settings. Key bindings, which you can use to create specific commands, take screenshots while in recording mode.
+ Image Recogniction Settings. Image recognition algorithm selections and framerate settings.
+ Compiler Settings: Additional compiler references can be added here if your scripts rely on external libraries.

All settings are saved in %AppData%/MouseRobot folder.

### Console

Shows errors, warnings and logs from application or user scripts if they use Logger.Log. Selecting message will show more information about the log if it has some. Right click will show copyable stacktrace information.

### Profiler

Will show how long certain operations took from application or user scripts if they use Profiler.Start. 

## Scripting Integration

Added C# scripts (.cs) to project directory will be compiled into single assembly (Metadata/CustomAssembly.dll). There are some abstract classes and interfaces which when overriden will appear in some parts of the application. For example, all types which implement abstract Command class will appear in Context Menu item 'Create <Command>' in Hierarchy and Test Fixture windows.

Solution and Project files are automatically generated. Saved copies in root will be replaced everytime scripts change.

Most of the scripting is based on MS Entity Framework. If a class in an user script needs to use some manager, it has to ask for it via constructor. For example, if script wants to access AssetManager, it has to have a constructor like this: 

```public SomeClass(IAssetManager AssetManager) {..}```

All managers in this applications can be accessed using this method.

Other classes and interfaces user can implement:

### Command

Is used to describe a single action or operation, like mouse click. Has a single Run method user needs to implement. Doesn't have the context where it is located.

### IRunner

Custom runner can run commands, recording and tests. Takes IRunnable as parameter. Has the context about the location of the IRunnable object. Can access all the managers to get additional information. For example, ForImageCommand is completely empty, but has an ImageCommandRunner which gets bitamp for image from AssetManager and runs all the nested commands while passing image coordinates.

### CommandProperties

Can be used to change how commands are visualized/drawn in the inspector.

### IPaintOnScreen

Has a OnDraw method which can be used to draw things or text on screen. Useful to debug some information on screen, while tests are running. Can draw on any place on screen even if application is not visible, minimized or in different screen.

### FeatureDetector

Is used to find position of image on screen. Can be used to replace default Image Detection solutions. User implementations will appear in Image Detection Settings window.

### BaseSettings

Can be used for storing custom settings. Those settings can be accessed by user scripts using SettingsManager.GetSettings<CustomSettings>(). Settings are automatically save to AppData folder when application exits and are loaded on application startup or after script recompilation. If application quits unexpectedly, settings will not be saved.

Settings can also be saved manually using SettingsManager.SaveSettings

## Command Line Integration

RobotRuntime.exe can be used to load project and run tests from command line. Produces test result report. Supported command line arguments:

+  -p, --projectPath    Required. Project path.
+  -f, --filter         (Default: .) Regex Test Filter for which tests to run.
+  -r, --recording      (Default: ) Run single recording by path.
+  -o, --output         (Default: TestResults.txt) Output file path with extension.
+  --help               Display this help screen.
+  --version            Display version information.

Example: `RobotRuntime.exe -p C:\MyProject -f SomeTestName`
