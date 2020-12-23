using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public Transform[] cannons;
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public float despawnTime = 5f;
    public float damage;
    public float fireRate;

    private float _nextShotTime;

    public void FireProjectile() {
        if (Time.time >= _nextShotTime)
        {
            _nextShotTime = Time.time + 1f / fireRate;
            for (int i = 0; i < cannons.Length; i++)
            {
                Transform c = cannons[i];
                GameObject proj = Instantiate(projectilePrefab, c.transform.position, Quaternion.LookRotation(c.forward));
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (!rb) continue;
                rb.velocity = c.forward * projectileSpeed;
                Destroy(proj, despawnTime);
            }
        }
    }
}
