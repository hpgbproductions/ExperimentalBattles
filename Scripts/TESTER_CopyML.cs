using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTER_CopyML : MonoBehaviour
{
    [SerializeField]
    private GameObject Launcher;

    [SerializeField]
    private GameObject LBase;

    private GameObject ActiveLauncher;

    private float TimeCounter = 0.0f;
    private bool LActive = false;

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Repeat(TimeCounter, 20.0f) > 10.0f && !LActive)
        {
            ActiveLauncher = Instantiate(Launcher, LBase.transform);
            ActiveLauncher.transform.position = Vector3.zero - ServiceProvider.Instance.GameWorld.FloatingOriginOffset;
            ActiveLauncher.SetActive(true);
            LActive = true;
        }
        else if (Mathf.Repeat(TimeCounter, 20.0f) < 10.0f && LActive)
        {
            Destroy(ActiveLauncher);
            LActive = false;
        }

        TimeCounter += Time.deltaTime;
    }
}
