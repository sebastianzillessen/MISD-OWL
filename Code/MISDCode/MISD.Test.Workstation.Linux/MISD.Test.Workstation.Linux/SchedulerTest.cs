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
/// SchedulerTest.cs - NUnit Test Cases for Scheduler
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
    public class SchedulerTest {

		#region SetUp() and TearDown()
		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			// Nothing
		}

		/// <summary>
		/// This method is called after every test method.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			// Nothing
		}
		#endregion

		#region Testmethods

		/// <summary>
		/// Tests the get mechanism of the property Instance.
		/// </summary>
		[Test]
		public void TestGetInstance() {
			Assert.NotNull(MISD.Workstation.Linux.Scheduling.Scheduler.Instance, "SchedulerTest:TestGetInstance() #01");
		}
		#endregion
	}
}
