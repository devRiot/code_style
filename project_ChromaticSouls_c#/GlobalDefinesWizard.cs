using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


public class GlobalDefinesWizard : ScriptableWizard
{
	[System.Serializable]
	public class GlobalDefine : ISerializable
	{
		public string define;
		public bool enabled;


		public GlobalDefine() { }


		protected GlobalDefine(SerializationInfo info, StreamingContext context)
		{
			define = info.GetString("define");
			enabled = info.GetBoolean("enabled");
		}


		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("define", define);
			info.AddValue("enabled", enabled);
		}

	}

	private const string DEFINE_HEADER = "-define:";
	private const string SMCS_FILE_NAME = "smcs.rsp";
	private const string GMCS_FILE_NAME = "gmcs.rsp";
	private const string DEFINES_FILE_NAME = "defines.txt";

	public List<GlobalDefine> _globalDefines = new List<GlobalDefine>();
	[MenuItem("Global Defines/Edit Global Defines")]
	static void createWizardFromMenu()
	{
		var helper = ScriptableWizard.DisplayWizard<GlobalDefinesWizard>("Global Defines Manager", "Save", "Cancel");
		helper.minSize = new Vector2(500, 300);
		helper.maxSize = new Vector2(500, 300);
	}


	void OnGUI()
	{
		if (_globalDefines.Count <= 0)
		{
			loadDefines();
		}

		var toRemove = new List<GlobalDefine>();

		foreach (var define in _globalDefines)
		{
			if (defineEditor(define))
			{
				toRemove.Add(define);
			}
		}

		foreach (var define in toRemove)
		{
			_globalDefines.Remove(define);
		}

		if (GUILayout.Button("Add Define"))
		{
			var d = new GlobalDefine();
			d.define = "NEW_DEFINE";
			d.enabled = true;
			_globalDefines.Add(d);
		}
		GUILayout.Space(40);

		if (GUILayout.Button("Save"))
		{
			Save();
			Close();
		}
	}


	private void Save()
	{
		// nothing to save means delete everything
		if (_globalDefines.Count == 0)
		{
			deleteFiles();
			deleteDefines();
			
			Close();
			return;
		}

		// what shall we write to disk?
		writeDefines();

		var toDisk = _globalDefines.Where(d => d.enabled).Select(d => d.define).ToArray();
		if (toDisk.Length > 0)
		{
			var builder = new System.Text.StringBuilder(DEFINE_HEADER);
			for (var i = 0; i < toDisk.Length; i++)
			{
				if (i < toDisk.Length - 1)
					builder.AppendFormat("{0};", toDisk[i]);
				else
					builder.Append(toDisk[i]);
			}
			
			writeFiles(builder.ToString());

			AssetDatabase.Refresh();
			reimportSomethingToForceRecompile();
		}
		else
		{
			// nothing enabled to save, kill the files
			deleteFiles();
		}
	}


	private void reimportSomethingToForceRecompile()
	{
		var dataPathDir = new DirectoryInfo(Application.dataPath);
		var dataPathUri = new System.Uri(Application.dataPath);
		foreach (var file in dataPathDir.GetFiles("GlobalDefinesWizard.cs", SearchOption.AllDirectories))
		{
			var relativeUri = dataPathUri.MakeRelativeUri(new System.Uri(file.FullName));
			var relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());
			AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
		}
	}


	private void deleteFiles()
	{
		var smcsFile = Path.Combine(Application.dataPath, SMCS_FILE_NAME);
		var gmcsFile = Path.Combine(Application.dataPath, GMCS_FILE_NAME);

		if (File.Exists(smcsFile))
			File.Delete(smcsFile);

		if (File.Exists(gmcsFile))
			File.Delete(gmcsFile);
	}

	private void deleteDefines()
	{		
		var definesFile = Path.Combine(Application.dataPath, DEFINES_FILE_NAME);

		if (File.Exists(definesFile))
			File.Delete(definesFile);
	}


	private void writeFiles(string data)
	{
		var smcsFile = Path.Combine(Application.dataPath, SMCS_FILE_NAME);
		var gmcsFile = Path.Combine(Application.dataPath, GMCS_FILE_NAME);

		// -define:debug;poop
		File.WriteAllText(smcsFile, data);
		File.WriteAllText(gmcsFile, data);
	}

	private void loadDefines()
	{
		string definesData = null;

		var definesFile = Path.Combine(Application.dataPath, DEFINES_FILE_NAME);

		if (File.Exists(definesFile)) { definesData = File.ReadAllText(definesFile); }

		if (definesData != null)
		{
			definesData = definesData.Trim();
			if (!String.IsNullOrEmpty(definesData))
			{
				string[] defineList = definesData.Split(';');
				foreach (string defineString in defineList)
				{
					if (defineString != null)
					{
						string[] defineValue = defineString.Split(':');
						if (defineValue == null || defineValue.Length != 2) { continue; }

						GlobalDefine globalDefine = new GlobalDefine();
						globalDefine.define = defineValue[0];
						globalDefine.enabled = (defineValue[1].ToLower() == "true") ? true : false;

						_globalDefines.Add(globalDefine);
					}
				}
			}
		}
	}

	private void writeDefines()
	{
		var builder = new System.Text.StringBuilder();
		var definesFile = Path.Combine(Application.dataPath, DEFINES_FILE_NAME);
		
		for (var i = 0; i < _globalDefines.Count; i++)
		{
			GlobalDefine globalDefine = _globalDefines[i];
			if (globalDefine == null) { continue; }

			string defineString = globalDefine.define + ":" + globalDefine.enabled;
			if (i < _globalDefines.Count - 1)
			{
				builder.AppendFormat("{0};", defineString);
			}
			else
			{
				builder.Append(defineString);
			}
		}		

		File.WriteAllText(definesFile, builder.ToString());
	}

	private bool defineEditor(GlobalDefine define)
	{
		EditorGUILayout.BeginHorizontal();

		define.define = EditorGUILayout.TextField(define.define);
		define.enabled = EditorGUILayout.Toggle(define.enabled);

		var remove = false;
		if (GUILayout.Button("Remove"))
			remove = true;

		EditorGUILayout.EndHorizontal();

		return remove;
	}


	// Called when the 'save' button is pressed
	void OnWizardCreate()
	{
		// .Net 2.0 Subset: smcs.rsp
		// .Net 2.0: gmcs.rsp
		// -define:debug;poop
	}


	void OnWizardOtherButton()
	{
		this.Close();
	}

}