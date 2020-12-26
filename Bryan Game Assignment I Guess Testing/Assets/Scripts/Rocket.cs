using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public Transform target;
    public float damage;
    public float projectileSpeed;
    public float turnSpeed;
    private Rigidbody _rb;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!_rb || !target) return;
        _rb.velocity = transform.forward * projectileSpeed;
        Quaternion rot = Quaternion.LookRotation(target.position - transform.position);
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") {
            AudioManager.Instance.SpawnParticle(0, transform.position);
            Destroy(gameObject);
        }
    }
}
