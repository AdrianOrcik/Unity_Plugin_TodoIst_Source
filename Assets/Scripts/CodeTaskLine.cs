using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CodeTaskLine : TaskLine
{
    public CodeTaskLine(MonoScript _script, string _message, string _scriptName, int _line, string _scriptPath, int _taskPriority)
    {
        Script = _script;
        Message = _message;
        Line = _line;
        ScriptName = _scriptName;
        ScriptPath = _scriptPath;
        TaskPriority = _taskPriority;
    }
    
    public string Message { get; set; }
    public int Line { get; set; }
    public string ScriptName { get; set; }
    
    public string ScriptPath { get; set; }

    public MonoScript Script { get; set; }

    public int TaskPriority { get; set; }

}
