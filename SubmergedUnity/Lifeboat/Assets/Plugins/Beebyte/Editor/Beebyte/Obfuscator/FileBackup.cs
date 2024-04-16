/*
 * Copyright (c) 2015-2019 Beebyte Limited. All rights reserved.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Beebyte.Obfuscator
{
	public class FileBackup
	{
		/**
		 * Returns a map consisting of keys of the original file and a proposed target location.
		 */
		public static IDictionary<string, string> GetBackupMap(IEnumerable<string> locations)
		{
			IDictionary<string, string> pathMap = new Dictionary<string, string>();
		
			foreach (string location in locations)
			{
				string backupLocation = location + ".bb_obf_backup.pre";
				pathMap.Add(location, backupLocation);
			}

			return pathMap;
		}
		
		/**
		 * Backs up files specified in the keys of the map to their corresponding values.
		 */
		public static void Backup(IDictionary<string, string> backupMap)
		{
			foreach (KeyValuePair<string,string> keyValuePair in backupMap)
			{
				//This throws an exception if the backup already exists - we want this to happen
				File.Copy(keyValuePair.Key, keyValuePair.Value);
			}
		}

		/**
		 *
		 */
		public static void Restore(IDictionary<string, string> pathMap)
		{
			foreach (KeyValuePair<string, string> entry in pathMap)
			{
				string target = entry.Key;
				string backup = entry.Value;
				
				try
				{
					if (!File.Exists(backup)) continue;
						
					DeleteFileWhenPermitted(target);
					File.Move(backup, target);
				}
				catch (Exception e)
				{
					Debug.LogError("Could not restore original DLL to " + target + "\n" + e);
				}
			}
		}
		
		private static void DeleteFileWhenPermitted(string target)
		{
			int attempts = 60;
			while (attempts > 0)
			{
				try
				{
					if (File.Exists(target)) File.Delete(target);
					if (attempts < 60) Debug.LogWarning("Successfully accessed " + target);
					return;
				}
				catch (Exception)
				{
					Debug.LogWarning("Failed to access " + target + " - Retrying...");
					Thread.Sleep(500);
					if (--attempts <= 0) throw;
				}
			}
		}
	}
}
