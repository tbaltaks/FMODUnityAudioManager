#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using FMODUnity;
using UnityEditor;

namespace TBaltaks.FMODManagement.Editor
{
    public class FMODEventsSO : ScriptableObject
    {
        [Serializable]
        class EventGroup
        {
            [SerializeField] private string label;
            public EventReference[] events;
        }

        [SerializeField] private EventGroup[] eventGroups;

        [Space]

        [InspectorButton("TryGenerateRuntimeScript", ButtonWidth = 200)]
        [SerializeField] private bool generateRuntimeScript;


        private const string GeneratedScriptsFolder = "Assets/Plugins/FMOD Management/GeneratedScripts";
        private const string FileName = "FMODEvents.cs";


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

            stringBuilder.AppendLine("using FMODUnity;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("/* THIS IS A CODE GENERATED SCRIPT */");
            stringBuilder.AppendLine("   /* ... PLEASE NO TOUCHY ... */");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace TBaltaks.FMODManagement");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    public static class FMODEvents");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        public static bool IsInitialized { get; private set; }");
            stringBuilder.AppendLine();

            AppendEventReferences(stringBuilder, eventGroups);

            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();


            void AppendEventReferences(StringBuilder stringBuilder, EventGroup[] eventGroups)
            {
                List<string> eventLabels = new();

                if (eventGroups.Length < 1)
                {
                    stringBuilder.AppendLine($"        // No events listed");
                }
                else
                {
                    foreach (EventGroup group in eventGroups)
                    {
                        if (group.events.Length > 0)
                        {
                            foreach (EventReference reference in group.events)
                            {
                                string label = FormattedEventLabel(reference);
                                stringBuilder.AppendLine($"        public static EventReference {label};");
                                eventLabels.Add(label);
                            }
                        }
                    }

                    if (eventLabels.Count < 1)
                    {
                        stringBuilder.AppendLine($"        // No events listed");
                    }
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine("        public static void Initialize(");

                for (int i = 0; i < eventLabels.Count; i++)
                {
                    string lineEnd = (i < eventLabels.Count - 1) ? "," : "";
                    stringBuilder.AppendLine($"            EventReference {eventLabels[i]}Reference{lineEnd}");
                }

                stringBuilder.AppendLine("        ){");
                stringBuilder.AppendLine("            if (IsInitialized) return;");
                stringBuilder.AppendLine();

                foreach (string label in eventLabels)
                {
                    stringBuilder.AppendLine($"            {label} = {label}Reference;");
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine("            IsInitialized = true;");
                stringBuilder.AppendLine("        }");
            }


            string FormattedEventLabel(EventReference eventReference)
            {
                string path = eventReference.ToString();
                string name = path.Substring(path.LastIndexOf('/') + 1);

                int dotIndex = name.LastIndexOf('.');
                if (dotIndex > 0) name = name.Substring(0, dotIndex);

                string[] parts = Regex.Split(name, @"[^A-Za-z0-9]+").Where(s => s.Length > 0).ToArray();

                StringBuilder stringBuilder = new();
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

                string formattedLabel = stringBuilder.ToString();

                if (char.IsDigit(formattedLabel[0])) formattedLabel = "_" + formattedLabel;

                var csharpKeywords = new HashSet<string>(StringComparer.Ordinal)
                {
                    "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
                    "decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally",
                    "fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long",
                    "namespace","new","null","object","operator","out","override","params","private","protected","public","readonly",
                    "ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this",
                    "throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"
                };
                if (csharpKeywords.Contains(formattedLabel)) formattedLabel = "_" + formattedLabel;

                formattedLabel = Regex.Replace(formattedLabel, @"[^A-Za-z0-9_]", "");

                return formattedLabel;
            }
        }
    }
}
#endif