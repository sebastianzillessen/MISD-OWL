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
using CPU;

/// <summary>
/// CpuTest.cs - NUnit Test Cases for the plugin Cpu
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
	public class CpuTest
	{

		private IPlugin cpuPlugin;

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp ()
		{
			cpuPlugin = new Cpu();
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
			List<IndicatorSettings> indicatorSettings = cpuPlugin.GetIndicatorSettings ();
			Assert.NotNull (indicatorSettings, "CpuTest:TestGetIndicatorSettings() #01");
			Assert.IsNotEmpty (indicatorSettings, "CpuTest:TestGetIndicatorSettings() #02");

			foreach (IndicatorSettings indicator in indicatorSettings)
			{
				Assert.NotNull(indicator, "CpuTest:TestGetIndicatorSettings() #03");

				Assert.NotNull (indicator.PluginName, "CpuTest:TestGetIndicatorSettings() #04");
				Assert.IsNotEmpty (indicator.PluginName, "CpuTest:TestGetIndicatorSettings() #05");
				Assert.AreEqual ("CPU", indicator.PluginName, "CpuTest:TestGetIndicatorSettings() #06");

				Assert.NotNull (indicator.IndicatorName, "CpuTest:TestGetIndicatorSettings() #07");
				Assert.IsNotEmpty (indicator.IndicatorName, "CpuTest:TestGetIndicatorSettings() #08");

				Assert.NotNull (indicator.MonitoredSystemMAC, "CpuTest:TestGetIndicatorSettings() #09");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "CpuTest:TestGetIndicatorSettings() #10");

				Assert.NotNull (indicator.FilterStatement, "CpuTest:TestGetIndicatorSettings() #11");
				Assert.IsNotEmpty (indicator.FilterStatement, "CpuTest:TestGetIndicatorSettings() #12");

				Assert.NotNull (indicator.UpdateInterval, "CpuTest:TestGetIndicatorSettings() #13");
				Assert.Greater (indicator.UpdateInterval.Ticks, 0L, "CpuTest:TestGetIndicatorSettings() #14");

				Assert.NotNull (indicator.StorageDuration, "CpuTest:TestGetIndicatorSettings() #15");
				Assert.Greater (indicator.StorageDuration.Ticks, 0L, "CpuTest:TestGetIndicatorSettings() #16");
			
				Assert.NotNull (indicator.MappingDuration, "CpuTest:TestGetIndicatorSettings() #17");
				Assert.Greater (indicator.MappingDuration.Ticks, 0L, "CpuTest:TestGetIndicatorSettings() #18");

				Assert.NotNull (indicator.DataType, "CpuTest:TestGetIndicatorSettings() #19");
				bool dataTypeMatched = false;
				foreach (var dataType in Enum.GetValues (typeof(MISD.Core.DataType)))
				{
					if (dataType.Equals(indicator.DataType))
					{
						dataTypeMatched = true;
						break;
					}
				}
				Assert.True (dataTypeMatched, "CpuTest:TestGetIndicatorSettings() #20");

				Assert.NotNull (indicator.MetricWarning, "CpuTest:TestGetIndicatorSettings() #21");		
				Assert.NotNull (indicator.MetricCritical, "CpuTest:TestGetIndicatorSettings() #22");

				Assert.NotNull (indicator.MonitoredSystemMAC, "CpuTest:TestGetIndicatorSettings() #23");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "CpuTest:TestGetIndicatorSettings() #24");
			}

		}

		/// <summary>
		/// Tests the get mechanism of the property TargetPlatform.
		/// </summary>
		[Test]
		public void TestGetTargetPlatform ()
		{
			MISD.Core.Platform targetPlatform = cpuPlugin.TargetPlatform;
			Assert.NotNull (targetPlatform, "CpuTest:TestGetTargetPlatform() #01");
			Assert.AreEqual(MISD.Core.Platform.Linux, targetPlatform, "CpuTest:TestGetTargetPlatform() #02");
		}

		/// <summary>
		/// Tests the method AcquireData().
		/// </summary>
		[Test]
		public void TestAcquireDataNoParam ()
		{
			List<IndicatorSettings> indicatorSettings = cpuPlugin.GetIndicatorSettings();

			List<Tuple<string, Object, DataType>> resultList;
			resultList = cpuPlugin.AcquireData ();

			Assert.NotNull (resultList, "CpuTest:TestAcquireDataNoParam() #01");
			Assert.IsNotEmpty (resultList, "CpuTest:TestAcquireDataNoParam() #02");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "CpuTest:TestAcquireDataNoParam() #03");
				Assert.IsNotEmpty (tuple.Item1, "CpuTest:TestAcquireDataNoParam() #04");
				Assert.NotNull (tuple.Item3, "CpuTest:TestAcquireDataNoParam() #05");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "CpuTest:TestAcquireDataNoParam() #06");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings) {
					if (indicatorSetting.IndicatorName == tuple.Item1) {
						Assert.True (indicatorSetting.DataType.Equals (tuple.Item3), "CpuTest:TestAcquireDataNoParam() #07");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "CpuTest:TestAcquireDataNoParam() #08"  + ":" + tuple.Item1.ToString());
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
			resultList = cpuPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "CpuTest:TestAcquireDataWithParam() #01");
			Assert.IsEmpty (resultList, "CpuTest:TestAcquireDataWithParam() #02");

			indicatorNames.Add ("");
			Assert.Throws (typeof(ArgumentOutOfRangeException), 
			          	delegate {
							cpuPlugin.AcquireData (indicatorNames);
						}, "CpuTest:TestAcquireDataWithParam() #03");

			indicatorNames.Clear ();
			List<IndicatorSettings> indicatorSettings = cpuPlugin.GetIndicatorSettings ();
			foreach (IndicatorSettings indicator in indicatorSettings) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			resultList = cpuPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "CpuTest:TestAcquireDataWithParam() #04");
			Assert.IsNotEmpty (resultList, "CpuTest:TestAcquireDataWithParam() #05");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "CpuTest:TestAcquireDataWithParam() #06");
				Assert.IsNotEmpty (tuple.Item1, "CpuTest:TestAcquireDataWithParam() #07");
				Assert.NotNull (tuple.Item3, "CpuTest:TestAcquireDataWithParam() #08");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "CpuTest:TestAcquireDataWithParam() #09");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings)
				{
					if (indicatorSetting.IndicatorName == tuple.Item1)
					{
						Assert.True (indicatorSetting.DataType.Equals(tuple.Item3), "CpuTest:TestAcquireDataWithParam() #10");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "CpuTest:TestAcquireDataWithParam() #10");
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
							cpuPlugin.AcquireData ("");
						}, "CpuTest:TestAcquireDataCluster1() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("NUnit_TestClient");
						}, "CpuTest:TestAcquireDataCluster1() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster2 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("");
						}, "CpuTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("NUnit_TestClient");
						}, "CpuTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster3 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("");
						}, "CpuTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("NUnit_TestClient");
						}, "CpuTest:TestAcquireDataCluster2() #03");
		}

		/// <summary>
		/// Tests the method AcquireData(List<string> indicatorName, string monitoredSystemName, IClusterConnection clusterConnection).
		/// </summary>
		[Test]
		public void TestAcquireDataCluster4 ()
		{
			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("");
						}, "CpuTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							cpuPlugin.AcquireData ("NUnit_TestClient");
						}, "CpuTest:TestAcquireDataCluster2() #03");
		}
		#endregion
	}
}
