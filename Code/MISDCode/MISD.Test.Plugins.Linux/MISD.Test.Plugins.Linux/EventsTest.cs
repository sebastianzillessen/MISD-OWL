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
using Events;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

/// <summary>
/// OsTest.cs - NUnit Test Cases for the plugin Network
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
	public class EventsTest
	{

		private Events.Events eventsPlugin;

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp ()
		{
			eventsPlugin = new Events.Events ();	
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
		
		#region helper
		public String generateLogEntry (String s = null)
		{
			if (s == null)
				s = "Test_"+(new Random()).Next();
			try {
				// This is the code for the base process
	            Process myProcess = new Process();
	            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("logger", s);
	            myProcess.StartInfo = myProcessStartInfo;
	            myProcess.Start();
	            myProcess.WaitForExit();
	            myProcess.Close();
			} catch (Exception e) {
				throw new Exception (e.Message);
			}	
			return s;
		}
		#endregion
		#region Testmethods

		/// <summary>
		/// TTests the method GetIndicatorSettings().
		/// </summary>
		[Test]
		public void TestShouldGetAListOfEvents ()
		{
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsEmpty(eventsPlugin.LastLogs());
		}
		
		[Test]
		public void AddingAnItemShouldReturnOneElement ()
		{
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsEmpty(eventsPlugin.LastLogs());
			String e = generateLogEntry();
			Thread.Sleep(1000);
			List<string> l = eventsPlugin.LastLogs ();
			Assert.IsNotEmpty(l);
			Assert.AreEqual(1,l.Count);
			Assert.IsTrue(l[0].Contains(e));
			
		}
		
		[Test]
		public void ListShouldBeEmptyAfterReading ()
		{
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsEmpty(eventsPlugin.LastLogs());
			generateLogEntry();
			Thread.Sleep(1000);
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsEmpty(eventsPlugin.LastLogs());
			generateLogEntry();
			Thread.Sleep(1000);
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsEmpty(eventsPlugin.LastLogs());
		}
		
		[Test]
		public void GetEventsShouldContainOneTuple (){
			Assert.IsNotEmpty (eventsPlugin.LastLogs ());
			Assert.IsInstanceOf<List<Tuple<string,object,DataType>>>(eventsPlugin.GetEvents());
			Assert.IsEmpty(eventsPlugin.GetEvents());
			String e = generateLogEntry();
			generateLogEntry(e+"v2");
			List<Tuple<string, object, DataType>> l = eventsPlugin.GetEvents();
			foreach(Tuple<string,object,DataType> i in l){
				Console.WriteLine (i.Item2);
				Assert.AreEqual("Event",i.Item1);
				Assert.IsTrue(((string)(i.Item2)).Contains(e));
				Assert.AreEqual(DataType.String,i.Item3);
			}
			Assert.AreEqual(2, l.Count);
			
			Assert.IsEmpty(eventsPlugin.GetEvents());
			
		}
		
		
		#endregion
		
		#region notworking_tests
		
		/*
		[Test]
		public void TestGetIndicatorSettings ()
		{
			List<IndicatorSettings> indicatorSettings = eventsPlugin.GetIndicatorSettings ();
			Assert.NotNull (indicatorSettings, "EventsTest:TestGetIndicatorSettings() #01");
			Assert.IsNotEmpty (indicatorSettings, "EventsTest:TestGetIndicatorSettings() #02");

			foreach (IndicatorSettings indicator in indicatorSettings)
			{
				Assert.NotNull(indicator, "EventsTest:TestGetIndicatorSettings() #03");

				Assert.NotNull (indicator.PluginName, "EventsTest:TestGetIndicatorSettings() #04");
				Assert.IsNotEmpty (indicator.PluginName, "EventsTest:TestGetIndicatorSettings() #05");
				Assert.AreEqual ("Os", indicator.PluginName, "EventsTest:TestGetIndicatorSettings() #06");

				Assert.NotNull (indicator.IndicatorName, "EventsTest:TestGetIndicatorSettings() #07");
				Assert.IsNotEmpty (indicator.IndicatorName, "EventsTest:TestGetIndicatorSettings() #08");

				Assert.NotNull (indicator.MonitoredSystemMAC, "EventsTest:TestGetIndicatorSettings() #09");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "EventsTest:TestGetIndicatorSettings() #10");

				Assert.NotNull (indicator.FilterStatement, "EventsTest:TestGetIndicatorSettings() #11");
				Assert.IsNotEmpty (indicator.FilterStatement, "EventsTest:TestGetIndicatorSettings() #12");

				Assert.NotNull (indicator.UpdateInterval, "EventsTest:TestGetIndicatorSettings() #13");
				Assert.Greater (indicator.UpdateInterval.Ticks, 0L, "EventsTest:TestGetIndicatorSettings() #14");

				Assert.NotNull (indicator.StorageDuration, "EventsTest:TestGetIndicatorSettings() #15");
				Assert.Greater (indicator.StorageDuration.Ticks, 0L, "EventsTest:TestGetIndicatorSettings() #16");
			
				Assert.NotNull (indicator.MappingDuration, "EventsTest:TestGetIndicatorSettings() #17");
				Assert.Greater (indicator.MappingDuration.Ticks, 0L, "EventsTest:TestGetIndicatorSettings() #18");

				Assert.NotNull (indicator.DataType, "EventsTest:TestGetIndicatorSettings() #19");
				bool dataTypeMatched = false;
				foreach (var dataType in Enum.GetValues (typeof(MISD.Core.DataType)))
				{
					if (dataType.Equals(indicator.DataType))
					{
						dataTypeMatched = true;
						break;
					}
				}
				Assert.True (dataTypeMatched, "EventsTest:TestGetIndicatorSettings() #20");

				Assert.NotNull (indicator.MetricWarning, "EventsTest:TestGetIndicatorSettings() #21");		
				Assert.NotNull (indicator.MetricCritical, "EventsTest:TestGetIndicatorSettings() #22");

				Assert.NotNull (indicator.MonitoredSystemMAC, "EventsTest:TestGetIndicatorSettings() #23");
				Assert.IsEmpty (indicator.MonitoredSystemMAC, "EventsTest:TestGetIndicatorSettings() #24");
			}

		}

		/// <summary>
		/// Tests the get mechanism of the property TargetPlatform.
		/// </summary>
		[Test]
		public void TestGetTargetPlatform ()
		{
			MISD.Core.Platform targetPlatform = osPlugin.TargetPlatform;
			Assert.NotNull (targetPlatform, "EventsTest:TestGetTargetPlatform() #01");
			Assert.AreEqual(MISD.Core.Platform.Linux, targetPlatform, "EventsTest:TestGetTargetPlatform() #02");
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

			Assert.NotNull (resultList, "EventsTest:TestAcquireDataNoParam() #01");
			Assert.IsNotEmpty (resultList, "EventsTest:TestAcquireDataNoParam() #02");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "EventsTest:TestAcquireDataNoParam() #03");
				Assert.IsNotEmpty (tuple.Item1, "EventsTest:TestAcquireDataNoParam() #04");
				Assert.NotNull (tuple.Item3, "EventsTest:TestAcquireDataNoParam() #05");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "EventsTest:TestAcquireDataNoParam() #06");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings) {
					if (indicatorSetting.IndicatorName == tuple.Item1) {
						Assert.True (indicatorSetting.DataType.Equals (tuple.Item3), "EventsTest:TestAcquireDataNoParam() #07");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "EventsTest:TestAcquireDataNoParam() #08"  + ":" + tuple.Item1.ToString());
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
			Assert.NotNull (resultList, "EventsTest:TestAcquireDataWithParam() #01");
			Assert.IsEmpty (resultList, "EventsTest:TestAcquireDataWithParam() #02");

			indicatorNames.Add ("");
			Assert.Throws (typeof(ArgumentOutOfRangeException), 
			          	delegate {
							osPlugin.AcquireData (indicatorNames);
						}, "EventsTest:TestAcquireDataWithParam() #03");

			indicatorNames.Clear ();
			List<IndicatorSettings> indicatorSettings = osPlugin.GetIndicatorSettings ();
			foreach (IndicatorSettings indicator in indicatorSettings) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			resultList = osPlugin.AcquireData (indicatorNames);
			Assert.NotNull (resultList, "EventsTest:TestAcquireDataWithParam() #04");
			Assert.IsNotEmpty (resultList, "EventsTest:TestAcquireDataWithParam() #05");
			foreach (Tuple<string, Object, DataType> tuple in resultList) {
				Assert.NotNull (tuple, "EventsTest:TestAcquireDataWithParam() #06");
				Assert.IsNotEmpty (tuple.Item1, "EventsTest:TestAcquireDataWithParam() #07");
				Assert.NotNull (tuple.Item3, "EventsTest:TestAcquireDataWithParam() #08");
				bool validDataType = false;
				foreach (var dataType in Enum.GetValues(typeof(MISD.Core.DataType))) {
					if (tuple.Item3.Equals (dataType)) {
						validDataType = true;
						break;
					}
				}
				Assert.True (validDataType, "EventsTest:TestAcquireDataWithParam() #09");

				foreach (IndicatorSettings indicatorSetting in indicatorSettings)
				{
					if (indicatorSetting.IndicatorName == tuple.Item1)
					{
						Assert.True (indicatorSetting.DataType.Equals(tuple.Item3), "EventsTest:TestAcquireDataWithParam() #10");
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
						Assert.AreEqual(sollType, tuple.Item2.GetType(), "EventsTest:TestAcquireDataWithParam() #10");
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
						}, "EventsTest:TestAcquireDataCluster1() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "EventsTest:TestAcquireDataCluster1() #03");
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
						}, "EventsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "EventsTest:TestAcquireDataCluster2() #03");
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
						}, "EventsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "EventsTest:TestAcquireDataCluster2() #03");
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
						}, "EventsTest:TestAcquireDataCluster2() #03");

			Assert.Throws (typeof(NotImplementedException), 
			          	delegate {
							osPlugin.AcquireData ("NUnit_TestClient");
						}, "EventsTest:TestAcquireDataCluster2() #03");
		}
		#endregion
		*/
		#endregion
	}
}
