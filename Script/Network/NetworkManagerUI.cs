using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    //[SerializeField]
    //private Button refreshButton;

    //[SerializeField]
    //private Button buildButton;

    //[SerializeField]
    //private Canvas menuUI;

    //[SerializeField]
    //private GameObject roomButtonPrefab;

    //[SerializeField] 
    //UnityTransport unityTransport;

    //private int buildRoomPort = -1;//是否是房主

    //private List<Button> rooms = new List<Button>();

    //// Start is called before the first frame update
    //void Start()
    //{
    //    setConfig();
    //    initButtons();
    //    RefreshRoomList();
    //}

    ////点×前的操作
    //private void OnApplicationQuit()
    //{
    //    if (buildRoomPort != -1)//是房主，则退出时自动移除房间
    //    {
    //        RemoveRoom();
    //    }
    //}
    //private void setConfig() 
    //{
    //    var args = System.Environment.GetCommandLineArgs(); // 获取命令行参数

    //    for (int i = 0; i < args.Length; i++) // 先更改端口号，再启动server
    //    {
    //        if (args[i] == "-port")
    //        {
    //            ushort port = ushort.Parse(args[i + 1]);
    //            unityTransport.ConnectionData.Address = "47.97.41.184";
    //            unityTransport.ConnectionData.Port = port;
    //            unityTransport.ConnectionData.ServerListenAddress = "0.0.0.0";
    //        }
    //    }

    //    for (int i = 0; i < args.Length; i++)
    //    {
    //        if (args[i] == "-launch-as-server")
    //        {
    //            NetworkManager.Singleton.StartServer();
    //            DestroyAllButtons();
    //        }
    //    }
    //}

    //private void initButtons() 
    //{
    //    refreshButton.onClick.AddListener(()=>
    //    {
    //        RefreshRoomList();
    //    });
    //    buildButton.onClick.AddListener(() =>
    //    {
    //        BuildRoom();
    //    });
    //}

    //private void RefreshRoomList()
    //{
    //    StartCoroutine(RefreshRoomListRequest("http://47.97.41.184:8080/fps/get_room_list"));
    //}

    //IEnumerator RefreshRoomListRequest(string uri)
    //{
    //    UnityWebRequest uwr = UnityWebRequest.Get(uri);
    //    yield return uwr.SendWebRequest();

    //    if (uwr.result != UnityWebRequest.Result.ConnectionError)
    //    {
    //        var resp = JsonUtility.FromJson<GetRoomListResponse>(uwr.downloadHandler.text);
    //        foreach (var room in rooms)
    //        {
    //            room.onClick.RemoveAllListeners();
    //            Destroy(room.gameObject);
    //        }

    //        rooms.Clear();

    //        print(resp.error_message);

    //        int k = 0;
    //        foreach(var room in resp.rooms)
    //        {
    //            GameObject buttonObj = Instantiate(roomButtonPrefab, menuUI.transform);
    //            buttonObj.transform.localPosition = new Vector3(-21, 92 - k * 60, 0);
    //            k++;
    //            Button button = buttonObj.GetComponent<Button>();
    //            button.GetComponentInChildren<TextMeshProUGUI>().text = room.name;
    //            button.onClick.AddListener(() =>
    //            {
    //                unityTransport.ConnectionData.Port = (ushort)room.port;
    //                NetworkManager.Singleton.StartClient();
    //                //Debug.Log("创建客户端");
    //                DestroyAllButtons();
    //            });
    //            rooms.Add(button);
    //        }
    //    }
    //}

    //private void BuildRoom()
    //{
    //    StartCoroutine(BuildRoomRequest("http://47.97.41.184:8080/fps/build_room"));
    //}

    //IEnumerator BuildRoomRequest(string uri)
    //{
    //    UnityWebRequest uwr = UnityWebRequest.Get(uri);
    //    yield return uwr.SendWebRequest();

    //    if (uwr.result != UnityWebRequest.Result.ConnectionError)
    //    {
    //        var resp = JsonUtility.FromJson<BuildRoomResponse>(uwr.downloadHandler.text);
    //        //print(resp.error_message);
    //        if (resp.error_message == "success")
    //        {
    //            //print("成功！！！");
    //            unityTransport.ConnectionData.Port = (ushort)resp.port;
    //            NetworkManager.Singleton.StartClient();
    //            DestroyAllButtons();

    //            buildRoomPort = (ushort)resp.port;//为房主附上房间号
    //        }

    //    }
    //}

    //private void RemoveRoom()
    //{
    //    StartCoroutine(RemoveRoomRequest("http://47.97.41.184:8080/fps/remove_room/?port=" + buildRoomPort));
    //}

    //IEnumerator RemoveRoomRequest(string uri)
    //{
    //    UnityWebRequest uwr = UnityWebRequest.Get(uri);
    //    yield return uwr.SendWebRequest();

    //    if (uwr.result != UnityWebRequest.Result.ConnectionError)
    //    {
    //        var resp = JsonUtility.FromJson<BuildRoomResponse>(uwr.downloadHandler.text);
    //        if (resp.error_message == "success")
    //        {
    //            //暂时为空
    //        }

    //    }
    //}


    //private void DestroyAllButtons() 
    //{
    //    refreshButton.onClick.RemoveAllListeners();
    //    buildButton.onClick.RemoveAllListeners();

    //    Destroy(refreshButton.gameObject);
    //    Destroy(buildButton.gameObject);

    //    foreach (var room in rooms)
    //    {
    //        room.onClick.RemoveAllListeners();
    //        Destroy(room.gameObject);
    //    }
    //}
    [SerializeField]
    private Button hostBtn; //拖动赋值
    [SerializeField]
    private Button serverBtn; //拖动赋值
    [SerializeField]
    private Button clientBtn; //拖动赋值

    [SerializeField]
    private NetworkManager networkManager;

    // Start is called before the first frame update
    void Start()
    {
        hostBtn.onClick.AddListener(() =>
        {
            networkManager.StartHost();
            DestroyAllButtons();
        });
        serverBtn.onClick.AddListener(() =>
        {
            networkManager.StartServer();
            DestroyAllButtons();
        });
        clientBtn.onClick.AddListener(() =>
        {
            networkManager.StartClient();
            DestroyAllButtons();
        });
    }

    private void DestroyAllButtons()
    {
        serverBtn.onClick.RemoveAllListeners();
        clientBtn.onClick.RemoveAllListeners();
        hostBtn.onClick.RemoveAllListeners();

        Destroy(serverBtn.gameObject);
        Destroy(clientBtn.gameObject);
        Destroy(hostBtn.gameObject);
    }
}
