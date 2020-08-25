namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerToFightersLocn : MonoBehaviour
    {
        public GameObject target1;
        public GameObject target2;
        public GameObject target3;

        private Vector3 tgtpos1;
        private Vector3 deltapos1;
        public Vector3 relpos1;

        private Vector3 tgtpos2;
        private Vector3 deltapos2;
        public Vector3 relpos2;

        private Vector3 tgtpos3;
        private Vector3 deltapos3;
        public Vector3 relpos3;

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.position = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
            gameObject.transform.eulerAngles = new Vector3(ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation.x, ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation.y);

            tgtpos1 = target1.transform.position;
            deltapos1 = tgtpos1 - gameObject.transform.position;
            relpos1 = transform.InverseTransformDirection(deltapos1);

            tgtpos2 = target2.transform.position;
            deltapos2 = tgtpos2 - gameObject.transform.position;
            relpos2 = transform.InverseTransformDirection(deltapos2);

            tgtpos3 = target3.transform.position;
            deltapos3 = tgtpos3 - gameObject.transform.position;
            relpos3 = transform.InverseTransformDirection(deltapos3);
        }
    }
}