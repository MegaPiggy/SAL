﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SALT.Console.Commands
{
	/// <summary>
	/// A command that dumps stuff to dump files
	/// </summary>
	public class DumpCommand : ConsoleCommand
	{
		public override string ID { get; } = "dump";
		public override string Usage { get; } = "dump <type>";
		public override string Description { get; } = "Dumps information to a dump file. Made for developer use";

		public override string ExtendedDescription => "<color=#77DDFF><type></color> - The type of dump to execute. '<color=#77DDFF>all</color>' can be used to execute all dumps.";

		public override bool Execute(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				Console.LogError($"The '<color=white>{ID}</color>' command takes 1 argument");
				return false;
			}

			if (ArgsOutOfBounds(args.Length, 1, 1))
				return false;

			try
			{
				if (args[0].Equals("all"))
				{
					bool DidAll = true;
					foreach (string file in Console.dumpActions.Keys)
					{
						bool did = DumpFile(file);
						if (!did)
							DidAll = false;
					}

					return DidAll;
				}
				else
				{
					return DumpFile(args[0]);
				}
			}
			catch { }

			Console.LogError($"Couldn't find or create file '<color=white>{args[0]}</color>'");
			return false;
		}

		private bool DumpFile(string name)
		{
			if (!Console.dumpActions.ContainsKey(name))
			{
				Console.LogError($"No dump action found for '<color=white>{name}</color>'");
				return false;
			}

			string path = Path.Combine(UnityEngine.Application.dataPath, $"../Dumps/{name}.dump");

			if (!Directory.Exists(path.Substring(0, path.LastIndexOf('/'))))
				Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));

			using (StreamWriter writer = File.CreateText(path))
			{
				Console.dumpActions[name].Invoke(writer);
			}

			Console.LogSuccess($"File '<color=white>{name}</color>' dumped succesfully");
			return true;
		}

		public override List<string> GetAutoComplete(int argIndex, string argText)
		{
			if (argIndex == 0)
			{
				List<string> vs = new List<string>{ "all" };
				vs.AddRange(Console.dumpActions.Keys);
				return vs;
			}
			else
				return base.GetAutoComplete(argIndex, argText);
		}
	}
}
