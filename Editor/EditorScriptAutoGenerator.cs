#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class EditorScriptAutoGenerator
{
    private const string TargetFolder = "Assets/Plugins/FMOD Management";
    private const string TemplatesSubfolder = "Templates";


    static EditorScriptAutoGenerator()
    {
        EditorApplication.delayCall += TryGenerateScripts;
    }
    
    
    private static void TryGenerateScripts()
    {
        string packagePath = FindThisPackagePath();
        if (packagePath == null)
        {
            Debug.LogWarning("[FMOD Manager] Could not find package path — skipping script generation.");
            return;
        }

        string templatesPath = Path.Combine(packagePath, TemplatesSubfolder);
        if (!Directory.Exists(templatesPath))
        {
            Debug.LogWarning("[FMOD Manager] Templates folder missing in package: " + templatesPath);
            return;
        }

        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
        }

        CopyTemplate(templatesPath, "FMODParameters.cs.txt", "FMODParameters.cs");
        CopyTemplate(templatesPath, "FMODEvents.cs.txt", "FMODEvents.cs");
        CopyTemplate(templatesPath, "AudioManager.cs.txt", "AudioManager.cs");

        AssetDatabase.Refresh();
        Debug.Log("[FMOD Manager] Generated scripts in " + TargetFolder);
    }


    private static void CopyTemplate(string templatesPath, string sourceFile, string destinationFile)
    {
        string sourcePath = Path.Combine(templatesPath, sourceFile);
        string destinationPath = Path.Combine(TargetFolder, destinationFile);

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, overwrite: false);
        }
        else
        {
            Debug.LogWarning("[FMOD Manager] Missing template: " + sourcePath);
        }
    }


    private static string FindThisPackagePath()
    {
        // Find this script's location inside the Packages folder
        string[] guids = AssetDatabase.FindAssets("EditorScriptAutoGenerator t:Script");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Packages/"))
            {
                return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), ".."));
            }
        }
        return null;
    }
}
#endif