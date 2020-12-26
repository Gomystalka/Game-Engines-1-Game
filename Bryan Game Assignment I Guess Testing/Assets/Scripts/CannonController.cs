using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public Transform[] projectileSources;
    public GameObject projectilePrefab;
    public GameObject rocketPrefab;
    public float projectileSpeed;
    public float rocketSpeed;
    public float rocketTurnSpeed;
    public float despawnTime = 5f;
    public float rocketDespawnTime = 10f;
    public float damage;
    public float fireRate;
    public float rocketRechargeDelay = 2f;

    private float _nextShotTime;

    public bool RocketActive { get; private set; } = true;

    public void FireProjectile() {
        if (Time.time >= _nextShotTime)
        {
            _nextShotTime = Time.time + 1f / fireRate;
            for (int i = 0; i < projectileSources.Length - 1; i++)
            {
                Transform c = projectileSources[i];
                GameObject proj = Instantiate(projectilePrefab, c.transform.position, Quaternion.LookRotation(c.forward));
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (!rb) continue;
                rb.velocity = c.forward * projectileSpeed;
                Destroy(proj, despawnTime);
            }
        }
    }

    public void FireRocket(Transform target) {
        if (!RocketActive) return;
        RocketActive = false;
        GameObject rocket = Instantiate(rocketPrefab, projectileSources[2].position, Quaternion.LookRotation(projectileSources[2].forward));
        Rocket r = rocket.GetComponent<Rocket>();
        r.target = target;
        r.projectileSpeed = rocketSpeed;
        r.turnSpeed = rocketTurnSpeed;
        r.damage = damage * 2f;
        Destroy(rocket, rocketDespawnTime);
        Invoke("ResetRocket", rocketRechargeDelay);
    }

    private void ResetRocket() => RocketActive = true;
}
