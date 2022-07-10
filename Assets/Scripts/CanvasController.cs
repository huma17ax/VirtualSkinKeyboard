using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    private ImageReceiver imageReceiver;
    private SharedData<Color32[]> sh_image;

    private LandmarksReceiver landmarksReceiver;
    private SharedData<Vector2[]> sh_landmarks;

    private TouchReceiver touchReceiver;
    private SharedData<bool[]> sh_touches;

    private RawImage rawImage;
    private Texture2D texture;
    private Color32[] colors;

    private GameObject[] circles;
    private GameObject keyboard;
    private GameObject inputs;

    private bool[] pre = {false, false, false, false};

    private char[] hovered_chars = {' ', ' ', ' ', ' '};

    void Start()
    {
        this.rawImage = GetComponent<RawImage>();
        this.texture = new Texture2D(640, 480);
        this.rawImage.texture = this.texture;
        this.colors = new Color32[0];

        this.sh_image = new SharedData<Color32[]>();
        this.imageReceiver = new ImageReceiver(this.sh_image);
        this.imageReceiver.Start();

        this.sh_landmarks = new SharedData<Vector2[]>();
        this.landmarksReceiver = new LandmarksReceiver(this.sh_landmarks);
        this.landmarksReceiver.Start();

        this.sh_touches = new SharedData<bool[]>();
        this.touchReceiver = new TouchReceiver(this.sh_touches);
        this.touchReceiver.Start();

        this.circles = new GameObject[4];
        this.circles[0] = GameObject.Find("Canvas/Circle1");
        this.circles[1] = GameObject.Find("Canvas/Circle2");
        this.circles[2] = GameObject.Find("Canvas/Circle3");
        this.circles[3] = GameObject.Find("Canvas/Circle4");
        Debug.Log(this.circles[0]);
        this.keyboard = GameObject.Find("Canvas/Keyboard");
        this.inputs = GameObject.Find("Canvas/Inputs");
    }

    void Update()
    {
        if (this.sh_image.TryGet(out this.colors)) {
            this.texture.SetPixels32(this.colors);
            this.texture.Apply();
        }

        Vector2[] v;
        if (this.sh_landmarks.TryGet(out v)) {
            float width = 640 * this.rawImage.GetComponent<RectTransform>().localScale.x;
            float height = 480 * this.rawImage.GetComponent<RectTransform>().localScale.y;
            this.circles[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[0].x*width-width/2, -(v[0].y*height-height/2), 0);
            this.circles[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[1].x*width-width/2, -(v[1].y*height-height/2), 0);
            this.circles[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[2].x*width-width/2, -(v[2].y*height-height/2), 0);
            this.circles[3].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[3].x*width-width/2, -(v[3].y*height-height/2), 0);
        }

        CalcHoverKey();

        bool[] b;
        if (this.sh_touches.TryGet(out b)) {
            this.circles[0].GetComponent<RectTransform>().localScale = new Vector3(b[0] ? 2 : 1, b[0] ? 2 : 1 , 1);
            this.circles[1].GetComponent<RectTransform>().localScale = new Vector3(b[1] ? 2 : 1, b[1] ? 2 : 1 , 1);
            this.circles[2].GetComponent<RectTransform>().localScale = new Vector3(b[2] ? 2 : 1, b[2] ? 2 : 1 , 1);
            this.circles[3].GetComponent<RectTransform>().localScale = new Vector3(b[3] ? 2 : 1, b[3] ? 2 : 1 , 1);

            for (int i=0; i<4; i++) {
                if (pre[i]==false && b[i]==true) this.inputs.GetComponent<Text>().text += this.hovered_chars[i];
                if (this.inputs.GetComponent<Text>().text.Length > 35) this.inputs.GetComponent<Text>().text.Remove(0,1);
            }
            pre = b;
        }

    }

    private void CalcHoverKey() {
        string top_keys = "QWERTYUIOP";
        string mid_keys = "ASDFGHJKL";
        string bottom_keys = "ZXCVBNM";

        for (int i=0; i<4; i++) {
            RectTransform rt = this.keyboard.GetComponent<RectTransform>();
            Vector2 normalized_position = this.circles[i].GetComponent<RectTransform>().anchoredPosition/ (rt.sizeDelta * rt.localScale) + new Vector2(0.5f, 0.5f);

            if (normalized_position.y < 1f/3f) {
                // bottom row
                int x = (int)Mathf.Floor(normalized_position.x*10f-1f);
                if (x < 0 || x > 6) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = bottom_keys[x];
            }
            else if (normalized_position.y < 2f/3f) {
                // middle row
                int x = (int)Mathf.Floor(normalized_position.x*10f-0.5f);
                if (x < 0 || x > 8) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = mid_keys[x];
            }
            else {
                // top row
                int x = (int)Mathf.Floor(normalized_position.x*10f);
                if (x < 0 || x > 9) this.hovered_chars[i] = ' ';
                else this.hovered_chars[i] = top_keys[x];
            }

        }
    }

    void OnDestroy()
    {
        this.imageReceiver.Stop();
        this.landmarksReceiver.Stop();
        this.touchReceiver.Stop();
    }
}
