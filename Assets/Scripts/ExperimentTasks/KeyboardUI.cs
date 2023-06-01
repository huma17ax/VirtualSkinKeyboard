using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class KeyState
{
    public RectTransform rectTransform;
    public float timer;
    public KeyState(RectTransform rt)
    {
        this.rectTransform = rt;
        this.timer = 0f;
    }
}

public class KeyboardUI : MonoBehaviour, IExperimentUI
{
    public GameObject keyPrefab;

    private const float MARKER_SIZE = 23;// 実際のマーカーの大きさ[mm]
    private const float KEY_SIZE = 10;// キーの一辺の大きさ[mm]
    private const float KEY_DISTANCE = 15.8f;// キーの中心間の距離[mm]
    private const float DISTANCE_FROM_MARKER = 40;// ARマーカーからキーUIの距離[mm]
    private readonly Vector2 DETECTION_CENTER_OFFSET = new Vector2(-1.88f, -0.35f);// キーの中心と当たり判定の中心の差
    private const int FONT_SIZE = 110;// キーに表示される文字の大きさ

    private RectTransform rect_transform;
    private ARMarkerDetector detector;
    private RectTransform background_transform;

    private Dictionary<char, KeyState> keys = new Dictionary<char, KeyState>();
    private KeyState SD_key;

    private char[] hovered_chars = { ' ', ' ', ' ', ' ' };
    private char[] clicked_chars = { ' ', ' ', ' ', ' ' };

    private RectTransform phrase, warning, warning2;

    private string inputted_chars = "";
    private string incorrect_chars = "";
    private string required_chars = "";
    private static readonly string[,] phrases_set = {
        {"ABCDEFGHIJKLM","NOPQRSTUVWXYZ","ABCDEFGHIJKLM","NOPQRSTUVWXYZ","ABCDEFGHIJKLM","NOPQRSTUVWXYZ"},
        {"NPDMHAZOIBWLS","EUXRTKGJVYCFQ","VLHWJRCEYQDIB","UPAGKOZNTFSXM","CAOUYZRIHDLVP","XTWQBSEFNKJMG"}, 
        {"UHNIJKGAMXSZF","PLRCDBWOEYQVT","OPIXTRBKLHAFJ","DQVSGYEWMNCUZ","NJKYHUSMVDBQE","RFZLIWOXTCAPG"},
        {"OJAVLUFRIGHEW","DCMKNTZXSQBYP","LMIRZJCSVFQNU","HGPBTAEODXKWY","ERCPHABTIZLON","VDWKQGUSXYMFJ"},
        {"SLGMQZUWEPIXO","KHDVNBRFYATJC","IATLCQSMJWNPO","YUFDKZBVRGHXE","PCQGUOLFZVBJY","RSTAWIDNEKXMH"},
    };
    public int phrases_set_index = 0;
    private int phrase_index = -1;
    private float phrase_timer = 0;

    bool input_accepting = false;

    private static readonly string[] keys_array = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

    private Texture2D normal_key_texture, clicked_key_texture, touching_key_texture, disabled_key_texture;

    public bool forceStop = false;

    void Start()
    {
        this.rect_transform = this.GetComponent<RectTransform>();

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.background_transform = GameObject.Find("Canvas/Background").GetComponent<RectTransform>();
        this.phrase = this.rect_transform.Find("Phrase").GetComponent<RectTransform>();
        this.warning = this.rect_transform.Find("Warning").GetComponent<RectTransform>();
        this.warning2 = GameObject.Find("Canvas/Warning2").GetComponent<RectTransform>();

        for (int i = 0; i < 26; i++)
        {
            GameObject obj = Instantiate(this.keyPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0), this.transform);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            Text key_char = rt.Find("Char").GetComponent<Text>();
            key_char.text = "" + (char)('A' + i);
            key_char.fontSize = FONT_SIZE;
            this.keys.Add((char)('A' + i), new KeyState(rt));
        }
        RectTransform sd_rt = Instantiate(this.keyPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0), this.transform).GetComponent<RectTransform>();
        sd_rt.localPosition = Vector3.zero;
        sd_rt.Find("Char").GetComponent<Text>().text = "";
        this.SD_key = new KeyState(sd_rt);

        this.normal_key_texture = Resources.Load<Texture2D>("Images/black_box");
        this.clicked_key_texture = Resources.Load<Texture2D>("Images/red_box_filled");
        this.touching_key_texture = Resources.Load<Texture2D>("Images/green_box");
        this.disabled_key_texture = Resources.Load<Texture2D>("Images/gray_out_box");

        this.StopTyping();
    }

    void Update()
    {
        if (this.forceStop) {
            this.forceStop = false;
            this.input_accepting = false;
            Logger.Logging(
                new PhraseStateLog(
                    "ForceStop", this.phrases_set_index, this.phrase_index, ""));
            this.inputted_chars = "";
            this.incorrect_chars = "";
            this.required_chars = phrases_set[this.phrases_set_index, this.phrase_index];
            this.phrase.GetComponent<Text>().text = this.required_chars;
        }

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
                Vector2 pos = scaled_marker_position + scaled_axis * ((10f - j) * KEY_DISTANCE / MARKER_SIZE + offset_x + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * offset_y;
                this.keys[target_char].rectTransform.anchoredPosition = pos;

                this.keys[target_char].rectTransform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

                float scale = KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / this.keys[target_char].rectTransform.sizeDelta.x;
                this.keys[target_char].rectTransform.localScale = new Vector3(scale, scale, 0);

                if (this.keys[target_char].timer > 0f)
                {
                    this.keys[target_char].timer -= Time.deltaTime;
                }
                Logger.Logging(new KeyLog(target_char, this.keys[target_char].rectTransform, KEY_SIZE));
            }
        }

        Vector2 sd_pos = scaled_marker_position + scaled_axis * (DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * 0f;
        this.SD_key.rectTransform.anchoredPosition = sd_pos;
        this.SD_key.rectTransform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        float sd_scale = 1f / MARKER_SIZE * scaled_axis.magnitude / this.SD_key.rectTransform.sizeDelta.x;
        this.SD_key.rectTransform.localScale = new Vector3(KEY_DISTANCE * sd_scale, 45.5f * sd_scale, 0);
        if (this.SD_key.timer > 0f)
        {
            this.SD_key.timer -= Time.deltaTime;
        }
        Logger.Logging(new KeyLog('#', this.SD_key.rectTransform, KEY_DISTANCE));

        this.phrase.anchoredPosition = scaled_marker_position + scaled_axis * (5.5f * KEY_DISTANCE / MARKER_SIZE + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * -2f * KEY_DISTANCE / MARKER_SIZE;
        this.phrase.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        this.phrase.localScale = new Vector3(1, 1, 0) * KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / 40f;
        if (this.phrase_timer > 0f) {
            this.phrase_timer -= Time.deltaTime;
            if (this.phrase_timer < 0f) {
                this.phrase_timer = 0f;
            }
            this.phrase.GetComponent<Text>().fontSize = (int)(40f * (1f - this.phrase_timer / 0.5f));
        }

        this.warning.anchoredPosition = scaled_marker_position + downward * -1f;
        this.warning.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        this.warning.localScale = new Vector3(1, 1, 0) * KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / 40f;
        this.warning.gameObject.SetActive(this.detector.markerTiltWarning || !this.detector.isDetected);

        this.UpdateKeyTextures();
    }

    private void UpdateKeyTextures()
    {
        Texture2D applying_texture;
        // char[] touching_chars = this.hovered_chars.Where((c, i) => this.is_touching[i]).ToArray();

        foreach (KeyValuePair<char, KeyState> target in this.keys)
        {
            if (clicked_chars.Contains(target.Key)) applying_texture = this.clicked_key_texture;
            else if (this.input_accepting == false) applying_texture = this.disabled_key_texture;
            // else if (touching_chars.Contains(target.Key)) applying_texture = this.touching_key_texture;
            else applying_texture = this.normal_key_texture;
            target.Value.rectTransform.GetComponent<RawImage>().texture = applying_texture;
        }

        if (clicked_chars.Contains('#')) applying_texture = this.clicked_key_texture;
        // else if (touching_chars.Contains('#')) applying_texture = this.touching_key_texture;
        else applying_texture = this.normal_key_texture;
        this.SD_key.rectTransform.GetComponent<RawImage>().texture = applying_texture;
    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
        // 触れた位置と判定中心の距離がradius以下であるような，最も近いキーに判定を入れる
        float radius = Mathf.Sqrt(Mathf.Pow(KEY_DISTANCE/2, 2) + Mathf.Pow(45.5f/2f-KEY_DISTANCE, 2));

        for (int i = 0; i < 4; i++)
        {
            float min_dist = float.MaxValue;
            char _char = ' ';
            foreach (KeyValuePair<char, KeyState> target in this.keys)
            {
                float ratio_dots_per_mm = (target.Value.rectTransform.sizeDelta.x * target.Value.rectTransform.localScale.x) / KEY_SIZE;
                float dist = Vector2.Distance(target.Value.rectTransform.anchoredPosition + DETECTION_CENTER_OFFSET * ratio_dots_per_mm, fingertipAnchoredPositions[i]);
                float lim = radius * ratio_dots_per_mm;
                if (dist <= lim && dist < min_dist)
                {
                    min_dist = dist;
                    _char = target.Key;
                }
            }
            this.hovered_chars[i] = _char;

            float key_angle_rad = this.SD_key.rectTransform.localEulerAngles.z * Mathf.Deg2Rad;
            Vector2 lateral = new Vector2(Mathf.Cos(key_angle_rad), Mathf.Sin(key_angle_rad)) * 10;
            float dist_height = DistancePointToLine(
                fingertipAnchoredPositions[i] - this.SD_key.rectTransform.anchoredPosition,
                lateral);
            Vector2 longitudinal = Quaternion.Euler(0, 0, 90) * lateral;
            float dist_width = DistancePointToLine(
                fingertipAnchoredPositions[i] - this.SD_key.rectTransform.anchoredPosition,
                longitudinal);
            Vector2 sd_key_range = this.SD_key.rectTransform.sizeDelta * this.SD_key.rectTransform.localScale / 2;
            if (dist_width < sd_key_range.x && dist_height < sd_key_range.y)
            {
                this.hovered_chars[i] = '#';
            }
        }

        for (int i=0; i<4; i++) {
            if (this.clicked_chars[i] != this.hovered_chars[i]) this.clicked_chars[i] = ' ';
        }

        Logger.Logging(new FingerHoverLog(fingertipAnchoredPositions, this.hovered_chars));
    }

    private static float DistancePointToLine(Vector2 point, Vector2 line)
    {
        // 直線(ベクトル)と点の距離を返す
        float angle = Vector2.Angle(line, point) * Mathf.Deg2Rad;
        return point.magnitude * Mathf.Sin(angle);
    }

    public void NotifyWristPosition(Vector2 pos) {
        this.warning2.anchoredPosition = new Vector2(pos.x, (-240 * this.background_transform.localScale.y)+30);
        bool wrist_in_frame = pos.y > (-240 * this.background_transform.localScale.y);
        this.warning2.gameObject.SetActive(!wrist_in_frame);
    }

    public void Press(int index)
    {
        char c = this.hovered_chars[index];
        Logger.Logging(new PressedKeyLog(c, index));
        this.clicked_chars[index] = c;
        if (this.input_accepting == false)
        {
            if (c == '#')
            {
                // Startキーとして機能する
                this.StartTyping();
                this.SD_key.timer = 0.15f;
            }
        }
        else
        {
            if (c == '#')
            {
                // Deleteキーとして機能する
                this.DeleteChar();
                this.SD_key.timer = 0.15f;
            }
            else if (c != ' ')
            {
                this.InputChar(c);
                this.keys[c].timer = 0.15f;
                if (this.required_chars == "") this.StopTyping();
            }
        }
    }
    public void Release(int index)
    {
        this.clicked_chars[index] = ' ';
        Logger.Logging(new ReleasedKeyLog(index));
    }

    private void StartTyping()
    {
        if (this.phrase_index == phrases_set.GetLength(1)) return;
        this.input_accepting = true;
        Logger.Logging(
            new PhraseStateLog(
                "Start", this.phrases_set_index, this.phrase_index,
                phrases_set[this.phrases_set_index, this.phrase_index]));
    }

    private void StopTyping()
    {
        this.input_accepting = false;
        Logger.Logging(
            new PhraseStateLog(
                "Stop", this.phrases_set_index, this.phrase_index, ""));

        // foreach (KeyValuePair<char, KeyState> target in this.keys)
        // {
        //     target.Value.timer = 0f;
        // }
        this.phrase_timer = 0.5f;

        this.phrase_index++;
        this.inputted_chars = "";
        if (this.phrase_index == phrases_set.GetLength(1))
        {
            this.required_chars = "";
        }
        else
        {
            this.required_chars = phrases_set[this.phrases_set_index, this.phrase_index];
        }
        this.phrase.GetComponent<Text>().text = this.required_chars;
    }

    private void InputChar(char c)
    {
        if (this.incorrect_chars.Length > 0)
        {
            this.incorrect_chars += c;
            Logger.Logging(new UpdateTextLog(c.ToString(), false));
        }
        else if (this.required_chars.Length > 0 && this.required_chars[0] == c)
        {
            this.inputted_chars += this.required_chars[0];
            this.required_chars = this.required_chars.Remove(0, 1);
            Logger.Logging(new UpdateTextLog(c.ToString(), true));
        }
        else
        {
            this.incorrect_chars += c;
            Logger.Logging(new UpdateTextLog(c.ToString(), false));
        }
        this.phrase.GetComponent<Text>().text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
    }

    private void DeleteChar()
    {
        if (this.incorrect_chars.Length > 0)
        {
            this.incorrect_chars = this.incorrect_chars.Remove(this.incorrect_chars.Length - 1);
            Logger.Logging(new UpdateTextLog("", true));
        }
        else if (this.inputted_chars.Length > 0)
        {
            this.required_chars = this.inputted_chars[this.inputted_chars.Length - 1] + this.required_chars;
            this.inputted_chars = this.inputted_chars.Remove(this.inputted_chars.Length - 1);
            Logger.Logging(new UpdateTextLog("", false));
        }
        this.phrase.GetComponent<Text>().text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
    }
}
