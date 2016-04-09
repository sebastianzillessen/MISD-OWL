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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MISD.Client.Model;
using MISD.Client.Model.Managers;
using MISD.TCPUtil;

namespace MISD.Client.Managers
{
    public class LayoutManager : BindableBase
    {

        #region variables
        public enum PropertyValues
        {
            PLUGIN_PRIORITY,
            NUMBER_OF_CHAR,
            FONT_SIZE,
            FONT_NAME
        };
        private Layout currentLayout;
        private ExtendedObservableCollection<string> pluginPriority;
        private TCPConnection tcpConnection;
        #endregion

        #region properties

        public TCPConnection TcpConnection
        {
            get
            {
                return this.tcpConnection;
            }

            private set
            {
                this.tcpConnection = value;
            }
        }


        public Layout CurrentLayout
        {
            get
            {
                if (currentLayout == null)
                {
                    //CurrentLayout = DataModel.Instance.Layouts.First();
                    this.CurrentLayout = new MISD.Client.Model.Layout();
                }
                return currentLayout;
            }
            set
            {
                // reset old event handlers
                if (currentLayout != null)
                {
                    currentLayout.MonitoredSystemStateChanged -= currentLayout_MonitoredSystemStateChanged;
                    currentLayout.OUStateChanged -= currentLayout_OUStateChanged;
                    currentLayout.PropertyChanged -= currentLayout_PropertyChanged;
                    currentLayout.ValueChanged -= currentLayout_ValueChanged;
                    currentLayout = new Layout();
                }
                // set value
                currentLayout = value;
                // add new event handlers
                currentLayout.MonitoredSystemStateChanged += currentLayout_MonitoredSystemStateChanged;
                currentLayout.OUStateChanged += currentLayout_OUStateChanged;
                currentLayout.PropertyChanged += currentLayout_PropertyChanged;
                currentLayout.ValueChanged += currentLayout_ValueChanged;
                // => Sebastian meinte das...
                if (this.LayoutChanged != null)
                {
                    LayoutChanged(currentLayout);
                }
                foreach (var ou in currentLayout.StateOUs)
                {
                    if (OUStateChanged != null)
                    {
                        currentLayout_OUStateChanged(ou.Key, ou.Value);
                        OUStateChanged(ou.Key, ou.Value);
                    }
                }
                foreach (var ms in currentLayout.StateMSs)
                {
                    if (MonitoredSystemStateChanged != null && ms.Value != null)
                    {
                        currentLayout_MonitoredSystemStateChanged(ms.Key, ms.Value);
                        MonitoredSystemStateChanged(ms.Key, ms.Value.Level, ms.Value.ShownPlugins.ToArray());
                    }
                }
                foreach (var v in currentLayout.ValueMS)
                {
                    if (ValueChanged != null)
                    {
                        currentLayout_ValueChanged(v.Key, false, v.Value);
                        ValueChanged(v.Key, false, v.Value);
                    }
                }
                foreach (var v in currentLayout.ValueOU)
                {
                    if (ValueChanged != null)
                    {
                        currentLayout_ValueChanged(v.Key, true, v.Value);
                        ValueChanged(v.Key, true, v.Value);
                    }
                }
                foreach (var prop in currentLayout.Properties)
                {
                    currentLayout_PropertyChanged(new KeyValuePair<string, string>(prop.Key, prop.Value));
                    if (PropertyChanged != null)
                    {
                        currentLayout_PropertyChanged(new KeyValuePair<string, string>(prop.Key, prop.Value));
                        PropertyChanged(new KeyValuePair<string, string>(prop.Key, prop.Value));
                    }
                }
                OnPropertyChanged();
                SendLayoutChanges(new LayoutChangeCommand(currentLayout));
            }
        }

        public ExtendedObservableCollection<string> PluginPriority
        {
            get
            {
                if (this.pluginPriority == null)
                {
                    pluginPriority = new ExtendedObservableCollection<string>();
                    WorkerThread workerThread = ThreadManager.CreateWorkerThread("ChangePluginPriority", () =>
                    {
                        foreach (var plugin in PluginManager.Instance.Plugins)
                        {
                            this.pluginPriority.AddOnUI(plugin.Name);
                        }
                    }, false);
                }
                return this.pluginPriority;
            }
            set
            {
                if (pluginPriority != value)
                {
                    if (pluginPriority != null)
                    {
                        pluginPriority.Clear();
                        pluginPriority.AddRange(value);
                    }
                    else
                    {
                        this.pluginPriority = value;
                    }
                    OnPropertyChanged();
                    this.SetProperty(PropertyValues.PLUGIN_PRIORITY.ToString(), String.Join(";", PluginPriority));
                }
            }
        }


        #endregion

        #region event handlers
        public delegate void PropertyChangedEventHandler(KeyValuePair<string, string> property);
        public delegate void ValueChangedEventHandler(int ID, bool IsOu, float Value);
        public delegate void MonitoredSystemStateChangedEventHandler(int ID, int level, string[] flippedPlugins);
        public delegate void OUStateChangedEventHandler(int ID, bool open);
        public delegate void LayoutChangedEventHandler(Layout l);

        public event PropertyChangedEventHandler PropertyChanged;
        public event ValueChangedEventHandler ValueChanged;
        public event MonitoredSystemStateChangedEventHandler MonitoredSystemStateChanged;
        public event OUStateChangedEventHandler OUStateChanged;
        public event LayoutChangedEventHandler LayoutChanged;

        #endregion

        #region Singleton

        private static volatile LayoutManager instance;
        private static object syncRoot = new Object();
        private int counterLayoutChanges = 0;


        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static LayoutManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new LayoutManager();
                    }
                }

                return instance;
            }
        }
        #endregion

        #region constructors
        private LayoutManager()
        {
            if (ConfigClass.IsOperator)
            {
                ClientLogger.Instance.WriteEntry(">>>Starting as Operator", MISD.Core.LogType.Info);
                IPAddress a = IPAddress.Parse(ConfigClass.OperatorIP);
                tcpConnection = new TCPConnection(TCPRole.Server, a, MISD.Client.Model.Properties.Settings.Default.ServerPort);
            }
            else if (ConfigClass.IsPowerwall)
            {
                Console.WriteLine(">>> Starting as Powerwall");
                ClientLogger.Instance.WriteEntry(">>>Starting as Powerwall", MISD.Core.LogType.Info);

                tcpConnection = new TCPConnection(TCPRole.Client, IPAddress.Parse(ConfigClass.OperatorIP), MISD.Client.Model.Properties.Settings.Default.ServerPort);
                Console.WriteLine(">>> Connection as Client inited");

                tcpConnection.newDataReceived += tcpConnection_newDataReceived;
                Console.WriteLine(">>> Started as Powerwall");
            }
            else
            {
                ClientLogger.Instance.WriteEntry(">>>Starting as Desktop", MISD.Core.LogType.Info);
                Console.WriteLine(">>> Starting as Desktop");

            }
            this.PluginPriority.CollectionChanged += PluginPriority_CollectionChanged;
        }

        #endregion

        #region current-layout changes receiver

        private void PluginPriority_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!ConfigClass.IsPowerwall)
            {
                SetProperty(PropertyValues.PLUGIN_PRIORITY.ToString(), String.Join(";", PluginPriority));
            }
            if (ConfigClass.IsOperator)
            {
                SendLayoutChanges(new LayoutChangeCommand(this.PluginPriority));
            }
        }

        private void currentLayout_ValueChanged(int ID, bool IsOu, float Value)
        {
            if (this.ValueChanged != null)
                this.ValueChanged(ID, IsOu, Value);
            if (ConfigClass.IsOperator)
                SendLayoutChanges(new LayoutChangeCommand(ID, IsOu, Value));
        }

        private void currentLayout_PropertyChanged(KeyValuePair<string, string> property)
        {
            // alert event handlers
            if (this.PropertyChanged != null)
                this.PropertyChanged(property);

            if (ConfigClass.IsOperator)
                SendLayoutChanges(new LayoutChangeCommand(property));


            if (property.Key.Equals(PropertyValues.PLUGIN_PRIORITY.ToString()))
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("PluginPriority_CollectionChanged"))
                {
                    ExtendedObservableCollection<string> o = new ExtendedObservableCollection<string>();
                    foreach (string s in property.Value.Split(';'))
                    {
                        o.Add(s);
                    }
                    this.PluginPriority = o;
                }
                //else { Console.WriteLine("Skipped to set the Collection new"); }
                stackTrace = null;
            }

        }

        private void currentLayout_OUStateChanged(int ID, bool open)
        {
            // alert event handlers
            if (this.OUStateChanged != null)
                this.OUStateChanged(ID, open);
            if (ConfigClass.IsOperator)
                SendLayoutChanges(new LayoutChangeCommand(ID, open));
        }

        private void currentLayout_MonitoredSystemStateChanged(int ID, MonitoredSystemState state, bool sendFullLayout=false)
        {
            if (state != null)
            {
                if (MonitoredSystemStateChanged != null)
                    MonitoredSystemStateChanged(ID, state.Level, state.ShownPlugins.ToArray());
                if (ConfigClass.IsOperator)
                    SendLayoutChanges(new LayoutChangeCommand(ID, state), sendFullLayout);
            }
        }
        #endregion

        #region public Methods

        /// <summary>
        /// Sets the MonitoredSystemState of a MS with the ID in the current layout.
        /// </summary>
        /// <param name="ID">the MS id</param>
        /// <param name="Level">the level of the MS shown</param>
        /// <param name="plugin">specifies a plugin name to add as flipped to the MS</param>
        /// <param name="add">if add is false, then the defined plugin will be removed from the MonitoredSystemstate</param>
        public void SetMSState(int ID, int Level, string plugin = "", bool add = true, bool sendFullLayout=false)
        {
            MonitoredSystemState s = CurrentLayout.GetMSState(ID);
            if (s == null)
            {
                s = new MonitoredSystemState();
            }
            s.Level = Level;
            if (plugin != "")
            {
                bool hasCustomUI;
                var currentLevel = (from p in DataModel.Instance.LevelDefinitions
                                    where p.LevelID == s.Level
                                    select p as LevelDefinition).FirstOrDefault();

                if (currentLevel == null)
                {
                    hasCustomUI = false;
                }
                else
                {
                    hasCustomUI = currentLevel.UseCustomUI;
                }

                if (add)
                {
                    if (s.ShownPlugins.Contains(plugin))
                    {
                        s.HidePlugin(plugin);
                    }
                    s.ShowPlugin(plugin);
                }
                else
                {
                    s.HidePlugin(plugin);
                }
            }
            CurrentLayout.SetState(ID, s, sendFullLayout);
        }


        /// <summary>
        /// Set the state of an ou in the current layout
        /// </summary>
        /// <param name="ID"> the ID of the ou</param>
        /// <param name="open">true=opened, false=hidden</param>
        public void SetOUState(int ID, bool open)
        {
            CurrentLayout.SetState(ID, open);
        }

        /// <summary>
        /// Sets the Property using the Key and the Value in the current layout
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetProperty(string Key, string Value)
        {
            CurrentLayout.SetProperty(Key, Value);
        }


        /// <summary>
        /// Gets the Value of a Element (ID)
        /// </summary>
        /// <param name="ID">The ID of the element</param>
        /// <param name="isOu">if it is Ou or MS</param>
        /// <returns>Sorting Value in this Layout for the element ID</returns>
        public float GetValue(int ID, bool isOu)
        {
            return CurrentLayout.GetValue(ID, isOu);
        }


        /// <summary>
        /// Gets a property defined by the key. If the Property is not defined in the current layout it returns
        /// the Default value
        /// </summary>
        /// <param name="Key">Key for the property</param>
        /// <param name="Default">Default Value </param>
        /// <returns>Value or [Default]</returns>
        public string GetProperty(string Key, string Default)
        {
            return CurrentLayout.GetProperty(Key, Default);
        }


        /// <summary>
        /// returns the state of the OU in the current layout
        /// </summary>
        /// <param name="ID">ID of the OU</param>
        /// <returns>True if opened, false if closed, null if not defined.</returns>
        public bool? GetOuState(int ID)
        {
            return CurrentLayout.GetOUState(ID);
        }

        /// <summary>
        /// Returns the State of the MS in the current Layout
        /// </summary>
        /// <param name="ID">ID of the MS</param>
        /// <returns>the state of the MS</returns>
        public MonitoredSystemState GetMSState(int ID)
        {
            return CurrentLayout.GetMSState(ID);
        }

        /// <summary>
        /// sets the ordering value of an TilableElement.
        /// </summary>
        /// <param name="ID">The Id of the element. It should be uniq with the scope IsOu.</param>
        /// <param name="IsOu">Defines if the Element is an Ou  (=true) or an MonitoredSystem (=false)</param>
        /// <param name="Value">The new Value defining the position of the Tilable Element.</param>
        public void SetValue(int ID, bool IsOu, float Value)
        {
            this.CurrentLayout.SetValue(ID, IsOu, Value);
        }


        /// <summary>
        /// Moves a tilable Element with the ID between two Elements defined by "BeforeID" and "AfterID".
        /// The ID-Element will have the Value (GetValue(AfterId,isOu) - GetValue(BeforeId,isOU))/2
        /// 
        /// The result will be:
        /// GetValue(BeforeId,isOu) < GetValue(ID,isOu) < GetValue(AfterId,isOu)
        /// </summary>
        /// <param name="ID">The Element to move to a new position (scope isOu!)</param>
        /// <param name="BeforeId">The Element where the ID-Element should be placed after (scope isOu!)</param>
        /// <param name="AfterId">The Element where the ID-Element should be placed before (scope isOu!)</param>
        /// <param name="isOu"></param>
        /// <param name="isOtherOu">defines if the element before is an OU</param>
        /// <param name="isOtherOu">defines if the element after is an OU</param>
        public void MoveBetween(int ID, int BeforeId, int AfterId, bool isOu, bool isBeforeOu, bool isAfterOu)
        {
            float a = CurrentLayout.GetValue(BeforeId, isBeforeOu);
            float b = CurrentLayout.GetValue(AfterId, isAfterOu);
            float v = (b + a) / 2.0f;
            CurrentLayout.SetValue(ID, isOu, v);
        }

        /// <summary>
        /// Moves the Element with the ID before an element of the same type with the OtherID
        /// GetValue(all but ID, isOu) < GetValue(Id,isOu) < GetValue(OtherID,isOU)
        /// 
        /// Both IDs must be in the same OU!
        /// 
        /// </summary>
        /// <param name="ID">Element which should be moved</param>
        /// <param name="OtherId">Element where the element above should be placed before</param>
        /// <param name="isOu">defines if it is an ou or an MS</param>
        /// <param name="isOtherOu">defines if the other element is an OU</param>
        public void MoveBefore(int ID, int OtherId, bool isOu, bool isOtherOu)
        {
            //Console.WriteLine("Move before {0},{1},{2},{3}", ID, OtherId, isOu, isOtherOu);
            float b = CurrentLayout.GetValueBefore(OtherId, isOtherOu);
            float a = CurrentLayout.GetValue(OtherId, isOtherOu);
            float v = (a + b) / 2.0f;
            CurrentLayout.SetValue(ID, isOu, v);
            //Console.WriteLine("SetValue: {0},{1},{2}", ID, isOu, v);

        }

        /// <summary>
        /// Moves the Element with the ID after an element of the same type with the OtherID
        /// GetValue(OtherID,isOU) < GetValue(Id,isOu) < GetValue(all but ID, isOu)
        /// </summary>
        /// <param name="ID">Element which should be moved</param>
        /// <param name="OtherId">Element where the element above should be placed after</param>
        /// <param name="isOu">defines if it is an ou or an MS</param>
        /// <param name="isOtherOu">defines if the other element is an OU</param>
        public void MoveAfter(int ID, int OtherId, bool isOu, bool isOtherOu)
        {
            float b = CurrentLayout.GetValueAfter(OtherId, isOtherOu);
            float a = CurrentLayout.GetValue(OtherId, isOtherOu);
            float v = (a + b) / 2.0f;
            CurrentLayout.SetValue(ID, isOu, v);
        }

        public string SerializeBase64(Layout o)
        {
            MemoryStream ws = new MemoryStream();
            try
            {
                // Serialize to a base 64 string
                byte[] bytes;
                long length = 0;
                BinaryFormatter sf = new BinaryFormatter();
                sf.Serialize(ws, o);
                length = ws.Length;
                bytes = ws.GetBuffer();
                string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                ws.Flush();
                ws.Close();
                return encodedData;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, MISD.Core.LogType.Exception);
                return null;
            }
            finally
            {
                ws.Flush();
                ws.Close();
                ws.Dispose();
            }
        }

        public string SerializeBase64(ExtendedObservableCollection<string> o)
        {
            MemoryStream ws = new MemoryStream();
            try
            {
                // Serialize to a base 64 string
                byte[] bytes;
                long length = 0;
                BinaryFormatter sf = new BinaryFormatter();
                sf.Serialize(ws, o);
                length = ws.Length;
                bytes = ws.GetBuffer();
                string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                ws.Flush();
                ws.Close();
                return encodedData;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, MISD.Core.LogType.Exception);
                return null;
            }
            finally
            {
                ws.Flush();
                ws.Close();
                ws.Dispose();
            }
        }

        public object DeserializeBase64(string s)
        {
            // We need to know the exact length of the string - Base64 can sometimes pad us by a byte or two
            int p = s.IndexOf(':');
            int length = Convert.ToInt32(s.Substring(0, p));

            // Extract data from the base 64 string!
            byte[] memorydata = Convert.FromBase64String(s.Substring(p + 1));
            MemoryStream rs = new MemoryStream(memorydata, 0, length);
            try
            {
                BinaryFormatter sf = new BinaryFormatter();
                object o = sf.Deserialize(rs);
                rs.Flush();
                rs.Close();
                return o;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, MISD.Core.LogType.Exception);
                return null;
            }
            finally
            {
                rs.Flush();
                rs.Close();
                rs.Dispose();
            }
        }



        #endregion

        #region private methods

        private void tcpConnection_newDataReceived(object sender, EventArgs e)
        {
            while (tcpConnection.HasData())
            {
                object data = tcpConnection.PopData();

                if (data.GetType() == typeof(MISD.Client.Model.LayoutChangeCommand))
                {
                    IncommingLayoutChanges((MISD.Client.Model.LayoutChangeCommand)data);
                }
                else if (data.GetType() == typeof(DataModelChangeCommand))
                {
                    DataModelManager.Instance.TcpConnection_newDataReceived(data);
                }
                else
                {
                  //  Console.WriteLine(">>> Unknown delivered type");
                }
            }
        }

        /// <summary>
        /// Sends a LayoutChangeCommand to the Powerwalls if this COmputer isOperatorPC.
        /// </summary>
        /// <param name="layoutChangeCommand">A Layout change command</param>
        private void SendLayoutChanges(LayoutChangeCommand layoutChangeCommand, bool sendFullLayout=false)
        {
            if (ConfigClass.IsOperator && tcpConnection != null)
            {               
                if (sendFullLayout)
                {
                    var l = new LayoutChangeCommand(this.CurrentLayout);
                    tcpConnection.Send(l);
                    counterLayoutChanges = 0;
                }
                else
                {
                    counterLayoutChanges++;
                    tcpConnection.Send(layoutChangeCommand);

                    if (counterLayoutChanges > 10)
                    {
                        var l = new LayoutChangeCommand(this.CurrentLayout);
                        tcpConnection.Send(l);
                        counterLayoutChanges = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Handels a LayoutChangeCommand in this LayoutManager.
        /// Commands are only handled if Layouter is defined and isPowerwall is true!
        /// </summary>
        /// <param name="cmd">The incomming LayoutChangeCommand</param>
        private void IncommingLayoutChanges(LayoutChangeCommand cmd)
        {
            if (ConfigClass.IsPowerwall)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (cmd.CommandType)
                    {
                        case LayoutCommand.MS_STATE_CHANGED:
                            CurrentLayout.SetState(cmd.ID, cmd.MSState());
                            break;
                        case LayoutCommand.OU_STATE_CHANGED:
                            CurrentLayout.SetState(cmd.ID, cmd.OUState());
                            break;
                        case LayoutCommand.MS_VALUE_CHANGED:
                            CurrentLayout.SetValue(cmd.ID, false, cmd.MSValue());
                            break;
                        case LayoutCommand.OU_VALUE_CHANGED:
                            CurrentLayout.SetValue(cmd.ID, true, cmd.OUValue());
                            break;
                        case LayoutCommand.PROPERTY_CHANGED:
                            CurrentLayout.SetProperty(cmd.Property().Key, cmd.Property().Value);
                            break;
                        case LayoutCommand.UPDATE_COMPLETE_LAYOUT:
                            CurrentLayout = cmd.GetLayout();
                            break;
                        case LayoutCommand.PRIORITY_LIST_CHANGED:
                            this.PluginPriority = cmd.GetPluginPriorityList();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("LayoutCommand is not known!");
                    }
                });
            }
            else
                throw new ArgumentOutOfRangeException("We are no Powerwall or no Layouter is defined!");
        }
        #endregion


        public void ShutdownClients()
        {
            if (ConfigClass.IsOperator)
            {
                Console.WriteLine("ShutHimDown");
                this.TcpConnection.ShutdownClients();
            }
        }
    }
}
