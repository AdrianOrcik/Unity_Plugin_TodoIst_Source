using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Editor.TodoIst;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Editor.TodoIst
{
    public struct TextureIcons
    {
        public Texture2D m_iconScript;
        public Texture2D m_iconDone;
        public Texture2D[] m_iconFlags;
    }

    public class TodoIstGUI
    {

        public TodoIstGUI(TodoIst _todoIst)
        {
            todoIst = _todoIst;
            Init();
        }

        private TodoIst todoIst;

        private TextureIcons m_icons;
        private const float ICON_SIZE = 20f;

        public void Init()
        {
            InitTextures();
        }

        void InitTextures()
        {
            m_icons.m_iconDone = Resources.Load<Texture2D>("TodoIstIcons/icon_done");
            m_icons.m_iconScript = Resources.Load<Texture2D>("TodoIstIcons/icon_script");
            m_icons.m_iconFlags = new Texture2D[3];
            m_icons.m_iconFlags[0] = Resources.Load<Texture2D>("TodoIstIcons/icon_value_1");
            m_icons.m_iconFlags[1] = Resources.Load<Texture2D>("TodoIstIcons/icon_value_2");
            m_icons.m_iconFlags[2] = Resources.Load<Texture2D>("TodoIstIcons/icon_value_3");
        }

        public GUIContent GetPriorityFlag(int priority)
        {
            return CreateGUIIcon(m_icons.m_iconFlags[priority]);
        }

        GUIContent CreateGUIIcon(Texture2D icon)
        {
            return new GUIContent(icon);
        }

        public void Task_DoneButton(CodeTaskLine task)
        {
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button(CreateGUIIcon(m_icons.m_iconDone), GUILayout.Width(20), GUILayout.Height(20)))
            {
                RemoveLineFromScript(task.ScriptPath, task);
            }

            GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
        }

        public Color GetPriorityColor(int priority)
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

        public void Task_ScriptButton(CodeTaskLine task)
        {
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button(CreateGUIIcon(m_icons.m_iconScript), GUILayout.Width(ICON_SIZE),
                GUILayout.Height(ICON_SIZE)))
            {
                AssetDatabase.OpenAsset(task.Script, task.Line);
            }

            GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
        }

        void RemoveLineFromScript(string pathScript, CodeTaskLine codeTaskLine)
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

            todoIst.m_tasks.Remove(codeTaskLine);
            File.Delete(pathScript);
            File.Move(tempFile, pathScript);
            AssetDatabase.Refresh();
        }

        public void Task_Header(CodeTaskLine task)
        {
            EditorGUILayout.LabelField(string.Format("{0}.{1}", task.ScriptName.Replace(
                    task.ScriptName.Substring(0, 1),
                    task.ScriptName.Substring(0, 1).ToUpper()),
                task.ScriptPath.EndsWith(".cs") ? "cs" : "js"), EditorStyles.boldLabel);
        }

        public void Task_ScriptPath(CodeTaskLine task)
        {
            EditorGUILayout.LabelField(string.Format("{0} at line {1}.", task.ScriptPath, task.Line));
        }
        public void Task_Message(CodeTaskLine task)
        {

            EditorGUILayout.LabelField("TODO:", EditorStyles.boldLabel, GUILayout.Width(45));
            EditorGUILayout.LabelField(string.Format("({0}) {1}" , task.Line, task.Message));


            //TODO: use or remove flag icon
//            GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
//            if (task.TaskPriority > 0)
//            {
//                GUI.backgroundColor = Color.white;
//                if (GUILayout.Button(GetPriorityFlag(task.TaskPriority - 1), GUILayout.Width(ICON_SIZE),
//                    GUILayout.Height(ICON_SIZE)))
//                {
//                    Debug.Log("This task has priority, keep working hard!");
//                }
//            }
//            GUI.backgroundColor = GetPriorityColor(task.TaskPriority);
        }
        
        public void DrawUILine(Color color, float thickness = 0.5f, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }

    }
    

}