﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }
}
