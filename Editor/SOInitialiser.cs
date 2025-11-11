#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TBaltaks.FMODManagement.Editor
{
    [InitializeOnLoad]
    public static class SOInitialiser
    {
        private const string FMODManagementFolder = "Assets/Plugins/FMOD Management";


        static SOInitialiser()
        {
            EditorApplication.delayCall += TryGenerateScriptableObjects;
        }


        private static void TryGenerateScriptableObjects()
        {
            if (!Directory.Exists(FMODManagementFolder)) Directory.CreateDirectory(FMODManagementFolder);

            CreateScriptableObject<FMODParametersSO>(FMODManagementFolder, "FMOD Parameters.asset");
            CreateScriptableObject<FMODEventsSO>(FMODManagementFolder, "FMOD Events.asset");

            AssetDatabase.Refresh();
        }


        private static void CreateScriptableObject<T>(string destinationFolder, string assetName) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids != null && guids.Length > 0)
            {
                Debug.Log($"[FMOD Manager] Found existing asset(s) of type {typeof(T).Name}; skipping creation of {assetName}.");
                return;
            }

            string destinationPath = Path.Combine(destinationFolder, assetName);

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, destinationPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FMOD Manager] Created {assetName}");
        }


        [MenuItem("Tools/FMOD Management/Generate Scriptable Objects")]
        private static void GenerateScriptsManually()
        {
            TryGenerateScriptableObjects();
        }
    }
}
#endif