﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RobotEditor.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RobotEditor.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap CollapseAll_16 {
            get {
                object obj = ResourceManager.GetObject("CollapseAll_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using RobotRuntime;
        ///using System;
        ///using RobotRuntime.Tests;
        ///
        ///namespace CustomNamespace
        ///{
        ///    [Serializable]
        ///    // [RunnerType(typeof(CustomCommandRunner))] // Can also use already implemented types: SimpleCommandRunner etc.
        ///    // [PropertyDesignerType(&quot;CustomCommandDesigner&quot;)] // Optional, will specify how to draw command in inspector
        ///    public class CustomCommand : Command
        ///    {
        ///        // This is what will appear in dropdown in inspector under Command Type. Must be unique
        ///        public ove [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CustomCommand {
            get {
                return ResourceManager.GetString("CustomCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using CustomNamespace;
        ///using Robot.Abstractions;
        ///using RobotEditor.Inspector;
        ///using RobotEditor.Utils;
        ///using System.ComponentModel;
        ///
        ///namespace RobotEditor.Resources.ScriptTemplates
        ///{
        ///    public class CustomCommandDesigner : CommandProperties
        ///    {
        ///        public CustomCommandDesigner(ICommandFactory CommandFactory)
        ///            : base(CommandFactory)
        ///        {
        ///            Properties = TypeDescriptor.GetProperties(this);
        ///        }
        ///
        ///        public override void HideProperties(ref DynamicTypeDes [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CustomCommandDesigner {
            get {
                return ResourceManager.GetString("CustomCommandDesigner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using RobotRuntime.Abstractions;
        ///using RobotRuntime;
        ///using RobotRuntime.Execution;
        ///using RobotRuntime.Tests;
        ///
        ///namespace RobotEditor.Resources.ScriptTemplates
        ///{
        ///    public class CustomCommandRunner : IRunner
        ///    {
        ///        private CommandRunningCallback m_Callback;
        ///
        ///        public CustomCommandRunner()
        ///        {
        ///            // Constructor actually can ask for other managers if needed, like IHierarchyManager etc.
        ///        }
        ///
        ///        public TestData TestData { set; get; }
        ///
        ///        public void P [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CustomCommandRunner {
            get {
                return ResourceManager.GetString("CustomCommandRunner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using RobotRuntime.Graphics;
        ///using System.Collections.Generic;
        ///using System.Drawing;
        ///
        ///namespace RobotEditor.Resources.ScriptTemplates
        ///{
        ///    public class CustomFeatureDetector : FeatureDetector
        ///    {
        ///        public override string Name { get { return &quot;Custom Detector&quot;; } } // Name must be unique. It is used in settings to choose detector
        ///
        ///        public override bool SupportsMultipleMatches { get { return true; } } // If detector cannot match multiple images on screen, set this to false
        ///
        ///         [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CustomFeatureDetector {
            get {
                return ResourceManager.GetString("CustomFeatureDetector", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using RobotEditor.Windows.Base;
        ///using RobotRuntime;
        ///using System;
        ///using System.Drawing;
        ///using System.Windows.Forms;
        ///
        ///namespace RobotEditor.Resources.ScriptTemplates
        ///{
        ///    public class CustomScreenPainter : IPaintOnScreen
        ///    {
        ///        // Constructor can ask for dependencies, IAssetManager, IMouseRobot etc.
        ///        public CustomScreenPainter()
        ///        {
        ///            Invalidate.Invoke();
        ///        }
        ///
        ///        // Call invalidate if you want OnPaint method to be called. Unless it won&apos;t draw anything [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CustomScreenPainter {
            get {
                return ResourceManager.GetString("CustomScreenPainter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using RobotRuntime.Settings;
        ///using System;
        ///using System.Windows.Forms;
        ///
        ///namespace RobotEditor.Resources.ScriptTemplates
        ///{
        ///    [Serializable]
        ///    public class UserSettings : BaseSettings 
        ///    { 
        ///        public int SomeInt { get; set; } = 15;
        ///        public string SomeString { get; set; } = &quot;string&quot;;
        ///        public bool SomeBool { get; set; } = false;
        ///        public Keys SomeKey { get; set; } = Keys.E;
        ///    }
        ///}
        ///.
        /// </summary>
        internal static string CustomSettings {
            get {
                return ResourceManager.GetString("CustomSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ExpandAll_16 {
            get {
                object obj = ResourceManager.GetObject("ExpandAll_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ExpandOne_16 {
            get {
                object obj = ResourceManager.GetObject("ExpandOne_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ExpandOne_Text_16 {
            get {
                object obj = ResourceManager.GetObject("ExpandOne_Text_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Eye_d_ICO_256 {
            get {
                object obj = ResourceManager.GetObject("Eye_d_ICO_256", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Eye_e_ICO_256 {
            get {
                object obj = ResourceManager.GetObject("Eye_e_ICO_256", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogDebug_32 {
            get {
                object obj = ResourceManager.GetObject("LogDebug_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogDebug_d_32 {
            get {
                object obj = ResourceManager.GetObject("LogDebug_d_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogError_32 {
            get {
                object obj = ResourceManager.GetObject("LogError_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogError_d_32 {
            get {
                object obj = ResourceManager.GetObject("LogError_d_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogInfo_32 {
            get {
                object obj = ResourceManager.GetObject("LogInfo_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogInfo_d_32 {
            get {
                object obj = ResourceManager.GetObject("LogInfo_d_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogWarning_32 {
            get {
                object obj = ResourceManager.GetObject("LogWarning_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap LogWarning_d_32 {
            get {
                object obj = ResourceManager.GetObject("LogWarning_d_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Fail_16 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Fail_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Fail_32 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Fail_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_info_16 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_info_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_info_32 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_info_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Pass_16 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Pass_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Pass_32 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Pass_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Q_16 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Q_16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap TestStatus_Q_32 {
            get {
                object obj = ResourceManager.GetObject("TestStatus_Q_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ToolButton_Play_32 {
            get {
                object obj = ResourceManager.GetObject("ToolButton_Play_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ToolButton_Record_32 {
            get {
                object obj = ResourceManager.GetObject("ToolButton_Record_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ToolButton_RecordStop_32 {
            get {
                object obj = ResourceManager.GetObject("ToolButton_RecordStop_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap ToolButton_Stop_32 {
            get {
                object obj = ResourceManager.GetObject("ToolButton_Stop_32", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap X_ICO_256 {
            get {
                object obj = ResourceManager.GetObject("X_ICO_256", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
