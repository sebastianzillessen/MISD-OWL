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
 * Version 3 or any later versionetworkPlugin. For details see here:
 * http://www.gnu.org/licenses/lgpl.html
 *
 * MISD-OWL is distributed without any warranty, without even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

using MISD.Core;
using NetworkAdapter;
using System.Threading;

/// <summary>
/// NetworkTest.cs - NUnit Test Cases for the plugin Network
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
	public class NetworkTest
	{

		private NetworkAdapter.NetworkAdapter networkPlugin;

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp ()
		{
			networkPlugin = new NetworkAdapter.NetworkAdapter();	
		}

		/// <summary>
		/// This method is called after every test method.
		/// </summary>
		[TearDown]
		public void TearDown ()
		{
			// Nothing
		}
		#endregion

		#region Testmethods

		/// <summary>
		/// TTests the method GetIndicatorSettings().
		/// </summary>
		[Test]
		public void TestShouldHaveMultipleNetworkDevices(){
			Assert.IsNotEmpty(networkPlugin.GetNetworkInterfaces());
		}

		[Test]
		public void TestShouldHaveNoLoAdapter(){
			foreach(string s in networkPlugin.GetNetworkInterfaces()){
				Assert.AreNotEqual(s,"lo");
			}
			
		}
		
		[Test]
		public void TestShouldHaveAEth0Adapter(){
			Assert.Contains("eth0",networkPlugin.GetNetworkInterfaces());
			
		}

		
		[Test]
		public void TestShouldContainAllNetworkadapters(){
			foreach(String s in networkPlugin.GetNetworkInterfaces()){
				Assert.True(((string)(networkPlugin.GetNamePerAdapter().Item2)).Contains(s));
			}
		}
		
		[Test]
		public void TestGetIpPerAdapter(){
			Assert.AreEqual(
				networkPlugin.GetNetworkInterfaces().Count, 
				((string)networkPlugin.GetIPPerAdapter().Item2).Split(';').Length);
		}
		
		[Test]
		public void TestGetUpload(){
			foreach(string dev in networkPlugin.GetNetworkInterfaces()){
				float f = networkPlugin.GetCurrentUpload(dev);
				if (f > 0){
					Thread.Sleep(1000);
					float n = networkPlugin.GetCurrentUpload(dev);
					Assert.IsTrue (n>0);
					Assert.IsTrue (f>n);
				}
			}
		}
		
		[Test]
		public void TestGetDownload(){
			foreach(string dev in networkPlugin.GetNetworkInterfaces()){
				float f = networkPlugin.GetCurrentDownload(dev);
				if (f > 0){
					Thread.Sleep(1000);
					float n = networkPlugin.GetCurrentDownload(dev);
					Assert.IsTrue (n>0);
					Assert.IsTrue (f>n);
				}
			}
		}


		
		[Test]
		public void TestGetMacPerAdapter(){
			Assert.AreEqual(
				networkPlugin.GetNetworkInterfaces().Count, 
				((string)networkPlugin.GetMACPerAdapter().Item2).Split(';').Length-1);
			foreach(string mac in ((string)networkPlugin.GetMACPerAdapter().Item2).Split(';')){
				if (!mac.Equals("")){
					string[] mac_parts =mac.Split(':'); 
					foreach(string s in mac_parts){
						if (!s.Equals("")){
							try{
								Int64.Parse(s, System.Globalization.NumberStyles.HexNumber);
							}catch(Exception){
								
								Assert.Fail("<"+s+">No Hex");
							}
						}
					}
					Assert.AreEqual(6,mac_parts.Length);
				}
			}
		}
		
		[Test]
		public void TestGetDownPerAdapter(){
			Assert.AreEqual(
				networkPlugin.GetNetworkInterfaces().Count, 
				((string)networkPlugin.GetDownPerAdapter().Item2).Split(';').Length-1);
		}
		
		[Test]
		public void TestGetUpPerAdapter(){
			Assert.AreEqual(
				networkPlugin.GetNetworkInterfaces().Count, 
				((string)networkPlugin.GetUpPerAdapter().Item2).Split(';').Length-1);
		}
		[Test]
		public void TestGetIndicatorSettings ()
		{
			List<IndicatorSettings> indicatorSettings = networkPlugin.GetIndicatorSettings ();
			Assert.NotNull (indicatorSettings, "NetworkTest:TestGetIndicatorSettings() #01");
			Assert.IsNotEmpty (indicatorSettings, "NetworkTest:TestGetIndicatorSettings() #02");

			foreach (IndicatorSettings indicator in indicatorSettings)
			{
				Assert.NotNull(indicator, "NetworkTest:TestGetIndicatorSettings() #03");

				Assert.NotNull (indicator.PluginName, "NetworkTest:TestGetIndicatorSettings() #04");
				Assert.IsNotEmpty (indicator.PluginName, "NetworkTest:TestGetIndicatorSettings() #05");
				Assert.AreEqual ("NetworkAdapter", indicator.PluginName, "NetworkTest:TestGetIndicatorSettings() #06");

				Assert.NotNull (indicator.IndicatorName, "NetworkTest:TestGetIndicatorSettings() #07");
				Assert.IsNotEmpty (indicator.IndicatorName, "NetworkTest:TestGetIndicatorSettings() #08");

				Assert.NotNull (indicator.MonitoredSystemMAC, "NetworkTest:TestGetIndicatorSettings() #09");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "NetworkTest:TestGetIndicatorSettings() #10");

				Assert.NotNull (indicator.FilterStatement, "NetworkTest:TestGetIndicatorSettings() #11");
				Assert.IsNotEmpty (indicator.FilterStatement, "NetworkTest:TestGetIndicatorSettings() #12");

				Assert.NotNull (indicator.UpdateInterval, "NetworkTest:TestGetIndicatorSettings() #13");
				Assert.Greater (indicator.UpdateInterval.Ticks, 0L, "NetworkTest:TestGetIndicatorSettings() #14");

				Assert.NotNull (indicator.StorageDuration, "NetworkTest:TestGetIndicatorSettings() #15");
				Assert.Greater (indicator.StorageDuration.Ticks, 0L, "NetworkTest:TestGetIndicatorSettings() #16");
			
				Assert.NotNull (indicator.MappingDuration, "NetworkTest:TestGetIndicatorSettings() #17");
				Assert.Greater (indicator.MappingDuration.Ticks, 0L, "NetworkTest:TestGetIndicatorSettings() #18");

				Assert.NotNull (indicator.DataType, "NetworkTest:TestGetIndicatorSettings() #19");
				bool dataTypeMatched = false;
				foreach (var dataType in Enum.GetValues (typeof(MISD.Core.DataType)))
				{
					if (dataType.Equals(indicator.DataType))
					{
						dataTypeMatched = true;
						break;
					}
				}
				Assert.True (dataTypeMatched, "NetworkTest:TestGetIndicatorSettings() #20");

				Assert.NotNull (indicator.MetricWarning, "NetworkTest:TestGetIndicatorSettings() #21");		
				Assert.NotNull (indicator.MetricCritical, "NetworkTest:TestGetIndicatorSettings() #22");

				Assert.NotNull (indicator.MonitoredSystemMAC, "NetworkTest:TestGetIndicatorSettings() #23");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "NetworkTest:TestGetIndicatorSettings() #24");
			}

		}

		/// <summary>
		/// Tests the get mechanism of the property TargetPlatform.
		/// </summary>
		[Test]
		public void TestGetTargetPlatform ()
		{
			MISD.Core.Platform targetPlatform = networkPlugin.TargetPlatform;
			Assert.NotNull (targetPlatform, "NetworkTest:TestGetTargetPlatform() #01");
			Assert.AreEqual(MISD.Core.Platform.Linux, targetPlatform, "NetworkTest:TestGetTargetPlatform() #02");
		}

		/// <summary>
		/// Tests the method AcquireData().
		/// </summary>
		[Test]
		public void TestAcquireDataNoParam ()
		{
			List<IndicatorSettings> indicatorSettings = networkPlugin.GetIndicatorSettings();

			List<Tuple<string, Object, DataType>> resultList;
			resultList = networkPlugin.AcquireData ();

			Assert.NotNull (resultList, "NetworkTest:TestAcquireDataNoParam() #01");
			Assert.IsNotEmpty (resultList, "NetworkTest:TestAcquireDataNoParam() #02");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "NetworkTest:TestAcquireDataNoParam() #03");
				Assert.IsNotEmpty (tuple.Item1, "NetworkTest:TestAcquireDataNoParam() #04");
				Assert.NotNull (tuple.Item3, "NetworkTest:TestAcquireDataNoParam() #05");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "NetworkTest:TestAcquireDataNoParam() #06");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings) {
					if (indicatorSetting.IndicatorName == tuple.Item1) {
						Assert.True (indicatorSetting.DataType.Equals (tuple.Item3), "NetworkTest:TestAcquireDataNoParam() #07");
					}
				}

				if (tuple.Item2 != null)
				{
					System.Type sollType;
					switch (tuple.Item3.ToString())
					{
					case "String":
						sollType = typeof(string);
						break;
					case "Byte":
						sollType = typeof(byte);
						break;
					case "Int":
						sollType = typeof(int);
						break;
					case "Float":
						sollType = typeof(float);
						break;
					default:
						sollType = null;
						break;
					}
					if (sollType != null) {
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "NetworkTest:TestAcquireDataNoParam() #08"  + ":" + tuple.Item1.ToString());
					}
					else {
						throw new Exception("Old test code cause of new datatypes in MISD.Core.DataType. Please update the test code!");
					}
				}
			}
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorNames).
		/// </summary>
		[Test]
		public void TestAcquireDataWithParam ()
		{
			List<string> indicatorNames = new List<string> ();

			List<Tuple<string, Object, DataType>> resultList;
			resultList = networkPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "NetworkTest:TestAcquireDataWithParam() #01");
			Assert.IsEmpty (resultList, "NetworkTest:TestAcquireDataWithParam() #02");

			indicatorNames.Add ("");
			Assert.Throws (typeof(ArgumentOutOfRangeException), 
			          	delegate {
							networkPlugin.AcquireData (indicatorNames);
						}, "NetworkTest:TestAcquireDataWithParam() #03");

			indicatorNames.Clear ();
			List<IndicatorSettings> indicatorSettings = networkPlugin.GetIndicatorSettings ();
			foreach (IndicatorSettings indicator in indicatorSettings) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			resultList = networkPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "NetworkTest:TestAcquireDataWithParam() #04");
			Assert.IsNotEmpty (resultList, "NetworkTest:TestAcquireDataWithParam() #05");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "NetworkTest:TestAcquireDataWithParam() #06");
				Assert.IsNotEmpty (tuple.Item1, "NetworkTest:TestAcquireDataWithParam() #07");
				Assert.NotNull (tuple.Item3, "NetworkTest:TestAcquireDataWithParam() #08");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "NetworkTest:TestAcquireDataWithParam() #09");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings)
				{
					if (indicatorSetting.IndicatorName == tuple.Item1)
					{
						Assert.True (indicatorSetting.DataType.Equals(tuple.Item3), "NetworkTest:TestAcquireDataWithParam() #10");
					}
				}

				if (tuple.Item2 != null)
				{
					System.Type sollType;
					switch (tuple.Item3.ToString())
					{
					case "String":
						sollType = typeof(string);
						break;
					case "Byte":
						sollType = typeof(byte);
						break;
					case "Int":
						sollType = typeof(int);
						break;
					case "Float":
						sollType = typeof(float);
						break;
					default:
						sollType = null;
						break;
					}
					if (sollType != null) {
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "NetworkTest:TestAcquireDataWithParam() #10");
					}
					else {
						throw new Exception("Old test code cause of new datatypes in MISD.Core.DataType. Please update the test code!");
					}
				}
			}
		}

		/// <summary>
		/// Tests the method AcquireData(string monitoredSystemName).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster1 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("");
						}, "NetworkTest:TestAcquireDataCluster1() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("NUnit_TestClient");
						}, "NetworkTest:TestAcquireDataCluster1() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster2 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("");
						}, "NetworkTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("NUnit_TestClient");
						}, "NetworkTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster3 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("");
						}, "NetworkTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("NUnit_TestClient");
						}, "NetworkTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster4 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("");
						}, "NetworkTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							networkPlugin.AcquireData ("NUnit_TestClient");
						}, "NetworkTest:TestAcquireDataCluster2() #03");
		}
		#endregion
	}
}
