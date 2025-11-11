#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TBaltaks.FMODManagement.Editor
{
    [InitializeOnLoad]
    public static class ScriptAutoGenerator
    {
        private const string GeneratedScriptsFolder = "Assets/Plugins/FMOD Management/GeneratedScripts";
        private const string TemplatesSubfolder = "Editor/Templates";


        static ScriptAutoGenerator()
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

            if (!Directory.Exists(GeneratedScriptsFolder)) Directory.CreateDirectory(GeneratedScriptsFolder);

            CreateScript(templatesPath, "FMODParametersTemplate.txt", "FMODParameters.cs");
            CreateScript(templatesPath, "FMODEventsTemplate.txt", "FMODEvents.cs");
            CreateScript(templatesPath, "AudioManagerTemplate.txt", "AudioManager.cs");

            AssetDatabase.Refresh();
        }


        private static void CreateScript(string sourceFolder, string sourceFile, string destinationFile)
        {
            string sourcePath = Path.Combine(sourceFolder, sourceFile);
            string destinationPath = Path.Combine(GeneratedScriptsFolder, destinationFile);

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning("[FMOD Manager] Missing template: " + sourcePath);
                return;
            }

            if (File.Exists(destinationPath))
            {
                Debug.Log($"[FMOD Manager] {destinationFile} already exsists");
                return;
            }

            File.Copy(sourcePath, destinationPath);
            Debug.Log($"[FMOD Manager] Created {destinationFile}");
        }


        private static string FindThisPackagePath()
        {
            string[] guids = AssetDatabase.FindAssets("ScriptAutoGenerator t:Script");
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


        [MenuItem("Tools/FMOD Management/Generate Runtime Scripts")]
        private static void GenerateScriptsManually()
        {
            TryGenerateScripts();
        }
    }
}
#endif