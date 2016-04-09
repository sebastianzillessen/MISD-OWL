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
 * Version 3 or any later versioosPlugin. For details see here:
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
using OS;
using System.Threading;

/// <summary>
/// OsTest.cs - NUnit Test Cases for the plugin Network
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
	public class OsTest
	{

		private Os osPlugin;

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp ()
		{
			osPlugin = new Os();	
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
		public void TestShouldHaveAUptime(){
			Assert.IsNotNull(osPlugin.getUptimeAsTuple());
			Assert.IsNotNull(osPlugin.getUptimeAsTuple().Item2);
		}
		
		[Test]
		public void UptimeShouldBeGreaterThen0(){
			TimeSpan t = TimeSpan.Parse((string)osPlugin.getUptimeAsTuple().Item2);
			Assert.IsNotNull(t);
			Assert.Greater(t.TotalSeconds, 0);
		}
		
		
		[Test]
		public void OSSystemShouldExist(){
			Assert.NotNull(osPlugin.getNameAsTuple());
			Assert.NotNull(osPlugin.getNameAsTuple().Item2);
			Assert.IsTrue(((string)osPlugin.getNameAsTuple().Item2).Length > 0);
		}
		
		[Test]
		public void VersionSystemShouldExist(){
			Assert.NotNull(osPlugin.getVersionAsTuple());
			Assert.NotNull(osPlugin.getVersionAsTuple().Item2);
			Assert.IsTrue(((string)osPlugin.getVersionAsTuple().Item2).Length > 0);
		}
	
		[Test]
		public void TestGetIndicatorSettings ()
		{
			List<IndicatorSettings> indicatorSettings = osPlugin.GetIndicatorSettings ();
			Assert.NotNull (indicatorSettings, "OsTest:TestGetIndicatorSettings() #01");
			Assert.IsNotEmpty (indicatorSettings, "OsTest:TestGetIndicatorSettings() #02");

			foreach (IndicatorSettings indicator in indicatorSettings)
			{
				Assert.NotNull(indicator, "OsTest:TestGetIndicatorSettings() #03");

				Assert.NotNull (indicator.PluginName, "OsTest:TestGetIndicatorSettings() #04");
				Assert.IsNotEmpty (indicator.PluginName, "OsTest:TestGetIndicatorSettings() #05");
				Assert.AreEqual ("Os", indicator.PluginName, "OsTest:TestGetIndicatorSettings() #06");

				Assert.NotNull (indicator.IndicatorName, "OsTest:TestGetIndicatorSettings() #07");
				Assert.IsNotEmpty (indicator.IndicatorName, "OsTest:TestGetIndicatorSettings() #08");

				Assert.NotNull (indicator.MonitoredSystemMAC, "OsTest:TestGetIndicatorSettings() #09");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "OsTest:TestGetIndicatorSettings() #10");

				Assert.NotNull (indicator.FilterStatement, "OsTest:TestGetIndicatorSettings() #11");
				Assert.IsNotEmpty (indicator.FilterStatement, "OsTest:TestGetIndicatorSettings() #12");

				Assert.NotNull (indicator.UpdateInterval, "OsTest:TestGetIndicatorSettings() #13");
				Assert.Greater (indicator.UpdateInterval.Ticks, 0L, "OsTest:TestGetIndicatorSettings() #14");

				Assert.NotNull (indicator.StorageDuration, "OsTest:TestGetIndicatorSettings() #15");
				Assert.Greater (indicator.StorageDuration.Ticks, 0L, "OsTest:TestGetIndicatorSettings() #16");
			
				Assert.NotNull (indicator.MappingDuration, "OsTest:TestGetIndicatorSettings() #17");
				Assert.Greater (indicator.MappingDuration.Ticks, 0L, "OsTest:TestGetIndicatorSettings() #18");

				Assert.NotNull (indicator.DataType, "OsTest:TestGetIndicatorSettings() #19");
				bool dataTypeMatched = false;
				foreach (var dataType in Enum.GetValues (typeof(MISD.Core.DataType)))
				{
					if (dataType.Equals(indicator.DataType))
					{
						dataTypeMatched = true;
						break;
					}
				}
				Assert.True (dataTypeMatched, "OsTest:TestGetIndicatorSettings() #20");

				Assert.NotNull (indicator.MetricWarning, "OsTest:TestGetIndicatorSettings() #21");		
				Assert.NotNull (indicator.MetricCritical, "OsTest:TestGetIndicatorSettings() #22");

				Assert.NotNull (indicator.MonitoredSystemMAC, "OsTest:TestGetIndicatorSettings() #23");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "OsTest:TestGetIndicatorSettings() #24");
			}

		}

		/// <summary>
		/// Tests the get mechanism of the property TargetPlatform.
		/// </summary>
		[Test]
		public void TestGetTargetPlatform ()
		{
			MISD.Core.Platform targetPlatform = osPlugin.TargetPlatform;
			Assert.NotNull (targetPlatform, "OsTest:TestGetTargetPlatform() #01");
			Assert.AreEqual(MISD.Core.Platform.Linux, targetPlatform, "OsTest:TestGetTargetPlatform() #02");
		}

		/// <summary>
		/// Tests the method AcquireData().
		/// </summary>
		[Test]
		public void TestAcquireDataNoParam ()
		{
			List<IndicatorSettings> indicatorSettings = osPlugin.GetIndicatorSettings();

			List<Tuple<string, Object, DataType>> resultList;
			resultList = osPlugin.AcquireData ();

			Assert.NotNull (resultList, "OsTest:TestAcquireDataNoParam() #01");
			Assert.IsNotEmpty (resultList, "OsTest:TestAcquireDataNoParam() #02");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "OsTest:TestAcquireDataNoParam() #03");
				Assert.IsNotEmpty (tuple.Item1, "OsTest:TestAcquireDataNoParam() #04");
				Assert.NotNull (tuple.Item3, "OsTest:TestAcquireDataNoParam() #05");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "OsTest:TestAcquireDataNoParam() #06");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings) {
					if (indicatorSetting.IndicatorName == tuple.Item1) {
						Assert.True (indicatorSetting.DataType.Equals (tuple.Item3), "OsTest:TestAcquireDataNoParam() #07");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "OsTest:TestAcquireDataNoParam() #08"  + ":" + tuple.Item1.ToString());
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
			resultList = osPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "OsTest:TestAcquireDataWithParam() #01");
			Assert.IsEmpty (resultList, "OsTest:TestAcquireDataWithParam() #02");

			indicatorNames.Add ("");
			Assert.Throws (typeof(ArgumentOutOfRangeException), 
			          	delegate {
							osPlugin.AcquireData (indicatorNames);
						}, "OsTest:TestAcquireDataWithParam() #03");

			indicatorNames.Clear ();
			List<IndicatorSettings> indicatorSettings = osPlugin.GetIndicatorSettings ();
			foreach (IndicatorSettings indicator in indicatorSettings) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			resultList = osPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "OsTest:TestAcquireDataWithParam() #04");
			Assert.IsNotEmpty (resultList, "OsTest:TestAcquireDataWithParam() #05");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "OsTest:TestAcquireDataWithParam() #06");
				Assert.IsNotEmpty (tuple.Item1, "OsTest:TestAcquireDataWithParam() #07");
				Assert.NotNull (tuple.Item3, "OsTest:TestAcquireDataWithParam() #08");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "OsTest:TestAcquireDataWithParam() #09");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings)
				{
					if (indicatorSetting.IndicatorName == tuple.Item1)
					{
						Assert.True (indicatorSetting.DataType.Equals(tuple.Item3), "OsTest:TestAcquireDataWithParam() #10");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "OsTest:TestAcquireDataWithParam() #10");
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
							osPlugin.AcquireData ("");
						}, "OsTest:TestAcquireDataCluster1() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "OsTest:TestAcquireDataCluster1() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster2 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("");
						}, "OsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "OsTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster3 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("");
						}, "OsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "OsTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster4 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("");
						}, "OsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "OsTest:TestAcquireDataCluster2() #03");
		}
		#endregion
	}
}
