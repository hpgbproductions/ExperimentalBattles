namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using UnityEngine;

    public class BomberScript : MonoBehaviour
    {
        public GameObject bomber_m;

        public ShipProxy bomber_t;

        public bool FailMission = false;

        private float speed = 150f;

        private float time = 280f;
        private bool ShownMinuteMessage = false;

        private float RandomRollRate;

        void Start()
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            RandomRollRate = Random.Range(-15f, 15f);
        }
        void Update()
        {
            gameObject.transform.Translate(0, 0, speed * Time.deltaTime);
            time -= Time.deltaTime;

            if (time <= 60 && !ShownMinuteMessage && !bomber_t.IsCriticallyDamaged)
            {
                ServiceProvider.Instance.GameWorld.ShowStatusMessage("The bomber is about to reach our airfield!");
                ShownMinuteMessage = true;
            }

            if (bomber_t.IsCriticallyDamaged)
            {
                gameObject.transform.eulerAngles = new Vector3(Mathf.Min(gameObject.transform.eulerAngles.x + 6 * Time.deltaTime, 45), gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z + RandomRollRate * Time.deltaTime);
            }
            else if (time <= 0)
            {
                FailMission = true;
            }
        }
    }
}