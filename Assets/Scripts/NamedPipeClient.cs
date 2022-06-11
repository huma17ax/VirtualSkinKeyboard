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

public class NamedPipeClient : MonoBehaviour
{
    private CancellationTokenSource tokenSource;
    private CancellationToken token;
    private AutoResetEvent cancel;

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
        this.cancel = new AutoResetEvent(false);

        this.queue = new ConcurrentQueue<Color32[]>();

        // this.data = new byte[921600];
        // Array.Clear(this.data, 0, 921600);
        this.syncObject = new object();

        this.webCamTexture = new WebCamTexture("Left camera", INPUT_WIDTH, INPUT_HEIGHT, FPS);
        // this.rawImage.texture = this.webCamTexture;
        Debug.Log(this.webCamTexture);
        this.webCamTexture.Play();

        this.rawImage = GetComponent<RawImage>();
        // this.texture = new Texture2D(640, 480);
        // this.texture.SetPixels32(colors);
        // this.rawImage.texture = this.texture;
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
        // this.texture.SetPixels32(this.webCamTexture.GetPixels32());
        // this.rawImage.texture = this.texture;
        // if (Monitor.TryEnter(this.syncObject))
        // {
        //     var sw = new System.Diagnostics.Stopwatch();
        //     sw.Restart();
        //     this.texture.LoadImage(this.data);
        //     sw.Stop();
        //     Debug.Log($"{sw.Elapsed}");
        //     Monitor.Exit(this.syncObject);
        // }
        // width = this.texture.width;
        // height = this.texture.height;
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
        using (NamedPipeServerStream pipe = new NamedPipeServerStream("DemoPipe"))
        {
            Debug.Log("create pipe and reader");

            IAsyncResult result = pipe.BeginWaitForConnection(null, pipe);
            int waitResult = WaitHandle.WaitAny(new[] { this.cancel, result.AsyncWaitHandle });

            switch (waitResult)
            {
                case 0:
                    Debug.Log("cancelled");
                    break;
                case 1:
                    Debug.Log("connected");
                    pipe.EndWaitForConnection(result);
                    this.wakedup = true;
                    WritePipe(pipe);
                    Debug.Log("end connect");
                    break;
            }
            Debug.Log("Close");
        }
        Debug.Log("shut down");
    }

    private void WritePipe(NamedPipeServerStream pipe)
    {
        using (BinaryWriter writer = new BinaryWriter(pipe))
        {
            // Int32 size = reader.ReadInt32();
            // byte[] bytes = reader.ReadBytes(size);
            // if (bytes == null) break;
            // if (this.token.IsCancellationRequested) break;

            // Debug.Log(string.Format("{0:mm.ss.fff}", DateTime.Now));

            // if (bytes.Length > 0)
            // {
            //     try
            //     {
            //         Monitor.Enter(this.syncObject);
            //         this.data = bytes;
            //     }
            //     finally
            //     {
            //         Monitor.Exit(this.syncObject);
            //     }
            // }
            Color32[] data;
            while (true)
            {
                if (this.queue.TryDequeue(out data))
                {
                    byte[] bytes = ColorsToBytes(data);
                    writer.Write(bytes, 0, bytes.Length);
                }
                if (this.token.IsCancellationRequested) break;
            }

            Debug.Log("END.");
        }
        Debug.Log("fin");
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
        this.cancel.Set();
        this.tokenSource.Cancel();
    }
}
