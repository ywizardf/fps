using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private Behaviour[] componentsToDisable;
    private bool[] componentsEnabled;
    private bool colliderEnabled;

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>();

    public void Setup() 
    {
        componentsEnabled = new bool[componentsToDisable.Length];
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsEnabled[i] = componentsToDisable[i].enabled;
        }
        Collider collider = GetComponent<Collider>();
        colliderEnabled = collider.enabled;

        SetDefaults();
    }

    private IEnumerator Respawn() //����
    {
        yield return new WaitForSeconds(GameManager.Singleton.MatchingSetting.respawnTime); //(������sleep)
        SetDefaults();

        GetComponentInChildren<Animator>().SetBool("isDead", false);
        GetComponent<Rigidbody>().useGravity = true;

        if (IsLocalPlayer)//�ǲ��ǿͻ���
        {
            transform.position = new Vector3(0f, 20f, 0f);
        }
    }

    private void SetDefaults() 
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = componentsEnabled[i];
        }
        Collider collider = GetComponent<Collider>();
        collider.enabled = colliderEnabled;


        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }
    }

    public void TakeDamage(int damage) //�ܵ����˺���ֻ�ڷ������˱�����
    {
        if (isDead.Value) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            isDead.Value = true;
            if (!IsHost)
            {
                DieOnServer();//�������host���ڷ�������ִ��
            }
            DieClientRpc();//�����ͻ���ִ��
        }
    } 

    public int GetHealth() 
    {
        return currentHealth.Value;
    }

    private void DieOnServer() 
    {
        Die();
    }

    [ClientRpc]
    private void DieClientRpc() 
    {
        Die();
    }

    public bool IsDead() 
    {
        return isDead.Value;
    }

    private void Die() 
    {
        GetComponent<PlayerShooting>().StopShooting();

        GetComponentInChildren<Animator>().SetBool("isDead", true);
        GetComponent<Rigidbody>().useGravity = false;

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        StartCoroutine(Respawn());//��һ���µ��߳�ִ��
    }
}
