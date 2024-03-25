using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Singleton;//有了

    private Player player = null;//有了

    [SerializeField]
    private TextMeshProUGUI bulletsText;
    [SerializeField]
    private GameObject bulletsObject;
    private WeaponManager weaponManager;//有了

    [SerializeField]
    private Transform healthBarFill;
    [SerializeField]
    private GameObject healthBarObject;


    private void Awake()
    {
        Singleton = this;
    }

    public void setPlayer(Player localplayer)
    {
        player = localplayer;
        weaponManager = player.GetComponent<WeaponManager>();
        bulletsObject.SetActive(true);
        healthBarObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        PlayerWeapon currentWeapon = weaponManager.GetCurrentWeapon();
        if (currentWeapon.isReloading)
        {
            bulletsText.text = "Reloading...";
        }
        else
        {
            bulletsText.text = "Bullets: " + currentWeapon.bullets + "/" + currentWeapon.maxBullets;
        }

        healthBarFill.localScale = new Vector3(player.GetHealth()/100f,1f,1f);
    }
}
