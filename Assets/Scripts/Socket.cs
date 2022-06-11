using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Socket
{
    
    public static void WakeUpServer()
    {
        byte[] bytes = new byte[1024];
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 10101);

        Socket listener = new Socket();

    }

}
