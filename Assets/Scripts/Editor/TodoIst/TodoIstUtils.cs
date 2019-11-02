using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.TodoIst
{
    public static class TodoIstUtils
    {
        public static List<string> ActiveTags = new List<string>();
        
        public static void SortTasksByPriority(ref List<TaskLine> m_tasks)
        {
            for (int j = 0; j <= m_tasks.Count - 2; j++) {
                for (int i = 0; i <= m_tasks.Count - 2; i++)
                {
                    CodeTaskLine codeTaskLine = (CodeTaskLine) m_tasks[i];
                    CodeTaskLine nextCodeTaskLine = (CodeTaskLine) m_tasks[i+1];
                    if (codeTaskLine.TaskPriority < nextCodeTaskLine.TaskPriority) {
                        TaskLine temp= m_tasks[i + 1];
                        m_tasks[i + 1] = m_tasks[i];
                        m_tasks[i] = temp;
                    }
                }
            }
        }
        
        public static void RemoveLineFromScript(string pathScript, CodeTaskLine codeTaskLine)
        {
            string tempFile = Path.GetTempFileName();

            int l_line = 0;
            using (var sr = new StreamReader(pathScript))
            using (var sw = new StreamWriter(tempFile))
            {
                string line = String.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    l_line++;

                    if (l_line != codeTaskLine.Line)
                        sw.WriteLine(line);
                }
            }

            File.Delete(pathScript);
            File.Move(tempFile, pathScript);
            AssetDatabase.Refresh();
        }
        
        public static void DrawUILine(Color color, float thickness = 0.5f, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }
        
        public static Color GetPriorityColor(int priority)
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
        
        public static string DropDownSelector(string label, string selectedItem, string[] items , params GUILayoutOption[] layoutOptions)
        {
            if( selectedItem == null ) selectedItem = items[0];
    
            var oldIndex = Array.IndexOf( items, selectedItem );
            if( oldIndex < 0 || oldIndex > items.Length )
            {
                oldIndex = 0;
                selectedItem = items[0];
            }
    
            var newIndex = EditorGUILayout.Popup( label, oldIndex, items, layoutOptions );
            selectedItem = items[newIndex];
    
            return selectedItem;
        }
        
        //NOT USED
        public static List<CodeTaskLine> SimpleTaskSort(List<CodeTaskLine> tasks)
        {
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
        
    }
}
