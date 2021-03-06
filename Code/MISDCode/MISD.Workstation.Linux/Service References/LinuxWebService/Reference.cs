﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.269
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MISD.Workstation.Linux.LinuxWebService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PluginMetadata", Namespace="http://schemas.datacontract.org/2004/07/MISD.Core")]
    [System.SerializableAttribute()]
    public partial class PluginMetadata : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AuthorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private MISD.Workstation.Linux.LinuxWebService.IndicatorSettings[] IndicatorsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int VersionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Author {
            get {
                return this.AuthorField;
            }
            set {
                if ((object.ReferenceEquals(this.AuthorField, value) != true)) {
                    this.AuthorField = value;
                    this.RaisePropertyChanged("Author");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileName {
            get {
                return this.FileNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FileNameField, value) != true)) {
                    this.FileNameField = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public MISD.Workstation.Linux.LinuxWebService.IndicatorSettings[] Indicators {
            get {
                return this.IndicatorsField;
            }
            set {
                if ((object.ReferenceEquals(this.IndicatorsField, value) != true)) {
                    this.IndicatorsField = value;
                    this.RaisePropertyChanged("Indicators");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Version {
            get {
                return this.VersionField;
            }
            set {
                if ((this.VersionField.Equals(value) != true)) {
                    this.VersionField = value;
                    this.RaisePropertyChanged("Version");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="IndicatorSettings", Namespace="http://schemas.datacontract.org/2004/07/MISD.Core")]
    [System.SerializableAttribute()]
    public partial class IndicatorSettings : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private MISD.Workstation.Linux.LinuxWebService.DataType DataTypeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FilterStatementField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IndicatorNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.TimeSpan MappingDurationField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PluginNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.TimeSpan StorageDurationField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.TimeSpan UpdateIntervalField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string WorkstationDomainNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public MISD.Workstation.Linux.LinuxWebService.DataType DataType {
            get {
                return this.DataTypeField;
            }
            set {
                if ((this.DataTypeField.Equals(value) != true)) {
                    this.DataTypeField = value;
                    this.RaisePropertyChanged("DataType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FilterStatement {
            get {
                return this.FilterStatementField;
            }
            set {
                if ((object.ReferenceEquals(this.FilterStatementField, value) != true)) {
                    this.FilterStatementField = value;
                    this.RaisePropertyChanged("FilterStatement");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IndicatorName {
            get {
                return this.IndicatorNameField;
            }
            set {
                if ((object.ReferenceEquals(this.IndicatorNameField, value) != true)) {
                    this.IndicatorNameField = value;
                    this.RaisePropertyChanged("IndicatorName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.TimeSpan MappingDuration {
            get {
                return this.MappingDurationField;
            }
            set {
                if ((this.MappingDurationField.Equals(value) != true)) {
                    this.MappingDurationField = value;
                    this.RaisePropertyChanged("MappingDuration");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PluginName {
            get {
                return this.PluginNameField;
            }
            set {
                if ((object.ReferenceEquals(this.PluginNameField, value) != true)) {
                    this.PluginNameField = value;
                    this.RaisePropertyChanged("PluginName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.TimeSpan StorageDuration {
            get {
                return this.StorageDurationField;
            }
            set {
                if ((this.StorageDurationField.Equals(value) != true)) {
                    this.StorageDurationField = value;
                    this.RaisePropertyChanged("StorageDuration");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.TimeSpan UpdateInterval {
            get {
                return this.UpdateIntervalField;
            }
            set {
                if ((this.UpdateIntervalField.Equals(value) != true)) {
                    this.UpdateIntervalField = value;
                    this.RaisePropertyChanged("UpdateInterval");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string WorkstationDomainName {
            get {
                return this.WorkstationDomainNameField;
            }
            set {
                if ((object.ReferenceEquals(this.WorkstationDomainNameField, value) != true)) {
                    this.WorkstationDomainNameField = value;
                    this.RaisePropertyChanged("WorkstationDomainName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DataType", Namespace="http://schemas.datacontract.org/2004/07/MISD.Core")]
    public enum DataType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        String = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Float = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Int = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Byte = 3,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PluginFile", Namespace="http://schemas.datacontract.org/2004/07/MISD.Core")]
    [System.SerializableAttribute()]
    public partial class PluginFile : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileAsBase64Field;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileAsBase64 {
            get {
                return this.FileAsBase64Field;
            }
            set {
                if ((object.ReferenceEquals(this.FileAsBase64Field, value) != true)) {
                    this.FileAsBase64Field = value;
                    this.RaisePropertyChanged("FileAsBase64");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileName {
            get {
                return this.FileNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FileNameField, value) != true)) {
                    this.FileNameField = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="LinuxWebService.IWorkstationWebService")]
    public interface IWorkstationWebService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/GetMainUpdateInterval", ReplyAction="http://tempuri.org/IWorkstationWebService/GetMainUpdateIntervalResponse")]
        System.TimeSpan GetMainUpdateInterval();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/SignIn", ReplyAction="http://tempuri.org/IWorkstationWebService/SignInResponse")]
        bool SignIn(string workstationName, byte operatingSystem);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/SignOut", ReplyAction="http://tempuri.org/IWorkstationWebService/SignOutResponse")]
        bool SignOut(string workstationName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/GetPluginList", ReplyAction="http://tempuri.org/IWorkstationWebService/GetPluginListResponse")]
        MISD.Workstation.Linux.LinuxWebService.PluginMetadata[] GetPluginList(string workstationDomainName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/DownloadPlugins", ReplyAction="http://tempuri.org/IWorkstationWebService/DownloadPluginsResponse")]
        MISD.Workstation.Linux.LinuxWebService.PluginFile[] DownloadPlugins(string workstationDomainName, string[] pluginNames);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/UploadIndicatorValues", ReplyAction="http://tempuri.org/IWorkstationWebService/UploadIndicatorValuesResponse")]
        bool UploadIndicatorValues(string workstationDomainName, string pluginName, System.Tuple<string, object, MISD.Workstation.Linux.LinuxWebService.DataType, System.DateTime>[] indicatorValues);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/GetFilters", ReplyAction="http://tempuri.org/IWorkstationWebService/GetFiltersResponse")]
        System.Tuple<string, string>[] GetFilters(string workstationDomainName, string pluginName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWorkstationWebService/GetUpdateIntervals", ReplyAction="http://tempuri.org/IWorkstationWebService/GetUpdateIntervalsResponse")]
        System.Tuple<string, System.Nullable<long>>[] GetUpdateIntervals(string workstationDomainName, string pluginName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWorkstationWebServiceChannel : MISD.Workstation.Linux.LinuxWebService.IWorkstationWebService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WorkstationWebServiceClient : System.ServiceModel.ClientBase<MISD.Workstation.Linux.LinuxWebService.IWorkstationWebService>, MISD.Workstation.Linux.LinuxWebService.IWorkstationWebService {
        
        public WorkstationWebServiceClient() {
        }
        
        public WorkstationWebServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WorkstationWebServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WorkstationWebServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WorkstationWebServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.TimeSpan GetMainUpdateInterval() {
            return base.Channel.GetMainUpdateInterval();
        }
        
        public bool SignIn(string workstationName, byte operatingSystem) {
            return base.Channel.SignIn(workstationName, operatingSystem);
        }
        
        public bool SignOut(string workstationName) {
            return base.Channel.SignOut(workstationName);
        }
        
        public MISD.Workstation.Linux.LinuxWebService.PluginMetadata[] GetPluginList(string workstationDomainName) {
            return base.Channel.GetPluginList(workstationDomainName);
        }
        
        public MISD.Workstation.Linux.LinuxWebService.PluginFile[] DownloadPlugins(string workstationDomainName, string[] pluginNames) {
            return base.Channel.DownloadPlugins(workstationDomainName, pluginNames);
        }
        
        public bool UploadIndicatorValues(string workstationDomainName, string pluginName, System.Tuple<string, object, MISD.Workstation.Linux.LinuxWebService.DataType, System.DateTime>[] indicatorValues) {
            return base.Channel.UploadIndicatorValues(workstationDomainName, pluginName, indicatorValues);
        }
        
        public System.Tuple<string, string>[] GetFilters(string workstationDomainName, string pluginName) {
            return base.Channel.GetFilters(workstationDomainName, pluginName);
        }
        
        public System.Tuple<string, System.Nullable<long>>[] GetUpdateIntervals(string workstationDomainName, string pluginName) {
            return base.Channel.GetUpdateIntervals(workstationDomainName, pluginName);
        }
    }
}
