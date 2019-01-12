using UnityEngine;
using UnityEditor;

// System scopes
using System; // for Uri
using System.IO; // for FileInfo + DirectoryInfo
using System.Collections.Generic; // for List


// Auto builder class
// can be invoked from the command line:
// Unity -batchmode -projectPath . -logFile build.log -executeMethod Builder.BuildGame -quit -buildGame={GameName}
public class Builder
{
	static string gameFlag = "-buildGame=";
	static string versionFlag = "-buildVersion=";
	static string targetFlag = "-target=";

	static void _build(string[] scenes, string name, BuildTarget target)
	{
		// I don't know why we need to switch build target, but apparently we do
		// also, can we load BuildOptions from somewhere?
		EditorUserBuildSettings.SwitchActiveBuildTarget(target);
		BuildPipeline.BuildPlayer(scenes, name, target,BuildOptions.None);
	}

	/*static void _rebuildMenu(string name)
	{
		GameObject prefab = Resources.Load(name + "MenuState") as GameObject;
		GameObject ms = GameObject.Instantiate(prefab) as GameObject;
		GameStateMenu menu = ms ? ms.GetComponent<GameStateMenu>() : null;
		if(menu) 
		{
			menu.UpdateSceneNames();
			PrefabUtility.ReplacePrefab(ms, prefab);
		}
	}*/
	
	static void _build(string[] scenes, string name, string version, string[] targets)
	{
		PlayerSettings.bundleVersion = version;
		
		// this isn't exactly pretty... but it works
		foreach(string _target in targets)
		{
			if(_target == "windows")
				_build(scenes, name + "Win-" + version + ".exe", BuildTarget.StandaloneWindows);
			if(_target == "osx")
				_build(scenes, name + "OSX-" + version + ".app", BuildTarget.StandaloneOSXIntel);
			if(_target == "linux")
				_build(scenes, name + "Linux-" + version + ".run", BuildTarget.StandaloneLinux);
			if(_target == "ios")
				_build(scenes, name + "iOS", BuildTarget.iPhone);
			if(_target == "web")
				_build(scenes, "WebPlayer", BuildTarget.WebPlayer);
		}
		
	}
	
	static void _setIcon(string name)
	{
		// we first have to delete the destination file to be able to 
		// copy the new one on top, otherwise it complains
		FileUtil.DeleteFileOrDirectory("Assets/Icon.png");
		FileUtil.CopyFileOrDirectory("Assets/" + name + "/Icon.png", "Assets/Icon.png");
		
		// then we have to make sure that the new file is actually picked up
		AssetDatabase.Refresh();
	}
	
	static string[] _getScenes(string name)
	{
		List<string> scenes = new List<string>();
		
		// The first built scene should be our loader scene
		// this should always be Assets/Scenes/{GameName}Game.unity
		// this scene will load first, and then load any other
		// scenes required
		//scenes.Add("Assets/Scenes/" + name + "Game.unity"); // don't have a loader (yet?)
		
		// Any other scenes should be relative to the game directory
		// it doesn't matter where they are, this should find them and
		// add them. We (usually) don't load scenes by index, so the order
		// doesn't matter. Just use the System to give us all *.unity files
		DirectoryInfo di = new DirectoryInfo("Assets/Scenes");
		FileInfo[] fi = di.GetFiles("*.unity", SearchOption.AllDirectories);
		foreach(FileInfo file in fi)
		{
			// Annoyingly, we have to rebuild the relative path to give to Unity
			Uri path = new Uri(file.FullName);
			Uri folder = new Uri(di.Parent.FullName);
			scenes.Add(Uri.UnescapeDataString(folder.MakeRelativeUri(path).ToString()));
		}

		return scenes.ToArray();
	}
	
	// Actual build script
	static public void BuildGame()
	{
		string name = "";
		string ver = "";
		List<string> targets = new List<string>();
		foreach(string str in System.Environment.GetCommandLineArgs())
		{
			if(str.StartsWith(gameFlag))
				name = str.Replace(gameFlag, "");
			if(str.StartsWith(versionFlag))
				ver = str.Replace(versionFlag, "");
			if(str.StartsWith(targetFlag))
			{
				string[] tmp = str.Replace(targetFlag, "").Split(',');
				foreach(string tar in tmp)
					targets.Add(tar);
			}
		}
		if(string.IsNullOrEmpty(name))
		{
			Console.WriteLine("Unable to find project in command line (have you passed " + gameFlag + "?)");
			// something to crash to make it fail;
			name = null;
			name.ToCharArray();
			return;
		}
		string[] scenes = _getScenes(name);
		
		if(true) // debug functionality
		{
			Console.WriteLine("================================================================================");
			Console.WriteLine("Building project: " + name);
			Console.WriteLine("--------------------------------------------------------------------------------");
			Console.WriteLine("Scenes:");
			foreach(string scene in scenes)
				Console.WriteLine("\t" + scene);
			Console.WriteLine("================================================================================");
		}
		
		//_setIcon(name);

		//_rebuildMenu(name);
		
		_build (scenes, name, ver, targets.ToArray());
		scenes = null;
	}
}