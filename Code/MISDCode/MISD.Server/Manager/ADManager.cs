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
 * MISD-OWL is distributed without any warranty, without even the 
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using MISD.Server.Manager;
using MISD.Core;

namespace MISD.Server.Manager
{
    public class ADManager
    {
        #region Singleton

        private static volatile ADManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static ADManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ADManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Organisation units
        /// <summary>
        /// Find the organiation untis of a computer
        /// </summary>
        /// <param name="domainname">Domain name</param>
        /// <returns>List of FQDN of the organisation units</returns>
        public List<string> GetOU(string domainname)
        {
            lock (this)
            {
                var defaultOU = new List<string>();
                defaultOU.Add(Properties.Settings.Default.DefaultOU);
                try
                {
                    if (domainname != null && domainname != "")
                    {
                        var domainnameArray = domainname.Split('.');

                        string domaninnameTrimmed;
                        if (domainnameArray.Count() > 0)
                        {
                            domaninnameTrimmed = domainnameArray[0];
                        }
                        else
                        {
                            domaninnameTrimmed = domainname;
                        }

                        string ldap = "LDAP://DC=visus, DC=uni-stuttgart, DC=de";

                        string distinguishedName = "";
                        try
                        {
                            DirectoryEntry ad = new DirectoryEntry(ldap);

                            var searcher = new DirectorySearcher(ad);
                            searcher.Filter = "(&(objectClass=computer)(cn=" + domaninnameTrimmed + "))";

                            if (searcher.FindOne() != null)
                            {
                                var entry = searcher.FindOne().GetDirectoryEntry();
                                distinguishedName = entry.Properties["distinguishedName"].Value.ToString(); //ex. CN=VISGS15,OU=VisSimLabor,OU=Pool,OU=VIS,OU=Computer,OU=VIS(US),DC=visus,DC=uni-stuttgart,DC=de
                            }
                            else
                            {
                                //logging no entry
                                var messageW1 = new StringBuilder();
                                messageW1.Append("ADMangaer_GetOU: ");
                                messageW1.Append("No entry for domain name: " + domainname);
                                MISD.Core.Logger.Instance.WriteEntry(messageW1.ToString(), LogType.Warning);
                                return defaultOU;
                            }
                        }
                        catch (Exception e)
                        {
                            //logging error
                            var messageEx3 = new StringBuilder();
                            messageEx3.Append("ADMangaer_GetOU: ");
                            messageEx3.Append("Can't read entry for computer " + domainname + ". " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx3.ToString(), LogType.Exception);

                            //if the active directory isn't available, take the default ou.
                            return defaultOU;
                        }

                        return distinguishedName == null ? new List<string>() : this.FindOUinString(distinguishedName);
                    }
                }
                catch (Exception e)
                {
                    //logging error
                    var messageEx4 = new StringBuilder();
                    messageEx4.Append("ADMangaer_GetOU: ");
                    messageEx4.Append("Exception for  " + domainname + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx4.ToString(), LogType.Exception);
                }
                MISD.Core.Logger.Instance.WriteEntry("ADManager_GetOU: Domainname cannot be empty", LogType.Warning);
                return defaultOU;
            }
        }

        /// <summary>
        /// Find the organisation units in a distinguishedName string of a computer
        /// </summary>
        /// <param name="distinguishedName">distinguishedName from the computer</param>
        /// <returns>List of FQDN of the organisation units</returns>
        private List<string> FindOUinString(string distinguishedName)
        {
            List<string> result = new List<string>();

            string[] splitedDN = distinguishedName.Split(',');

            foreach (string value in splitedDN)
            {
                if (value.Contains("OU="))
                {
                    result.Add(value.Replace("OU=", ""));
                }
            }

            result.Reverse();
            //create FQDN
            if (result.Count > 1)
            {
                for (int i = result.IndexOf(result.First()) + 1; i <= result.IndexOf(result.Last()); i++)
                {
                    result[i] = result[i - 1] + "." + result[i];
                }
            }

            return result;
        }
    }

        #endregion
}
