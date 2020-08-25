using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoExpire : MonoBehaviour
{
    public float Lifetime;

    // Update is called once per frame
    void Update()
    {
        Lifetime -= Time.deltaTime;

        if (Lifetime < 0)
        {
            Destroy(gameObject);
        }
    }
}
