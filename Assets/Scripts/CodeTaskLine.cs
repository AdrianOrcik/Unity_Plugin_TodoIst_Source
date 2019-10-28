using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CodeTaskLine : TaskLine
{
    public CodeTaskLine(string _message, string _scriptName, int _line)
    {
        Message = _message;
        Line = _line;
        ScriptName = _scriptName;
    }
    
    public string Message { get; set; }
    public int Line { get; set; }
    
    public string ScriptName { get; set; }

    public string GetCodeHash()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ScriptName);
        sb.Append(Line);
        return sb.ToString();
    }
}
