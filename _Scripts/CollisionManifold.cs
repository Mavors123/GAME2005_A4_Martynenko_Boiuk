using System.Collections.Generic;
using UnityEngine;

public enum CollisionType {
    None,
    Colliding,
    Penetrating,
}

public struct SphereCollisionInfo {
    public Vector3 Position;
    public float Radius;
}

public struct CubeCollisionInfo {
    public Vector3 Min;
    public Vector3 Max;
}

public struct CollisionManifold {
    public Vector3 Normal;
    public float PenetrationDistance;
    public List<Vector3> ContactPoints;

    public List<GameObject> Objects;
    public Vector3 RelativeVelocity;
    public CollisionType CollisionType;
}
