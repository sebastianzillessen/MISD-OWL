/*
 * Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
 * Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
 * Arno Schneider, Sebastian Zillessen
 *
 * This file is part of MISD-OWL, a project of the
 * University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
 *
 * MISD-OWL is published under GNU Lesser General Public License Version 3.
 * MISD-OWL is free software, you are allowed to redistribute and/or
 * modify it under the terms of the GNU Lesser General Public License
 * Version 3 or any later version. For details see here:
 * http://www.gnu.org/licenses/lgpl.html
 *
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

using MISD.Core;
using MISD.Client.Model;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    /// <summary>
    /// Contains information about a single indicator for a monitored system.
    /// It belongs to a plugin and a monitored system and contains the filterStatement,
    /// the update interval, the storage duration, the data type and the metric settings.
    /// Each plugin defines these IndicatorSettings for its indicators.
    /// </summary>
    [Serializable]
    public class Indicator : BindableBase
    {
        #region Fields

        private string name;
        private string pluginName;
        private string filterStatement;
        private TimeSpan updateInterval;
        private TimeSpan storageDuration;
        private TimeSpan mappingDuration;
        private DataType dataType;
        private string statementWarning;
        private string statementCritical;
        private IndicatorValue currentValue;
        private ExtendedObservableCollection<IndicatorValue> indicatorValues;
        private string monitoredSystemMAC;
        private MappingState indicatorMapping;

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor of this indicator.
        /// Generally it shouldn't be used.
        /// </summary>
        //public Indicator()
        //{
        //    Initialize();
        //}

        /// <summary>
        /// This constructor sets all attributes of this indicator.
        /// Generally this constructor should be used for the instantiation of a new indicator object.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pluginName"></param>
        /// <param name="filterStatement"></param>
        /// <param name="updateInterval"></param>
        /// <param name="storageDuration"></param>
        /// <param name="mappingDuration"></param>
        /// <param name="dataType"></param>
        /// <param name="statementWarning"></param>
        /// <param name="statementCritical"></param>
        /// <param name="indicatorValues"></param>
        public Indicator(string name, string pluginName, string filterStatement, TimeSpan updateInterval, TimeSpan storageDuration, TimeSpan mappingDuration,
            DataType dataType, string statementWarning, string statementCritical, ExtendedObservableCollection<IndicatorValue> indicatorValues, string monitoredSystemMAC, MappingState indicatorMapping)
        {
            Initialize();

            this.Name = name;
            this.PluginName = pluginName;
            this.FilterStatement = filterStatement;
            this.UpdateInterval = updateInterval;
            this.StorageDuration = storageDuration;
            this.MappingDuration = mappingDuration;
            this.DataType = dataType;
            this.StatementWarning = statementWarning;
            this.StatementCritical = statementCritical;
            this.IndicatorValues = indicatorValues;
            this.MonitoredSystemMAC = monitoredSystemMAC;
            this.indicatorMapping = indicatorMapping;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of this indicator.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the indicator mapping.
        /// </summary>
        public MappingState IndicatorMapping
        {
            get
            {
                return indicatorMapping;
            }
            set
            {
                if (indicatorMapping != value)
                {
                    indicatorMapping = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the plugin name of this indicator.
        /// </summary>
        public string PluginName
        {
            get
            {
                return pluginName;
            }
            set
            {
                if (pluginName != value)
                {
                    pluginName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter statement for this indicator.
        /// </summary>
        public string FilterStatement
        {
            get
            {
                return filterStatement;
            }
            set
            {
                if (filterStatement != value)
                {
                    filterStatement = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the update interval of this indicator.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get
            {
                return updateInterval;
            }
            set
            {
                if (updateInterval != value)
                {
                    updateInterval = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the storage duratio of this indcator.
        /// </summary>
        public TimeSpan StorageDuration
        {
            get
            {
                return storageDuration;
            }
            set
            {
                if (storageDuration != value)
                {
                    storageDuration = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or sets the mapping duration of this indicator.
        /// </summary>
        public TimeSpan MappingDuration
        {
            get
            {
                return mappingDuration;
            }
            set
            {
                if (mappingDuration != value)
                {
                    mappingDuration = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value of this indicator.
        /// </summary>
        public DataType DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                if (dataType != value)
                {
                    dataType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning statement of this indicator.
        /// </summary>
        public string StatementWarning
        {
            get
            {
                return statementWarning;
            }
            set
            {
                if (statementWarning != value)
                {
                    statementWarning = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the critical statement of this indicator.
        /// </summary>
        public string StatementCritical
        {
            get
            {
                return statementCritical;
            }
            set
            {
                if (statementCritical != value)
                {
                    statementCritical = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value of this indicator.
        /// </summary>
        public IndicatorValue CurrentValue
        {
            get
            {
                return this.currentValue;
            }
            set
            {
                if (this.currentValue != value)
                {
                    this.currentValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets all cached indicator values of this indicator.
        /// </summary>
        public ExtendedObservableCollection<IndicatorValue> IndicatorValues
        {
            get
            {
                if (this.indicatorValues == null) this.indicatorValues = new ExtendedObservableCollection<IndicatorValue>();
                return this.indicatorValues;
            }
            set
            {
                if (this.indicatorValues != value)
                {
                    this.indicatorValues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the monitoredSystemMAC of this indicator.
        /// </summary>
        public string MonitoredSystemMAC
        {
            get
            {
                return this.monitoredSystemMAC;
            }
            set
            {
                if (this.monitoredSystemMAC != value)
                {
                    this.monitoredSystemMAC = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            this.PropertyChanged += Indicator_PropertyChanged;
        }

        private void Indicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    switch (e.PropertyName)
                    {
                        case "Name":
                            /*Do nothing*/
                            break;
                        case "PluginName":
                            /*Do nothing*/
                            break;
                        case "DataType":
                            /*Do nothing*/
                            break;
                        case "CurrentValue":
                            /*Do nothing*/
                            break;
                        case "IndicatorValues":
                            /*Do nothing*/
                            break;
                        default:
                            WorkerThread workerThreadStatementCritical = ThreadManager.CreateWorkerThread("BackSync_Indicator.StatementCitical", () =>
                            {
                                List<IndicatorSettings> paramList = new List<IndicatorSettings>();
                                paramList.Add(new IndicatorSettings(this.PluginName, this.Name, this.MonitoredSystemMAC, this.FilterStatement, this.UpdateInterval, this.StorageDuration, this.MappingDuration, this.DataType, this.StatementWarning, this.StatementCritical));
                                bool result = DataModel.Instance.SyncBackIndicator(paramList);
                            }, false);
                            break;
                    }
                }
                stackTrace = null;
            }
        }

        #endregion
    }
}

