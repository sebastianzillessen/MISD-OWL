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
using System.Collections.Generic;
using System.Text;

using MISD.Core;
using RAM;

/// <summary>
/// RamTest.cs - NUnit Test Cases for the plugin Ram
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
	public class RamTest
	{

		private IPlugin ramPlugin;

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp ()
		{
			ramPlugin = new Ram ();
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
		public void TestGetIndicatorSettings ()
		{
			List<IndicatorSettings> indicatorSettings = ramPlugin.GetIndicatorSettings ();
			Assert.NotNull (indicatorSettings, "RamTest:TestGetIndicatorSettings() #01");
			Assert.IsNotEmpty (indicatorSettings, "RamTest:TestGetIndicatorSettings() #02");

			foreach (IndicatorSettings indicator in indicatorSettings)
			{
				Assert.NotNull(indicator, "RamTest:TestGetIndicatorSettings() #03");

				Assert.NotNull (indicator.PluginName, "RamTest:TestGetIndicatorSettings() #04");
				Assert.IsNotEmpty (indicator.PluginName, "RamTest:TestGetIndicatorSettings() #05");
				Assert.AreEqual ("RAM", indicator.PluginName, "RamTest:TestGetIndicatorSettings() #06");

				Assert.NotNull (indicator.IndicatorName, "RamTest:TestGetIndicatorSettings() #07");
				Assert.IsNotEmpty (indicator.IndicatorName, "RamTest:TestGetIndicatorSettings() #08");

				Assert.NotNull (indicator.MonitoredSystemMAC, "RamTest:TestGetIndicatorSettings() #09");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "RamTest:TestGetIndicatorSettings() #10");

				Assert.NotNull (indicator.FilterStatement, "RamTest:TestGetIndicatorSettings() #11");
				Assert.IsNotEmpty (indicator.FilterStatement, "RamTest:TestGetIndicatorSettings() #12");

				Assert.NotNull (indicator.UpdateInterval, "RamTest:TestGetIndicatorSettings() #13");
				Assert.Greater (indicator.UpdateInterval.Ticks, 0L, "RamTest:TestGetIndicatorSettings() #14");

				Assert.NotNull (indicator.StorageDuration, "RamTest:TestGetIndicatorSettings() #15");
				Assert.Greater (indicator.StorageDuration.Ticks, 0L, "RamTest:TestGetIndicatorSettings() #16");
			
				Assert.NotNull (indicator.MappingDuration, "RamTest:TestGetIndicatorSettings() #17");
				Assert.Greater (indicator.MappingDuration.Ticks, 0L, "RamTest:TestGetIndicatorSettings() #18");

				Assert.NotNull (indicator.DataType, "RamTest:TestGetIndicatorSettings() #19");
				bool dataTypeMatched = false;
				foreach (var dataType in Enum.GetValues (typeof(MISD.Core.DataType)))
				{
					if (dataType.Equals(indicator.DataType))
					{
						dataTypeMatched = true;
						break;
					}
				}
				Assert.True (dataTypeMatched, "RamTest:TestGetIndicatorSettings() #20");

				Assert.NotNull (indicator.MetricWarning, "RamTest:TestGetIndicatorSettings() #21");		
				Assert.NotNull (indicator.MetricCritical, "RamTest:TestGetIndicatorSettings() #22");

				Assert.NotNull (indicator.MonitoredSystemMAC, "RamTest:TestGetIndicatorSettings() #23");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "RamTest:TestGetIndicatorSettings() #24");
			}

		}

		/// <summary>
		/// Tests the get mechanism of the property TargetPlatform.
		/// </summary>
		[Test]
		public void TestGetTargetPlatform ()
		{
			MISD.Core.Platform targetPlatform = ramPlugin.TargetPlatform;
			Assert.NotNull (targetPlatform, "RamTest:TestGetTargetPlatform() #01");
			Assert.AreEqual(MISD.Core.Platform.Linux, targetPlatform, "RamTest:TestGetTargetPlatform() #02");
		}

		/// <summary>
		/// Tests the method AcquireData().
		/// </summary>
		[Test]
		public void TestAcquireDataNoParam ()
		{
			List<IndicatorSettings> indicatorSettings = ramPlugin.GetIndicatorSettings ();

			List<Tuple<string, Object, DataType>> resultList;
			resultList = ramPlugin.AcquireData ();

			Assert.NotNull (resultList, "RamTest:TestAcquireDataNoParam() #01");
			Assert.IsNotEmpty (resultList, "RamTest:TestAcquireDataNoParam() #02");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "RamTest:TestAcquireDataNoParam() #03");
				Assert.IsNotEmpty (tuple.Item1, "RamTest:TestAcquireDataNoParam() #04");
				Assert.NotNull (tuple.Item3, "RamTest:TestAcquireDataNoParam() #05");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "RamTest:TestAcquireDataNoParam() #06");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings) {
					if (indicatorSetting.IndicatorName == tuple.Item1) {
						Assert.True (indicatorSetting.DataType.Equals (tuple.Item3), "RamTest:TestAcquireDataNoParam() #07");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "RamTest:TestAcquireDataNoParam() #08");
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
			resultList = ramPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "RamTest:TestAcquireDataWithParam() #01");
			Assert.IsEmpty (resultList, "RamTest:TestAcquireDataWithParam() #02");

			indicatorNames.Add ("");
			Assert.Throws (typeof(ArgumentOutOfRangeException), 
			          	delegate {
							ramPlugin.AcquireData (indicatorNames);
						}, "RamTest:TestAcquireDataWithParam() #03");

			indicatorNames.Clear ();
			List<IndicatorSettings> indicatorSettings = ramPlugin.GetIndicatorSettings ();
			foreach (IndicatorSettings indicator in indicatorSettings) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			resultList = ramPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "RamTest:TestAcquireDataWithParam() #04");
			Assert.IsNotEmpty (resultList, "RamTest:TestAcquireDataWithParam() #05");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "RamTest:TestAcquireDataWithParam() #06");
				Assert.IsNotEmpty (tuple.Item1, "RamTest:TestAcquireDataWithParam() #07");
				Assert.NotNull (tuple.Item3, "RamTest:TestAcquireDataWithParam() #08");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "RamTest:TestAcquireDataWithParam() #09");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings)
				{
					if (indicatorSetting.IndicatorName == tuple.Item1)
					{
						Assert.True (indicatorSetting.DataType.Equals(tuple.Item3), "RamTest:TestAcquireDataWithParam() #10");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "RamTest:TestAcquireDataWithParam() #11");
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
							ramPlugin.AcquireData ("");
						}, "RamTest:TestAcquireDataCluster1() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("NUnit_TestClient");
						}, "RamTest:TestAcquireDataCluster1() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster2 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("");
						}, "RamTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("NUnit_TestClient");
						}, "RamTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster3 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("");
						}, "RamTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("NUnit_TestClient");
						}, "RamTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster4 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("");
						}, "RamTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							ramPlugin.AcquireData ("NUnit_TestClient");
						}, "RamTest:TestAcquireDataCluster2() #03");
		}
		#endregion
	}
}
