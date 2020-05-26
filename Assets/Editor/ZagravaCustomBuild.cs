using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ZagravaCustomBuild
{

    public static void BuildTeamCity_Android()
    {
        SetBuildTargetSettings(BuildTargetGroup.Android, "com.zagravagames.creaturemixer", "");

        string defaultDirectory = "";
        string defaultFilename = "CreatureMixer";
        //string defaultFilename = "CreatureMixer_" + CurrentDateToString();

        CreateBuild(BuildTarget.Android, defaultDirectory, defaultFilename, true);
    }

    public static void BuildTeamCity_iOS()
    {
        SetBuildTargetSettings(BuildTargetGroup.iOS, "com.zagravagames.creaturemixer", "NO_GPGS");

        string defaultDirectory = "";
        string defaultFilename = "CreatureMixer";

        CreateBuild(BuildTarget.iOS, defaultDirectory, defaultFilename, true);
        PrepareDownloadsFile(Path.Combine(defaultDirectory, "ios_template.html"), Path.Combine(defaultDirectory, "ios.html"), "[IOSBUILDDATE]", CurrentDateTimeToString());
    }

    static void CreateBuild(BuildTarget target, string defaultDirectory, string defaultFilename, bool silent = false)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] {
            "Assets/Scenes/StartGameScene.unity",
            "Assets/Scenes/GameScene.unity",
        };
        string path = "";
        if (silent)
        {
            if (target == BuildTarget.Android)
            {
                if (UnityEditor.EditorUserBuildSettings.buildAppBundle)
                {
                    path = Path.Combine(defaultDirectory, defaultFilename + ".aab");
                }
                else
                {
                    path = Path.Combine(defaultDirectory, defaultFilename + ".apk");
                }
            }
            else if (target == BuildTarget.iOS)
            {
                path = Path.Combine(defaultDirectory, "iOS_Build");
                DeleteDir(path);
            }
        }
        else
        {
            if (target == BuildTarget.Android)
            {
                path = EditorUtility.SaveFilePanel("Choose APK file to save", defaultDirectory, defaultFilename, "apk");
            }
        }
        if (path != "")
        {
            if (!silent) EditorPrefs.SetString("BuildDirectory", Path.GetDirectoryName(path));
            buildPlayerOptions.locationPathName = path;
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = BuildOptions.None;
            BuildAssetBundles(target);
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (!silent) EditorUtility.RevealInFinder(path);
        }
        PrepareDownloadsFile(Path.Combine(defaultDirectory, "android_template.html"), Path.Combine(defaultDirectory, "android.html"), "[ANDROIDBUILDDATE]", CurrentDateTimeToString());
    }

    static void PrepareDownloadsFile(string templateFile, string destFile, string templateString, string destString)
    {
        string textFile = File.ReadAllText(templateFile);
        textFile = textFile.Replace(templateString, destString);
        File.WriteAllText(destFile, textFile);
    }

    #region common
    static void BuildAssetBundles(BuildTarget target)
    {
        string path = Application.dataPath;
        string destPath = "Assets/StreamingAssets/";
        if (target == BuildTarget.Android)
        {
            DeleteDir(Path.Combine(path, "StreamingAssets/iOS"));
            DeleteFile(Path.Combine(path, "StreamingAssets/iOS.meta"));

            destPath += "Android";
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

        }
        else if (target == BuildTarget.iOS)
        {
            DeleteDir(Path.Combine(path, "StreamingAssets/Android"));
            DeleteFile(Path.Combine(path, "StreamingAssets/Android.meta"));
            destPath += "iOS";
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
        }
        BuildPipeline.BuildAssetBundles(destPath, BuildAssetBundleOptions.None, target);
    }

    public static void SetBuildTargetSettings(BuildTargetGroup targetGroup, string packageName, string defineSymbols)
    {
        if (targetGroup == BuildTargetGroup.Android)
        {
            UnityEditor.PlayerSettings.Android.keystorePass = "android";
            UnityEditor.PlayerSettings.Android.keyaliasPass = "android";
            UnityEditor.PlayerSettings.bundleVersion = CurrentDateToString();
        }
        UnityEditor.PlayerSettings.SetApplicationIdentifier(targetGroup, packageName);
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
    }

    #endregion

    #region sdk_tools
    public static void RemoveAllSDK(bool silent = false)
    {
        Remove_Facebook(silent: true);
        if (!silent) AssetDatabase.Refresh();
    }

    public static void Add_Facebook(bool silent = false)
    {
        CopyAndReplaceDirectory(Path.Combine(GetProjectRootFolder(), "external_libs/FacebookSDK"), Path.Combine(GetProjectDataFolder(), "FacebookSDK"));
        if (!silent) AssetDatabase.Refresh();
    }

    public static void Remove_Facebook(bool silent = false)
    {
        DeleteDir(Path.Combine(GetProjectDataFolder(), "FacebookSDK"));
        if (!silent) AssetDatabase.Refresh();
    }
    #endregion

    #region helpers
    static string CurrentDateToString()
    {
        return System.DateTime.Now.Year.ToString("D4") + System.DateTime.Now.Month.ToString("D2") + System.DateTime.Now.Day.ToString("D2");
    }

    static string CurrentDateTimeToString()
    {
        return CurrentDateToString() + " " + System.DateTime.Now.Hour.ToString("D2") + System.DateTime.Now.Minute.ToString("D2") + System.DateTime.Now.Second.ToString("D2");
    }

    static string GetProjectDataFolder()
    {
        return Application.dataPath;
    }


    static string GetProjectRootFolder()
    {
        return Directory.GetParent(GetProjectDataFolder()).ToString();
    }

    static void DeleteDir(string dirName)
    {
        if (Directory.Exists(dirName))
        {
            Directory.Delete(dirName, true);
        }
    }

    static void DeleteFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
        {
            Directory.Delete(dstPath, true);
        }

        if (File.Exists(dstPath))
        {
            File.Delete(dstPath);
        }

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
        {
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
        }

        foreach (var dir in Directory.GetDirectories(srcPath))
        {
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }

    static void CopyAndReplaceFile(string srcPath, string dstPath)
    {
        string dirName = Path.GetDirectoryName(dstPath);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }

        if (File.Exists(dstPath))
        {
            File.Delete(dstPath);
        }

        File.Copy(srcPath, dstPath);
    }

    private static List<string> GetDefinesList(BuildTargetGroup group)
    {
        return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
    }

    private static void EnableDefine(BuildTargetGroup group, string defineName, bool enable)
    {
        var defines = GetDefinesList(group);
        if (enable)
        {
            if (defines.Contains(defineName))
            {
                return;
            }
            defines.Add(defineName);
        }
        else
        {
            if (!defines.Contains(defineName))
            {
                return;
            }
            while (defines.Contains(defineName))
            {
                defines.Remove(defineName);
            }
        }
        string definesString = string.Join(";", defines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
    }

    private static BuildTargetGroup GetCurrentBuildTarget()
    {
        return BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
    }

    private static void SetBuildTarget(BuildTarget target)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
    }
    #endregion
}
