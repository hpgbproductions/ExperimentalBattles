namespace Assets.Scripts
{
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SnowstoneLevelController : MonoBehaviour
    {
        public ShipProxy runway_t;
        public ShipProxy base_t;

        public ShipProxy[] extra_t;

        public ScoreDispScript ScoreScript;

        public int TargetsDestroyed;
        public int ExtraDestroyed;

        public int ExtraDestroyedMsg = 0;

        private float Timer = 0;

        void Update()
        {
            TargetsDestroyed = 0;
            // Count targets destroyed
            if (runway_t.IsCriticallyDamaged) TargetsDestroyed++;
            if (base_t.IsCriticallyDamaged) TargetsDestroyed++;

            ExtraDestroyed = 0;
            // Count destroyers sunk
            for (int i = 0; i < extra_t.Length; i++)
            {
                if (extra_t[i].IsCriticallyDamaged) ExtraDestroyed++;
            }

            // Display bonus points message
            if (ExtraDestroyedMsg != ExtraDestroyed)
            {
                ServiceProvider.Instance.GameWorld.ShowStatusMessage("Destroyer sunk!\nBonus: 2500");
                ExtraDestroyedMsg = ExtraDestroyed;
            }

            ScoreScript.tgt = TargetsDestroyed;

            if (TargetsDestroyed != 2)
            {
                ScoreScript.score = Mathf.Clamp(10000 - Mathf.RoundToInt(Timer * 30) + 2500 * ExtraDestroyed, 0, 15000);
            }

            Timer += Time.deltaTime;
        }
    }
}