#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TBaltaks.FMODManagement.Editor
{
    [InitializeOnLoad]
    public static class ScriptAutoGenerator
    {
        public static bool debugLogging;

        private const string TargetFolder = "Assets/Plugins/FMOD Management";
        private const string TargetSubfolder = "/Generated Resources";
        private static string pathToThis;


        static ScriptAutoGenerator()
        {
            EditorApplication.delayCall += TryGenerateScripts;
        }

        
        private static void TryGenerateScripts()
        {
            pathToThis = FindPathToThis();
            if (pathToThis == null)
            {
                Debug.LogWarning("[FMOD Manager] Could not find package path — skipping script generation.");
                return;
            }

            if (!Directory.Exists(TargetFolder + TargetSubfolder)) Directory.CreateDirectory(TargetFolder + TargetSubfolder);

            EditorUtils.LoadPreviewBanks();
            try
            {
                var fmodSystem = EditorUtils.System;
                List<EventDescription> eventDescriptions = GetAllEventDescriptions(fmodSystem);
                List<string> globalParameterNames = GetGlobalParameterNames(fmodSystem);
                List<string> localParameterNames = GetLocalParameterNames(eventDescriptions);

                GenerateFMODEventsScript(eventDescriptions);
                GenerateFMODParametersScript(globalParameterNames, localParameterNames);
                GenerateAudioManagerScript();
            }
            finally
            {
                EditorUtils.UnloadPreviewBanks();
            }

            AssetDatabase.Refresh();
            if (debugLogging) Debug.Log($"[FMOD Manager] ScriptAutoGenerator finished generating");
        }


        private static List<EventDescription> GetAllEventDescriptions(FMOD.Studio.System fmodSystem)
        {
            List<EventDescription> allEventDescriptions = new();

            if (fmodSystem.getBankList(out Bank[] banks) != FMOD.RESULT.OK) return null;
            foreach (Bank bank in banks)
            {
                if (bank.getEventList(out EventDescription[] thisBanksEvents) != FMOD.RESULT.OK) return null;
                allEventDescriptions.AddRange(thisBanksEvents);
            }

            return allEventDescriptions;
        }


        private static List<string> GetGlobalParameterNames(FMOD.Studio.System fmodSystem)
        {
            HashSet<string> parameterNames = new();

            if (fmodSystem.getParameterDescriptionList(out PARAMETER_DESCRIPTION[] globalParameters) != FMOD.RESULT.OK) return null;
            foreach (PARAMETER_DESCRIPTION parameterDescription in globalParameters)
            {
                string parameterName = parameterDescription.name;
                parameterNames.Add(parameterName);
                if (debugLogging) Debug.Log($"[FMOD Manager] Found global parameter {parameterName} during script generation");
            }

            return parameterNames.ToList();
        }


        private static List<string> GetLocalParameterNames(List<EventDescription> eventDescriptions)
        {
            HashSet<string> parameterNames = new();

            foreach (EventDescription eventDescription in eventDescriptions)
            {
                if (eventDescription.getParameterDescriptionCount(out int parameterCount) != FMOD.RESULT.OK) return null;
                for (int i = 0; i < parameterCount; i++)
                {
                    if (eventDescription.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION parameterDescription) != FMOD.RESULT.OK) return null;
                    string parameterName = parameterDescription.name;
                    if (!parameterNames.Contains(parameterName)) parameterNames.Add(parameterName);
                    if (debugLogging) Debug.Log($"[FMOD Manager] Found local parameter {parameterName} during script generation");
                }
            }

            return parameterNames.ToList();
        }


        private static void GenerateFMODEventsScript(List<EventDescription> eventDescriptions)
        {
            string destinationPath = Path.Combine(TargetFolder + TargetSubfolder, "FMODEvents.cs");
            string fileContents = ConstructFileContents(eventDescriptions);

            if (File.Exists(destinationPath))
            {
                string existingContents = File.ReadAllText(destinationPath);
                if (existingContents == fileContents)
                {
                    if (debugLogging) Debug.Log($"No changes detected in FMODEvents.cs; skipping rewrite.");
                    return;
                }
            }

            File.WriteAllText(destinationPath, fileContents);
            if (debugLogging) Debug.Log($"[FMOD Manager] Created FMODEvents.cs");


            static string ConstructFileContents(List<EventDescription> eventDescriptions)
            {
                StringBuilder stringBuilder = new();

                stringBuilder.AppendLine("/* THIS IS A CODE GENERATED SCRIPT */");
                stringBuilder.AppendLine("   /* ... PLEASE NO TOUCHY ... */");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("namespace TBaltaks.FMODManagement");
                stringBuilder.AppendLine("{");
                stringBuilder.AppendLine("    public static class FMODEvents");
                stringBuilder.AppendLine("    {");

                AppendEventReferences(stringBuilder, eventDescriptions);

                stringBuilder.AppendLine("    }");
                stringBuilder.AppendLine("}");

                return stringBuilder.ToString();


                void AppendEventReferences(StringBuilder stringBuilder, List<EventDescription> eventDescriptions)
                {
                    if (eventDescriptions == null || eventDescriptions.Count < 1)
                    {
                        stringBuilder.AppendLine($"        // No events listed");
                        return;
                    }

                    foreach (EventDescription description in eventDescriptions)
                    {
                        description.getPath(out string path);
                        string label = FormattedEventLabel(path);
                        if (debugLogging) Debug.Log("Found and added event: " + label);

                        stringBuilder.AppendLine($"        public static string {label} = \"{path}\";");
                    }
                }


                string FormattedEventLabel(string eventPath)
                {
                    StringBuilder stringBuilder = new();
                    string name = eventPath.Substring(eventPath.LastIndexOf('/') + 1);

                    int dotIndex = name.LastIndexOf('.');
                    if (dotIndex > 0) name = name.Substring(0, dotIndex);

                    string[] parts = Regex.Split(name, @"[^A-Za-z0-9]+").Where(s => s.Length > 0).ToArray();
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


        private static void GenerateFMODParametersScript(List<string> globalParameters, List<string> localParameters)
        {
            string destinationPath = Path.Combine(TargetFolder + TargetSubfolder, "FMODParameters.cs");
            string fileContents = ConstructFileContents(globalParameters, localParameters);

            if (File.Exists(destinationPath))
            {
                string existingContents = File.ReadAllText(destinationPath);
                if (existingContents == fileContents)
                {
                    if (debugLogging) Debug.Log($"No changes detected in FMODParameters.cs; skipping rewrite.");
                    return;
                }
            }

            File.WriteAllText(destinationPath, fileContents);
            if (debugLogging) Debug.Log($"[FMOD Manager] Created FMODParameters.cs");


            static string ConstructFileContents(List<string> globalParameters, List<string> localParameters)
            {
                StringBuilder stringBuilder = new();

                stringBuilder.AppendLine("/* THIS IS A CODE GENERATED SCRIPT */");
                stringBuilder.AppendLine("   /* ... PLEASE NO TOUCHY ... */");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("namespace TBaltaks.FMODManagement");
                stringBuilder.AppendLine("{");

                // --- GlobalAudioParameters ---
                stringBuilder.AppendLine("    public enum GlobalAudioParameter");
                stringBuilder.AppendLine("    {");
                AppendEnumValues(stringBuilder, globalParameters);
                stringBuilder.AppendLine("    }");
                
                stringBuilder.AppendLine();

                // --- LocalAudioParameters ---
                stringBuilder.AppendLine("    public enum LocalAudioParameter");
                stringBuilder.AppendLine("    {");
                AppendEnumValues(stringBuilder, localParameters);
                stringBuilder.AppendLine("    }");

                stringBuilder.AppendLine("}");

                return stringBuilder.ToString();


                void AppendEnumValues(StringBuilder stringBuilder, List<string> values)
                {
                    if (values == null || values.Count < 1)
                    {
                        stringBuilder.AppendLine("        // No parameters listed");
                        return;
                    }

                    foreach (string value in values)
                    {
                        stringBuilder.AppendLine($"        {value},");
                    }
                }


                string FormattedEnumValue(string value) // UNUSED - DO NOT SANITISE
                {
                    if (char.IsDigit(value[0])) value = "_" + value;

                    var csharpKeywords = new HashSet<string>(StringComparer.Ordinal)
                {
                    "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
                    "decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally",
                    "fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long",
                    "namespace","new","null","object","operator","out","override","params","private","protected","public","readonly",
                    "ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this",
                    "throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while"
                };
                    if (csharpKeywords.Contains(value)) value = "_" + value;

                    string formattedValue = Regex.Replace(value, @"[^A-Za-z0-9_]", "");
                    return formattedValue;
                }
            }
        }


        private static void GenerateAudioManagerScript()
        {
            string sourcePath = Path.Combine(pathToThis, "AudioManagerTemplate.txt");
            string destinationPath = Path.Combine(TargetFolder, "AudioManager.cs");

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"[FMOD Manager] Missing template: {sourcePath}");
                return;
            }

            if (File.Exists(destinationPath))
            {
                if (debugLogging) Debug.Log($"[FMOD Manager] AudioManager.cs already exsists");
                return;
            }

            File.Copy(sourcePath, destinationPath);
            if (debugLogging) Debug.Log($"[FMOD Manager] Created AudioManager.cs");
        }


        private static string FindPathToThis()
        {
            string[] guids = AssetDatabase.FindAssets("ScriptAutoGenerator t:Script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    return Path.GetFullPath(directory);
                }
            }
            return null;
        }


        [MenuItem("Tools/FMOD-Unity Audio Manager/Regenerate FMOD resources")]
        private static void GenerateScriptsManually()
        {
            bool previousSetting = debugLogging;
            debugLogging = true;
            TryGenerateScripts();
            debugLogging = previousSetting;
        }
    }
}
#endif