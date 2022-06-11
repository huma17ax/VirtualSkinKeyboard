using System.IO.Pipes;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class NamedPipeServerTest : IDisposable
{
    public enum Status
    {
        Initial,// 初期状態
        Waiting,// 接続待ち
        Connected,// 接続完了
        Disposed// 終了
    }
    public Status status{protected set; get;} = Status.Initial;
    private NamedPipeServerStream pipe = null;
    private BinaryWriter writer = null;
    private BinaryReader reader = null;
    private AutoResetEvent waitingCancel;

    private string pipe_name;

    public NamedPipeServerTest(string pipe_name)
    {
        this.pipe_name = pipe_name;
        this.pipe = new NamedPipeServerStream(this.pipe_name);
        this.waitingCancel = new AutoResetEvent(false);
        Debug.Log("[" + this.pipe_name + "] " + "Initialized");
    }

    ~NamedPipeServerTest()
    {
        Debug.Log("[" + this.pipe_name + "] " + "On destructor");
        this.Dispose();
    }

    public void Dispose()
    {
        if (this.status != Status.Disposed)
        {
            Debug.Log("[" + this.pipe_name + "] " + "Start disposing");
            this.status = Status.Disposed;

            NamedPipeClientStream dummy = new NamedPipeClientStream(this.pipe_name);
            try
            {
                dummy.Connect(100);
            }
            catch
            {
            }
            dummy.Dispose();

            this.waitingCancel.Set();
            if (this.writer != null) writer.Dispose();
            if (this.reader != null) reader.Dispose();
            if (this.pipe != null) pipe.Dispose();
            Debug.Log("[" + this.pipe_name + "] " + "End disposing");
        }
    }

    public async Task WakeUp()
    {
        if (this.status == Status.Initial)
        {
            Debug.Log("[" + this.pipe_name + "] " + "Start waking up");
            this.status = Status.Waiting;
            await Task.Run(() =>
            {
                this.pipe.WaitForConnection();
                if (this.status == Status.Waiting) {
                    Debug.Log("[" + this.pipe_name + "] " + "Pipe connected");
                    this.status = Status.Connected;
                    this.writer = new BinaryWriter(this.pipe);
                    this.reader = new BinaryReader(this.pipe);
                }
                else{
                    Debug.Log("[" + this.pipe_name + "] " + "No longer waiting");
                }
                // IAsyncResult res = this.pipe.BeginWaitForConnection(null, this.pipe);
                // int endIdx = WaitHandle.WaitAny(new[] { this.waitingCancel, res.AsyncWaitHandle });
                // if (endIdx == 0)
                // {
                //     Debug.Log("[" + this.pipe_name + "] " + "Waiting cancelled");
                // }
                // else if (endIdx == 1)
                // {
                //     Debug.Log("[" + this.pipe_name + "] " + "Pipe connected");
                //     this.pipe.EndWaitForConnection(res);
                //     this.status = Status.Connected;
                //     this.writer = new BinaryWriter(this.pipe);
                //     this.reader = new BinaryReader(this.pipe);
                // }
            });
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void Write(byte[] bytes)
    {
        if (this.status == Status.Connected)
        {
            this.writer.Write(bytes, 0, bytes.Length);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public byte[] Read(int size)
    {
        if (this.status == Status.Connected)
        {
            return this.reader.ReadBytes(size);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

}
