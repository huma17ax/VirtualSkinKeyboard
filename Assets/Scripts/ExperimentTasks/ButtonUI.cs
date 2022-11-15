using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour, IExperimentUI
{
    public GameObject buttonPrefab;

    private const float MARKER_SIZE = 23;// 実際のマーカーの大きさ[mm]
    public float[] KEY_SIZE = {15};// キーの大きさ[mm]
    private const float KEY_DISTANCE = 45;// キーの中心間の距離[mm]

    private float key_scale;
    private float key_dist = KEY_DISTANCE/MARKER_SIZE;
    private const int KEY_NUM = 4;// キーの数
    private const int COUNT_PER_STEP = 5;// 1ステップ(キーサイズ)ごとの試行数

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
    int[] key_pick_order;
    int key_pick_count = 0;
    int key_size_step = 0;
    Vector2[] fingerPositions;

    Texture2D normal_button_texture, picked_button_texture, selected_button_texture;

    void Start()
    {
        this.buttons = new RectTransform[KEY_NUM];

        for (int i = 0; i < KEY_NUM; i++)
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

        this.InitStep();
    }

    void Update()
    {
        if (this.timer > 0f)
        {
            this.timer -= Time.deltaTime;
            if (this.timer <= 0f)
            {
                if (this.timer_state == TIMER_STATE.ACCEPTING) {
                    Logger.Logging(new TimerStateLog("REST", this.picked_index));
                    this.timer = 0.5f;
                    this.timer_state = TIMER_STATE.REST;
                    this.buttons[picked_index].Find("Fill").GetComponent<RectTransform>().localScale = Vector3.zero;
                    this.buttons[picked_index].GetComponent<RawImage>().texture = this.normal_button_texture;
                    this.picked_index = -1;
                    if (this.key_pick_count == KEY_NUM*COUNT_PER_STEP) {
                        if (this.key_size_step == this.KEY_SIZE.Length-1) {
                            this.timer = 0f;// 終了
                        }
                        else {
                            this.key_size_step++;
                            this.InitStep();
                        }
                    }
                }
                else if (this.timer_state == TIMER_STATE.REST) {
                    this.timer = 0f;
                    this.picked_index = this.key_pick_order[this.key_pick_count];
                    this.key_pick_count++;
                    this.timer_state = TIMER_STATE.WAIT;
                    this.buttons[picked_index].GetComponent<RawImage>().texture = this.picked_button_texture;
                    Logger.Logging(new TimerStateLog("WAIT", this.picked_index));
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

        for (int i = 0; i < KEY_NUM; i++)
        {
            Vector2 pos = scaled_marker_position + scaled_axis * (2 + (KEY_NUM-1 - i) * this.key_dist) + downward * 0f;
            this.buttons[i].anchoredPosition = pos;

            this.buttons[i].localRotation = Quaternion.Euler(0, 0, 360 * angle / (2 * Mathf.PI));

            float scale = this.key_scale * scaled_axis.magnitude / this.buttons[i].sizeDelta.x;
            this.buttons[i].localScale = new Vector3(scale, scale, 0);

            Logger.Logging(new ButtonLog(pos, angle, scale * this.buttons[i].sizeDelta.x, this.KEY_SIZE[this.key_size_step], i));
        }
    }

    public void CalcHoverKey(Vector2[] fingertipAnchoredPositions)
    {
        this.fingerPositions = fingertipAnchoredPositions;
    }

    public void Click(int index)
    {
        Logger.Logging(
            new TouchToButtonLog(
                index,
                (index == this.picked_index),
                this.fingerPositions[index]
            )
        );
        if (this.timer_state == TIMER_STATE.WAIT) {
            if (index == this.picked_index) {
                this.timer = 1f;
                this.timer_state = TIMER_STATE.ACCEPTING;
                this.buttons[this.picked_index].GetComponent<RawImage>().texture = this.selected_button_texture;
                Logger.Logging(new TimerStateLog("ACC", this.picked_index));
            }
        }
    }

    private void InitStep() {
        this.key_scale = this.KEY_SIZE[this.key_size_step]/MARKER_SIZE;
        this.key_pick_order = this.GenerateRandomOrder();
        this.key_pick_count = 0;
    }

    private int[] GenerateRandomOrder() {
        int [] index_list = new int[KEY_NUM];
        for (int i=0; i < KEY_NUM; i++) index_list[i] = i;

        List<int> order = new List<int> {};
        for (int i=0; i<COUNT_PER_STEP; i++) {
            if (i==0) {
                order.AddRange(index_list.OrderBy(e => Guid.NewGuid()));
            }
            else {
                int j = UnityEngine.Random.Range(0,3);
                order.Add(index_list.Where(e => e!=order.Last()).ToList()[j]);
                order.AddRange(index_list.Where(e => e!=order.Last()).OrderBy(e => Guid.NewGuid()));
            }
        }

        return order.ToArray();
    }
}
