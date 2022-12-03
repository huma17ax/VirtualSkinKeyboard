using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardUI : MonoBehaviour, IExperimentUI
{
    private RectTransform rect_transform;
    private ARMarkerDetector detector;
    private RectTransform background_transform;

    private char[] hovered_chars = { ' ', ' ', ' ', ' ' };

    private Text input_text;

    private string inputted_chars = "";
    private string incorrect_chars = "";
    private string required_chars = "ABCDEFGHIJ";

    void Start()
    {
        this.rect_transform = this.GetComponent<RectTransform>();

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.background_transform = GameObject.Find("Canvas/Background").GetComponent<RectTransform>();
        this.input_text = this.rect_transform.Find("InputTexts").GetComponent<Text>();
    }

    void Update()
    {
        Vector2 axis = this.detector.nextPosition - this.detector.markerPosition;
        Vector2 downward = new Vector2(-axis.y, axis.x);
        float rate = 11f;
        Vector2 pos = this.detector.markerPosition + axis * (rate * 21f / 40f) + downward * (rate / 20f);

        this.rect_transform.anchoredPosition =
            new Vector3(
                pos.x * 640 * this.background_transform.localScale.x,
                pos.y * 480 * this.background_transform.localScale.y,
                0);

        float angle = Mathf.Atan2(-axis.y, -axis.x);
        this.rect_transform.localRotation = Quaternion.Euler(0, 0, 360 * angle / (2 * Mathf.PI));

        float scale = rate * Vector2.Distance(
            new Vector2(640 * this.detector.markerPosition.x, 480 * this.detector.markerPosition.y),
            new Vector2(640 * this.detector.nextPosition.x, 480 * this.detector.nextPosition.y)
        ) / this.rect_transform.sizeDelta.x;
        this.rect_transform.localScale = new Vector3(scale, scale, 0);
    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
        const string top_keys = "QWERTYUIOP";
        const string mid_keys = "ASDFGHJKL";
        const string bottom_keys = "ZXCVBNM";

        for (int i = 0; i < 4; i++)
        {
            float angle = Mathf.Atan2(
                fingertipAnchoredPositions[i].y - this.rect_transform.anchoredPosition.y,
                fingertipAnchoredPositions[i].x - this.rect_transform.anchoredPosition.x
            ) / (2 * Mathf.PI) * 360;
            float dist = Vector2.Distance(this.rect_transform.anchoredPosition, fingertipAnchoredPositions[i]);
            Vector2 normalized_position =
                new Vector2(
                    dist * Mathf.Cos((angle - this.rect_transform.localRotation.eulerAngles.z) / 360 * (2 * Mathf.PI)),
                    dist * Mathf.Sin((angle - this.rect_transform.localRotation.eulerAngles.z) / 360 * (2 * Mathf.PI))
                ) / (this.rect_transform.sizeDelta * this.rect_transform.localScale) + new Vector2(0.5f, 0.5f);

            if (normalized_position.y < 1f / 3f)
            {
                // bottom row
                int x = (int)Mathf.Floor(normalized_position.x * 10f - 1f);
                if (x < 0 || x > 6) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = bottom_keys[x];
            }
            else if (normalized_position.y < 2f / 3f)
            {
                // middle row
                int x = (int)Mathf.Floor(normalized_position.x * 10f - 0.5f);
                if (x < 0 || x > 8) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = mid_keys[x];
            }
            else
            {
                // top row
                int x = (int)Mathf.Floor(normalized_position.x * 10f);
                if (x < 0 || x > 9) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = top_keys[x];
            }

        }
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
