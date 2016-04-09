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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;
using System.Threading;

namespace MISD.Workstation.Linux
{
	/// <summary>
	/// Workstation service.
	/// </summary>
	public class WorkstationService : ServiceBase
	{		
		/// <summary>
		/// Initializes a new instance of the <see cref="MISD.Workstation.Linux.WorkstationService"/> class.
		/// The Process ID of the scheduling will be stored in the file "processID.log".
		/// </summary>
		public WorkstationService()
		{
			Thread thread = new Thread (new ThreadStart (this.StartScheduling));
			thread.Name = "MISD.Workstation.Linux";
			int processID = System.Diagnostics.Process.GetCurrentProcess ().Id;
			
 			StreamWriter sw = new StreamWriter(@"processID.log");
            sw.WriteLine(processID.ToString ());
            sw.Close();
			
			thread.Start ();
		}
		
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments. You need one argument. Either "login" to login into the Server and start
		/// the scheduling or "logout" to stop scheduling and logout from the server.
		/// </param>
		public static void Main (string[] args)
		{			
			WorkstationLogger.Instance.WriteLog("Called Main with "+string.Join(",", args), MISD.Core.LogType.Debug, false);
			
			bool wrongArguments = false;
			if (args.Length >= 1 & args.Length <= 3)
			{
				if (args[0].Equals("login"))
				{
					if (args.Length == 1)
					{
						Login ();
					}
					else if (args.Length == 2 & args[1].Equals("log"))
					{
						WorkstationLogger.Instance.LogMode = true;
						Login ();
					}
					else if (args.Length == 2 & args[1].Equals("console"))
					{
						WorkstationLogger.Instance.ConsoleMode = true;
						Login ();
					}
					else if (args.Length == 3 & args[1].Equals("log") & args[2].Equals("console"))
					{
						WorkstationLogger.Instance.LogMode = true;
						WorkstationLogger.Instance.ConsoleMode = true;
						Login ();
					}
					else
					{
						wrongArguments = true;
					}
						    
				}
				else if (args[0].Equals("logout"))
				{
					if (args.Length == 1)
					{
						Logout ();
					}
					else if (args.Length == 2 & args[1].Equals("log"))
					{
						WorkstationLogger.Instance.LogMode = true;
						Logout ();
					}
					else if (args.Length == 2 & args[1].Equals("console"))
					{
						WorkstationLogger.Instance.ConsoleMode = true;
						Logout ();
					}
					else if (args.Length == 3 & args[1].Equals("log") & args[2].Equals("console"))
					{
						WorkstationLogger.Instance.LogMode = true;
						WorkstationLogger.Instance.ConsoleMode = true;
						Logout ();
					}
					else
					{
						wrongArguments = true;
					}
				}
				else
				{
					wrongArguments = true;
				}
			}
			else
			{
				wrongArguments = true;
			}
					
			if (wrongArguments)
			{
				Console.WriteLine ("");
				Console.WriteLine ("Wrong Arguments");
				Console.WriteLine ("You have to choose on of the the following alternatives:");
				Console.WriteLine ("->	MISD.Workstation.Linux.exe login [log|console|log console]");
				Console.WriteLine ("->	MISD.Workstation.Linux.exe logout [log|console|log console]");
				Console.WriteLine ("---------");
				Console.WriteLine ("log:     output in log file");
				Console.WriteLine ("console: output in terminal");
				Console.WriteLine ("");
			}	
		} 
		
		/// <summary>
		/// Login this workstation.
		/// </summary>
		private static void Login ()
		{
			// Try to kill an old process
			try
			{
				StopScheduling ();
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_WorkstationService_StopScheduling: double running instances", MISD.Core.LogType.Warning, true);
			}
			catch (Exception){}

			// Sign in on the server
			bool signedIn = ServerConnection.Instance.SignIn ();
			WorkstationLogger.Instance.WriteLog("Workstation Signed in: " + signedIn, MISD.Core.LogType.Debug, false);

			// Start the workstation service
			new WorkstationService ();
						
			// Program shouldn't finish.
			Console.ReadLine ();
		}
		
		/// <summary>
		/// Logout this workstation.
		/// </summary>
		private static void Logout ()
		{
			// Try to stop the workstationService
			try
			{
				StopScheduling ();
			}
			catch (Exception e)
			{
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_WorkstationService_StopScheduling: " + e.Message, MISD.Core.LogType.Exception, true);
			}

			// Sign out on the server
			bool signedOut = ServerConnection.Instance.SignOut ();
			WorkstationLogger.Instance.WriteLog("Workstation Signed out: " + signedOut, MISD.Core.LogType.Debug, false);
		}
		
		/// <summary>
		/// Starts the scheduling.
		/// </summary>
		private void StartScheduling()
		{	
			Scheduling.Scheduler.Instance.Start ();
		}

		/// <summary>
		/// Stops the scheduling.
		/// The method read out the Process ID which is stored in the file "processID.log".
		/// With this Process ID the scheduling will be killed.
		/// </summary>
		private static void StopScheduling()
		{			
			StreamReader sr = new StreamReader(@"processID.log");
			int processID = int.Parse(sr.ReadLine());
			sr.Close ();
			
			System.Diagnostics.Process.GetProcessById(processID).Kill ();
		}
	}	
}
