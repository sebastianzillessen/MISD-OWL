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
using System.Collections.Generic;
using System.Text;

using MISD.Core;

/// <summary>
/// IndicatorTimerJobTest.cs - NUnit Test Cases for IndicatorTimerJob
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
    public class IndicatorTimerJobTest {

		#region SetUp() and TearDown()
		/// <summary>
		/// Attributes to save the current state of the IndicatorTimerJob.
		/// </summary>
		Scheduling.TimerJobs.IndicatorTimerJob myIndicatorJob;
		MISD.Core.IPlugin myIPlugin;
		string myIndicator;
		TimeSpan myInterval;

		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			myInterval = new TimeSpan (0, 0, 30);
			myIndicator = "ProcessorName";
			myIPlugin = new MyTestPlugin();
			myIndicatorJob = new Scheduling.TimerJobs.IndicatorTimerJob (myIPlugin, myIndicator, myInterval);
		}

		/// <summary>
		/// This method is called after every test method.
		/// </summary>
		[TearDown]
		public void TearDown()
		{

		}
		#endregion

		#region Testmethods

		/// <summary>
		/// Tests the get mechanism of the property Indicator.
		/// </summary>
		[Test]
		public void TestGetIndicator() {
			Assert.AreEqual(myIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestGetIndicator() #01");

			string newIndicator = "Ram";
			myIndicatorJob.Indicator = newIndicator;
			Assert.AreEqual(newIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestGetIndicator() #02");
		}

		/// <summary>
		/// Tests the set mechanism of the property Indicator.
		/// </summary>
		[Test]
		public void TestSetIndicator() {
			string newIndicator = "Ram";
			myIndicatorJob.Indicator = newIndicator;
			Assert.AreEqual(newIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestSetIndicator() #01");

			newIndicator = "Cpu";
			myIndicatorJob.Indicator = newIndicator;
			Assert.AreEqual(newIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestSetIndicator() #02");
		}
		#endregion
	}
}

public class MyTestPlugin : MISD.Core.IPlugin {

    public List<IndicatorSettings> GetIndicatorSettings()
	{
		return new List<IndicatorSettings>();;
	}

    public Platform TargetPlatform
	{ 
		get
		{
        	return Platform.Linux;
    	}
	}

	public List<Tuple<string, object, DataType>> AcquireData ()
	{
		List<string> indicatorNames = new List<string> ();
		
		foreach (IndicatorSettings indicator in GetIndicatorSettings()) {
			indicatorNames.Add (indicator.IndicatorName);
		}
		
		return AcquireData (indicatorNames);
	}

	public List<Tuple<string, object, DataType>> AcquireData (List<string> indicatorNames)
	{
		List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>> ();
		return result;
	}

    public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName)
	{
		throw new NotImplementedException ("This method is accessible for clusters only.");
	}

    public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName, ClusterConnection clusterConnection)
	{
		throw new NotImplementedException ("This method is accessible for clusters only.");
	}

    public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
	{
		throw new NotImplementedException ("This method is accessible for clusters only.");
	}

    public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, ClusterConnection clusterConnection)
	{
		throw new NotImplementedException ("This method is accessible for clusters only.");
	}

}