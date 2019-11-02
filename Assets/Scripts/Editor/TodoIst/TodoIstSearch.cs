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
        private string[] HASTAGS_PREFIX = new[] {"TODO", "FIX", "BUG"}; //TOOD: add to settings?
        private const int WHITE_SPACE_POSTFIX = 2;
        
        public void Search(ref List<TaskLine> m_tasks, ref List<string> m_scriptNames)
        {
            m_tasks = new List<TaskLine>();
            m_scriptNames = new List<string>();
        
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        
            List<MonoScript> scripts = new List<MonoScript>();
            Dictionary<string,string> pathScript = new Dictionary<string, string>();

            HandleCorrectScripts(assetPaths, ref scripts, ref pathScript);
            for(int i = 0; i < HASTAGS_PREFIX.Length; i++){
                FindHastagInScripts(scripts, pathScript,HASTAGS_PREFIX[i]);
            }

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

        void FindHastagInScripts(List<MonoScript> _scripts, Dictionary<string,string> _pathScript , string _hastag )
        {
            foreach (MonoScript script in _scripts)
            {
                var scriptText = script.text;
                var lineCount = 0;

                bool isRenderingBox = false;
            
                while (scriptText != String.Empty)
                {
                    if (scriptText.Contains(_hastag))
                    {
                        int taskPriority = PriorityHandler(scriptText, _hastag);
                        int start = scriptText.IndexOf(_hastag, StringComparison.Ordinal) + _hastag.Length + WHITE_SPACE_POSTFIX;
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
                        AddNewCodeTask(todoIst.Get_tasks,script,todo, script.name, lineCount, _pathScript[script.name], taskPriority, _hastag);
                    }
                    else
                    {
                        lineCount = 0;
                        scriptText = String.Empty;
                    }
                }
            }
        }

        int PriorityHandler(string _scriptText, string _hastag)
        {
            int priorityTask = 0;
            string prioritySubString =
                _scriptText.Substring(_scriptText.IndexOf(_hastag, StringComparison.Ordinal) - 3, 3);
            for (int i = 0; i < prioritySubString.Length; i++)
            {
                if (prioritySubString[i] == '!') priorityTask++;
            }

            return priorityTask;
        }
    
        //TODO: make generic
        void AddNewCodeTask(List<TaskLine> m_tasks, MonoScript _script, string _todo, string _scriptName, int _line, string _scriptPath, int _taskPriority, string _hastag)
        {
            CodeTaskLine codeTaskLine = new CodeTaskLine(_script,_todo,_scriptName, _line, _scriptPath, _taskPriority,_hastag);

            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTask = (CodeTaskLine) task;
                if (codeTask.HashKey.Equals(codeTaskLine.HashKey)) return;
            }

            if(!m_tasks.Contains(codeTaskLine))
            { 
                m_tasks.Add(codeTaskLine);
            }
        }
    }
}
