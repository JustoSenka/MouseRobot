﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Robot.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Robot.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;Project ToolsVersion=&quot;15.0&quot; xmlns=&quot;http://schemas.microsoft.com/developer/msbuild/2003&quot;&gt;
        ///  &lt;Import Project=&quot;$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props&quot; Condition=&quot;Exists(&apos;$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props&apos;)&quot; /&gt;
        ///  &lt;PropertyGroup&gt;
        ///    &lt;Configuration Condition=&quot; &apos;$(Configuration)&apos; == &apos;&apos; &quot;&gt;Debug&lt;/Configuration&gt;
        ///    &lt;Platform Condition=&quot; &apos;$(Platform)&apos; == &apos;&apos; &quot;&gt;AnyCPU&lt;/Platform&gt;
        ///    &lt;ProjectGuid&gt;{{{0}}}&lt;/ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ProjectTemplate {
            get {
                return ResourceManager.GetString("ProjectTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Software\Rotesting\MouseRobot.
        /// </summary>
        internal static string RegistryRoot {
            get {
                return ResourceManager.GetString("RegistryRoot", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to V1hwT1lWWXdOVWhYYkVrOQ==.
        /// </summary>
        internal static string Secret1 {
            get {
                return ResourceManager.GetString("Secret1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to l0IkYYuav8175TsCXP+E9Q==.
        /// </summary>
        internal static string Secret2 {
            get {
                return ResourceManager.GetString("Secret2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to l0IkYYuav82XcrRYJhWVBg==.
        /// </summary>
        internal static string Secret3 {
            get {
                return ResourceManager.GetString("Secret3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Microsoft Visual Studio Solution File, Format Version 12.00
        ///# Visual Studio 15
        ///VisualStudioVersion = 15.0.28010.2026
        ///MinimumVisualStudioVersion = 10.0.40219.1
        ///Project(&quot;{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}&quot;) = &quot;{0}&quot;, &quot;{1}&quot;, &quot;{{{2}}}&quot;
        ///EndProject
        ///Global
        ///	GlobalSection(SolutionConfigurationPlatforms) = preSolution
        ///		Debug|Any CPU = Debug|Any CPU
        ///		Release|Any CPU = Release|Any CPU
        ///	EndGlobalSection
        ///	GlobalSection(ProjectConfigurationPlatforms) = postSolution
        ///		{{{2}}}.Debug|Any CPU.ActiveCfg = D [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SolutionTemplate {
            get {
                return ResourceManager.GetString("SolutionTemplate", resourceCulture);
            }
        }
    }
}
