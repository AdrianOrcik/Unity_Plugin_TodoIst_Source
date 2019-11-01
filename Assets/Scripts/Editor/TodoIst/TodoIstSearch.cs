using System;
using System.Collections.Generic;
using UnityEditor;

namespace Editor.TodoIst
{
    public class TodoIstSearch 
    {
        public TodoIstSearch(TodoIst _todoIst)
        {
            todoIst = _todoIst;
        }

        private TodoIst todoIst;
        private const string HASTAG_PREFIX = "TODO:";
    
        public void Search(ref List<TaskLine> m_tasks, ref List<string> m_scriptNames)
        {
        
            m_tasks = new List<TaskLine>();
            m_scriptNames = new List<string>();
        
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        
            List<MonoScript> scripts = new List<MonoScript>();
            Dictionary<string,string> pathScript = new Dictionary<string, string>();

            HandleCorrectScripts(assetPaths, ref scripts, ref pathScript);
            FindHastagInScripts(scripts, pathScript);

        }
    
        void HandleCorrectScripts(string[] _assetPaths, ref List<MonoScript> _scripts, ref Dictionary<string,string> _pathScript)
        {
            foreach (string assetPath in _assetPaths)
            {
                if (assetPath.Contains("/Editor/") || !assetPath.Contains("Assets")) continue;
                if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js"))
                {
                    MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    _scripts.Add(mono);
                    if(!_pathScript.ContainsKey(mono.name)){ 
                        _pathScript.Add(mono.name,assetPath);
                    }
                }
            }
        }

        void FindHastagInScripts(List<MonoScript> _scripts, Dictionary<string,string> _pathScript )
        {
            foreach (MonoScript script in _scripts)
            {
                var scriptText = script.text;
                var lineCount = 0;

                bool isRenderingBox = false;
            
                while (scriptText != String.Empty)
                {
                    if (scriptText.Contains(HASTAG_PREFIX))
                    {
                        int taskPriority = PriorityHandler(scriptText);
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
                        if(!todoIst.Get_scriptNames.Contains(script.name))todoIst.Get_scriptNames.Add(script.name);
                        AddNewCodeTask(todoIst.Get_tasks,script,todo, script.name, lineCount, _pathScript[script.name], taskPriority);
                    }
                    else
                    {
                        lineCount = 0;
                        scriptText = String.Empty;
                    }
                }
            }
        }

        int PriorityHandler(string scriptText)
        {
            int priorityTask = 0;
            string prioritySubString =
                scriptText.Substring(scriptText.IndexOf(HASTAG_PREFIX, StringComparison.Ordinal) - 3, 3);
            for (int i = 0; i < prioritySubString.Length; i++)
            {
                if (prioritySubString[i] == '!') priorityTask++;
            }

            return priorityTask;
        }
    
        //TODO: make generic
        void AddNewCodeTask(List<TaskLine> m_tasks, MonoScript _script, string _todo, string _scriptName, int _line, string _scriptPath, int _taskPriority)
        {
            CodeTaskLine codeTaskLine = new CodeTaskLine(_script,_todo,_scriptName, _line, _scriptPath, _taskPriority);
            if(!m_tasks.Contains(codeTaskLine))
            { 
                m_tasks.Add(codeTaskLine);
            }
        }
    }
}
