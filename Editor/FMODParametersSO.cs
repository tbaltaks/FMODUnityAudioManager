#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace TBaltaks.FMODManagement.Editor
{
    public class FMODParametersSO : ScriptableObject
    {
        [SerializeField] private string[] LocalAudioParameters;
        [SerializeField] private string[] GlobalAudioParameters;

        [Space]

        [InspectorButton("TryGenerateRuntimeScript", ButtonWidth = 200)]
        [SerializeField] private bool generateRuntimeScript;


        private const string GeneratedScriptsFolder = "Assets/Plugins/FMOD Management/GeneratedScripts";
        private const string FileName = "FMODParameters.cs";


        private void TryGenerateRuntimeScript()
        {
            if (!Directory.Exists(GeneratedScriptsFolder)) Directory.CreateDirectory(GeneratedScriptsFolder);

            string destinationPath = Path.Combine(GeneratedScriptsFolder, FileName);
            string fileContents = ContructFileContents();

            if (File.Exists(destinationPath))
            {
                string existingContents = File.ReadAllText(destinationPath);
                if (existingContents == fileContents)
                {
                    Debug.Log($"No changes detected in {FileName}; skipping rewrite.");
                    return;
                }
            }

            File.WriteAllText(destinationPath, fileContents);
            AssetDatabase.Refresh();
            Debug.Log($"[FMOD Manager] Created {FileName}");
        }


        private string ContructFileContents()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("namespace TBaltaks.FMODManagement");
            stringBuilder.AppendLine("{");

            // --- LocalAudioParameters ---
            stringBuilder.AppendLine("    public enum LocalAudioParameter");
            stringBuilder.AppendLine("    {");
            AppendEnumValues(stringBuilder, LocalAudioParameters);
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine();

            // --- GlobalAudioParameters ---
            stringBuilder.AppendLine("    public enum GlobalAudioParameter");
            stringBuilder.AppendLine("    {");
            AppendEnumValues(stringBuilder, GlobalAudioParameters);
            stringBuilder.AppendLine("    }");

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();


            void AppendEnumValues(StringBuilder stringBuilder, string[] values)
            {
                if (values == null || values.Length == 0)
                {
                    stringBuilder.AppendLine("        // No parameters listed");
                    return;
                }

                for (int i = 0; i < values.Length; i++)
                {
                    string lineEnd = (i < values.Length - 1) ? "," : "";
                    stringBuilder.AppendLine($"        {values[i]}{lineEnd}");
                }
            }
        }
    }
}
#endif