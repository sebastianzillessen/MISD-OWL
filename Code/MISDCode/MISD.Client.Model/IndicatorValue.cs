using MISD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Model
{
    [Serializable]
    public class IndicatorValue : BindableBase
    {
        #region Fields

        private object value;
        private DataType dataType;
        private DateTime timestamp;
        private MappingState mappingState;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the indicator value is object.
        /// </summary>
        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the datatype of the indicator
        /// </summary>
        public DataType DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                if (this.dataType != value)
                {
                    this.dataType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the timestamp of the indicator value.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return this.timestamp;
            }

            set
            {
                if (this.timestamp != value)
                {
                    this.timestamp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the mapping state of this indicator value.
        /// </summary>
        public MappingState MappingState
        {
            get
            {
                return mappingState;
            }
            set
            {
                if (this.mappingState != value)
                {
                    this.mappingState = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public IndicatorValue()
        {

        }

        public IndicatorValue(object value, DataType dataType, DateTime timestamp, MappingState mappingState)
        {
            this.Value = value;
            this.DataType = dataType;
            this.Timestamp = timestamp;
            this.MappingState = mappingState;
        }

        #endregion
    }
}
