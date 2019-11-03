using System.Collections.Generic;
using System.Linq;

namespace Editor.TodoIst.DataStructure
{
    public class Group
    {
        public Group()
        {
            GroupTasks = new Dictionary<string, GroupTask>();
        }
    
        public void AddNew(CodeTaskLine task)
        {

            if (!GroupTasks.Keys.Contains(task.ScriptName))
            {
                GroupTask groupTask = new GroupTask();
                groupTask.AddNewTask(task);
                GroupTasks.Add(task.ScriptName,groupTask);
            }
            else
            {
                GroupTasks[task.ScriptName].AddNewTask(task);
            }
        }

        public  Dictionary<string, GroupTask> SortDicByValue()
        {
            Dictionary<string, GroupTask> sortedDictionary = new Dictionary<string, GroupTask>(); 
       
            List<GroupTask> values = GroupTasks.Values.ToList();
            values = new List<GroupTask>(values.OrderByDescending(x => x.Value));
            foreach (var value in values)
            {
                sortedDictionary.Add(value.ScriptName,value);
            }

            return sortedDictionary;
        }

        public Dictionary<string, GroupTask> GroupTasks { get; set; }
    }
}