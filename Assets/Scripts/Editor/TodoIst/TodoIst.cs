using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.TodoIst
{
    public class TodoIst 
    {
        public TodoIst(){}
    
        private TodoIstSearch todoIstSearch;
        private TodoIstGUI todoIstGUI;

        private List<string> m_scriptNames;
        private List<TaskLine> m_tasks;
    
        public List<string> Get_scriptNames { get { return m_scriptNames; } }
        public List<TaskLine> Get_tasks 
        { 
            get { return m_tasks; }
            set { m_tasks = value; }
        }
    
        public void Init()
        {
            m_scriptNames = new List<string>();
            m_tasks = new List<TaskLine>();
        
            if(todoIstSearch == null)todoIstSearch = new TodoIstSearch(this);
            if(todoIstGUI == null)todoIstGUI = new TodoIstGUI(this);
        }

        public void Search()
        {
            todoIstSearch.Search(ref m_tasks,ref m_scriptNames);
        }

        public void GUI_TaskHeader(CodeTaskLine task)
        {
            todoIstGUI.Task_Header(task);  
        }

        public void GUI_ScriptPath(CodeTaskLine task)
        {
            todoIstGUI.Task_ScriptPath(task);
        }

        public void GUI_TaskMessage(CodeTaskLine task)
        {
            todoIstGUI.Task_Message(task);
        }
        
        public void GUI_Button_Done(CodeTaskLine task)
        {
            todoIstGUI.Task_DoneButton(task);
        }
    
        public void GUI_Button_Script(CodeTaskLine task)
        {
            todoIstGUI.Task_ScriptButton(task);
        }

        public void GUI_Tag(CodeTaskLine task)
        {
            todoIstGUI.Task_Tag(task);
        }

        public void GUI_SetPriorityColor(int taskPriority = -1)
        {
            if(taskPriority == -1) GUI.backgroundColor = Color.white;
            GUI.backgroundColor = TodoIstUtils.GetPriorityColor(taskPriority);
        }
        
        public void GUI_DrawLine(Color color)
        {
            TodoIstUtils.DrawUILine(color);
        }

        public void SortTasksByPriority()
        {
            TodoIstUtils.SortTasksByPriority(ref m_tasks);
        }
    }
}
