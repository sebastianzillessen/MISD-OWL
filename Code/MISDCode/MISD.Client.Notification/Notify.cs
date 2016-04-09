using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MISD.Client.Notification
{
    public class Notify
    {
        private GrowlNotifiactions growlNotifications;
        #region singelton
        private static volatile Notify instance;
        private static object syncRoot = new Object();


        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static Notify Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Notify();
                    }
                }

                return instance;
            }
        }
        #endregion

        private Notify()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                growlNotifications = new GrowlNotifiactions();
                growlNotifications.Top = SystemParameters.WorkArea.Top + 20;
                growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - 380;
            }));
        }

        public void Message(string headline, string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                growlNotifications.AddNotification(new Notification { Title = headline, ImageUrl = "Resources/notification-icon.png", Message = message }); 
            }));
        }



    }
}
