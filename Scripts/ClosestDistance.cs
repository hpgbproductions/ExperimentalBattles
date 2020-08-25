namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ClosestDistance : MonoBehaviour
    {
        public float closest;
        public float defdist = 100000f;

        private float current;

        [SerializeField]
        private FighterScript[] objs;

        private float[] objdist;

        private void Start()
        {
            objdist = new float[objs.Length];
        }

        void LateUpdate()
        {
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].EnemyState == FighterScript.EnemyStates.Destroyed)
                {
                    objdist[i] = defdist;
                }
                else
                {
                    objdist[i] = (gameObject.transform.position - objs[i].transform.position).magnitude;
                }
            }

            for (int i = 1; i < objs.Length; i++)
            {
                current = Mathf.Min(objdist[i], objdist[i - 1]);
            }

            closest = current;
        }
    }
}