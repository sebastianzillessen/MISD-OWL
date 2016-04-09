using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using MISD.Client.Managers;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    [Serializable]
    public enum LayoutCommand
    {
        PROPERTY_CHANGED,
        OU_VALUE_CHANGED,
        MS_VALUE_CHANGED,
        MS_STATE_CHANGED,
        OU_STATE_CHANGED,
        UPDATE_COMPLETE_LAYOUT,
        UPDATE_DATAMODEL_ELEMENTS,
        PRIORITY_LIST_CHANGED
    }
    [Serializable]
    public class LayoutChangeCommand
    {
        #region private variables
        private int id;
        private string data;
        #endregion

        #region properties
        /// <summary>
        /// Returns the CommandType of this command
        /// </summary>
        public LayoutCommand CommandType
        {
            get;
            private set;
        }
        /// <summary>
        /// Used to set or get the ID which is stored in the database to the object which is modified by this command. 
        /// 
        /// For Get: If no ID is set or the CommandType is not in OU_VALUE_CHANGED, MS_VALUE_CHANGED, MS_STATE_CHANGED,OU_STATE_CHANGED 
        /// a Argument Exception will be raised
        /// For Set: You can only set the ID if the Command Type is in OU_VALUE_CHANGED, MS_VALUE_CHANGED, MS_STATE_CHANGED,OU_STATE_CHANGED .
        /// Make sure you set it before.
        /// </summary>
        public int ID
        {
            get
            {
                if (id == null || CommandType == LayoutCommand.PROPERTY_CHANGED || CommandType == LayoutCommand.UPDATE_COMPLETE_LAYOUT)
                {
                    throw new ArgumentException("This ChangeCommand has no ID");
                }
                else
                {
                    return id;
                }
            }
            private set
            {
                if (CommandType == LayoutCommand.PROPERTY_CHANGED || CommandType == LayoutCommand.UPDATE_COMPLETE_LAYOUT)
                {
                    throw new ArgumentException("This ChangeCommand has no ID");
                }
                else
                {
                    this.id = value;
                }
            }
        }
        #endregion

        #region Constructors for each type
        private LayoutChangeCommand()
        {
        }

        public LayoutChangeCommand(int ID, MonitoredSystemState state)
        {
            CommandType = LayoutCommand.MS_STATE_CHANGED;
            this.ID = ID;
            data = state.ToString();
        }

        public LayoutChangeCommand(int ID, bool open)
        {
            CommandType = LayoutCommand.OU_STATE_CHANGED;
            this.ID = ID;
            data = Convert.ToString(open);
        }

        public LayoutChangeCommand(KeyValuePair<string, string> p)
        {
            CommandType = LayoutCommand.PROPERTY_CHANGED;
            data = p.Key + "---###---" + p.Value;
        }

        public LayoutChangeCommand(int ID, bool isOu, float value)
        {
            if (isOu)
                CommandType = LayoutCommand.OU_VALUE_CHANGED;
            else
                CommandType = LayoutCommand.MS_VALUE_CHANGED;
            this.ID = ID;
            data = Convert.ToString(value);
        }


        public LayoutChangeCommand(MISD.Client.Model.Layout layout)
        {
            CommandType = LayoutCommand.UPDATE_COMPLETE_LAYOUT;

            string s = LayoutManager.Instance.SerializeBase64(layout);
            data = s;
        }

        public LayoutChangeCommand(ExtendedObservableCollection<TileableElement> elements)
        {
            CommandType = LayoutCommand.UPDATE_DATAMODEL_ELEMENTS;

            data = DataModelManager.Instance.SerializeBase64(elements);
        }

        public LayoutChangeCommand(ExtendedObservableCollection<string> pluginPriority)
        {
            CommandType = LayoutCommand.PRIORITY_LIST_CHANGED;
            data = LayoutManager.Instance.SerializeBase64(pluginPriority);
        }

        #endregion

        #region TypeConverter for each type
        public MonitoredSystemState MSState()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.MS_STATE_CHANGED)
                return new MonitoredSystemState(data);
            else
                throw new ArgumentException("This is no MS_STATE_CHANGED Command");
        }

        public bool OUState()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.OU_STATE_CHANGED)
                return Convert.ToBoolean(data);
            else
                throw new ArgumentException("This is no OU_STATE_CHANGED Command");
        }

        public KeyValuePair<string, string> Property()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.PROPERTY_CHANGED)
            {
                try
                {
                    string[] stringSeparators = new string[] { "---###---" };
                    string[] splitted = data.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    return new KeyValuePair<string, string>(splitted[0], splitted[1]);
                }
                catch
                {
                    return new KeyValuePair<string, string>("", "");
                }
            }
            else
                throw new ArgumentException("This is no POPERTY_CHANGED Command");
        }

        public float OUValue()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.OU_VALUE_CHANGED)
                return (float)Convert.ToDouble(data);
            else
                throw new ArgumentException("This is no MS_VALUE_CHANGED Command");
        }

        public float MSValue()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.MS_VALUE_CHANGED)
                return (float)Convert.ToDouble(data);
            else
                throw new ArgumentException("This is no MS_VALUE_CHANGED Command");
        }

        public Layout GetLayout()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.UPDATE_COMPLETE_LAYOUT)
            {
                return LayoutManager.Instance.DeserializeBase64(this.data) as Layout;
            }
            else
                throw new ArgumentException("This is no UPDATE_COMPLETE_LAYOUT Command");
        }

        public ExtendedObservableCollection<string> GetPluginPriorityList()
        {
            if (data != null && !data.Equals("") && CommandType == LayoutCommand.PRIORITY_LIST_CHANGED)
            {
                return LayoutManager.Instance.DeserializeBase64(this.data) as ExtendedObservableCollection<string>;
            }
            else
            {
                throw new ArgumentException("This is no PRIORITY_LIST_CHANGED Command");
            }
        }

        #endregion


        public override string ToString()
        {
            return this.CommandType + " Data: " + data;

        }
    }
}
