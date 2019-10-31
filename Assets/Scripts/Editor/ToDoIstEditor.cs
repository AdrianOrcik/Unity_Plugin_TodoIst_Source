using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//TODO: still update as Renderer
//TODO: Scroll panel vertical

public class ToDoIstEditor : EditorWindow
{

    private Vector2 m_scrollPosition;
    private Texture2D m_iconScript;
    private Texture2D m_iconDone;
    private Texture2D[] m_iconFlags = new Texture2D[3];
    
    private const float ICON_SIZE = 20f;
    
    private List<TaskLine> m_tasks;
    private List<string> m_scriptNames;
    private bool m_settings;
    private bool m_sorting;
    
    private enum GuiType{Simple,Group}
    private GuiType m_guiMode = GuiType.Simple;

    private const string HASTAG_PREFIX = "TODO:";
    
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

    List<TaskLine> SimpleTaskSort(List<TaskLine> tasks)
    {
        //TODO: refactor
        for (int j = 0; j <= tasks.Count - 2; j++) {
            for (int i = 0; i <= tasks.Count - 2; i++)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) tasks[i];
                CodeTaskLine nextCodeTaskLine = (CodeTaskLine) tasks[i+1];
                if (codeTaskLine.TaskPriority < nextCodeTaskLine.TaskPriority) {
                   TaskLine temp= tasks[i + 1];
                   tasks[i + 1] = tasks[i];
                   tasks[i] = temp;
                }
            }
        }

        return tasks;
    }
    
    List<CodeTaskLine> SimpleTaskSort(List<CodeTaskLine> tasks)
    {
        //TODO: refactor
        for (int j = 0; j <= tasks.Count - 2; j++) {
            for (int i = 0; i <= tasks.Count - 2; i++)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) tasks[i];
                CodeTaskLine nextCodeTaskLine = (CodeTaskLine) tasks[i+1];
                if (codeTaskLine.TaskPriority < nextCodeTaskLine.TaskPriority) {
                    CodeTaskLine temp= tasks[i + 1];
                    tasks[i + 1] = tasks[i];
                    tasks[i] = temp;
                }
            }
        }

        return tasks;
    }
    


    void GUIRender()
    {
        m_settings = EditorGUILayout.Foldout(m_settings,"Settings:");
        if(m_settings)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
                m_guiMode = (GuiType)EditorGUILayout.EnumPopup("Show Mode: ",m_guiMode);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tasks by priority:",GUILayout.Width(145));
                m_sorting = EditorGUILayout.Toggle(m_sorting, GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        //TODO: sortovanie podla priority

        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
        
        if (m_guiMode == GuiType.Group)
        {
            GroupGUIMode(m_tasks);
        }
        else if(m_guiMode == GuiType.Simple){
            
            if(m_sorting) m_tasks = SimpleTaskSort(m_tasks);
            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                SimpleGUIMode(codeTaskLine);
            }
        }
        
        EditorGUILayout.EndScrollView();
    }

   //------------GUI Modes
    void GroupGUIMode(List<TaskLine> m_tasks)
    {
        Group group = new Group();
        foreach (var task in m_tasks)
        {
            CodeTaskLine codeTaskLine = (CodeTaskLine) task;
            group.AddNew(codeTaskLine);
        }

        //TODO: refactor
        if(m_sorting) group.GroupTasks = group.SortDicByValue();
        
        int taskID = 0;
        foreach (var key in  group.GroupTasks.Keys)
        {
            EditorGUILayout.BeginVertical("Box");
            GroupTask groupTask = group.GroupTasks[key];
            Task_Header(groupTask.Tasks[taskID]);
        
            for (int i = 0; i < group.GroupTasks[key].Tasks.Count; i++)
            {
                GUI.backgroundColor = GetPriorityColor(groupTask.Tasks[taskID].TaskPriority);
                EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" ");
                        Task_DoneButton(groupTask.Tasks[taskID]);
                    EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                    Task_ScriptPath(groupTask.Tasks[taskID]);
                    Task_ScriptButton(groupTask.Tasks[taskID]);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                    Task_Message(groupTask.Tasks[taskID]);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                
                GUI.backgroundColor = Color.white;
                GUILayout.Space(2);
                EditorGUILayout.EndHorizontal();
                taskID++;
            }

            taskID = 0;
            EditorGUILayout.EndVertical();

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
