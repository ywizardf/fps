using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class playerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    private Camera scenceCamera;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Remote Player"));
            DisableComponets();
        }
        else 
        {
            PlayerUI.Singleton.setPlayer(GetComponent<Player>());
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Player"));
            scenceCamera = Camera.main;
            if (scenceCamera != null)
            {
                scenceCamera.gameObject.SetActive(false);
            }
        }

        //SetPlayerName();
        string name = "Player " + GetComponent<NetworkObject>().NetworkObjectId.ToString();
        Player player = GetComponent<Player>();
        player.Setup();
        GameManager.Singleton.RegisterPlayer(name, player);
    }

    //private void SetPlayerName() 
    //{
    //    transform.name = "Player " + GetComponent<NetworkObject>().NetworkObjectId;
    //}

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask)
    {
        transform.gameObject.layer = layerMask;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerMaskForAllChildren(transform.GetChild(i), layerMask);
        }
    }

    private void DisableComponets() 
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (scenceCamera != null)
        {
            scenceCamera.gameObject.SetActive(true);
        }

        GameManager.Singleton.UnRegisterPlayer(transform.name);

    }
}
