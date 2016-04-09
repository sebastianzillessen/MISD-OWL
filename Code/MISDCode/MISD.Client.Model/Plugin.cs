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

using MISD.Core;
using MISD.Client.Managers;

namespace MISD.Client.Model
{
    /// <summary>
    /// Contains information about a plugin without containing the plugin file itself.
    /// </summary>
    [Serializable]
    public class Plugin : BindableBase
    {
        #region Fields

        private Version version;
        private string name;
        private string description;
        private string copyright;
        private string fileName;
        private string company;
        private string product;
        private string platform;
        private ExtendedObservableCollection<Indicator> indicators;
        private string mainValue;
        private MappingState currentMapping = MappingState.OK;

        #endregion

        public override string ToString()
        {
            return this.Name;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the plugins mapping state.
        /// </summary>
        public MappingState CurrentMapping
        {
            get
            {
                return currentMapping;
            }
            set
            {
                if (this.currentMapping != value)
                {
                    currentMapping = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the plugin version.
        /// </summary>
        public Version Version
        {
            get
            {
                return version;
            }
            set
            {
                if (this.version != value)
                {
                    this.version = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the plugin name.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Company
        {
            get
            {
                return company;
            }
            set
            {
                if (this.company != value)
                {
                    this.company = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (this.fileName != value)
                {
                    this.fileName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Copyright
        {
            get
            {
                return copyright;
            }
            set
            {
                if (this.copyright != value)
                {
                    this.copyright = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Product
        {
            get
            {
                return product;
            }
            set
            {
                if (this.product != value)
                {
                    this.product = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Platform
        {
            get
            {
                return this.platform;
            }
            set
            {
                if (this.platform != value)
                {
                    this.platform = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the indicators of this plugin.
        /// </summary>
        public ExtendedObservableCollection<Indicator> Indicators
        {
            get
            {
                if (this.indicators == null) this.indicators = new ExtendedObservableCollection<Indicator>();
                return this.indicators;
            }
            set
            {
                if (this.indicators != value)
                {
                    if (this.indicators != null)
                    {
                        this.indicators.CollectionChanged -= value_CollectionChanged;
                    }
                    if (value != null)
                    {
                        value.CollectionChanged += value_CollectionChanged;
                    }
                    this.indicators = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged("PluginMapping");
                }
            }
        }

        private void value_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("PluginMapping");
        }

        public MappingState PluginMapping
        {
            get
            {
                if (this.Indicators.Count == 0) return MappingState.OK;
                return (MappingState)this.Indicators.Max(p => (byte)p.IndicatorMapping);
            }
        }

        /// <summary>
        /// Gets and sets the main value of this monitored System.
        /// </summary>
        public string MainValue
        {
            get
            {
                return this.mainValue;
            }
            set
            {
                if (this.mainValue != value)
                {
                    this.mainValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public ITileCustomUI CustomUI
        {
            get
            {
                var customUI = (from p in PluginManager.Instance.TileCustomUIs
                                where p.GetType().Name == (this.Name + "TileCustomUI")
                                select Activator.CreateInstance(p.GetType())).FirstOrDefault();
                return (ITileCustomUI)customUI;
            }
        }

        public int SortingProperty
        {
            get
            {
                return LayoutManager.Instance.PluginPriority.IndexOf(Name);
            }
            private set
            {
            }
        }

        #endregion
    }
}
