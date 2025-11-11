#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
            string fileContents = ConstructFileContents();

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


        private string ConstructFileContents()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("/* THIS IS A CODE GENERATED SCRIPT */");
            stringBuilder.AppendLine("   /* ... PLEASE NO TOUCHY ... */");
            stringBuilder.AppendLine();
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
                    stringBuilder.AppendLine($"        {FormattedEnumValue(values[i])}{lineEnd}");
                }
            }


            string FormattedEnumValue(string value)
            {
                StringBuilder stringBuilder = new();

                string[] parts = Regex.Split(value, @"[^A-Za-z0-9]+").Where(s => s.Length > 0).ToArray();
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (i == 0)
                    {
                        stringBuilder.Append(char.ToLowerInvariant(part[0]));
                        if (part.Length > 1) stringBuilder.Append(part.Substring(1));
                    }
                    else
                    {
                        stringBuilder.Append(char.ToUpperInvariant(part[0]));
                        if (part.Length > 1) stringBuilder.Append(part.Substring(1).ToLowerInvariant());
                    }
                }

                string formattedValue = stringBuilder.ToString();

                if (char.IsDigit(formattedValue[0])) formattedValue = "_" + formattedValue;

                var csharpKeywords = new HashSet<string>(StringComparer.Ordinal)
                {
                    "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
                    "decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally",
                    "fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long",
                    "namespace","new","null","object","operator","out","override","params","private","protected","public","readonly",
                    "ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this",
                    "throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"
                };
                if (csharpKeywords.Contains(formattedValue)) formattedValue = "_" + formattedValue;

                formattedValue = Regex.Replace(formattedValue, @"[^A-Za-z0-9_]", "");

                return formattedValue;
            }
        }
    }
}
#endif