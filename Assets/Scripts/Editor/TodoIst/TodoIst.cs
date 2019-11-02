using System.Collections.Generic;
using UnityEngine;

namespace Editor.TodoIst
{
    public class TodoIst 
    {
        public TodoIst(){}
    
        private TodoIstSearch todoIstSearch;
        private TodoIstGUI todoIstGUI;
    
        private List<string> m_scriptNames;
        public List<TaskLine> m_tasks;
    
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

        public void GUI_SetPriorityColor(int taskPriority = -1)
        {
            if(taskPriority == -1) GUI.backgroundColor = Color.white;
            GUI.backgroundColor = todoIstGUI.GetPriorityColor(taskPriority);
        }
        
        public void GUI_DrawLine(Color color)
        {
            todoIstGUI.DrawUILine(color);
        }

        public void SortTasksByPriority()
        {
            //TODO: refactor
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

//        List<CodeTaskLine> SimpleTaskSort(List<CodeTaskLine> tasks)
//        {
//            //TODO: refactor
//            for (int j = 0; j <= tasks.Count - 2; j++) {
//                for (int i = 0; i <= tasks.Count - 2; i++)
//                {
//                    CodeTaskLine codeTaskLine = (CodeTaskLine) tasks[i];
//                    CodeTaskLine nextCodeTaskLine = (CodeTaskLine) tasks[i+1];
//                    if (codeTaskLine.TaskPriority < nextCodeTaskLine.TaskPriority) {
//                        CodeTaskLine temp= tasks[i + 1];
//                        tasks[i + 1] = tasks[i];
//                        tasks[i] = temp;
//                    }
//                }
//            }
//
//            return tasks;
//        }
//    
    }
}
