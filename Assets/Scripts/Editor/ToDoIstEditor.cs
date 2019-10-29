using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;



public class ToDoIstEditor : EditorWindow
{
    private Texture2D m_iconScript;
    private Texture2D m_iconDone;
    private Texture2D[] m_iconFlags = new Texture2D[3];
    
    private const float ICON_SIZE = 20f;
    
    private List<TaskLine> m_tasks;

    private bool m_settings;
    
    private enum GuiType{Simple,Group}
    
    private GuiType m_guiMode = GuiType.Simple;

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
            
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (scriptText != String.Empty)
            {
                if (scriptText.Contains("//TODO:"))
                {
                    //TODO: dokoncit
                    //Priority handler
//                    string prioritySubString =
//                        scriptText.Substring(scriptText.IndexOf("//TODO:", StringComparison.Ordinal) - 3, 10);
                    int priorityTask = 0;
//                    for (int i = 0; i < prioritySubString.Length; i++)
//                    {
//                        if (prioritySubString[i] == '!') priorityTask++;
//                    }
                    //-----------------
                    
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
        m_settings = EditorGUILayout.Foldout(m_settings,"Settings:");
        if(m_settings){
            m_guiMode = (GuiType)EditorGUILayout.EnumPopup("Show Mode: ",m_guiMode);
        }
        
        foreach (var task in m_tasks)
        {
            CodeTaskLine codeTaskLine = (CodeTaskLine) task;

            if(m_guiMode == GuiType.Simple)
            {
                SimpleGUIMode(codeTaskLine);
            }
            else
            {
                
            }
        }
    }


    void SimpleGUIMode(CodeTaskLine codeTaskLine)
    {

        GUI.backgroundColor = GetPriorityColor(codeTaskLine.TaskPriority);
        EditorGUILayout.BeginVertical("Box");
                
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(string.Format("{0}.{1}",codeTaskLine.ScriptName.Replace(
                codeTaskLine.ScriptName.Substring(0,1),
                codeTaskLine.ScriptName.Substring(0,1).ToUpper()),
            codeTaskLine.ScriptPath.EndsWith(".cs")? "cs" : "js"),EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button(CreateGUIIcon(m_iconDone),GUILayout.Width(20),GUILayout.Height(20)))
        {
            RemoveLineFromScript(codeTaskLine.ScriptPath, codeTaskLine);
        }
        GUI.backgroundColor = GetPriorityColor(codeTaskLine.TaskPriority);
        
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} at line {1}.", codeTaskLine.ScriptPath,codeTaskLine.Line));
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button(CreateGUIIcon(m_iconScript),GUILayout.Width(20),GUILayout.Height(20)))
        {
            AssetDatabase.OpenAsset (codeTaskLine.Script, codeTaskLine.Line);
        }
        GUI.backgroundColor = GetPriorityColor(codeTaskLine.TaskPriority);
                        
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("TODO: ", EditorStyles.boldLabel,GUILayout.Width(57));
        EditorGUILayout.LabelField(string.Format("{0} (!{1})",codeTaskLine.Message,codeTaskLine.TaskPriority));
        if(codeTaskLine.TaskPriority > 0)
        { 
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(GetPriorityFlag(codeTaskLine.TaskPriority-1),GUILayout.Width(20),GUILayout.Height(20)))
            {
                Debug.Log("This task has priority, keep working hard!");
            }
            GUI.backgroundColor = GetPriorityColor(codeTaskLine.TaskPriority);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        GUI.backgroundColor = Color.white;

    }

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
    
    
}
