using System.Collections.Generic;

namespace Editor.TodoIst.DataStructure
{
    public class GroupTask
    {
        public GroupTask()
        {
            Tasks = new List<CodeTaskLine>();
        }

        public void AddNewTask(CodeTaskLine task)
        {
            if (Tasks.Contains(task)) return;
            Tasks.Add(task);
            Value += task.TaskPriority;
            ScriptName = task.ScriptName;
        }
    
        public string ScriptName { get; set; }
        public List<CodeTaskLine> Tasks { get; set; }
        public int Value { get; set; }
    }
}
