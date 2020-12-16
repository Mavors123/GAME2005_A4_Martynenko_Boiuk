using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public Transform bulletSpawn;
    public GameObject bullet;
    public int fireRate;


    public BulletManager bulletManager;

    void start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _Fire();
    }

    private void _Fire()
    {
        if (Input.GetAxisRaw("Fire1") > 0.0f)
        {
            // delays firing
            if (Time.frameCount % fireRate == 0)
            {
                // var tempBullet = Instantiate(bullet, bulletSpawn.position, Quaternion.identity);
                var tempBullet = bulletManager.Get();
                if (tempBullet) {
                    tempBullet.transform.position = bulletSpawn.position;
                    // tempBullet.GetComponent<BulletBehaviour>().direction = bulletSpawn.forward;
                    tempBullet.GetComponent<SphereBehaviour>().velocity = bulletSpawn.forward * bulletManager.bulletSpeed;
                    tempBullet.transform.SetParent(bulletManager.gameObject.transform);
                }
            }

        }
    }
}
