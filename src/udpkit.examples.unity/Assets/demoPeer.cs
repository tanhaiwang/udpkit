using UdpKit;
using UnityEngine;

public class demoPeer : MonoBehaviour {
    UdpSocket socket;

    [HideInInspector]
    internal bool isServer;
    
    [HideInInspector]
    internal string serverAddress = "127.0.0.1:14000";

    void Awake () {
        UdpLog.SetWriter(s => Debug.Log(s));
    }

    void Start () {
        socket = UdpKitUnityUtils.CreatePlatformSpecificSocket<demoSerializer>();

        if (isServer) {
            socket.Start(new UdpEndPoint(serverAddress));
        } else {
            socket.Start(UdpEndPoint.Any);
            socket.Connect(new UdpEndPoint(serverAddress));
        }
    }

    void OnDestroy () {
        socket.Close();
    }

    void Update () {
        UdpEvent ev = default(UdpEvent);

        while (socket.Poll(ref ev)) {
            switch (ev.EventType) {
                case UdpEventType.Connected:
                    UdpLog.User("Client connect from {0}", ev.Connection.RemoteEndPoint);
                    break;
            }
        }
    }
}
