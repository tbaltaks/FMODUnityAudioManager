#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class EditorScriptAutoGenerator
{
    private const string GeneratedFolder = "Assets/Plugins/FMOD Management";
    private const string ParamsPath = GeneratedFolder + "/FMODParameters.cs";
    private const string EventsPath = GeneratedFolder + "/FMODEvents.cs";
    private const string PackagePath = "Packages/FMOD-Unity Audio Manager";

    static EditorScriptAutoGenerator()
    {
        EditorApplication.delayCall += () =>
        {
            // Only run when the package folder is present
            //if (!Directory.Exists(PackagePath)) return;

            CreateIfMissing();
        };
    }

    private static void CreateIfMissing()
    {
        if (!Directory.Exists(GeneratedFolder))
            Directory.CreateDirectory(GeneratedFolder);

        if (!File.Exists(ParamsPath))
        {
            var paramsContent = @"namespace TBaltaks.FMODManagement
{
    public enum LocalAudioParameter
    {
        // Local parameters go here and must be spelt exactly as they are in the FMOD project
    }

    public enum GlobalAudioParameter
    {
        // Global parameters go here and must be spelt exactly as they are in the FMOD project
    }
}";
            File.WriteAllText(ParamsPath, paramsContent);
            AssetDatabase.ImportAsset(ParamsPath);
            Debug.Log($"EditorScriptAutoGenerator Created {ParamsPath}");
        }

        if (!File.Exists(EventsPath))
        {
            var eventsContent = @"using UnityEngine;
using FMODUnity;

namespace TBaltaks.FMODManagement
{
    public class FMODEvents : MonoBehaviour
    {
        public static FMODEvents Instance;

        /* Add and fill out fmod events as needed */
        [Header(""Music"")]
        public EventReference musicEvent;

        [Header(""Ambience"")]
        public EventReference ambienceEvent;

        [Header(""SFX"")]
        public EventReference sfxEvent;

        [Header(""UI"")]
        public EventReference uiEvent;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}";
            File.WriteAllText(EventsPath, eventsContent);
            AssetDatabase.ImportAsset(EventsPath);
            Debug.Log($"EditorScriptAutoGenerator Created {EventsPath}");
        }

        AssetDatabase.Refresh();
    }
}
#endif