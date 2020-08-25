using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTester : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + new Vector3(0, 100, 0), 10*Time.deltaTime);
    }
}
