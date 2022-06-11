using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

public class NamedPipe : MonoBehaviour
{
    private CancellationTokenSource tokenSource;
    private CancellationToken token;

    private byte[] data;// shared
    private object syncObject;
    private Texture2D texture;

    private RawImage rawImage;

    // public int width;
    // public int height;
    private static int INPUT_WIDTH = 640;
    private static int INPUT_HEIGHT = 480;
    private static int FPS = 30;
    WebCamTexture webCamTexture;

    public static int count = 0;

    private ConcurrentQueue<Color32[]> queue;

    private bool wakedup = false;

    void Start()
    {
        this.tokenSource = new CancellationTokenSource();
        this.token = this.tokenSource.Token;

        this.queue = new ConcurrentQueue<Color32[]>();

        this.syncObject = new object();

        this.webCamTexture = new WebCamTexture("Left camera", INPUT_WIDTH, INPUT_HEIGHT, FPS);
        Debug.Log(this.webCamTexture);
        this.webCamTexture.Play();

        this.rawImage = GetComponent<RawImage>();
        this.rawImage.texture = this.webCamTexture;

        this.data = null;

        AsyncWrapper();
        Debug.Log("START");

    }

    void Update()
    {
        if (wakedup)
        {
            Color32[] colors = this.webCamTexture.GetPixels32();
            this.queue.Enqueue(colors);
        }
    }

    private void AsyncWrapper()
    {
        Thread thread = new Thread(WakeUpPipe);
        thread.IsBackground = true;
        thread.Start();
        Debug.Log("started thread");
    }

    private void WakeUpPipe()
    {
        using (NamedPipeServerTest pipe = new NamedPipeServerTest("ImagePipe"))
        {
            Debug.Log("create pipe");

            var _ = pipe.WakeUp();

            Color32[] data;
            while (true)
            {
                try
                {
                    if (this.token.IsCancellationRequested) break;
                    if (pipe.status == NamedPipeServerTest.Status.Connected)
                    {
                        this.wakedup = true;
                        if (this.queue.TryDequeue(out data))
                        {
                            byte[] bytes = ColorsToBytes(data);
                            pipe.Write(bytes);
                        }
                    }
                }
                catch
                {
                    break;
                }
            }
            Debug.Log("Close");
        }
        Debug.Log("shut down");
    }

    private byte[] ColorsToBytes(Color32[] colors)
    {
        int size = Marshal.SizeOf(typeof(Color32));

        int length = size * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle)) handle.Free();
        }

        return bytes;
    }

    void OnDestroy()
    {
        this.tokenSource.Cancel();
    }
}
