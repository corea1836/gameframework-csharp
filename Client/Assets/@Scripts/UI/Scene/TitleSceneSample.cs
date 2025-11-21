using System.Collections;
using System.Net;
using Google.Protobuf.Protocol;
using UnityEngine;

public class TitleSceneSample : UI_Base
{
    enum GameObjects
    {
        StartButton,
    }

    enum Texts
    {
        StatusText,
    }

    enum TitleSceneState
    {
        None,
        AssetLoading,
        AssetLoaded,
        ConnectingToServer,
        ConnectedServer,
        FailedToConnectToServer,
    }

    private TitleSceneState _state = TitleSceneState.None;

    TitleSceneState State
    {
        get { return _state; }
        set
        {
            _state = value;
            switch (value)
            {
                case TitleSceneState.None:
                    break;
                case TitleSceneState.AssetLoading:
                    GetText((int)Texts.StatusText).text = "TODO 에셋 로딩 중";
                    break;
                case TitleSceneState.AssetLoaded:
                    GetText((int)Texts.StatusText).text = "TODO 에셋 로딩 완료";
                    break;
                case TitleSceneState.ConnectingToServer:
                    GetText((int)Texts.StatusText).text = "TODO 서버 연결 중";
                    break;
                case TitleSceneState.ConnectedServer:
                    GetText((int)Texts.StatusText).text = "TODO 서버 연결 완료";
                    break;
                case TitleSceneState.FailedToConnectToServer:
                    GetText((int)Texts.StatusText).text = "TODO 서버 연결 실패";
                    break;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        
        GetObject((int)GameObjects.StartButton).BindEvent((evt) =>
        {
            Debug.Log("OnClick");
            Managers.Scene.LoadScene(Define.EScene.GameScene);
        });
        
        GetObject((int)GameObjects.StartButton).gameObject.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();

        State = TitleSceneState.AssetLoading;
        Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
        {
            GetText((int)Texts.StatusText).text = $"TODO 로딩중 : {key} {count} / {totalCount}";

            if (count == totalCount)
            {
                OnAssetLoaded();
            }
        });
    }

    void OnAssetLoaded()
    {
        State = TitleSceneState.AssetLoaded;
        
        Debug.Log("Connecting To Server");
        State = TitleSceneState.ConnectingToServer;
        
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);
        Managers.Network.GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);
    }

    void OnConnectionSuccess()
    {
        Debug.Log("Connected To Server");
        State = TitleSceneState.ConnectedServer;
        
        GetObject((int)GameObjects.StartButton).gameObject.SetActive(true);

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
            yield return new WaitForSeconds(1);
            
            C_Test testPkt = new C_Test();
            testPkt.Temp = 1;
            Managers.Network.Send(testPkt);
        }
    }
}
