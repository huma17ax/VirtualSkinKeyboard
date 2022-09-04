using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;

public class Logger
{
    static List<string> logs = new List<string>();

    public static void Logging<T>(T data)
    {
        string timestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffffff");
        string datastr = JsonUtility.ToJson(data);
        var caller = new System.Diagnostics.StackFrame(1, false).GetMethod();
        string callerstr = caller.DeclaringType.FullName + "/" + caller.Name;

        logs.Add(string.Join(",", new[] { timestr, callerstr, ConvertToCSVFormat(datastr) }));
    }

    private static string ConvertToCSVFormat(string str)
    {
        if (str.Contains(","))
        {
            return "\"" + str.Replace("\"", "\"\"") + "\"";
        }
        else
        {
            return str;
        }
    }

    public static void Output()
    {
        using (var writer = new StreamWriter("./logs.csv"))
        {
            foreach (var log in logs) writer.Write(log + "\n");
        }
    }
}

public class ARMarkerLog
{
    [SerializeField] private float[] position = new float[2];
    [SerializeField] private float angle = 0.0f;

    public ARMarkerLog(Vector2 _pos, Vector2 _next)
    {
        this.position[0] = _pos.x;
        this.position[1] = _pos.y;
        Vector2 offset = _next - _pos;
        this.angle = Mathf.Atan2(offset.y, offset.x);
    }
}

public class TouchedKeyLog
{
    [SerializeField] private string key;

    public TouchedKeyLog(char _key)
    {
        this.key = "" + _key;
    }
}