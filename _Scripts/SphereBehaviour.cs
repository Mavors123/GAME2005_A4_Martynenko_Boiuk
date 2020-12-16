using System.Collections.Generic;
using UnityEngine;

public class SphereBehaviour : MonoBehaviour {
    public float Radius;
    public bool isColliding;

    public List<CubeBehaviour> cubes;

    public float range = 20;
    
    public Vector3 velocity;
    public float mass = 1.0f;
    public float friction;
    public float bounciness;

    public CollisionManager CollisionManager;
    public bool isUsed = false;

    public void Start() {
        var mesh = GetComponent<MeshFilter>().mesh;
        var bounds = mesh.bounds;
        Radius = bounds.extents.x;

        CollisionManager = FindObjectOfType<CollisionManager>();
    }

    void Update() {
        _CheckBounds();
    }
    
    void FixedUpdate() {
        if (velocity.magnitude > 0) {
            transform.position += velocity * Time.fixedDeltaTime;
        } 
        // 1. Find a collision manifold
        // 2. Push out an object to remove penetration.
    }
    
    private void _CheckBounds()
    {
        if (Vector3.Distance(transform.position, Vector3.zero) > range) {
            BulletManager.Instance.Reset(this);
        }

        if (velocity.magnitude <= 0.001f) {
            BulletManager.Instance.Reset(this);
        }
    }
}
