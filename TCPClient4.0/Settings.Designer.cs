﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TADS文件传输客户端 {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("请在此处配置近端IP地址，类似为192.168.10.2")]
        public string HostIP {
            get {
                return ((string)(this["HostIP"]));
            }
            set {
                this["HostIP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1321")]
        public int HostPort {
            get {
                return ((int)(this["HostPort"]));
            }
            set {
                this["HostPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("请在此处配置远端IP地址，类似为192.168.10.3")]
        public string ClientIP {
            get {
                return ((string)(this["ClientIP"]));
            }
            set {
                this["ClientIP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ClientPort {
            get {
                return ((int)(this["ClientPort"]));
            }
            set {
                this["ClientPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MAxleArch")]
        public string sendSoundFilePath {
            get {
                return ((string)(this["sendSoundFilePath"]));
            }
            set {
                this["sendSoundFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\Syncfiles\\TrainID.txt")]
        public string clientTrainIDPath {
            get {
                return ((string)(this["clientTrainIDPath"]));
            }
            set {
                this["clientTrainIDPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MCurrentData\\")]
        public string receiveFSFilePath {
            get {
                return ((string)(this["receiveFSFilePath"]));
            }
            set {
                this["receiveFSFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MCurrentData")]
        public string sendFSFilePath {
            get {
                return ((string)(this["sendFSFilePath"]));
            }
            set {
                this["sendFSFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MAxleArch\\")]
        public string receiveSoundFilePath {
            get {
                return ((string)(this["receiveSoundFilePath"]));
            }
            set {
                this["receiveSoundFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MSendAxles")]
        public string sendErrorFilePath {
            get {
                return ((string)(this["sendErrorFilePath"]));
            }
            set {
                this["sendErrorFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MSendAxles\\")]
        public string receiveErrorFilePath {
            get {
                return ((string)(this["receiveErrorFilePath"]));
            }
            set {
                this["receiveErrorFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MAxleArch")]
        public string sendDatFilePath {
            get {
                return ((string)(this["sendDatFilePath"]));
            }
            set {
                this["sendDatFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\CHINA_TADS_M\\MAxleArch\\")]
        public string receiveDatFilePath {
            get {
                return ((string)(this["receiveDatFilePath"]));
            }
            set {
                this["receiveDatFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20000")]
        public int TimeOut {
            get {
                return ((int)(this["TimeOut"]));
            }
            set {
                this["TimeOut"] = value;
            }
        }
    }
}