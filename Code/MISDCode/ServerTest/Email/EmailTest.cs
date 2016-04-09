using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MISD.Server.Email;
using System.Net.Mail;

namespace ServerTest.Email
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für EmailTest
    /// </summary>
    [TestClass]
    public class EmailTest
    {
        private Mailer target;

        public EmailTest()
        {
            target = Mailer.Instance;
        }
        


        #region Zusätzliche Testattribute
        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SendMailTest()
        {
            try
            {
                //target.SendMail(new MailAddress(
            }
            catch
            {
            }
        }

        //[TestMethod]
        //public void SendAllWarningsTest()
        //{
        //    try
        //    {
        //        target.SendAllWarnings("Anton", new DateTime(2012, 10, 12), "CPU", "Auslastung", "80%");
        //        Assert.IsTrue(true);
        //    }
        //    catch
        //    {
        //        Assert.IsTrue(false);
        //    }
        //}
        
        [TestMethod]
        public void SendAllDailyMails()
        {
            try
            {
                target.SendAllDailyMails();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }
    }
}
