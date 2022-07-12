using UnityEngine;
using System;

public class ImageReceiver: ThreadRunner
{
    private SharedData<Color32[]> sh_colors;

    public ImageReceiver(SharedData<Color32[]> sh_colors)
    {
        this.sh_colors = sh_colors;
    }

    protected override void Run()
    {
        using (NamedPipeServer pipe = new NamedPipeServer("ImagePipe"))
        {
            var _ = pipe.WakeUp();
            while (true)
            {
                try
                {
                    if (this.token.IsCancellationRequested) break;
                    if (pipe.status == NamedPipeServer.Status.Connected)
                    {
                        byte[] bytes = pipe.Read(640 * 480 * 4);
                        if (bytes == null) break;
                        this.sh_colors.Set(BytesToColors(bytes));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
                    break;
                }
            }
            Debug.Log("Loop end");
        }
        Debug.Log("Thread end");
    }

    private Color32[] BytesToColors(byte[] bytes)
    {
        Color32[] colors = new Color32[bytes.Length / 4];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].r = bytes[4 * i + 0];
            colors[i].g = bytes[4 * i + 1];
            colors[i].b = bytes[4 * i + 2];
            colors[i].a = bytes[4 * i + 3];
        }

        return colors;
    }
}
