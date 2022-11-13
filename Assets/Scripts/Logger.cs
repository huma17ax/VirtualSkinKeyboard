using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;

// ログ残すクラス
// <日時(ミリ秒まで)>, <Front/Back>, <ログ種別(クラス)>, <呼び出し元情報>, <実データ>
public class Logger
{
    static List<string> logs = new List<string>();

    public static void Logging<T>(T data)
    {
        string timestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffffff");
        string datastr = JsonUtility.ToJson(data);
        var caller = new System.Diagnostics.StackFrame(1, false).GetMethod();
        string callerstr = caller.DeclaringType.FullName + "/" + caller.Name;
        string className = data.GetType().Name;

        logs.Add(string.Join(",", new[] { timestr, "Front", className, callerstr, ConvertToCSVFormat(datastr) }));
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

public class ButtonLog
{
    [SerializeField] private float[] position = new float[2];
    [SerializeField] private float angle;
    [SerializeField] private float size_dots;
    [SerializeField] private float size_mm;
    [SerializeField] private float index;

    public ButtonLog(Vector2 _pos, float _angle, float _size_dots, float _size_mm, float _index)
    {
        this.position[0] = _pos.x;
        this.position[1] = _pos.y;
        this.angle = _angle;
        this.size_dots = _size_dots;
        this.size_mm = _size_mm;
        this.index = _index;
    }
}

public class TimerStateLog
{
    [SerializeField] private string state;
    [SerializeField] private int pick_index;

    public TimerStateLog(string _state, int _pick_index)
    {
        this.state = _state;
        this.pick_index = _pick_index;
    }
}

public class TouchToButtonLog
{
    [SerializeField] private int index;
    [SerializeField] private bool is_picked;
    [SerializeField] private float[] position = new float[2];

    public TouchToButtonLog(int _index, bool _is_picked, Vector2 _pos)
    {
        this.position[0] = _pos.x;
        this.position[1] = _pos.y;
        this.index = _index;
        this.is_picked = _is_picked;
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