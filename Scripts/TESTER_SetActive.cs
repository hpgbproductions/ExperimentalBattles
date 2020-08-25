using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTER_SetActive : MonoBehaviour
{
    [SerializeField]
    private GameObject Launcher;

    private float TimeCounter = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (TimeCounter > 6.0f && TimeCounter < 9.0f) Launcher.SetActive(false);
        else Launcher.SetActive(true);

        TimeCounter += Time.deltaTime;
    }
}
