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

    private const float MARKER_SIZE = 24;// 実際のマーカーの大きさ[mm]
    private const float KEY_SIZE = 10;// キーの一辺の大きさ[mm]
    private const float KEY_DISTANCE = 17.1f;// キーの中心間の距離[mm]
    private const float DISTANCE_FROM_MARKER = 30;// ARマーカーからキーUIの距離[mm]
    private readonly Vector2 DETECTION_CENTER_OFFSET = new Vector2(-1.88f, -0.35f);// キーの中心と当たり判定の中心の差
    private const int FONT_SIZE = 110;// キーに表示される文字の大きさ

    private RectTransform rect_transform;
    private ARMarkerDetector detector;
    private RectTransform background_transform;

    private Dictionary<char, KeyState> keys = new Dictionary<char, KeyState>();
    private KeyState SD_key;

    private char[] hovered_chars = { ' ', ' ', ' ', ' ' };
    private bool[] is_touching = { false, false, false, false };

    private RectTransform phrase;

    private string inputted_chars = "";
    private string incorrect_chars = "";
    private string required_chars = "";
    private static readonly string[,] phrases_set = {
        {"GZASNDFYUE","JWOTXPCHVB","QMKIRLHFCJ","XSNMPTYORL","QBWUVAIGDE",
        "ZKTLENRHCJ","UDWZSQBFAP","MIGVOXKYQT","WOEJNDRCBV","GLUMHZYFSA",
        "KIXPZLMOUT","RPKDINQSJF","YGWBCHEXAV"},
        {"UMEVWZLJNK","IDTCYFBPOQ","HSRGAXLZCU","RGKXFBOYET","JQVWHNMADI",
        "PSKYVCROGQ","XFTEIMWDAS","JNLBUZHPEQ","NGZPCMAFJL","DVBUHIYTSX",
        "KWOREDMVQC","ZXUKHBRNTO","WPYIAFLGSJ",},
        {"ZCLHQWYPFD","IANGTSXUKM","EOJBVRCAZH","WYMLRSGUVT","XDFNEQIBJP",
        "OKFPUZWDSE","BYIQGMLNJR","CAHOVXKTQB","JEYSTCNMDW","APGZRLKIVF",
        "UOXHZGDWFR","KPULSVOHJN","QYIETAMBCX",},
        {"PIZCEHWKVN","YGUQLXDBRA","TSMFJOUAGC","IWFTPDZYVS","MNBOEQLXRJ",
        "KHGXTYACPL","SUBFKVIDOR","WHJQZMENSB","NQEFRHKUXJ","OAGDZCLMVI",
        "TWYPBCFRND","PEMJOYKGHV","WAQSXTZULI",}
    };
    public int phrases_set_index = 0;
    private int phrase_index = -1;

    bool input_accepting = false;

    private static readonly string[] keys_array = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

    private Texture2D normal_key_texture, clicked_key_texture, touching_key_texture, disabled_key_texture;

    void Start()
    {
        this.rect_transform = this.GetComponent<RectTransform>();

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.background_transform = GameObject.Find("Canvas/Background").GetComponent<RectTransform>();
        this.phrase = this.rect_transform.Find("Phrase").GetComponent<RectTransform>();

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
        this.clicked_key_texture = Resources.Load<Texture2D>("Images/red_box");
        this.touching_key_texture = Resources.Load<Texture2D>("Images/green_box");
        this.disabled_key_texture = Resources.Load<Texture2D>("Images/gray_out_box");

        this.StopTyping();
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
                Vector2 pos = scaled_marker_position + scaled_axis * ((11.25f - j) * KEY_DISTANCE / MARKER_SIZE + offset_x + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * offset_y;
                this.keys[target_char].rectTransform.anchoredPosition = pos;

                this.keys[target_char].rectTransform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

                float scale = KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / this.keys[target_char].rectTransform.sizeDelta.x;
                this.keys[target_char].rectTransform.localScale = new Vector3(scale, scale, 0);

                if (this.keys[target_char].timer > 0f)
                {
                    this.keys[target_char].timer -= Time.deltaTime;
                }
            }
        }

        Vector2 sd_pos = scaled_marker_position + scaled_axis * (1 * KEY_DISTANCE / MARKER_SIZE + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * 0f;
        this.SD_key.rectTransform.anchoredPosition = sd_pos;
        this.SD_key.rectTransform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        float sd_scale = KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / this.SD_key.rectTransform.sizeDelta.x;
        this.SD_key.rectTransform.localScale = new Vector3(sd_scale, sd_scale * 3, 0);
        if (this.SD_key.timer > 0f)
        {
            this.SD_key.timer -= Time.deltaTime;
        }

        this.phrase.anchoredPosition = scaled_marker_position + scaled_axis * (6.75f * KEY_DISTANCE / MARKER_SIZE + DISTANCE_FROM_MARKER / MARKER_SIZE) + downward * -2f * KEY_DISTANCE / MARKER_SIZE;
        this.phrase.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        this.phrase.localScale = new Vector3(1, 1, 0) * KEY_SIZE / MARKER_SIZE * scaled_axis.magnitude / 40f;

        this.UpdateKeyTextures();
    }

    private void UpdateKeyTextures()
    {
        Texture2D applying_texture;
        char[] touching_chars = this.hovered_chars.Where((c, i) => this.is_touching[i]).ToArray();

        foreach (KeyValuePair<char, KeyState> target in this.keys)
        {
            if (this.input_accepting == false) applying_texture = this.disabled_key_texture;
            else if (target.Value.timer > 0f) applying_texture = this.clicked_key_texture;
            else if (touching_chars.Contains(target.Key)) applying_texture = this.touching_key_texture;
            else applying_texture = this.normal_key_texture;
            target.Value.rectTransform.GetComponent<RawImage>().texture = applying_texture;
        }

        if (this.SD_key.timer > 0f) applying_texture = this.clicked_key_texture;
        else if (touching_chars.Contains('#')) applying_texture = this.touching_key_texture;
        else applying_texture = this.normal_key_texture;
        this.SD_key.rectTransform.GetComponent<RawImage>().texture = applying_texture;
    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
        // 触れた位置と判定中心の距離がKEY_DISTANCE*5/8以下であるような，最も近いキーに判定を入れる

        for (int i = 0; i < 4; i++)
        {
            float min_dist = float.MaxValue;
            char _char = ' ';
            foreach (KeyValuePair<char, KeyState> target in this.keys)
            {
                float ratio_dots_per_mm = (target.Value.rectTransform.sizeDelta.x * target.Value.rectTransform.localScale.x) / KEY_SIZE;
                float dist = Vector2.Distance(target.Value.rectTransform.anchoredPosition + DETECTION_CENTER_OFFSET * ratio_dots_per_mm, fingertipAnchoredPositions[i]);
                float lim = KEY_DISTANCE * ratio_dots_per_mm * 5f / 8f;
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
    }

    private static float DistancePointToLine(Vector2 point, Vector2 line)
    {
        // 直線(ベクトル)と点の距離を返す
        float angle = Vector2.Angle(line, point) * Mathf.Deg2Rad;
        return point.magnitude * Mathf.Sin(angle);
    }

    public void Press(int index)
    {
        this.is_touching[index] = true;
        char c = this.hovered_chars[index];
        Logger.Logging(new TouchedKeyLog(c));
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
        this.is_touching[index] = false;
    }

    private void StartTyping()
    {
        if (this.phrase_index == phrases_set.GetLength(1)) return;
        this.input_accepting = true;
    }

    private void StopTyping()
    {
        this.input_accepting = false;

        foreach (KeyValuePair<char, KeyState> target in this.keys)
        {
            target.Value.timer = 0f;
        }

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
        this.phrase.GetComponent<Text>().text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
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
        this.phrase.GetComponent<Text>().text = "<color=silver>" + this.inputted_chars + "</color><color=red>" + this.incorrect_chars + "</color>" + this.required_chars;
    }
}
