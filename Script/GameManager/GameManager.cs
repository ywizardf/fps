using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    [SerializeField]
    public MatchingSetting MatchingSetting;

    private static Dictionary<string,Player>players = new Dictionary<string, Player>();

    private void Awake()
    {
        Singleton = this;
    }

    public void RegisterPlayer(string name, Player player)
    {
        player.transform.name = name;
        players.Add(name, player);
    }

    public void UnRegisterPlayer(string name)
    {
        players.Remove(name);
    }

    public Player GetPlayer(string name)
    {
        return players[name];
    }
    //private static string info;

    //public static void UpdateInfo(string _info) 
    //{
    //    info = _info;
    //}

    //每帧会调用一次
    //private void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(200f, 200f, 200f, 400f));
    //    GUILayout.BeginVertical();//竖排展示

    //    GUI.color = Color.red;
    //    //GUILayout.Label(info);//画画
    //    foreach (string name in players.Keys)
    //    {
    //        Player player = GetPlayer(name);
    //        GUILayout.Label(name + " - "+ player.GetHealth());
    //    }

    //    GUILayout.EndVertical();//闭合
    //    GUILayout.EndArea();
    //}
}
