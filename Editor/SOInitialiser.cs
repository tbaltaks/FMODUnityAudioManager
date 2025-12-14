#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TBaltaks.FMODManagement.Editor
{
    [InitializeOnLoad]
    public static class SOInitialiser
    {
        public static bool debugLogging;

        private const string TargetFolder = "Assets/Plugins/FMOD Management";


        static SOInitialiser()
        {
            EditorApplication.delayCall += TryGenerateScriptableObjects;
        }


        private static void TryGenerateScriptableObjects()
        {
            if (!Directory.Exists(TargetFolder)) Directory.CreateDirectory(TargetFolder);
            CreateScriptableObject<FMODManagementSettingsSO>(TargetFolder, "FMOD Management Settings");
            AssetDatabase.Refresh();
        }


        private static void CreateScriptableObject<T>(string destinationFolder, string assetName) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids != null && guids.Length > 0)
            {
                if (debugLogging) Debug.Log($"[FMOD Management] Found existing asset(s) of type {typeof(T).Name}; skipping creation of {assetName}.");
                return;
            }

            string destinationPath = Path.Combine(destinationFolder, assetName + ".asset");

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, destinationPath);
            AssetDatabase.SaveAssets();
            if (debugLogging) Debug.Log($"[FMOD Management] Created {assetName}");
        }


        [MenuItem("Tools/FMOD-Unity Audio Manager/Generate settings SO")]
        private static void GenerateScriptsManually()
        {
            bool previousSetting = debugLogging;
            debugLogging = true;
            TryGenerateScriptableObjects();
            debugLogging = previousSetting;
        }
    }
}
#endif