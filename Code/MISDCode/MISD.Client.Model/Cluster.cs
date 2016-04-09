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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MISD.Client.Model.Managers;
using MISD.Core;

namespace MISD.Client.Model
{
    public class Cluster : BindableBase
    {
        #region Fields

        private int id;
        private string headnodeAddress;
        private string currentstring;
        private string username;
        private string password;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the id of this cluster.
        /// </summary>
        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Get or sets the heanode address of this cluster.
        /// </summary>
        public string HeadnodeAddress
        {
            get
            {
                return this.headnodeAddress;
            }
            set
            {
                this.headnodeAddress = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the platform of this cluster.
        /// </summary>
        public string CurrentPlatform
        {
            get
            {
                return this.currentstring;
            }
            set
            {
                this.currentstring = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user name of login data for this cluster.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the passowrd of login data for this cluster.
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public Cluster(int id, string headnodeaddress, string platform, string username, string passowrd)
        {
            this.Initialize();

            this.ID = id;
            this.HeadnodeAddress = headnodeaddress;
            this.CurrentPlatform = platform;
            this.UserName = username;
            this.Password = password;
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            this.PropertyChanged += Cluster_PropertyChanged;
        }

        void Cluster_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    switch (e.PropertyName)
                    {
                        case "HeadnodeAddress":
                            WorkerThread workerThreadHeadnodeAddress = ThreadManager.CreateWorkerThread("BackSync_Cluster.HeadNodeAddress", () =>
                            {
                                List<Tuple<int, Tuple<string, string, string, string>>> paramList = new List<Tuple<int, Tuple<string, string, string, string>>>();
                                paramList.Add(new Tuple<int, Tuple<string, string, string, string>>(this.ID, new Tuple<string, string, string, string>(this.HeadnodeAddress, this.UserName, this.Password, this.CurrentPlatform)));
                                bool result = DataModel.Instance.SyncBackClusterChanged(paramList);
                            }, false);
                            break;
                        case "CurrentPlatform":
                            WorkerThread workerThreadCurrentPlatform= ThreadManager.CreateWorkerThread("BackSync_Cluster.CurrentPlatform", () =>
                            {
                                List<Tuple<int, Tuple<string, string, string, string>>> paramList = new List<Tuple<int, Tuple<string, string, string, string>>>();
                                paramList.Add(new Tuple<int, Tuple<string, string, string, string>>(this.ID, new Tuple<string, string, string, string>(this.HeadnodeAddress, this.UserName, this.Password, this.CurrentPlatform)));
                                bool result = DataModel.Instance.SyncBackClusterChanged(paramList);
                            }, false);
                            break;
                        case "UserName":
                            WorkerThread workerThreadUserName = ThreadManager.CreateWorkerThread("BackSync_Cluster.UserName", () =>
                            {
                                List<Tuple<int, Tuple<string, string, string, string>>> paramList = new List<Tuple<int, Tuple<string, string, string, string>>>();
                                paramList.Add(new Tuple<int, Tuple<string, string, string, string>>(this.ID, new Tuple<string, string, string, string>(this.HeadnodeAddress, this.UserName, this.Password, this.CurrentPlatform)));
                                bool result = DataModel.Instance.SyncBackClusterChanged(paramList);
                            }, false);
                            break;
                        case "Password":
                            WorkerThread workerThreadPassword = ThreadManager.CreateWorkerThread("BackSync_Cluster.Password", () =>
                            {
                                List<Tuple<int, Tuple<string, string, string, string>>> paramList = new List<Tuple<int, Tuple<string, string, string, string>>>();
                                paramList.Add(new Tuple<int, Tuple<string, string, string, string>>(this.ID, new Tuple<string, string, string, string>(this.HeadnodeAddress, this.UserName, this.Password, this.CurrentPlatform)));

                                bool result = DataModel.Instance.SyncBackClusterChanged(paramList);
                            }, false);
                            break;
                        default:
                            break;
                    }
                }
                stackTrace = null;
            }
        }

        #endregion
    }
}
