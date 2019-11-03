using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Editor.TodoIst;
using Editor.TodoIst.DataStructure;
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
                TodoIstUtils.RemoveLineFromScript(task.ScriptPath, task);
            }

            GUI.backgroundColor = TodoIstUtils.GetPriorityColor(task.TaskPriority);
        }
        

        public void Task_ScriptButton(CodeTaskLine task)
        {
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button(CreateGUIIcon(m_icons.m_iconScript), GUILayout.Width(ICON_SIZE),
                GUILayout.Height(ICON_SIZE)))
            {
                AssetDatabase.OpenAsset(task.Script, task.Line);
            }

            GUI.backgroundColor = TodoIstUtils.GetPriorityColor(task.TaskPriority);
        }
        
        public void Task_Tag(CodeTaskLine task)
        {
            for(int i = 0; i< task.Tags.Count;i++){
                GUI.backgroundColor = Color.white;
                GUILayout.Button(task.Tags[i], "MiniButton", GUILayout.Width(10 * task.Tags[i].Length));
                GUI.backgroundColor = TodoIstUtils.GetPriorityColor(task.TaskPriority);
            }
        }
        
        public void Task_Header(CodeTaskLine task)
        {
            EditorGUILayout.LabelField(string.Format("{0}.{1}", task.ScriptName.Replace(
                    task.ScriptName.Substring(0, 1),
                    task.ScriptName.Substring(0, 1).ToUpper()),
                task.ScriptPath.EndsWith(".cs") ? "cs" : "js"), EditorStyles.boldLabel);
        }
        
        public void Task_Tag_Header(string header)
        {
            GUILayout.Button(header, "MiniButton", GUILayout.Width(10 * header.Length));
        }

        public void Task_ScriptPath(CodeTaskLine task)
        {
            EditorGUILayout.LabelField(string.Format("{0} at line {1}.", task.ScriptPath, task.Line));
        }
        public void Task_Message(CodeTaskLine task)
        {

            EditorGUILayout.LabelField(string.Format("{0}:",task.Hastag), EditorStyles.boldLabel, GUILayout.Width(45));
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
        

    }
    

}