using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon primaryWeapon;//主武器

    [SerializeField]
    private PlayerWeapon secondaryWeapon;//副武器

    [SerializeField]
    private GameObject weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;
    private AudioSource currentAudioSource;


    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
        //Debug.Log("已装备主武器");
    }

    public PlayerWeapon GetCurrentWeapon() 
    {
        //Debug.Log("获得当前武器");
        //EquipWeapon(primaryWeapon);
        if (currentWeapon == null)
        {
            EquipWeapon(primaryWeapon);
        }
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics() 
    {
        return currentGraphics;
    }

    public AudioSource GetCurrentAudioSource() 
    {
        return currentAudioSource;
    }

    public void EquipWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;

        while (weaponHolder.transform.childCount > 0)
        {
            //Destroy()有延时，可能在下一帧
            DestroyImmediate(weaponHolder.transform.GetChild(0).gameObject);
        }

        //渲染武器
        GameObject weaponObject = Instantiate(currentWeapon.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);//实例化
        weaponObject.transform.SetParent(weaponHolder.transform);

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();
        currentAudioSource = weaponObject.GetComponent<AudioSource>();

        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f;
        }
    }

    public void ToggleWeapon()//切换武器
    {
        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        }
        else 
        {
            EquipWeapon(primaryWeapon);
        }
    }

    [ClientRpc]
    private void ToggleWeaponClientRpc() 
    {
        ToggleWeapon();
    }

    [ServerRpc]
    private void ToggleWeaponServerRpc()
    {
        if (!IsHost)
        {
            ToggleWeapon();//切服务器的枪（由于host既包含客户端又包含服务端所以会切两次导致不变）
        }

        ToggleWeaponClientRpc();//切每个客户端的枪
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeaponServerRpc();
            }
        }
    }

    public void Reload(PlayerWeapon playerWeapon)
    {
        if (playerWeapon.isReloading) return;
        playerWeapon.isReloading = true;

        //print("Reload...");

        StartCoroutine(ReloadCoroutine(playerWeapon));
    }
    private IEnumerator ReloadCoroutine(PlayerWeapon playerWeapon)
    {
        yield return new WaitForSeconds(playerWeapon.reloadTime);//等待playerWeapon.reloadTime秒后执行后面的逻辑

        playerWeapon.bullets = playerWeapon.maxBullets;
        playerWeapon.isReloading = false;
    }
}
