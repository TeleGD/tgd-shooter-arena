﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerID;
    private Rigidbody rb;
    private Animator anim;
    private float speed = 5;
    private Vector3 externalForce;
    public float airDrag = 1.5f;

    public GameObject bulletPrefab;
    private float nextFireTime;

    public Transform weaponHolder;
    public WeaponData[] weaponsData;
    private int currentWeapon = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        SetWeapon(-1);
        externalForce = new Vector3(0,0,0);
    }
    
    void FixedUpdate()
    {
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal" + playerID) * speed,rb.velocity.y, Input.GetAxis("Vertical" + playerID) * speed);
        
        rb.velocity = dir;
        float actualSpeed = dir.magnitude;
        if(actualSpeed > speed * 0.25f)
        {
            Quaternion targetRot = Quaternion.Euler(0, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * 1000);
        }

        anim.SetFloat("speed", actualSpeed);
        rb.velocity += externalForce;
        updateForce();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Fire" + playerID) && Time.time > nextFireTime)
        {
            if(currentWeapon != -1)
            {
                nextFireTime = Time.time + GetWeapon().reloadTime;

                if (currentWeapon == 0)
                    FireBullet(0);
                else
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        FireBullet(i * 5);
                    }
                }
            }

        }
    }

    private WeaponData GetWeapon()
    {
        return weaponsData[currentWeapon];
    }

    public void SetWeapon(int id)
    {
        currentWeapon = id;

        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            weaponHolder.GetChild(i).gameObject.SetActive(currentWeapon == i);
        }
    }

    private void FireBullet(float angleOffset)
    {
        Vector3 pos = GetWeapon().muzzle.position;
        GameObject go = Instantiate(bulletPrefab, pos, Quaternion.Euler(0, transform.eulerAngles.y + angleOffset, 0));
        go.GetComponent<BulletMove>().SetOwner(gameObject);
        go.GetComponent<BulletMove>().speed = GetWeapon().bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && other.GetComponent<BulletMove>().owner != gameObject)
        {
            GameManager.instance.EndRound();
            gameObject.SetActive(false);
        }
    }

    [System.Serializable]
    public struct WeaponData
    {
        public Transform muzzle;
        public float reloadTime;
        public float bulletSpeed;
    }

    public void setExternalForce(Vector3 v)
    {
        externalForce = new Vector3(v.x, 0, v.z);
        rb.velocity = new Vector3(rb.velocity.x, v.y, rb.velocity.z);
    }

    private void updateForce()
    {

        print(externalForce);
        externalForce = Vector3.Lerp(externalForce,Vector3.zero, Time.deltaTime*airDrag);
    }
}

