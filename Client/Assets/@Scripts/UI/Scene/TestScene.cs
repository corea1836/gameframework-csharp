
using System.Collections;
using System.Net;
using Google.Protobuf.Protocol;
using UnityEngine;

public class TestScene: MonoBehaviour
{
    void Start()
    {
        Debug.Log("Connecting To Server");
        
        IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
        Managers.Network.GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);
    }

    void OnConnectionSuccess()
    {
        Debug.Log("Connected To Server");

        StartCoroutine(CoSendTestPacket());
    }

    void OnConnectionFailed()
    {
        Debug.Log("Failed To Connect To Server");
    }

    IEnumerator CoSendTestPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            C_Test pkt = new C_Test()
            {
                Temp = 1
            };
            Managers.Network.Send(pkt);
        }
    }
}
