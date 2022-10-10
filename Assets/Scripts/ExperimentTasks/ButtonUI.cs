using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour, IExperimentUI
{
    public GameObject buttonPrefab;

    private float key_scale = 1f;// キーの大きさ
    private float key_dist = 1.5f;// キーの中心間の距離
    private int keynum = 5;// キーの数

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
        Vector2 downward = new Vector2(-axis.y, axis.x);

        float angle = Mathf.Atan2(-axis.y, -axis.x);
        for (int i = 0; i < keynum; i++)
        {
            Vector2 pos = this.detector.markerPosition + axis * (i + 2) + downward * 0f;
            this.buttons[i].anchoredPosition =
                new Vector3(
                    pos.x * 640 * this.background_transform.localScale.x,
                    pos.y * 480 * this.background_transform.localScale.y,
                    0);

            this.buttons[i].localRotation = Quaternion.Euler(0, 0, 360 * angle / (2 * Mathf.PI));

            float scale = this.key_scale * Vector2.Distance(
                new Vector2(640 * this.detector.markerPosition.x, 480 * this.detector.markerPosition.y),
                new Vector2(640 * this.detector.nextPosition.x, 480 * this.detector.nextPosition.y)
            ) / this.buttons[i].sizeDelta.x;
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
