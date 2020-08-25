namespace Assets.Scripts
{
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class HellkeskaLevelController : MonoBehaviour
    {
        public FighterScript[] fighters = new FighterScript[5];
        public RotatingMissileLauncherProxy[] f_t = new RotatingMissileLauncherProxy[5];

        public PlayerToFightersLocn2 ptfl;
        public ClosestDistance ClosestDistanceScript;
        public ScoreDispScript ScoreScript;

        public enum LevelStates { Start, StartAttack, Attack, Break }
        public LevelStates LevelState = LevelStates.Start;

        public enum DogfightStates { Start, NoneAhead, EnemyAhead }
        public DogfightStates DogfightState = DogfightStates.Start;

        private Vector3 PlayerPosition;
        private int EnemyAhead = -1;        // Enemy given the weak ahead state
        private int EnemyObj = -1;         // Enemy that ignores the player

        private float BomberTimer;    // Timer locks selected enemies in bomber state
        private bool ShownBomberAlert = false;

        private float AttackTimer;    // Timer used to control level state

        private float LevelTimer = 0;    // Total mission time

        public int TargetsDestroyed = 0;
        public bool FailMission = false;

        int CheckEnemyAhead()
        {
            int id = -1;

            if (ptfl.relpos0.z > 0 && fighters[0].EnemyState != FighterScript.EnemyStates.Destroyed && fighters[0].EnemyState != FighterScript.EnemyStates.Bomber) id = 0;
            else if (ptfl.relpos1.z > 0 && fighters[1].EnemyState != FighterScript.EnemyStates.Destroyed && fighters[1].EnemyState != FighterScript.EnemyStates.Bomber) id = 1;
            else if (ptfl.relpos2.z > 0 && fighters[2].EnemyState != FighterScript.EnemyStates.Destroyed && fighters[2].EnemyState != FighterScript.EnemyStates.Bomber) id = 2;
            else if (ptfl.relpos3.z > 0 && fighters[3].EnemyState != FighterScript.EnemyStates.Destroyed && fighters[3].EnemyState != FighterScript.EnemyStates.Bomber) id = 3;
            else if (ptfl.relpos4.z > 0 && fighters[4].EnemyState != FighterScript.EnemyStates.Destroyed && fighters[4].EnemyState != FighterScript.EnemyStates.Bomber) id = 4;

            return id;
        }

        void Start()
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }

        void Update()
        {
            TargetsDestroyed = 0;
            if (f_t[0].IsDisabled) TargetsDestroyed++;
            if (f_t[1].IsDisabled) TargetsDestroyed++;
            if (f_t[2].IsDisabled) TargetsDestroyed++;
            if (f_t[3].IsDisabled) TargetsDestroyed++;
            if (f_t[4].IsDisabled) TargetsDestroyed++;

            ScoreScript.tgt = TargetsDestroyed;

            PlayerPosition = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;

            if (ClosestDistanceScript.closest < 1000)
            {
                FailMission = true;
            }

            if (LevelState == LevelStates.Start)
            {
                // Squadron is near the player
                if ((fighters[0].transform.position - PlayerPosition).magnitude < 5000)
                {
                    for (int i = 0; i < fighters.Length; i++)
                    {
                        fighters[i].EnemyState = FighterScript.EnemyStates.Behind;
                    }

                    AttackTimer = Random.Range(30f, 50f);
                    LevelState = LevelStates.StartAttack;
                }
            }
            else if (LevelState == LevelStates.StartAttack)
            {
                AttackTimer -= Time.deltaTime;

                if (AttackTimer <= 0)
                {
                    if (DogfightState == DogfightStates.Start)
                    {
                        DogfightState = DogfightStates.NoneAhead;
                    }

                    // Select alive enemy
                    EnemyObj = Random.Range(0, 5 - TargetsDestroyed);
                    for (int i = 0; i < EnemyObj; i++)
                    {
                        if (fighters[i].EnemyState == FighterScript.EnemyStates.Destroyed)
                            EnemyObj++;
                    }

                    if (fighters[EnemyObj].EnemyState != FighterScript.EnemyStates.Ahead)
                    {
                        BomberTimer = 30;
                        ShownBomberAlert = false;
                        fighters[EnemyObj].EnemyState = FighterScript.EnemyStates.Bomber;

                        AttackTimer = 0;
                        LevelState = LevelStates.Attack;
                    }
                }
            }
            else if (LevelState == LevelStates.Attack)
            {
                AttackTimer += Time.deltaTime;
                BomberTimer -= Time.deltaTime;

                if (AttackTimer >= 10 && !ShownBomberAlert)
                {
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("An aircraft broke off and is headed towards our airfield! Intercept it now!");
                    ShownBomberAlert = true;
                }

                if (fighters[EnemyObj].EnemyState == FighterScript.EnemyStates.Destroyed)
                {
                    LevelState = LevelStates.Break;
                }

                if (BomberTimer <= 0 && (PlayerPosition - fighters[EnemyObj].transform.position).magnitude < 7000)
                {
                    fighters[EnemyObj].EnemyState = FighterScript.EnemyStates.Behind;
                    LevelState = LevelStates.Break;
                }
            }
            else if (LevelState == LevelStates.Break)
            {
                AttackTimer = Random.Range(25f, 35f);
                LevelState = LevelStates.StartAttack;
            }

            if (DogfightState == DogfightStates.NoneAhead)
            {
                EnemyAhead = CheckEnemyAhead();

                if (EnemyAhead != -1)
                {
                    for (int i = 0; i < fighters.Length; i++)
                    {
                        if (EnemyAhead == i)
                            fighters[i].EnemyState = FighterScript.EnemyStates.Ahead;
                    }

                    DogfightState = DogfightStates.EnemyAhead;
                }
            }
            else if (DogfightState == DogfightStates.EnemyAhead)
            {
                if (EnemyAhead == 0 && (ptfl.relpos0.z < 0 || fighters[0].EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 1 && (ptfl.relpos1.z < 0 || fighters[1].EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 2 && (ptfl.relpos2.z < 0 || fighters[2].EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 3 && (ptfl.relpos3.z < 0 || fighters[3].EnemyState == FighterScript.EnemyStates.Destroyed) ||
                    EnemyAhead == 4 && (ptfl.relpos4.z < 0 || fighters[4].EnemyState == FighterScript.EnemyStates.Destroyed))
                {
                    for (int i = 0; i < fighters.Length; i++)
                    {
                        if (EnemyAhead == i && fighters[i].EnemyState != FighterScript.EnemyStates.Destroyed)
                        {
                            fighters[i].EnemyState = FighterScript.EnemyStates.Behind;
                        }
                    }
                }
            }

            if (TargetsDestroyed != 5)
                ScoreScript.score = Mathf.Clamp(20000 - Mathf.RoundToInt(LevelTimer * 30), 0, 20000);

            LevelTimer += Time.deltaTime;
        }
    }
}