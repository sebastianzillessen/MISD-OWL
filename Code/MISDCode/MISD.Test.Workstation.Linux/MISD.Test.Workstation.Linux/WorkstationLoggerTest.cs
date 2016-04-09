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
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// WorkstationLoggerTest.cs - NUnit Test Cases for WorkstationLogger
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
    public class WorkstationLoggerTest {

		#region SetUp() and TearDown()
		/// <summary>
		/// Attributes to save the current state of the WorkstationLogger.
		/// </summary>
		private bool savedLogMode;
		private bool savedConsoleMode;
		private string savedLogPath;
		private string savedLogFileName;

		/// <summary>
		/// This method is called before every test method.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			savedLogMode = WorkstationLogger.Instance.LogMode;
			savedConsoleMode = WorkstationLogger.Instance.ConsoleMode;
			savedLogPath = WorkstationLogger.Instance.LogPath;
			savedLogFileName = WorkstationLogger.Instance.LogFileName;
		}

		/// <summary>
		/// This method is called after every test method.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			try
			{
				WorkstationLogger.Instance.LogMode = savedLogMode;
				WorkstationLogger.Instance.ConsoleMode = savedConsoleMode;
				WorkstationLogger.Instance.LogPath = savedLogPath;
				WorkstationLogger.Instance.LogFileName = savedLogFileName;
			}
			catch (Exception)
			{
				throw new NullReferenceException ("TearDown() was not possible");
			}
		}
		#endregion

		#region Testmethods
		/// <summary>
		/// Tests the set mechanism of the property LogMode.
		/// </summary>
		[Test]
		public void TestSetLogMode ()
		{
			WorkstationLogger.Instance.LogMode = true;
			Assert.IsTrue (WorkstationLogger.Instance.LogMode, "WorkstationLoggerTest:TestSetLogMode() #01");
			WorkstationLogger.Instance.LogMode = false;
			Assert.IsFalse (WorkstationLogger.Instance.LogMode, "WorkstationLoggerTest:TestSetLogMode() #02");
		}

		/// <summary>
		/// Tests the get mechanism of the property LogMode.
		/// </summary>
		[Test]
		public void TestGetLogMode ()
		{
			Assert.IsFalse (WorkstationLogger.Instance.LogMode, "WorkstationLoggerTest:TestGetLogMode() #01");
			WorkstationLogger.Instance.LogMode = true;
			Assert.IsTrue (WorkstationLogger.Instance.LogMode, "WorkstationLoggerTest:TestGetLogMode() #02");
		}

		/// <summary>
		/// Tests the set mechanism of the property ConsoleMode.
		/// </summary>
		[Test]
		public void TestSetConsoleMode ()
		{
			WorkstationLogger.Instance.ConsoleMode = true;
			Assert.IsTrue (WorkstationLogger.Instance.ConsoleMode, "WorkstationLoggerTest:TestSetConsoleMode() #01");
			WorkstationLogger.Instance.ConsoleMode = false;
			Assert.IsFalse (WorkstationLogger.Instance.ConsoleMode, "WorkstationLoggerTest:TestSetConsoleMode() #02");
		}

		/// <summary>
		/// Tests the get mechanism of the property ConsoleMode.
		/// </summary>
		[Test]
		public void TestGetConsoleMode ()
		{
			Assert.IsFalse (WorkstationLogger.Instance.ConsoleMode, "WorkstationLoggerTest:TestGetConsoleMode() #01");
			WorkstationLogger.Instance.ConsoleMode = true;
			Assert.IsTrue (WorkstationLogger.Instance.ConsoleMode, "WorkstationLoggerTest:TestGetConsoleMode() #02");
		}

		/// <summary>
		/// Tests the get mechanism of the property LogPath.
		/// </summary>
		[Test]
		public void TestSetLogPath ()
		{
			WorkstationLogger.Instance.LogPath = "newLogPath1";
			Assert.AreEqual ("newLogPath1", WorkstationLogger.Instance.LogPath, "WorkstationLoggerTest:TestSetLogPath() #01");
		}

		/// <summary>
		/// Tests the get mechanism of the property LogPath.
		/// </summary>
		[Test]
		public void TestGetLogPath ()
		{
			Assert.AreEqual ("/tmp/misd/", WorkstationLogger.Instance.LogPath, "WorkstationLoggerTest:TestGetLogPath() #01");
			WorkstationLogger.Instance.LogPath = "newLogPath2";
			Assert.AreEqual ("newLogPath2", WorkstationLogger.Instance.LogPath, "WorkstationLoggerTest:TestGetLogPath() #02");
		}

		/// <summary>
		/// Tests the get mechanism of the property LogFileName.
		/// </summary>
		[Test]
		public void TestSetLogFileName ()
		{
			WorkstationLogger.Instance.LogFileName = "newLogFileName";
			Assert.AreEqual ("newLogFileName", WorkstationLogger.Instance.LogFileName, "WorkstationLoggerTest:TestSetLogFileName() #01");
		}

		/// <summary>
		/// Tests the get mechanism of the property LogFileName.
		/// </summary>
		[Test]
		public void TestGetLogFileName ()
		{
			this.TearDown();
			Assert.AreEqual ("misdlog.log", WorkstationLogger.Instance.LogFileName, "WorkstationLoggerTest:TestGetLogFileName() #01");
			WorkstationLogger.Instance.LogFileName = "newLogFileName2";
			Assert.AreEqual ("newLogFileName2", WorkstationLogger.Instance.LogFileName, "WorkstationLoggerTest:TestGetLogFileName() #02");
		}


		/// <summary>
		/// Tests the WriteLog().
		/// Only the logging with the log file.
		/// </summary>
		[Test]
		public void TestWriteLog()
		{
			string message = "[NUnitTest_WorkstationLogger]";
			Regex regex = new Regex(message);
			string logPath = WorkstationLogger.Instance.LogPath;
			string logFileName = WorkstationLogger.Instance.LogFileName;
			WorkstationLogger.Instance.LogMode = true;

			WorkstationLogger.Instance.WriteLog(message, MISD.Core.LogType.Info, false);
			Assert.IsTrue(File.Exists(logPath + logFileName), "WorkstationLoggerTest:TestWriteLog() #01");

			StreamReader reader = new StreamReader(logPath + logFileName);
			Assert.IsTrue(regex.IsMatch(reader.ReadLine()), "WorkstationLoggerTest:TestWriteLog() #02");
			File.Delete(logPath + logFileName);
			Directory.Delete(logPath);

			logPath = "/tmp/testWorkstationLogger/";
			logFileName = "testLogFile.log";
			WorkstationLogger.Instance.LogPath = logPath;
			WorkstationLogger.Instance.LogFileName = logFileName;
			WorkstationLogger.Instance.WriteLog("[NUnitTest_WorkstationLogger]", MISD.Core.LogType.Info, false);
			Assert.IsTrue(File.Exists(logPath + logFileName), "WorkstationLoggerTest:TestWriteLog() #03");

			reader = new StreamReader(logPath + logFileName);
			Assert.IsTrue(regex.IsMatch(reader.ReadLine()), "WorkstationLoggerTest:TestWriteLog() #04");
			File.Delete(logPath + logFileName);
			Directory.Delete(logPath);
	    }
		#endregion
	}
}