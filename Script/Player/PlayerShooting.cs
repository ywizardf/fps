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

    private float shootCoolDownTime = 0f;//�����ϴο�ǹ�����˶�á���λ����
    private int autoShootCount = 0;//��ǰһ����������ǹ

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

        //���´����ֻ�ܱ��ؿͻ��˵���
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

        //GetButtonDown ��û�а���
        //GetButton  ��û�а�ס
        if (currentWeapon.shootRate <= 0 && shootCoolDownTime >= currentWeapon.shootCoolDownTime)//����
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime = 0f;//������ȴʱ��
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate);//����󴥷���ƽ��ÿ����֮���ʱ����

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

    //normal ��ͨ��������
    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material) //���е����Ч
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
        //var�൱��auto��̬�жϱ������
        ParticleSystem particleSystem = hitEffectObject.GetComponent<ParticleSystem>();
        particleSystem.Emit(1);//ȥ���ӳ٣�ֱ�Ӵ���
        particleSystem.Play();
        //ɾȥ����
        Destroy(hitEffectObject, 1f);
    }

    //�ȼ򵥽���һ��RPC������Remote Procedure Call�ļ�ơ���˼���ǿͻ��˵��÷������ĺ��������߷��������ÿͻ��˵ĺ�������ͬ�ڱ��ص���һ��
    //ClientRpc ����������ִ�����пͻ����ϵ�ClientRpc��
    //ServerRpc ServerRpcֻ���ɿͻ��˵��ã�����ʼ��ֻ��Host��Server��ִ��

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

    private void OnShoot(float recoilForce) //ÿ�������ص��߼���������Ч��������
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
        weaponManager.GetCurrentAudioSource().Play();

        if (IsLocalPlayer)//ʩ�Ӻ�����
        {
            playerController.AddRecoilForce(recoilForce);
        }
    }

    [ClientRpc]
    private void OnShootClientRpc(float recoilForce) //��ÿһ���ͻ��˷���
    {
        OnShoot(recoilForce);
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoilForce)//���ڷ���������Ⱦ���ٷ�������ÿ���ͻ��˷�����Ϣ
    {
        if (!IsHost)
        {
            OnShoot(recoilForce);
        }
        OnShootClientRpc(recoilForce);
    }

    private void Shoot()//���
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

        if (autoShootCount <= 3)//ǰ��������С
        {
            recoilForce *= 0.2f;
        }
        if (autoShootCount >= 100000)//���ⱬint���󼸷����Ŵ�
        {
            autoShootCount = 4;
        }

        OnShootServerRpc(recoilForce);

        RaycastHit hit;//���߹���
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
