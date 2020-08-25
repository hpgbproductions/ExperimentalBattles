namespace Assets.Scripts
{
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class InterceptLevelController : MonoBehaviour
    {
        public GameObject bomber;

        public FighterScript fighter1;
        public FighterScript fighter2;
        public FighterScript fighter3;

        public ShipProxy bomber_t;
        public RotatingMissileLauncherProxy f1_t;
        public RotatingMissileLauncherProxy f2_t;
        public RotatingMissileLauncherProxy f3_t;

        public PlayerToFightersLocn ptfl;
        public ScoreDispScript ScoreScript;

        private float HeadOnTimer = 0;
        private float Timer = 0;
        public int TargetsDestroyed = 0;

        public enum LevelStates { Start, HeadOn, NoneAhead, EnemyAhead }
        public LevelStates LevelState = LevelStates.Start;

        private Vector3 PlayerPosition;
        private int EnemyAhead = 0;        // Index of the enemy fighter

        // Get the id of the enemy in front of the player
        int CheckEnemyAhead()
        {
            int id = 0;

            if (ptfl.relpos1.z > 0 && fighter1.EnemyState != FighterScript.EnemyStates.Destroyed) id = 1;
            else if (ptfl.relpos2.z > 0 && fighter2.EnemyState != FighterScript.EnemyStates.Destroyed) id = 2;
            else if (ptfl.relpos3.z > 0 && fighter3.EnemyState != FighterScript.EnemyStates.Destroyed) id = 3;

            return id;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            TargetsDestroyed = 0;
            if (bomber_t.IsCriticallyDamaged) TargetsDestroyed++;
            if (f1_t.IsDisabled) TargetsDestroyed++;
            if (f2_t.IsDisabled) TargetsDestroyed++;
            if (f3_t.IsDisabled) TargetsDestroyed++;

            ScoreScript.tgt = TargetsDestroyed;

            PlayerPosition = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;

            if (LevelState == LevelStates.Start)
            {
                if ((bomber.transform.position - PlayerPosition).magnitude < 20000)
                {
                    fighter1.EnemyState = FighterScript.EnemyStates.HeadOn;
                    fighter2.EnemyState = FighterScript.EnemyStates.HeadOn;
                    fighter3.EnemyState = FighterScript.EnemyStates.HeadOn;
                    LevelState = LevelStates.HeadOn;
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Your aircraft has been detected!");
                }
            }
            else if (LevelState == LevelStates.HeadOn)
            {
                // All enemy fighters are behind player, or HeadOn state has timed out
                if (((ptfl.relpos1.z < 0 || fighter1.EnemyState == FighterScript.EnemyStates.Destroyed) &&
                    (ptfl.relpos2.z < 0 || fighter2.EnemyState == FighterScript.EnemyStates.Destroyed) &&
                    (ptfl.relpos3.z < 0 || fighter3.EnemyState == FighterScript.EnemyStates.Destroyed)) ||
                    HeadOnTimer >= 25f)
                {
                    fighter1.EnemyState = FighterScript.EnemyStates.Behind;
                    fighter2.EnemyState = FighterScript.EnemyStates.Behind;
                    fighter3.EnemyState = FighterScript.EnemyStates.Behind;
                    LevelState = LevelStates.NoneAhead;
                }

                HeadOnTimer += Time.deltaTime;
            }
            else if (LevelState == LevelStates.NoneAhead)
            {
                EnemyAhead = CheckEnemyAhead();

                if (EnemyAhead != 0)
                {
                    if (EnemyAhead == 1) fighter1.EnemyState = FighterScript.EnemyStates.Ahead;
                    else if (EnemyAhead == 2) fighter2.EnemyState = FighterScript.EnemyStates.Ahead;
                    else if (EnemyAhead == 3) fighter3.EnemyState = FighterScript.EnemyStates.Ahead;

                    LevelState = LevelStates.EnemyAhead;
                }
            }
            else if (LevelState == LevelStates.EnemyAhead)
            {
                if (EnemyAhead == 1 && (ptfl.relpos1.z < 0 || fighter1.EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 2 && (ptfl.relpos2.z < 0 || fighter2.EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 3 && (ptfl.relpos3.z < 0 || fighter3.EnemyState == FighterScript.EnemyStates.Destroyed))
                {
                    if (EnemyAhead == 1 && fighter1.EnemyState != FighterScript.EnemyStates.Destroyed) fighter1.EnemyState = FighterScript.EnemyStates.Behind;
                    else if (EnemyAhead == 2 && fighter2.EnemyState != FighterScript.EnemyStates.Destroyed) fighter2.EnemyState = FighterScript.EnemyStates.Behind;
                    else if (EnemyAhead == 3 && fighter3.EnemyState != FighterScript.EnemyStates.Destroyed) fighter3.EnemyState = FighterScript.EnemyStates.Behind;

                    EnemyAhead = 0;
                    LevelState = LevelStates.NoneAhead;
                }
            }

            if (TargetsDestroyed != 4)
            ScoreScript.score = Mathf.Clamp(20000 - Mathf.RoundToInt(Timer * 30), 0, 20000);

            Timer += Time.deltaTime;
        }
    }
}