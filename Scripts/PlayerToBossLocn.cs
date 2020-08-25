namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerToBossLocn : MonoBehaviour
    {
        public GameObject target;

        private Vector3 tgtpos;
        private Vector3 deltapos;
        public Vector3 relpos;

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.position = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
            gameObject.transform.eulerAngles = ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation;

            tgtpos = target.transform.position;
            deltapos = tgtpos - gameObject.transform.position;
            relpos = transform.InverseTransformDirection(deltapos);
        }
    }
}