using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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

public class TaskLine
{
    
}
