﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;



public class ToDoIstEditor : EditorWindow
{
    private Texture2D m_iconScript;
    private Texture2D m_iconDone;
    private Texture2D[] m_iconFlags = new Texture2D[3];
    
    private const float ICON_SIZE = 20f;
    
    private List<TaskLine> m_tasks;
    private List<string> m_scriptNames;
    private bool m_settings;
    
    private enum GuiType{Simple,Group}
    private GuiType m_guiMode = GuiType.Simple;

    private const string HASTAG_PREFIX = "TODO:";
    
    //TODO: still update as Renderer
    
    [MenuItem("Window/ToDoIst", false, 1)]
    static void Init()
    {
        ToDoIstEditor window = (ToDoIstEditor)GetWindow(typeof(ToDoIstEditor));
        window.Show();
    }

    void OnGUI()
    {

        InitTextureIcons();
        
        //--Task find algorithm
        m_tasks = new List<TaskLine>();
        //Get Assets paths
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();

        //Handle correct scripts
        List<MonoScript> scripts = new List<MonoScript>();
        Dictionary<string,string> pathScript = new Dictionary<string, string>();
        foreach (string assetPath in assetPaths)
        {
            if (assetPath.Contains("/Editor/") || !assetPath.Contains("Assets")) continue;
            if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js"))
            {
                MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                scripts.Add(mono);
                if(!pathScript.ContainsKey(mono.name)){ 
                    pathScript.Add(mono.name,assetPath);
                }
            }
            
        }
        
        //Find TODOs in filtered scripts
        foreach (MonoScript script in scripts)
        {
            var scriptText = script.text;
            var lineCount = 0;

            bool isRenderingBox = false;
            
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (scriptText != String.Empty)
            {
                if (scriptText.Contains(HASTAG_PREFIX))
                {
                    //Priority handler
                    string prioritySubString =
                        scriptText.Substring(scriptText.IndexOf(HASTAG_PREFIX, StringComparison.Ordinal) - 3, 3);
                    int priorityTask = 0;
                    for (int i = 0; i < prioritySubString.Length; i++)
                    {
                        if (prioritySubString[i] == '!') priorityTask++;
                    }
                    //-----------------
                    
                    int start = scriptText.IndexOf(HASTAG_PREFIX, StringComparison.Ordinal) + 5;
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
                    if(!m_scriptNames.Contains(script.name))m_scriptNames.Add(script.name);
                    AddNewCodeTask(script,todo, script.name, lineCount, pathScript[script.name], priorityTask);
                }
                else
                {
                    lineCount = 0;
                    scriptText = String.Empty;
                }
            }
        }
        
        //------------------------------------------
        GUIRender();
    }

    void GUIRender()
    {
        m_settings = EditorGUILayout.Foldout(m_settings,"Settings:");
        if(m_settings){
            m_guiMode = (GuiType)EditorGUILayout.EnumPopup("Show Mode: ",m_guiMode);
        }
        
        if (m_guiMode == GuiType.Group)
        {
            GroupGUIMode(m_tasks);
            return;
        }
        
        if(m_guiMode == GuiType.Simple){
            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                SimpleGUIMode(codeTaskLine);
            }
        }
    }

   //------------GUI Modes
    void GroupGUIMode(List<TaskLine> m_tasks)
    {
        CodeTaskLine _codeTaskLine = null;

        foreach (var scriptName in m_scriptNames)
        {
            List<CodeTaskLine> codeTaskLines = new List<CodeTaskLine>();
            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                if(codeTaskLine.ScriptName == scriptName) codeTaskLines.Add(codeTaskLine);
            }

            int taskID = 0;
            EditorGUILayout.BeginVertical("Box");
            Task_Header(codeTaskLines[taskID]);
            
            foreach (var task in codeTaskLines)
            {
                GUI.backgroundColor = GetPriorityColor(codeTaskLines[taskID].TaskPriority);
                EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" ");
                        Task_DoneButton(task);
                    EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                    Task_ScriptPath(task);
                    Task_ScriptButton(task);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                    Task_Message(task);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                
                GUI.backgroundColor = Color.white;
                GUILayout.Space(2);
                EditorGUILayout.EndHorizontal();

                taskID++; 
            }

            EditorGUILayout.EndHorizontal();
        }
    }
    
    void SimpleGUIMode(CodeTaskLine codeTaskLine)
    {

        GUI.backgroundColor = GetPriorityColor(codeTaskLine.TaskPriority);
        EditorGUILayout.BeginVertical("Box");
                
        EditorGUILayout.BeginHorizontal();
            Task_Header(codeTaskLine);
            Task_DoneButton(codeTaskLine);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        
        EditorGUILayout.BeginHorizontal();
            Task_ScriptPath(codeTaskLine);
            Task_ScriptButton(codeTaskLine);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
            Task_Message(codeTaskLine);
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        GUI.backgroundColor = Color.white;
    }
    
    //-------------Utils

    Color GetPriorityColor(int priority)
    {
        switch (priority)
        {
            case 0: return Color.white;
            case 1: return Color.cyan;
            case 2: return Color.yellow;
            case 3: return Color.red;
        }
        
        return Color.white;
    }

    GUIContent GetPriorityFlag(int priority)
    {
        return CreateGUIIcon(m_iconFlags[priority]);
    }
    
    void AddNewCodeTask(MonoScript _script, string _todo, string _scriptName, int _line, string _scriptPath, int _taskPriority)
    {
        CodeTaskLine codeTaskLine = new CodeTaskLine(_script,_todo,_scriptName, _line, _scriptPath, _taskPriority);
        if(!m_tasks.Contains(codeTaskLine))
        { 
            m_tasks.Add(codeTaskLine);
        }
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

        m_tasks.Remove(codeTaskLine);
        File.Delete(pathScript);
        File.Move(tempFile, pathScript);
        AssetDatabase.Refresh();
    }

    void InitTextureIcons()
    {
        m_iconDone = Resources.Load<Texture2D>("icons/icon_done");
        m_iconScript = Resources.Load<Texture2D>("icons/icon_script");
        m_iconFlags[0] = Resources.Load<Texture2D>("icons/icon_value_1");
        m_iconFlags[1] = Resources.Load<Texture2D>("icons/icon_value_2");
        m_iconFlags[2] = Resources.Load<Texture2D>("icons/icon_value_3");
    }

    GUIContent CreateGUIIcon(Texture2D icon)
    {
        return new GUIContent(icon);
    }
    
    
    //--------------GUI Elements
    void Task_Header(CodeTaskLine task)
    {
        EditorGUILayout.LabelField(string.Format("{0}.{1}",task.ScriptName.Replace(
                task.ScriptName.Substring(0,1),
                task.ScriptName.Substring(0,1).ToUpper()),
            task.ScriptPath.EndsWith(".cs")? "cs" : "js"),EditorStyles.boldLabel);
    }

    void Task_DoneButton(CodeTaskLine task)
    {
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button(CreateGUIIcon(m_iconDone),GUILayout.Width(20),GUILayout.Height(20)))
        {
            RemoveLineFromScript(task.ScriptPath, task);
        }
        GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
    }

    void Task_ScriptPath(CodeTaskLine task)
    {
        EditorGUILayout.LabelField(string.Format("{0} at line {1}.", task.ScriptPath,task.Line));
    }

    void Task_ScriptButton(CodeTaskLine task)
    {
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button(CreateGUIIcon(m_iconScript),GUILayout.Width(ICON_SIZE),GUILayout.Height(ICON_SIZE)))
        {
            AssetDatabase.OpenAsset (task.Script, task.Line);
        }
        GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
    }

    void Task_Message(CodeTaskLine task)
    {
        EditorGUILayout.LabelField("TODO: ", EditorStyles.boldLabel,GUILayout.Width(57));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.TextField(string.Format("{0} (!{1})",task.Message,task.TaskPriority));
        GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
        if(task.TaskPriority > 0)
        { 
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(GetPriorityFlag(task.TaskPriority-1),GUILayout.Width(ICON_SIZE),GUILayout.Height(ICON_SIZE)))
            {
                Debug.Log("This task has priority, keep working hard!");
            }
            GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
        }
    }
    
}
