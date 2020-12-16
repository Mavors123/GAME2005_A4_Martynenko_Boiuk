using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class CollisionManager : MonoBehaviour {
    public CubeBehaviour[] boxes;
    public SphereBehaviour[] spheres;

    // Start is called before the first frame update
    void Start() {
        boxes = FindObjectsOfType<CubeBehaviour>();
        spheres = FindObjectsOfType<SphereBehaviour>();
    }

    public InputField MassInput;
    public InputField FrictionInput;

    public void ApplyChanges() {
        if (MassInput == null || FrictionInput == null) {
            return;
        }

        var mass = float.Parse(MassInput.text);
        var friction = float.Parse(FrictionInput.text);

        foreach (var box in boxes) {
            box.friction = friction;
            box.mass = mass;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        for (int i = 0; i < boxes.Length; i++) {
            for (int j = 0; j < boxes.Length; j++) {
                if (i != j) {
                    CheckAABBs(boxes[i], boxes[j]);
                }
            }
        }

        for (int i = 0; i < boxes.Length; i++) {
            for (int j = 0; j < spheres.Length; j++) {
                CheckSphereAABB(spheres[j], boxes[i]);
            }
        }
    }

    public static void CheckAABBs(CubeBehaviour a, CubeBehaviour b) {
        if ((a.min.x <= b.max.x && a.max.x >= b.min.x) &&
            (a.min.y <= b.max.y && a.max.y >= b.min.y) &&
            (a.min.z <= b.max.z && a.max.z >= b.min.z)) {
            if (a.contacts.Contains(b)) {
                return;
            }

            a.contacts.Add(b);
            a.isColliding = true;
        } else {
            if (!a.contacts.Contains(b)) {
                return;
            }

            a.contacts.Remove(b);
            a.isColliding = false;
        }
    }

    public static void CheckSphereAABB(SphereBehaviour sphere, CubeBehaviour box) {
        var x = Math.Max(box.min.x, Math.Min(sphere.transform.position.x, box.max.x));
        var y = Math.Max(box.min.y, Math.Min(sphere.transform.position.y, box.max.y));
        var z = Math.Max(box.min.z, Math.Min(sphere.transform.position.z, box.max.z));

        var distance = Math.Sqrt((x - sphere.transform.position.x) * (x - sphere.transform.position.x) +
                                 (y - sphere.transform.position.y) * (y - sphere.transform.position.y) +
                                 (z - sphere.transform.position.z) * (z - sphere.transform.position.z));

        if (distance >= sphere.Radius) return;

        var manifold = getSphereToAABBCollisionManifold(sphere, box);

        sphere.isColliding = true;
        box.isColliding = true;

        var relativeVelocity = box.velocity - sphere.velocity;
        var relVelMagnitude = Vector3.Dot(relativeVelocity, manifold.Normal);
        var bounciness = Mathf.Min(box.bounciness, sphere.bounciness);
        // var scaledVelocity = (-bounciness) * relVelMagnitude;

        // print(String.Format("box mass {0} sphere mass {1}", box.mass, sphere.mass));

        var impulseMagnitude = -(1 + bounciness) * relVelMagnitude / (1 / sphere.mass + 1 / box.mass);

        sphere.velocity -= (impulseMagnitude / sphere.mass) * manifold.Normal;
        box.velocity += (impulseMagnitude / box.mass) * manifold.Normal;

        var tangentVector = relativeVelocity - (relVelMagnitude) * manifold.Normal;
        var frictionMagnitude = -(1 + bounciness) * Vector3.Dot(relativeVelocity, tangentVector) / (1 / sphere.mass + 1 / box.mass);
        var friction = Mathf.Sqrt(sphere.friction * box.friction);
        frictionMagnitude = Mathf.Max(frictionMagnitude, -impulseMagnitude * friction);
        frictionMagnitude = Mathf.Min(frictionMagnitude, impulseMagnitude * friction);

        sphere.velocity -= (frictionMagnitude / sphere.mass) * manifold.Normal;
        box.velocity += (frictionMagnitude / box.mass) * manifold.Normal;
    }

    public static CollisionManifold GetSphereToSphereCollisionManifold(SphereBehaviour sphereA, SphereBehaviour sphereB) {
        var manifold = new CollisionManifold();

        var direction = sphereA.transform.position - sphereB.transform.position;

        manifold.PenetrationDistance = (direction.magnitude - (sphereA.Radius + sphereB.Radius)) / 2;

        if (manifold.PenetrationDistance <= 0.001f && manifold.PenetrationDistance > 0) {
            manifold.CollisionType = CollisionType.Colliding;
        } else if (manifold.PenetrationDistance < 0) {
            manifold.CollisionType = CollisionType.Penetrating;
        } else {
            manifold.CollisionType = CollisionType.None;
            return manifold;
        }

        manifold.Objects.Add(sphereA.gameObject);
        manifold.Objects.Add(sphereB.gameObject);
        manifold.Normal = direction.normalized;

        return manifold;
    }

    public static float AABBToSphereDistanceSq(SphereBehaviour sphere, CubeBehaviour box) {
        var dx = Mathf.Max(box.min.x - sphere.transform.position.x, 0);
        dx = Mathf.Max(dx, sphere.transform.position.x - box.max.x);
        var dy = Mathf.Max(box.min.y - sphere.transform.position.y, 0);
        dy = Mathf.Max(dy, sphere.transform.position.y - box.max.y);
        var dz = Mathf.Max(box.min.z - sphere.transform.position.z, 0);
        dz = Mathf.Max(dz, sphere.transform.position.z - box.max.z);

        return dx * dx + dy * dy + dz * dz;
    }

    public static bool AABBToSphereIntersect(SphereBehaviour sphere, CubeBehaviour box) {
        var dSq = AABBToSphereDistanceSq(sphere, box);
        return dSq <= sphere.Radius * sphere.Radius;
    }

    public static Vector3 GetAABBClosestPoint(SphereBehaviour sphere, CubeBehaviour box) {
        var points = new List<Vector3> {
            new Vector3(box.min.x, box.min.y, box.min.z),
            new Vector3(box.min.x, box.min.y, box.max.z),
            new Vector3(box.max.x, box.min.y, box.max.z),
            new Vector3(box.max.x, box.min.y, box.min.z),
            new Vector3(box.min.x, box.max.y, box.min.z),
            new Vector3(box.min.x, box.max.y, box.max.z),
            new Vector3(box.max.x, box.max.y, box.max.z),
            new Vector3(box.max.x, box.max.y, box.min.z)
        };

        var minDistancePoint = points[0];
        var minDistance = Mathf.Infinity;

        foreach (var point in points) {
            var dist = (point - sphere.transform.position).magnitude;

            if (!(dist < minDistance)) {
                continue;
            }

            minDistance = dist;
            minDistancePoint = point;
        }

        return minDistancePoint;
    }

    public static CollisionManifold getSphereToAABBCollisionManifold(SphereBehaviour sphere, CubeBehaviour box) {
        var manifold = new CollisionManifold();

        var closestPoint = GetAABBClosestPoint(sphere, box);
        var direction = sphere.transform.position - closestPoint;

        manifold.PenetrationDistance = (direction.magnitude - sphere.Radius) / 2;

        if (manifold.PenetrationDistance <= 0.001f && manifold.PenetrationDistance > 0) {
            manifold.CollisionType = CollisionType.Colliding;
        } else if (manifold.PenetrationDistance < 0) {
            manifold.CollisionType = CollisionType.Penetrating;
        } else {
            manifold.CollisionType = CollisionType.None;
            return manifold;
        }

        // manifold.Objects.Add(sphere.gameObject);
        // manifold.Objects.Add(box.gameObject);
        manifold.Normal = direction.normalized;

        return manifold;
    }
}
