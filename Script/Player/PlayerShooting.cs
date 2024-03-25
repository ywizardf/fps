using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;

    private float shootCoolDownTime = 0f;//距离上次开枪，过了多久。单位：秒
    private int autoShootCount = 0;//当前一共连开多少枪

    [SerializeField]
    private LayerMask mask;

    private Camera cam;
    private PlayerController playerController;

    enum HitEffectMaterial 
    {
        Metal,
        Stone,
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        shootCoolDownTime += Time.deltaTime;

        if (!IsLocalPlayer) return;

        //以下代码均只能本地客户端调用
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (Input.GetKeyDown(KeyCode.K))
        {
            ShootServerRpc(transform.name, 10);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponManager.Reload(currentWeapon);
            return;
        }

        //GetButtonDown 有没有按下
        //GetButton  有没有按住
        if (currentWeapon.shootRate <= 0 && shootCoolDownTime >= currentWeapon.shootCoolDownTime)//单发
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime = 0f;//重置冷却时间
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate);//几秒后触发，平均每两次之间的时间间隔

            }
            else if (Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q))
            {
                CancelInvoke("Shoot");
            }
        }
    }

    public void StopShooting() 
    {
        CancelInvoke("Shoot");
    }

    //normal 普通，法向量
    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material) //击中点的特效
    {
        GameObject hitEffectPrefab;
        if (material == HitEffectMaterial.Metal)
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().metalHitEffectPrefab;
        }
        else 
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().stoneHitEffectPrefab;
        }
        
        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal));
        //var相当于auto动态判断变量类别
        ParticleSystem particleSystem = hitEffectObject.GetComponent<ParticleSystem>();
        particleSystem.Emit(1);//去掉延迟，直接触发
        particleSystem.Play();
        //删去粒子
        Destroy(hitEffectObject, 1f);
    }

    //先简单介绍一下RPC，它是Remote Procedure Call的简称。意思就是客户端调用服务器的函数，或者服务器调用客户端的函数就如同在本地调用一样
    //ClientRpc 服务器可以执行所有客户端上的ClientRpc。
    //ServerRpc ServerRpc只能由客户端调用，并且始终只在Host或Server上执行

        [ServerRpc]
    private void OnHitServerRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        if (!IsHost)
        {
            OnHit(pos, normal, material);
        }
        OnHitClientRpc(pos, normal, material);
    }

    [ClientRpc]
    private void OnHitClientRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        OnHit(pos, normal, material);
    }

    private void OnShoot(float recoilForce) //每次射击相关的逻辑，包括特效，声音等
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
        weaponManager.GetCurrentAudioSource().Play();

        if (IsLocalPlayer)//施加后座力
        {
            playerController.AddRecoilForce(recoilForce);
        }
    }

    [ClientRpc]
    private void OnShootClientRpc(float recoilForce) //向每一个客户端发送
    {
        OnShoot(recoilForce);
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoilForce)//先在服务器端渲染，再服务器向每个客户端发送信息
    {
        if (!IsHost)
        {
            OnShoot(recoilForce);
        }
        OnShootClientRpc(recoilForce);
    }

    private void Shoot()//射击
    {
        if (currentWeapon.bullets <= 0 || currentWeapon.isReloading == true) return;

        currentWeapon.bullets--;
        //print("Current bullets: " + currentWeapon.bullets);

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload(currentWeapon);
        }

        //Debug.Log("Shoot!!");
        autoShootCount++;
        float recoilForce = currentWeapon.recoilForce;

        if (autoShootCount <= 3)//前三发干扰小
        {
            recoilForce *= 0.2f;
        }
        if (autoShootCount >= 100000)//避免爆int，后几发干扰大
        {
            autoShootCount = 4;
        }

        OnShootServerRpc(recoilForce);

        RaycastHit hit;//射线攻击
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.range, mask))
        {
            //Debug.Log(hit.collider.name);
            //ShootServerRpc(hit.collider.name);
            if (hit.collider.tag == PLAYER_TAG)
            {
                ShootServerRpc(hit.collider.name, currentWeapon.damage);
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Metal);
            }
            else 
            { 
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Stone);
            }
        }
    }

    [ServerRpc]
    private void ShootServerRpc(string name, int damage) 
    {
        //Debug.Log(transform.name + " hit " + hittedName);
        //GameManager.UpdateInfo(transform.name + " hit " + name);
        Player player = GameManager.Singleton.GetPlayer(name);
        player.TakeDamage(damage);
    }
}
