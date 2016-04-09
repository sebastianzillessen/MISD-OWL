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
using System.Linq;
using System.Collections.Generic;
using System.Text;

using MISD.Core;
using MISD.Workstation.Linux.Plugins;

/// <summary>
/// PluginManagerTest.cs - NUnit Test Cases for PluginManager
/// </summary>
namespace MISD.Workstation.Linux
{
	[TestFixture]
    public class PluginManagerTest {

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
			Assert.NotNull(PluginManager.Instance, "PluginManagerTest:TestGetInstance() #01");
		}

		/// <summary>
		/// Tests the method GetLoadedPlugins().
		/// </summary>
		[Test]
		public void GetLoadedPlugins ()
		{
			List<IPlugin> loadedPlugins = PluginManager.Instance.GetLoadedPlugins ();
			Assert.NotNull (loadedPlugins, "PluginManagerTest:GetLoadedPlugins() #01");

			if (loadedPlugins.Count > 0) {
				foreach (IPlugin plugin in loadedPlugins)
				{
					Assert.NotNull (plugin, "PluginManagerTest:GetLoadedPlugins() #02");

					Assert.NotNull (plugin.GetName(), "PluginManagerTest:GetLoadedPlugins() #03");
					Assert.AreNotEqual ("", plugin.GetName(), "PluginManagerTest:GetLoadedPlugins() #04");

					Assert.NotNull (plugin.GetDescription(), "PluginManagerTest:GetLoadedPlugins() #05");
					Assert.AreNotEqual ("", plugin.GetDescription(), "PluginManagerTest:GetLoadedPlugins() #06");

					Assert.NotNull (plugin.GetCompany(), "PluginManagerTest:GetLoadedPlugins() #07");
					Assert.AreNotEqual ("", plugin.GetCompany(), "PluginManagerTest:GetLoadedPlugins() #08");

					Assert.NotNull (plugin.GetProduct(), "PluginManagerTest:GetLoadedPlugins() #09");
					Assert.AreNotEqual ("", plugin.GetProduct(), "PluginManagerTest:GetLoadedPlugins() #10");

					Assert.NotNull (plugin.GetCopyright(), "PluginManagerTest:GetLoadedPlugins() #11");
					Assert.AreNotEqual ("", plugin.GetCopyright(), "PluginManagerTest:GetLoadedPlugins() #12");

					Assert.NotNull (plugin.GetVersion(), "PluginManagerTest:GetLoadedPlugins() #13");
					Assert.AreNotEqual ("", plugin.GetVersion(), "PluginManagerTest:GetLoadedPlugins() #14");
				}
			}
		}

		/// <summary>
		/// Tests the method LoadAvailablePlugins().
		/// </summary>
		[Test]
		public void LoadAvailablePlugins() {
			List<IPlugin> loadedPlugins = PluginManager.Instance.LoadAvailablePlugins ();
			Assert.NotNull (loadedPlugins, "PluginManagerTest:LoadAvailablePlugins() #01");
			Assert.AreEqual (PluginManager.Instance.GetLoadedPlugins(), loadedPlugins, "PluginManagerTest:LoadAvailablePlugins() #02");

			if (loadedPlugins.Count > 0) {
				foreach (IPlugin plugin in loadedPlugins)
				{
					Assert.NotNull (plugin, "PluginManagerTest:LoadAvailablePlugins() #03");

					Assert.NotNull (plugin.GetName(), "PluginManagerTest:LoadAvailablePlugins() #04");
					Assert.AreNotEqual ("", plugin.GetName(), "PluginManagerTest:LoadAvailablePlugins() #05");

					Assert.NotNull (plugin.GetDescription(), "PluginManagerTest:LoadAvailablePlugins() #06");
					Assert.AreNotEqual ("", plugin.GetDescription(), "PluginManagerTest:LoadAvailablePlugins() #07");

					Assert.NotNull (plugin.GetCompany(), "PluginManagerTest:LoadAvailablePlugins() #08");
					Assert.AreNotEqual ("", plugin.GetCompany(), "PluginManagerTest:LoadAvailablePlugins() #09");

					Assert.NotNull (plugin.GetProduct(), "PluginManagerTest:LoadAvailablePlugins() #10");
					Assert.AreNotEqual ("", plugin.GetProduct(), "PluginManagerTest:LoadAvailablePlugins() #11");

					Assert.NotNull (plugin.GetCopyright(), "PluginManagerTest:LoadAvailablePlugins() #12");
					Assert.AreNotEqual ("", plugin.GetCopyright(), "PluginManagerTest:LoadAvailablePlugins() #13");

					Assert.NotNull (plugin.GetVersion(), "PluginManagerTest:LoadAvailablePlugins() #14");
					Assert.AreNotEqual ("", plugin.GetVersion(), "PluginManagerTest:LoadAvailablePlugins() #15");
				}
			}
		}

		/// <summary>
		/// Tests the method UpdatePlugins().
		/// </summary>
		[Test]
		public void UpdatePlugins() {
			List<IPlugin> updatedPlugins = PluginManager.Instance.UpdatePlugins ();
			Assert.NotNull (updatedPlugins, "PluginManagerTest:UpdatePlugins() #01");

			Assert.AreEqual (PluginManager.Instance.GetLoadedPlugins(), updatedPlugins, "PluginManagerTest:UpdatePlugins() #02");
		}


		#endregion
	}
}
