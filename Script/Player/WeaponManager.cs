using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon primaryWeapon;//������

    [SerializeField]
    private PlayerWeapon secondaryWeapon;//������

    [SerializeField]
    private GameObject weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;
    private AudioSource currentAudioSource;


    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
        //Debug.Log("��װ��������");
    }

    public PlayerWeapon GetCurrentWeapon() 
    {
        //Debug.Log("��õ�ǰ����");
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
            //Destroy()����ʱ����������һ֡
            DestroyImmediate(weaponHolder.transform.GetChild(0).gameObject);
        }

        //��Ⱦ����
        GameObject weaponObject = Instantiate(currentWeapon.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);//ʵ����
        weaponObject.transform.SetParent(weaponHolder.transform);

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();
        currentAudioSource = weaponObject.GetComponent<AudioSource>();

        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f;
        }
    }

    public void ToggleWeapon()//�л�����
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
            ToggleWeapon();//�з�������ǹ������host�Ȱ����ͻ����ְ�����������Ի������ε��²��䣩
        }

        ToggleWeaponClientRpc();//��ÿ���ͻ��˵�ǹ
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
        yield return new WaitForSeconds(playerWeapon.reloadTime);//�ȴ�playerWeapon.reloadTime���ִ�к�����߼�

        playerWeapon.bullets = playerWeapon.maxBullets;
        playerWeapon.isReloading = false;
    }
}
