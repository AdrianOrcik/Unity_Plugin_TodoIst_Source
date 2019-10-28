using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToDoIstEditor : EditorWindow
{
    private Dictionary<string,TaskLine> m_tasks = new Dictionary<string, TaskLine>();

    //TODO: still update as Renderer
    
    [MenuItem("Window/ToDoIst", false, 1)]
    static void Init()
    {
        ToDoIstEditor window = (ToDoIstEditor)GetWindow(typeof(ToDoIstEditor));
        window.Show();
    }

    void OnGUI()
    {
        //Get Assets paths
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();

        //Handle correct scripts
        List<MonoScript> scripts = new List<MonoScript>();
        Dictionary<string,string> pathScript = new Dictionary<string, string>();
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
        
        //Find TODOs in filtered scripts
        foreach (MonoScript script in scripts)
        {
            var scriptText = script.text;
            var lineCount = 0;

            bool isRenderingBox = false;
            if (scriptText.Contains("//TODO:"))
            {
                isRenderingBox = true;
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField(script.name + (pathScript[script.name].EndsWith(".js")? ".js" : ".cs"), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(pathScript[script.name]);
                GUILayout.Space(10);
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

                    CodeTaskLine codeTaskLine = AddNewCodeTask(todo, script.name, lineCount);
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button (todo + " :" + lineCount, EditorStyles.textArea)) 
                    {
                        AssetDatabase.OpenAsset (script, lineCount);
                    }
                    if (GUILayout.Button ("X", EditorStyles.miniButton)) {
                        
                      RemoveLineFromScript(pathScript[script.name],(CodeTaskLine)m_tasks[codeTaskLine.GetCodeHash()]);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    lineCount = 0;
                    scriptText = String.Empty;
                }
            }
            
            if(isRenderingBox){
                EditorGUILayout.EndVertical();
            }
        }
    }

    CodeTaskLine AddNewCodeTask(string _todo, string _scriptName, int _line)
    {
        CodeTaskLine codeTaskLine = new CodeTaskLine(_todo,_scriptName, _line);
        if(!m_tasks.ContainsKey(codeTaskLine.GetCodeHash()))
        { 
            m_tasks.Add(codeTaskLine.GetCodeHash(),codeTaskLine);
        }

        return codeTaskLine;
    }
    
    void RemoveLineFromScript(string pathScript, CodeTaskLine codeTaskLine)
    {
        string tempFile = Path.GetTempFileName();

        int l_line = 0;
        using(var sr = new StreamReader(pathScript))
        using(var sw = new StreamWriter(tempFile))
        {
            string line = String.Empty;

            while((line = sr.ReadLine()) != null)
            {
                l_line++;
                
                if(l_line != codeTaskLine.Line)
                    sw.WriteLine(line);
            }
        }

        m_tasks.Remove(codeTaskLine.GetCodeHash());
        File.Delete(pathScript);
        File.Move(tempFile, pathScript);
        AssetDatabase.Refresh();
    }
}
