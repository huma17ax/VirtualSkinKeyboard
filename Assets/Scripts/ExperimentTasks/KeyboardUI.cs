using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardUI : MonoBehaviour, IExperimentUI
{
    public GameObject keyPrefab;

    private const float MARKER_SIZE = 24;// 実際のマーカーの大きさ[mm]
    private const float KEY_SIZE = 15;// キーの一辺の大きさ[mm]
    private const float KEY_DISTANCE = 15;// キーの中心間の距離[mm]
    private const float DISTANCE_FROM_MARKER = 30;// ARマーカーからの距離[mm]
    private const int FONT_SIZE = 80;// キーに表示される文字の大きさ

    private RectTransform rect_transform;
    private ARMarkerDetector detector;
    private RectTransform background_transform;

    private Dictionary<char, RectTransform> keys = new Dictionary<char, RectTransform>();

    private char[] hovered_chars = { ' ', ' ', ' ', ' ' };

    private Text input_text;

    private string inputted_chars = "";
    private string incorrect_chars = "";
    private string required_chars = "ABCDEFGHIJ";

    private static readonly string[] keys_array = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

    void Start()
    {
        this.rect_transform = this.GetComponent<RectTransform>();

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.background_transform = GameObject.Find("Canvas/Background").GetComponent<RectTransform>();
        this.input_text = this.rect_transform.Find("InputTexts").GetComponent<Text>();

        for (int i = 0; i < 26; i++)
        {
            GameObject obj = Instantiate(this.keyPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0), this.transform);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            Text key_char = rt.Find("Char").GetComponent<Text>();
            key_char.text = "" + (char)('A'+i);
            key_char.fontSize = FONT_SIZE;
            this.keys.Add((char)('A' + i), rt);
        }
    }

    void Update()
    {
        Vector2 axis = this.detector.nextPosition - this.detector.markerPosition;
        Vector2 scaled_axis = axis * new Vector2(640, 480) * this.background_transform.localScale;
        Vector2 downward = new Vector2(-scaled_axis.y, scaled_axis.x);

        Vector2 scaled_marker_position = this.detector.markerPosition * new Vector2(640, 480) * this.background_transform.localScale;
        float angle = Mathf.Atan2(-scaled_axis.y, -scaled_axis.x);

        for (int i = 0; i < keys_array.Length; i++)
        {
            string keys_row = keys_array[i];
            float offset_y = (i - 1) * KEY_DISTANCE / MARKER_SIZE;
            float offset_x = -0.5f * i * KEY_DISTANCE / MARKER_SIZE;

            for (int j = 0; j < keys_row.Length; j++)
            {
                char target_char = keys_row[j];
                Vector2 pos = scaled_marker_position + scaled_axis * ((10 - j) * KEY_DISTANCE / MARKER_SIZE + offset_x + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * offset_y;
                this.keys[target_char].anchoredPosition = pos;

                this.keys[target_char].localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

                float scale = KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / this.keys[target_char].sizeDelta.x;
                this.keys[target_char].localScale = new Vector3(scale, scale, 0);
            }
        }

    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
    }

    public void Click(int index)
    {
        this.input_text.text += this.hovered_chars[index];
        Logger.Logging(new TouchedKeyLog(this.hovered_chars[index]));
        if (this.input_text.text.Length > 35) this.input_text.text.Remove(0, 1);
    }

    private void InputChar(char c)
    {
        if (this.incorrect_chars.Length > 0)
        {
            this.incorrect_chars += c;
        }
        else if (this.required_chars.Length > 0 && this.required_chars[0] == c)
        {
            this.inputted_chars += this.required_chars[0];
            this.required_chars = this.required_chars.Remove(0, 1);
        }
        else
        {
            this.incorrect_chars += c;
        }
        this.input_text.text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
    }

    private void DeleteChar()
    {
        if (this.incorrect_chars.Length > 0)
        {
            this.incorrect_chars = this.incorrect_chars.Remove(this.incorrect_chars.Length - 1);
        }
        else if (this.inputted_chars.Length > 0)
        {
            this.required_chars = this.inputted_chars[this.inputted_chars.Length - 1] + this.required_chars;
            this.inputted_chars = this.inputted_chars.Remove(this.inputted_chars.Length - 1);
        }
        this.input_text.text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
    }
}
