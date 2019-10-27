using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToDoIstEditor : EditorWindow
{
    //TODO: still update as Renderer
    
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/ToDoIst", false, 1)]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ToDoIstEditor window = (ToDoIstEditor)EditorWindow.GetWindow(typeof(ToDoIstEditor));
        window.Show();
    }

    void OnGUI()
    {
        //Get All directories in assets folder
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        
        //List of scripts
        //MonoScripts representing asset script
        List<MonoScript> scripts = new List<MonoScript>();
        Dictionary<string,string> pathScript = new Dictionary<string, string>();

        //GetAll Scripts in AssetsFolder
        foreach (string assetPath in assetPaths)
        {
            if (!assetPath.Contains("/Editor/"))
            {
                if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js"))
                {
                    MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    scripts.Add(mono);
                    if(!pathScript.ContainsKey(mono.name)){ 
                        pathScript.Add(mono.name,assetPath);
                    }
                }
            }
        }

        //Find Todos mark
        foreach (MonoScript script in scripts)
        {
            string scriptText;
            scriptText = script.text;

            int lineCount = 0;
            if (scriptText.Contains("//TODO:"))
            {
                GUILayout.Label ("In script \"" + script.name + "\"", EditorStyles.boldLabel);
            }

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (scriptText != String.Empty)
            {
                if (scriptText.Contains("//TODO:"))
                {
                    int start = scriptText.IndexOf("//TODO:", StringComparison.Ordinal) + 7;
                    int end = scriptText.Substring(start).IndexOf("\n", StringComparison.Ordinal);

                    for (int i = 0; i < start + end + 1; i++)
                    {
                        if (scriptText[i] == '\n')
                        {
                            lineCount++;
                        }
                    }

                    string todo = scriptText.Substring(start, end);
                    scriptText = scriptText.Substring(start + end + 1);
                    
                    
                    GUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button (todo + " :" + lineCount.ToString (), EditorStyles.textArea)) {
                        AssetDatabase.OpenAsset (script, lineCount);
                    }
                    if (GUILayout.Button ("X", EditorStyles.miniButton)) {
                        
                        string tempFile = Path.GetTempFileName();

                        int l_line = 0;
                        using(var sr = new StreamReader(pathScript[script.name]))
                        using(var sw = new StreamWriter(tempFile))
                        {
                            string line = String.Empty;

                            while((line = sr.ReadLine()) != null)
                            {
                                l_line++;
                                
                                
                                if(l_line != 13)
                                    sw.WriteLine(line);
                            }
                        }

                        File.Delete(pathScript[script.name]);
                        File.Move(tempFile, pathScript[script.name]);
                        AssetDatabase.Refresh();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    lineCount = 0;
                    scriptText = String.Empty;
                }

            }
        }
    }
}
