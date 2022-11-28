using System;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable Unity.PreferNonAllocApi

public class Bullet : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject explosion;
    [SerializeField] private LayerMask whatIsEnemies;

    [Header("Stats")] 
    [SerializeField] [Range(0f, 1f)] private float bounciness;
    [SerializeField] private bool useGrav;
    
    [Header("Damage")]
    [SerializeField] private int explosionDamage;
    [SerializeField] private float explosionRange;
    
    [Header("Lifetime")]
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private int maxCollisions;
    [SerializeField] private float maxLifetime;
    [SerializeField] private bool explodeOnTouch;

    private int collisions;
    private PhysicMaterial mat;
    private bool _isExplosionNotNull;

    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Start()
    {
        _isExplosionNotNull = explosion != null;
        mat = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };
        GetComponent<MeshCollider>().material = mat;

        rb.useGravity = useGrav;
    }

    private void Update()
    {
        if (collisions >= maxCollisions) Explode();

        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();
    }

    private void Explode()
    {
        if (_isExplosionNotNull) Instantiate(explosion, transform.position, quaternion.identity);

        //var enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        //for (var i = 0; i < enemies.Length; i++)
        //{
            //Damage enemy
        //}
        
        Invoke(nameof(Delay), 0.05f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        collisions++;
        
        //if (collision.collider.CompareTag($"Enemy") && explodeOnTouch) Explode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
