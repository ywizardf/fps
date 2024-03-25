using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerWeapon 
{
    public string name = "M16A1";
    public int damage = 3;
    public float range = 100f;

    public float shootRate = 10f;//一秒钟打多少发子弹，如果小于等于0，则为0发。
    public float shootCoolDownTime = 0.75f;//单发模式的冷却时间
    public float recoilForce = 2f;//后坐力，这里手枪无后坐力

    public GameObject graphics;

    public int maxBullets = 30;//最多子弹数量
    public int bullets = 30;//当前子弹数量
    public float reloadTime = 2f;//换弹时间

    //不在编辑器中展示
    [HideInInspector]
    public bool isReloading = false;
}
