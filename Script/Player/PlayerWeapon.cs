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

    public float shootRate = 10f;//һ���Ӵ���ٷ��ӵ������С�ڵ���0����Ϊ0����
    public float shootCoolDownTime = 0.75f;//����ģʽ����ȴʱ��
    public float recoilForce = 2f;//��������������ǹ�޺�����

    public GameObject graphics;

    public int maxBullets = 30;//����ӵ�����
    public int bullets = 30;//��ǰ�ӵ�����
    public float reloadTime = 2f;//����ʱ��

    //���ڱ༭����չʾ
    [HideInInspector]
    public bool isReloading = false;
}
