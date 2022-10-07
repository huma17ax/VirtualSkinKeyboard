using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    private ImageReceiver imageReceiver;
    private SharedData<Color32[]> sh_background;
    private SharedData<Color32[]> sh_foreground;

    private LandmarksReceiver landmarksReceiver;
    private SharedData<Vector2[]> sh_landmarks;

    private TouchReceiver touchReceiver;
    private SharedData<bool[]> sh_touches;

    private RawImage rawImage;
    private Texture2D background;
    private Texture2D foreground;
    private Color32[] colors;

    private GameObject[] circles;
    private IExperimentUI UI;
    private ARMarkerDetector detector;

    private bool[] pre = { false, false, false, false };

    private char[] hovered_chars = { ' ', ' ', ' ', ' ' };

    void Start()
    {
        this.rawImage = GetComponent<RawImage>();
        this.background = new Texture2D(640, 480);
        this.rawImage.texture = this.background;
        this.foreground = new Texture2D(640, 480);
        GameObject.Find("Canvas/Foreground").GetComponent<RawImage>().texture = this.foreground;
        this.colors = new Color32[0];

        this.sh_background = new SharedData<Color32[]>();
        this.sh_foreground = new SharedData<Color32[]>();
        this.imageReceiver = new ImageReceiver(this.sh_background, this.sh_foreground);
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
        this.UI = GameObject.Find("Canvas/Keyboard").GetComponent<KeyboardUI>();

        this.detector = GameObject.Find("ARMarkerDetecter").GetComponent<ARMarkerDetector>();
        this.detector.WakeUp(this.background);
    }

    void Update()
    {
        if (this.sh_background.TryGet(out this.colors))
        {
            this.background.SetPixels32(this.colors);
            this.background.Apply();
            this.detector.TextureUpdated();
        }
        if (this.sh_foreground.TryGet(out this.colors))
        {
            this.foreground.SetPixels32(MultiplyTransparency(0.7f, this.colors));
            this.foreground.Apply();
        }

        Vector2[] v;
        if (this.sh_landmarks.TryGet(out v))
        {
            float width = 640 * this.rawImage.GetComponent<RectTransform>().localScale.x;
            float height = 480 * this.rawImage.GetComponent<RectTransform>().localScale.y;
            this.circles[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[0].x * width - width / 2, -(v[0].y * height - height / 2), 0);
            this.circles[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[1].x * width - width / 2, -(v[1].y * height - height / 2), 0);
            this.circles[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[2].x * width - width / 2, -(v[2].y * height - height / 2), 0);
            this.circles[3].GetComponent<RectTransform>().anchoredPosition = new Vector3(v[3].x * width - width / 2, -(v[3].y * height - height / 2), 0);
        }

        this.UI.CalcHoverKey(new Vector2[] {
            this.circles[0].GetComponent<RectTransform>().anchoredPosition,
            this.circles[1].GetComponent<RectTransform>().anchoredPosition,
            this.circles[2].GetComponent<RectTransform>().anchoredPosition,
            this.circles[3].GetComponent<RectTransform>().anchoredPosition
        });

        bool[] b;
        if (this.sh_touches.TryGet(out b))
        {
            this.circles[0].GetComponent<RectTransform>().localScale = new Vector3(b[0] ? 2 : 1, b[0] ? 2 : 1, 1);
            this.circles[1].GetComponent<RectTransform>().localScale = new Vector3(b[1] ? 2 : 1, b[1] ? 2 : 1, 1);
            this.circles[2].GetComponent<RectTransform>().localScale = new Vector3(b[2] ? 2 : 1, b[2] ? 2 : 1, 1);
            this.circles[3].GetComponent<RectTransform>().localScale = new Vector3(b[3] ? 2 : 1, b[3] ? 2 : 1, 1);

            for (int i = 0; i < 4; i++)
            {
                if (pre[i] == false && b[i] == true) this.UI.Click(i);
            }
            pre = b;
        }

    }

    private Color32[] MultiplyTransparency(float rate, Color32[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = (byte)(colors[i].a * rate);
        }
        return colors;
    }

    void OnDestroy()
    {
        this.imageReceiver.Stop();
        this.landmarksReceiver.Stop();
        this.touchReceiver.Stop();

        Logger.Output();
    }
}
