using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour, IExperimentUI
{
    public GameObject buttonPrefab;

    private const float MARKER_SIZE = 23;// 実際のマーカーの大きさ[mm]
    private const float KEY_SIZE = 15;// キーの大きさ[mm]
    private const float KEY_DISTANCE = 45;// キーの中心間の距離[mm]

    private float key_scale = KEY_SIZE/MARKER_SIZE;
    private float key_dist = KEY_DISTANCE/MARKER_SIZE;
    private int keynum = 4;// キーの数

    private RectTransform[] buttons;
    private RectTransform background_transform;

    private ARMarkerDetector detector;

    enum TIMER_STATE {
        WAIT, // 押下待ち
        ACCEPTING, // 長押し中
        REST, // 次のキー指定まで空ける
    }
    TIMER_STATE timer_state = TIMER_STATE.REST;
    float timer = 0.5f;
    int picked_index = -1;
    Vector2[] fingerPositions;

    Texture2D normal_button_texture, picked_button_texture, selected_button_texture;

    void Start()
    {
        this.buttons = new RectTransform[this.keynum];

        for (int i = 0; i < this.keynum; i++)
        {
            GameObject obj = Instantiate(this.buttonPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0), this.transform);
            this.buttons[i] = obj.GetComponent<RectTransform>();
            this.buttons[i].localPosition = Vector3.zero;
        }

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.background_transform = GameObject.Find("Canvas/Background").GetComponent<RectTransform>();

        this.normal_button_texture = Resources.Load<Texture2D>("Images/box");
        this.picked_button_texture = Resources.Load<Texture2D>("Images/picked_box");
        this.selected_button_texture = Resources.Load<Texture2D>("Images/selected_box");
    }

    void Update()
    {
        if (this.timer > 0f)
        {
            this.timer -= Time.deltaTime;
            if (this.timer <= 0f)
            {
                if (this.timer_state == TIMER_STATE.ACCEPTING) {
                    this.timer = 0.5f;
                    this.timer_state = TIMER_STATE.REST;
                    this.buttons[picked_index].Find("Fill").GetComponent<RectTransform>().localScale = Vector3.zero;
                    this.buttons[picked_index].GetComponent<RawImage>().texture = this.normal_button_texture;
                    this.picked_index = -1;
                }
                else if (this.timer_state == TIMER_STATE.REST) {
                    this.timer = 0f;
                    this.picked_index = Random.Range(0, 4);
                    this.timer_state = TIMER_STATE.WAIT;
                    this.buttons[picked_index].GetComponent<RawImage>().texture = this.picked_button_texture;
                }
            }
            if (this.timer_state == TIMER_STATE.ACCEPTING) {
                this.buttons[picked_index].Find("Fill").GetComponent<RectTransform>().localScale = Vector3.one * (1f - this.timer);
            }
        }

        Vector2 axis = this.detector.nextPosition - this.detector.markerPosition;
        Vector2 scaled_axis = axis * new Vector2(640, 480) * this.background_transform.localScale;
        Vector2 downward = new Vector2(-scaled_axis.y, scaled_axis.x);

        Vector2 scaled_marker_position = this.detector.markerPosition * new Vector2(640, 480) * this.background_transform.localScale;
        float angle = Mathf.Atan2(-scaled_axis.y, -scaled_axis.x);

        for (int i = 0; i < keynum; i++)
        {
            Vector2 pos = scaled_marker_position + scaled_axis * (2 + i * this.key_dist) + downward * 0f;
            this.buttons[i].anchoredPosition = pos;

            this.buttons[i].localRotation = Quaternion.Euler(0, 0, 360 * angle / (2 * Mathf.PI));

            float scale = this.key_scale * scaled_axis.magnitude / this.buttons[i].sizeDelta.x;
            this.buttons[i].localScale = new Vector3(scale, scale, 0);
        }
    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
        this.fingerPositions = fingertipAnchoredPositions;
    }

    public void Click(int index)
    {
        if (this.timer_state == TIMER_STATE.WAIT) {
            if (index == 3 - this.picked_index) {
                this.timer = 1f;
                this.timer_state = TIMER_STATE.ACCEPTING;
                this.buttons[picked_index].GetComponent<RawImage>().texture = this.selected_button_texture;
            }
        }
    }
}
