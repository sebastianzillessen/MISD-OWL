﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MISD.Server.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=acid.visus.uni-stuttgart.de;Initial Catalog=MISD;Persist Security Inf" +
            "o=True;User ID=stupro;Password=C0mplex")]
        public string MISDConnectionString {
            get {
                return ((string)(this["MISDConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins/")]
        public string PluginPath {
            get {
                return ((string)(this["PluginPath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=acid.visus.uni-stuttgart.de;Initial Catalog=MISD;User ID=stupro;Passw" +
            "ord=C0mplex")]
        public string MISDConnectionString1 {
            get {
                return ((string)(this["MISDConnectionString1"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("smtp.gmail.com")]
        public string MailHost {
            get {
                return ((string)(this["MailHost"]));
            }
            set {
                this["MailHost"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("587")]
        public int MailPort {
            get {
                return ((int)(this["MailPort"]));
            }
            set {
                this["MailPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("misd.owl.mailer@gmail.com")]
        public string MailAdressFrom {
            get {
                return ((string)(this["MailAdressFrom"]));
            }
            set {
                this["MailAdressFrom"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("stupr[]M1SD")]
        public string MailAdressFromPW {
            get {
                return ((string)(this["MailAdressFromPW"]));
            }
            set {
                this["MailAdressFromPW"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DebugMode {
            get {
                return ((bool)(this["DebugMode"]));
            }
            set {
                this["DebugMode"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Workstation\\Windows")]
        public string PluginPathWorkstationWindows {
            get {
                return ((string)(this["PluginPathWorkstationWindows"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Workstation\\Linux")]
        public string PluginPathWorkstationLinux {
            get {
                return ((string)(this["PluginPathWorkstationLinux"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Cluster\\Bright")]
        public string PluginPathClusterBright {
            get {
                return ((string)(this["PluginPathClusterBright"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Cluster\\HPC")]
        public string PluginPathClusterHPC {
            get {
                return ((string)(this["PluginPathClusterHPC"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Server")]
        public string PluginPathServer {
            get {
                return ((string)(this["PluginPathServer"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Default")]
        public string DefaultOU {
            get {
                return ((string)(this["DefaultOU"]));
            }
            set {
                this["DefaultOU"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Program Files (x86)\\MISD")]
        public string MISDDirectory {
            get {
                return ((string)(this["MISDDirectory"]));
            }
            set {
                this["MISDDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Email\\Templates\\WarningMailTemplate.txt")]
        public string TemplatePath {
            get {
                return ((string)(this["TemplatePath"]));
            }
            set {
                this["TemplatePath"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.00:00:00")]
        public global::System.TimeSpan DailyMailInterval {
            get {
                return ((global::System.TimeSpan)(this["DailyMailInterval"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=ACID;Initial Catalog=MISD;Persist Security Info=True;User ID=sa;Passw" +
            "ord=C0mplex")]
        public string MISDConnectionString2 {
            get {
                return ((string)(this["MISDConnectionString2"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Plugins\\Visualization")]
        public string PluginPathVisualization {
            get {
                return ((string)(this["PluginPathVisualization"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8192")]
        public int CleanerTakeCount {
            get {
                return ((int)(this["CleanerTakeCount"]));
            }
            set {
                this["CleanerTakeCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.00:00:00")]
        public global::System.TimeSpan CleanerJobInterval {
            get {
                return ((global::System.TimeSpan)(this["CleanerJobInterval"]));
            }
            set {
                this["CleanerJobInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:15:00")]
        public global::System.TimeSpan MainUpdateInterval {
            get {
                return ((global::System.TimeSpan)(this["MainUpdateInterval"]));
            }
            set {
                this["MainUpdateInterval"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=acid.visus.uni-stuttgart.de;Initial Catalog=MISD;Integrated Security=" +
            "True")]
        public string MISDConnectionString3 {
            get {
                return ((string)(this["MISDConnectionString3"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MISD OWL - Tagesbericht")]
        public string MailFromDisplayName {
            get {
                return ((string)(this["MailFromDisplayName"]));
            }
            set {
                this["MailFromDisplayName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Tagesbericht")]
        public string DailyMailSubject {
            get {
                return ((string)(this["DailyMailSubject"]));
            }
            set {
                this["DailyMailSubject"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Warnung")]
        public string WarningMailSubject {
            get {
                return ((string)(this["WarningMailSubject"]));
            }
            set {
                this["WarningMailSubject"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MISD OWL - Warnung")]
        public string WarningMailFromDisplayName {
            get {
                return ((string)(this["WarningMailFromDisplayName"]));
            }
            set {
                this["WarningMailFromDisplayName"] = value;
            }
        }
    }
}