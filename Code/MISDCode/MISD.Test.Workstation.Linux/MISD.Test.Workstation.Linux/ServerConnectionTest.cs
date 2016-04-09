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

using NUnit.Framework;
using System;
using System.ServiceModel.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using MISD.Core;

/// <summary>
/// ServerConnectionTest.cs - NUnit Test Cases for ServerConnection
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
    public class ServerConnectionTest {

		#region SetUp() and TearDown()
		/// <summary>
		/// Attributes to save the current state of the WorkstationLogger.
		/// </summary>
		private bool connected = ServerConnection.Instance.IsConnected();

		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			ServerConnection.Instance.RestoreConnection();
		}

		/// <summary>
		/// This method is called after every test method.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			// nothing
		}
		#endregion

		#region Testmethods
		/// <summary>
		/// Tests the method GetWebServiceURI().
		/// </summary>
		[Test]
		public void TestGetWebServiceURI ()
		{
			System.Uri webServiceURI = ServerConnection.Instance.GetWebServiceURI();
			Assert.NotNull(webServiceURI, "ServerConnectionTest:TestGetWebServiceURI() #01");
			Assert.IsNotEmpty (webServiceURI.AbsolutePath, "ServerConnectionTest:TestGetWebServiceURI() #02");
			Assert.IsNotEmpty (webServiceURI.Host, "ServerConnectionTest:TestGetWebServiceURI() #03");
			Assert.IsNotEmpty (webServiceURI.OriginalString, "ServerConnectionTest:TestGetWebServiceURI() #04");
		}

		/// <summary>
		/// Tests the method IsConnected().
		/// But only whether the server is online or not.
		/// </summary>
		[Test]
		public void TestIsConnected ()
		{
			bool serverConnectionConnectionGood = ServerConnection.Instance.IsConnected ();
			Console.WriteLine (serverConnectionConnectionGood);

			bool connectionCouldBePossible = false;
			try {
				PingReply Result = new Ping ().Send (ServerConnection.Instance.GetWebServiceURI().Host);
				if (Result.Status == IPStatus.Success)
					connectionCouldBePossible = true;
				else
					connectionCouldBePossible = false;
			} catch (Exception) {
				connectionCouldBePossible = false;
			}

			Assert.AreEqual(connectionCouldBePossible, serverConnectionConnectionGood, "ServerConnectionTest:TestIsConnected() #01");
		}

		/// <summary>
		/// Tests the method GetWorkstationWebService().
		/// </summary>
		[Test]
		public void TestGetWorkstationWebService ()
		{
			WSWebService.WorkstationWebServiceClient webService = ServerConnection.Instance.GetWorkstationWebService();
			Assert.NotNull(webService, "ServerConnectionTest:TestGetWorkstationWebService() #01");
			Assert.AreEqual(System.ServiceModel.CommunicationState.Created, webService.State, "ServerConnectionTest:TestGetWorkstationWebService() #02");

			webService.Open();
			webService = ServerConnection.Instance.GetWorkstationWebService();
			Assert.AreEqual(System.ServiceModel.CommunicationState.Opened, webService.State, "ServerConnectionTest:TestGetWorkstationWebService() #03");

			webService.Close();
			webService = ServerConnection.Instance.GetWorkstationWebService();
			Assert.AreEqual(System.ServiceModel.CommunicationState.Closed, webService.State, "ServerConnectionTest:TestGetWorkstationWebService() #04");

		}

		/// <summary>
		/// Tests the method RestoreConnection().
		/// </summary>
		[Test]
		public void TestRestoreConnection ()
		{
			WSWebService.WorkstationWebServiceClient webService = ServerConnection.Instance.GetWorkstationWebService();
			webService.Close();
			webService = ServerConnection.Instance.RestoreConnection();
			Assert.NotNull(webService, "ServerConnectionTest:TestRestoreConnection() #01");
			Assert.AreEqual(System.ServiceModel.CommunicationState.Created, webService.State, "ServerConnectionTest:TestRestoreConnection() #02");

			webService.Open();
			Assert.AreEqual(System.ServiceModel.CommunicationState.Opened, webService.State, "ServerConnectionTest:TestRestoreConnection() #03");
		}

		/// <summary>
		/// Tests the method GetWorkstationName().
		/// </summary>
		[Test]
		public void TestGetWorkstationName ()
		{
			string myWorkstationName = ServerConnection.Instance.GetWorkstationName();
			Assert.NotNull(myWorkstationName, "ServerConnectionTest:TestGetWorkstationName() #01");
			Assert.AreNotEqual("", myWorkstationName, "ServerConnectionTest:TestGetWorkstationName() #02");

			Assert.AreEqual(Environment.UserDomainName + "." + IPGlobalProperties.GetIPGlobalProperties().DomainName, myWorkstationName, "ServerConnectionTest:TestGetWorkstationName() #03");
		}

		/// <summary>
		/// Tests the method SignIn().
		/// </summary>
		[Test]
		public void TestSignIn ()
		{
			if (connected) {
				bool resultValue = ServerConnection.Instance.SignIn ();
				Assert.True (resultValue, "ServerConnectionTest:TestSignIn() #01");

				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}
		}

		/// <summary>
		/// Tests the method SignOut().
		/// </summary>
		[Test]
		public void TestSignOut ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			bool resultValue = ServerConnection.Instance.SignOut ();
			if (connected) {
				Assert.True(resultValue, "ServerConnectionTest:TestSignOut() #01");
			} else {
				Assert.False(resultValue, "ServerConnectionTest:TestSignOut() #02");
			}
		}

		/// <summary>
		/// Tests the method GetPluginList().
		/// </summary>
		[Test]
		public void TestGetPluginList ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			PluginMetadata[] resultArray = ServerConnection.Instance.GetPluginList ();
			Assert.NotNull (resultArray, "ServerConnectionTest:TestGetPluginList() #01");

			if (resultArray.Length > 0) {
				foreach (Core.PluginMetadata metadata in resultArray)
				{
					Assert.NotNull (metadata, "ServerConnectionTest:TestGetPluginList() #02"); 
				}
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}

		/// <summary>
		/// Tests the method GetMainUpdateInterval().
		/// </summary>
		[Test]
		public void TestGetMainUpdateInterval ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			TimeSpan oldTimeSpan = new TimeSpan (99, 0, 0, 0);
			TimeSpan resultMainUpdateInterval = ServerConnection.Instance.GetMainUpdateInterval (oldTimeSpan);
			Assert.NotNull (resultMainUpdateInterval, "ServerConnectionTest:TestGetMainUpdateInterval() #01");
			if (!connected) {
				Assert.AreEqual(oldTimeSpan, resultMainUpdateInterval, "ServerConnectionTest:TestGetMainUpdateInterval() #02");
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}

		/// <summary>
		/// Tests the method DownloadPlugins().
		/// </summary>
		[Test]
		public void TestDownloadPlugins ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			MISD.Core.PluginMetadata[] neededPlugins = ServerConnection.Instance.GetPluginList ();
			string[] neededPluginNames = new string[neededPlugins.Length];
			for (int i=0; i<neededPluginNames.Length; i++) {
				neededPluginNames[i] = neededPlugins[i].Name;
			}
			Core.PluginFile[] resultPluginFiles = ServerConnection.Instance.DownloadPlugins (neededPluginNames);
			if (connected) {
				Assert.NotNull (resultPluginFiles, "ServerConnectionTest:TestDownloadPlugins() #01");
				Assert.AreEqual (neededPlugins.Length, resultPluginFiles.Length, "ServerConnectionTest:TestDownloadPlugins() #03");
				for (int i=0; i<neededPlugins.Length; i++)
				{
					Assert.AreEqual (neededPlugins[i].FileName, resultPluginFiles[i].FileName, "ServerConnectionTest:TestDownloadPlugins() #04");
				}
			} else {
				Assert.IsNull (resultPluginFiles, "ServerConnectionTest:TestDownloadPlugins() #02");
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}

		/// <summary>
		/// Tests the method UploadIndicatorValue().
		/// </summary>
		[Test]
		public void TestUploadIndicatorValue ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			bool resultValue = ServerConnection.Instance.UploadIndicatorValue ("", "", null, MISD.Core.DataType.Int, new DateTime(9999, 12, 24));
			Assert.False (resultValue, "ServerConnectionTest:TestUploadIndicatorValue() #01");

			resultValue = ServerConnection.Instance.UploadIndicatorValue ("CPU", "ProcessorName", "TestProcessor", MISD.Core.DataType.String, DateTime.Now);
			if (connected) {
				Assert.True(resultValue, "ServerConnectionTest:TestUploadIndicatorValue() #02");
			} else {
				Assert.False(resultValue, "ServerConnectionTest:TestUploadIndicatorValue() #03");
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}

		/// <summary>
		/// Tests the method GetFilter().
		/// </summary>
		[Test]
		public void TestGetFilter ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			string resultFilter = ServerConnection.Instance.GetFilter ("", "", "");
			Assert.IsNullOrEmpty (resultFilter, "ServerConnectionTest:TestGetFilter() #01");

			string oldFilter = "TestFilter123GRR";
			resultFilter = ServerConnection.Instance.GetFilter ("CPU", "ProcessorName", oldFilter);
			if (!connected) {
				Assert.AreEqual (oldFilter, resultFilter, "ServerConnectionTest:TestGetFilter() #02");
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}

		/// <summary>
		/// Tests the method GetUpdateInterval().
		/// </summary>
		[Test]
		public void TestGetUpdateInterval ()
		{
			if (connected) {
				ServerConnection.Instance.SignIn ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignIn() possible.");
			}

			long oldUdateInterval = -1L;
			long resultUpdateInterval = ServerConnection.Instance.GetUpdateInterval ("", "", oldUdateInterval);
			if (connected) {
				Assert.AreNotEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #01");
			} else {
				Assert.AreEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #02");
			}

			resultUpdateInterval = ServerConnection.Instance.GetUpdateInterval ("", "", oldUdateInterval);
			if (connected) {
				Assert.AreNotEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #03");
			} else {
				Assert.AreEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #04");
			}

			resultUpdateInterval = ServerConnection.Instance.GetUpdateInterval ("CPU", "", oldUdateInterval);
			if (connected) {
				Assert.AreNotEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #05");
			} else {
				Assert.AreEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #06");
			}

			oldUdateInterval = 0L;
			resultUpdateInterval = ServerConnection.Instance.GetUpdateInterval ("CPU", "ProcessorName", oldUdateInterval);
			if (!connected) {
				Assert.AreEqual (oldUdateInterval, resultUpdateInterval, "ServerConnectionTest:TestGetUpdateInterval() #07");
			}

			if (connected) {
				ServerConnection.Instance.SignOut ();
			} else {
				Console.WriteLine ("### No connection to the server : No SignOut() possible.");
			}
		}
		#endregion
	}
}
