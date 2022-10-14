using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour, IExperimentUI
{
    public GameObject buttonPrefab;

    private float key_scale = 1f;// キーの大きさ
    private float key_dist = 1.5f;// キーの中心間の距離
    private int keynum = 4;// キーの数

    private RectTransform[] buttons;
    private RectTransform background_transform;

    private ARMarkerDetector detector;

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
    }

    void Update()
    {

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

        for (int i = 0; i < keynum; i++)
        {

            foreach (var pos in fingertipAnchoredPositions)
            {

                float key_size = this.buttons[i].sizeDelta.x * this.buttons[i].localScale.x;

                if (this.buttons[i].anchoredPosition.x - key_size / 2 < pos.x && pos.x < this.buttons[i].anchoredPosition.x + key_size / 2 &&
                    this.buttons[i].anchoredPosition.y - key_size / 2 < pos.y && pos.y < this.buttons[i].anchoredPosition.y + key_size / 2)
                {

                }

            }
        }

    }

    public void Click(int index)
    {

    }
}
