using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
    public static BulletManager Instance;
    public SphereBehaviour[] spheres;
    public float bulletSpeed = 5.0f;
    
    // Start is called before the first frame update
    void Start() {
        if (!Instance) {
            Instance = this;
        }
        spheres = FindObjectsOfType<SphereBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public SphereBehaviour Get() {
        for (int i = 0; i < spheres.Length; i++) {
            if (!spheres[i].isUsed ) {
                spheres[i].isUsed = true;
                return spheres[i];
            }
        }

        return null;
    }
    
    public void Reset(SphereBehaviour sphere) {
        sphere.velocity = Vector3.zero;
        sphere.isUsed = false;
        sphere.transform.position = new Vector3(0, 0, -100);
    }
}
