using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.TodoIst
{
    public class TodoIstEditor : EditorWindow
    {
        private TodoIst todoIst;
    
        private Vector2 m_scrollPosition;
        private bool m_settings;
        private bool m_prioritySorting;
        private bool m_tagSorting;
    
        private enum GuiType{Simple,Group}
        private GuiType m_guiMode = GuiType.Group;

    
        [MenuItem("Window/ToDoIst", false, 1)]
        static void WindowInit()
        {
            TodoIstEditor window = (TodoIstEditor)GetWindow(typeof(TodoIstEditor));
            window.Show();
        }

        void Init()
        {
            if(todoIst == null) todoIst = new TodoIst();
        }

        void OnGUI()
        {
            Init();
            todoIst.Init();
            todoIst.Search();
            GUIRenderer();
        }
    
        void GUIRenderer()
        {
            m_settings = EditorGUILayout.Foldout(m_settings,"Settings:");
            if(m_settings)
            {
                SettingsLayout();
            }
        
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);


            if (m_tagSorting)
            {

                GroupTagsLayout(todoIst.Get_tasks, "SKI");

            }else{
                if (m_guiMode == GuiType.Group)
                {
                    GroupTaskLayout(todoIst.Get_tasks);
                }
                else if(m_guiMode == GuiType.Simple){

                    if(m_prioritySorting)todoIst.SortTasksByPriority();
                    foreach (var task in todoIst.Get_tasks)
                    {
                        CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                        SimpleTaskLayout(codeTaskLine);
                    }
                }
            }
        
            EditorGUILayout.EndScrollView();
        }
        
        void GroupTagsLayout(List<TaskLine> m_tasks, string tag)
        {
            
            List<CodeTaskLine> filteredTasks = new List<CodeTaskLine>();
            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                if (codeTaskLine.Tags.Contains(tag))
                {
                    filteredTasks.Add(codeTaskLine);
                }
            }
            
            EditorGUILayout.BeginVertical("Box");
            todoIst.GUI_TagHeader(tag);

            foreach (var task in  filteredTasks)
            {
                todoIst.GUI_SetPriorityColor(task.TaskPriority);
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();
                todoIst.GUI_TaskMessage(task);
                todoIst.GUI_Button_Done(task);
                todoIst.GUI_Button_Script(task);
                EditorGUILayout.EndHorizontal();
                
                todoIst.GUI_SetPriorityColor();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
    }
        
        void GroupTaskLayout(List<TaskLine> m_tasks)
        {
            Group group = new Group();
            foreach (var task in m_tasks)
            {
                CodeTaskLine codeTaskLine = (CodeTaskLine) task;
                group.AddNew(codeTaskLine);
            }

            if (m_prioritySorting)
            {
                group.GroupTasks = group.SortDicByValue();
                foreach (var tasks in group.GroupTasks.Values)
                {
                    TodoIstUtils.SimpleTaskSort(tasks.Tasks);
                }
            }
            
            int taskID = 0;
            foreach (var key in  group.GroupTasks.Keys)
            {
                EditorGUILayout.BeginVertical("Box");
                GroupTask groupTask = group.GroupTasks[key];
                todoIst.GUI_TaskHeader(groupTask.Tasks[taskID]);

                for (int i = 0; i < group.GroupTasks[key].Tasks.Count; i++)
                {
                    todoIst.GUI_SetPriorityColor(groupTask.Tasks[taskID].TaskPriority);
                    EditorGUILayout.BeginVertical("Box");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    todoIst.GUI_Tag(groupTask.Tasks[taskID]);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    todoIst.GUI_TaskMessage(groupTask.Tasks[taskID]);
                    todoIst.GUI_Button_Done(groupTask.Tasks[taskID]);
                    todoIst.GUI_Button_Script(groupTask.Tasks[taskID]);
                    EditorGUILayout.EndHorizontal();
                    
                    todoIst.GUI_SetPriorityColor();
                    EditorGUILayout.EndHorizontal();
                    taskID++;
                }

                taskID = 0;
                EditorGUILayout.EndVertical();

            }
        }
    
        void SimpleTaskLayout(CodeTaskLine codeTaskLine)
        {
          
            todoIst.GUI_SetPriorityColor(codeTaskLine.TaskPriority);
            EditorGUILayout.BeginVertical("Box");
                
            EditorGUILayout.BeginHorizontal();
            todoIst.GUI_TaskHeader(codeTaskLine);
            todoIst.GUI_Tag(codeTaskLine);
            todoIst.GUI_Button_Done(codeTaskLine);
            todoIst.GUI_Button_Script(codeTaskLine);
            EditorGUILayout.EndHorizontal();
        
            GUILayout.Space(2);

            todoIst.GUI_DrawLine(Color.gray);
            EditorGUILayout.BeginHorizontal();
            todoIst.GUI_TaskMessage(codeTaskLine);
            EditorGUILayout.EndHorizontal();
 
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            todoIst.GUI_SetPriorityColor(); 
        }

        void SettingsLayout()
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
            m_prioritySorting = EditorGUILayout.Toggle(m_prioritySorting, GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tasks by tags:",GUILayout.Width(145));
            m_tagSorting = EditorGUILayout.Toggle(m_tagSorting, GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    
    }
}
