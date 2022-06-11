using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WebCamTest : MonoBehaviour
{

    private static int INPUT_WIDTH = 640;
    private static int INPUT_HEIGHT = 480;
    private static int FPS = 30;

    RawImage rawImage;
    WebCamTexture webCamTexture;

    string[] webCamNames;

    float time = 0;

    int count = 0;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        this.webCamTexture = new WebCamTexture("Left camera", INPUT_WIDTH, INPUT_HEIGHT, FPS);
        // this.rawImage.texture = this.webCamTexture;
        this.webCamTexture.Play();

        webCamNames = new string[0];
    }

    void Update()
    {
        Destroy(this.rawImage.texture);
        Texture2D flip = OpenCV.Flipped(this.webCamTexture);
        Texture2D undist = OpenCV.Undistorted(flip);
        Texture2D crop = OpenCV.Cropped(undist);
        this.rawImage.texture = crop;

        time += Time.deltaTime;
        if (time >= 3.0f) {
            time -= 3.0f;
            File.WriteAllBytes(count.ToString() + "_raw.png", flip.EncodeToPNG());

            File.WriteAllBytes(count.ToString() + "_undist.png", undist.EncodeToPNG());
            File.WriteAllBytes(count.ToString() + "_crop.png", crop.EncodeToPNG());
            count += 1;
        }

        Destroy(flip);
        Destroy(undist);
        // Destroy(clop);

        WebCamDevice[] webCamDevices = WebCamTexture.devices;
        string[] newWebCamNames = webCamDevices.Select(dev => dev.name).ToArray();

        string[] acquiredWebCams = newWebCamNames.Except(webCamNames).ToArray();
        string[] lostWebCams = webCamNames.Except(newWebCamNames).ToArray();

        foreach (string name in acquiredWebCams) {
            Debug.Log("Acquire WebCam: " + name);
        }
        foreach (string name in lostWebCams) {
            Debug.Log("Lost WebCam: " + name);
        }

        webCamNames = newWebCamNames;
    }
}
