using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Imaging;

namespace MISD.Client.Model
{
    [Serializable]
    public class Item<K, V>
    {
        public K Key;
        public V Value;

        public override string ToString()
        {
            return Key.ToString() + "   " + Value.ToString();
        }
    }
    [Serializable]
    public class Layout : MISD.Core.Layout
    {

        #region properties

        public override int ID { get { return base.ID; } 
            set { base.ID = value; } }

        public override string UserName { get { return base.UserName; } set { base.UserName = value; } }

        public override DateTime Date { get { return base.Date; } set { base.Date = value; } }

        public override byte[] PreviewImage { get { return base.PreviewImage; } set { base.PreviewImage = value; } }

        public override string Name
        {
            get
            {
                if (base.Name != null)
                {
                    return base.Name;
                }
                else
                {
                    base.Name = System.DateTime.Now.ToString();
                    return base.Name;
                }
            }
            set
            {
                base.Name = value;
            }

        }


        public List<Item<int, float>> ValueMS
        {
            get
            {
                List<Item<int, float>> list = new List<Item<int, float>>();
                foreach (KeyValuePair<string, float> x in value)
                {
                    var s = x.Key.Split('_');
                    if (s[1] == "ms")
                        list.Add(new Item<int, float> { Key = Convert.ToInt32(s[0]), Value = x.Value });
                }
                return list;
            }
            private set
            {
                throw new NotImplementedException();
            }
        }

        public List<Item<int, float>> ValueOU
        {
            get
            {
                List<Item<int, float>> list = new List<Item<int, float>>();
                foreach (KeyValuePair<string, float> x in value)
                {
                    var s = x.Key.Split('_');
                    if (s[1] == "ou")
                        list.Add(new Item<int, float> { Key = Convert.ToInt32(s[0]), Value = x.Value });
                }
                return list;
            }
            private set
            {
                throw new NotImplementedException();
            }
        }

        public List<Item<int, bool>> StateOUs
        {
            get
            {
                List<Item<int, bool>> list = new List<Item<int, bool>>();
                foreach (KeyValuePair<int, bool> x in stateOUs)
                {
                    list.Add(new Item<int, bool> { Key = x.Key, Value = x.Value });
                }
                return list;
            }
            set
            {
                stateOUs = new Dictionary<int, bool>();
                foreach (var k in value)
                {
                    stateOUs.Add(k.Key, k.Value);
                }
            }
        }

        public List<Item<int, MonitoredSystemState>> StateMSs
        {
            get
            {
                List<Item<int, MonitoredSystemState>> list = new List<Item<int, MonitoredSystemState>>();
                foreach (KeyValuePair<int, MonitoredSystemState> x in stateMSs)
                {
                    list.Add(new Item<int, MonitoredSystemState> { Key = x.Key, Value = x.Value });
                }
                return list;
            }
            set
            {
                stateMSs = new Dictionary<int, MonitoredSystemState>();
                foreach (var k in value)
                {
                    stateMSs.Add(k.Key, k.Value);
                }
            }
        }

        public List<Item<string, string>> Properties
        {
            get
            {
                List<Item<string, String>> list = new List<Item<string, string>>();
                foreach (KeyValuePair<string, string> x in properties)
                {
                    list.Add(new Item<string, string> { Key = x.Key, Value = x.Value });
                }
                return list;
            }
            set
            {
                properties = new Dictionary<string, string>();
                foreach (var k in value)
                {
                    properties.Add(k.Key, k.Value);
                }
            }
        }

        #endregion

        #region variable declatations
        protected Dictionary<string, float> value = new Dictionary<string, float>();
        protected Dictionary<int, bool> stateOUs = new Dictionary<int, bool>();
        protected Dictionary<int, MonitoredSystemState> stateMSs = new Dictionary<int, MonitoredSystemState>();
        protected Dictionary<string, string> properties = new Dictionary<string, string>();

        #endregion

        #region event handlers
        public delegate void PropertyChangedEventHandler(KeyValuePair<string, string> property);
        public delegate void ValueChangedEventHandler(int ID, bool IsOu, float Value);
        public delegate void MonitoredSystemStateChangedEventHandler(int ID, MonitoredSystemState state, bool sendFullLayout=false);
        public delegate void OUStateChangedEventHandler(int ID, bool open);

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        [field: NonSerialized]
        public event ValueChangedEventHandler ValueChanged;
        [field: NonSerialized]
        public event MonitoredSystemStateChangedEventHandler MonitoredSystemStateChanged;
        [field: NonSerialized]
        public event OUStateChangedEventHandler OUStateChanged;

        #endregion

        public Layout()
        {

        }

        #region public methods
        /// <summary>
        /// Returns the value of an ui element.
        /// </summary>
        /// <param name="ID">the id of the ui element. under the scope of IsOU it must be uniq</param>
        /// <param name="IsOU">defines if the ui element is a ou or an MS</param>
        /// <returns>a float value which is representing the ordering position of this element.</returns>
        public float GetValue(int ID, bool IsOU)
        {
            var s = ID + "_";
            if (IsOU)
            {
                s += "ou";
            }
            else
            {
                s += "ms";
            }

            float v;
            if (value.TryGetValue(s, out v))
            {
                return v;
            }
            else
            {
                return ID;
            }

        }


        /// <summary>
        /// Sets the value of a ui element of the layout to the given value. 
        /// If it is not existing right now it is created in the layout.
        /// </summary>
        /// <param name="ID">the id of the ui element. under the scope of IsOU it must be uniq!</param>
        /// <param name="IsOU">defines if the ui element is a ou or an MS.</param>
        /// <param name="Value">the new value of this element.</param>
        /// <returns>the value setted to.</returns>
        public float SetValue(int ID, bool IsOU, float Value, bool fireNotification = true)
        {
            if (GetValue(ID, IsOU) != Value || GetValue(ID, IsOU) == ID)
            {
                // fire event
                if (fireNotification && this.ValueChanged != null)
                    this.ValueChanged(ID, IsOU, Value);
                var s = ID + "_";
                if (IsOU)
                {
                    s += "ou";
                }
                else
                {
                    s += "ms";

                }
                value.Remove(s);
                value.Add(s, Value);
            }
            return Value;

        }

        /// <summary>
        /// is used to store properties of the layout in the database (e.g. plugin priority list)
        /// </summary>
        /// <param name="Key">A key to store the property. If a property is already available with this key it is overwritten.</param>
        /// <param name="Value">the value of this property.</param>
        public void SetProperty(string Key, string Value)
        {
            if (GetProperty(Key, "") != Value)
            {
                properties.Remove(Key);
                properties.Add(Key, Value);
                if (this.PropertyChanged != null)
                    this.PropertyChanged(new KeyValuePair<string, string>(Key, Value));
            }
        }

        /// <summary>
        /// Gets a layout property from this layout. If no value is stored in the Layout it returns the default value.
        /// </summary>
        /// <param name="Key">The key of the propery (see SetProperty)</param>
        /// <param name="DefaultValue">the default value which will be returned if no value is stored for the given key.</param>
        /// <returns>the value of the property defined by the key or the defaultvalue</returns>
        public string GetProperty(string Key, string DefaultValue)
        {
            string v;
            if (properties.TryGetValue(Key, out v))
            {
                return v;
            }
            else
            {
                return DefaultValue;
            }
        }


        /// <summary>
        /// Sets a given Monitored System to a defined State. This Method must be 
        /// called each time a monitored system changes its visualisation state.
        /// </summary>
        /// <param name="ID">The ID of the MonitoredSystem from the Database. Make sure that this id is uniq.</param>
        /// <param name="state">The complete State of the visualisation of the Monitored System (level, opend plugins)</param>
        public void SetState(int ID, MonitoredSystemState state, bool sendFullLayout=false)
        {
            if (state != null)
            {
                stateMSs.Remove(ID);
                stateMSs.Add(ID, state);
                // fire event
                if (this.MonitoredSystemStateChanged != null)
                {
                    this.MonitoredSystemStateChanged(ID, state, sendFullLayout);
                }
            }
        }

        /// <summary>
        /// Sets a given OU to a defined sstate. This Method must be 
        /// called each time a ou changes its visualisation state.
        /// </summary>
        /// <param name="ID">The ID of the OU from the Database. Make sure that this id is uniq.</param>
        /// <param name="open">true if opened, false if closed</param>
        public void SetState(int ID, bool open)
        {
            stateOUs.Remove(ID);
            stateOUs.Add(ID, open);
            // fire event
            if (this.OUStateChanged != null)
            {
                this.OUStateChanged(ID, open);
            }
        }




        /// <summary>
        /// Returns a state of a monitored System by its id. 
        /// If the Monitored System is not stored in the layout it will return null.
        /// </summary>
        /// <param name="ID">The ID of the MonitoredSystem from the Database. Make sure that this id is uniq.</param>
        /// <returns>the State or an IndexOutOfRangeException</returns>
        public MonitoredSystemState GetMSState(int ID)
        {
            MonitoredSystemState s;
            if (stateMSs.TryGetValue(ID, out s))
            {
                return s;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Returns a state of a ou by its id. 
        /// If the ou is not stored in the layout null will be returned.
        /// </summary>
        /// <param name="ID">The ID of the OU from the Database. Make sure that this id is uniq.</param>
        /// <returns>the State or an IndexOutOfRangeException</returns>
        public bool? GetOUState(int ID)
        {
            bool b;
            if (stateOUs.TryGetValue(ID, out b))
            {
                return b;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Returns the nearest lower value of an Tilable Element in this Layout. This is used to place other elements before 
        /// this element. 
        /// If no Element with less value exists it returns GetValue(ID,isOu) - 1
        /// </summary>
        /// <param name="ID"> the id of the element</param>
        /// <param name="isOu">defines if it is an Ou or an MS</param>
        /// <returns> the nearest lower value in this Layout or GetValue(ID,isOu) - 1 if non exists</returns>
        public float GetValueBefore(int ID, bool isOu)
        {
            float v = this.GetValue(ID, isOu);
            float? max = 0.0f;
            foreach (float f in value.Values)
            {
                if (f < v && f > max.Value)
                {
                    max = f;
                }
            }
            max = max != 0.0f ? max.Value : v - 1;
            //Console.WriteLine("GetValue Before is returning {0}", max);
            foreach (var x in value)
            {
                //Console.WriteLine(x);
            }
            return max.Value;
        }

        /// <summary>
        /// Returns the nearest higher value of an Tilable Element in this Layout. This is used to place other elements after 
        /// this element. 
        /// If no Element with higher value exists it returns GetValue(ID,isOu) + 1
        /// </summary>
        /// <param name="ID"> the id of the element</param>
        /// <param name="isOu">defines if it is an Ou or an MS</param>
        /// <returns> the nearest higher value in this Layout or GetValue(ID,isOu) + 1 if non exists</returns>
        public float GetValueAfter(int ID, bool isOu)
        {
            float v = this.GetValue(ID, isOu);
            float? min = float.MaxValue;
            foreach (float f in value.Values)
            {
                if (f > v && f < min.Value)
                {
                    min = f;
                }
            }
            min = min != float.MaxValue ? min.Value : v + 1;
            return min.Value;
        }



        /// <summary>
        /// this method is used to normalize the Values in the dictionaries.
        /// </summary>
        public void NormalizeData()
        {
            Dictionary<string, float> sortedDict = (from entry in value orderby entry.Value ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            for (int i = 0; i < sortedDict.Count; i++)
            {
                var s = sortedDict.ElementAt(i).Key.Split('_');
                SetValue(Convert.ToInt32(s[0]), s[1] == "ou", i * 10.0f);
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Resets all Dictionaries and values to default state.
        /// </summary>
        private void ResetData()
        {
            value = new Dictionary<string, float>();
            stateOUs = new Dictionary<int, bool>();
            stateMSs = new Dictionary<int, MonitoredSystemState>();
            properties = new Dictionary<string, string>();
        }



        #endregion
    }
}
