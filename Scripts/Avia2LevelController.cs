namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using Jundroo.SimplePlanes.ModTools;
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using SimplePlanesReflection.Assets.Scripts.Levels.Enemies;
    using UnityEngine;

    public class Avia2LevelController : MonoBehaviour
    {
        public GameObject boss;
        public GameObject boss_model;
        public GameObject familiars;
        public GameObject fa;
        public GameObject fb;
        public GameObject laser_f;
        public GameObject laser_r;
        public GameObject barrage_c;     // Optimal Lead
        public GameObject barrage_i;     // Focal Point
        public GameObject barrage_f1;    // Hyper Strike
        public GameObject barrage_f2;
        public GameObject plot_armor;

        public ShipProxy boss_t;
        public RotatingMissileLauncherProxy fa_t;
        public RotatingMissileLauncherProxy fb_t;
        public MapStartLocation StartLocation;

        public ScoreDispScript score_disp;

        public enum LevelStates { HeadOn, BossBehind, LaserAttack, Destroyed }
        public LevelStates LevelState = LevelStates.HeadOn;

        public PlayerToBossLocn _ptbl;
        private Vector3 BossRelPosFromPlayer;
        bool BossInFrontOfPlayer;
        bool PlayerInFrontOfBoss;

        // Standard player position data
        private Vector3 VectorDelta;           // Points from boss to player in world space
        private Vector3 PlayerPosition;
        private Vector3 RelPlayerPosition;     // Points from boss to player in local space
        private Vector3 RelPlayerDirection;
        private float PlayerDistance = 25000;

        // Leading system data
        private Vector3 LeadingVectorDelta;        // Points from boss to predicted location in world space
        private Vector3 LeadingPlayerPosition;     // Predicted location that boss points to in head-ons
        private Vector3 PreviousPlayerPosition;    // Previous frame data
        private float PreviousPlayerDistance;
        private float InterceptTime;               // Predicted time for player to reach predicted location

        // Barrage leading system data
        private Vector3 BarrageLeadingVectorDelta;
        private Vector3 BarrageLeadingPlayerPosition;

        // Common attack data
        private float AttackTimer = 0;
        private int AttackStage = 0;
        private int AttackCount = 0;

        private Vector3 TargetPlayerPosition;     // Intersect point for laser attacks

        private float PlayerSpeed;

        private float BossSpeed = 500f;
        private float BossWeakSpeed;
        private float BossAliveTime = 0;

        private int FamiliarsBonus = 0;
        private bool ShownBonusMessageA = false;
        private bool ShownBonusMessageB = false;

        // Reset common settings and change state
        private void ChangeLevelState(LevelStates _ls)
        {
            AttackTimer = 0;
            AttackStage = 0;
            LevelState = _ls;
        }

        private void Start()
        {
            // Generate first player position and distance using start location
            PlayerPosition = StartLocation.transform.position;
            PlayerDistance = (StartLocation.transform.position - boss.transform.position).magnitude;

            // RNG seed
            Random.InitState(System.DateTime.Now.Millisecond);

            // Set up target values for score display
            score_disp.tgt = 0;
            score_disp.tgt_max = 1;
        }

        void Update()
        {
            // Prevents the program from breaking when the game is paused
            if (ServiceProvider.Instance.GameState.IsPaused)
            {
                return;
            }

            FamiliarsBonus = 0;

            if (fa_t.IsDisabled)
            {
                fa.SetActive(false);
                FamiliarsBonus += 5000;
                if (!ShownBonusMessageA)
                {
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Support drone destroyed!\nBonus: 5000");
                    ShownBonusMessageA = true;
                }
            }
            if (fb_t.IsDisabled)
            {
                fb.SetActive(false);
                FamiliarsBonus += 5000;
                if (!ShownBonusMessageB)
                {
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Support drone destroyed!\nBonus: 5000");
                    ShownBonusMessageB = true;
                }
            }

            // Destroyed targets
            if (boss_t.IsCriticallyDamaged)
            {
                fa.SetActive(false);
                fb.SetActive(false);
                boss_model.SetActive(false);
                score_disp.tgt = 1;
                ChangeLevelState(LevelStates.Destroyed);
            }
            else
            {
                score_disp.score = Mathf.Clamp(30000 - Mathf.RoundToInt(BossAliveTime * 60) + FamiliarsBonus, 0, 40000);
            }

            // Store previous frame values before new values are written
            PreviousPlayerPosition = PlayerPosition;
            PreviousPlayerDistance = PlayerDistance;

            // Player variables
            PlayerPosition = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
            PlayerSpeed = ServiceProvider.Instance.PlayerAircraft.Airspeed;

            // Current player location variables
            VectorDelta = PlayerPosition - boss.transform.position;
            RelPlayerPosition = transform.InverseTransformDirection(VectorDelta);
            RelPlayerDirection = RelPlayerPosition.normalized;
            PlayerDistance = VectorDelta.magnitude;

            BossRelPosFromPlayer = _ptbl.relpos;
            BossInFrontOfPlayer = BossRelPosFromPlayer.z > 0;
            PlayerInFrontOfBoss = RelPlayerDirection.z > 0;

            // Lead prediction variables
            InterceptTime = Mathf.Max(PlayerDistance / ((PreviousPlayerDistance - PlayerDistance) / Time.deltaTime), 0);
            LeadingPlayerPosition = PlayerPosition + ((PlayerPosition - PreviousPlayerPosition) * InterceptTime / Time.deltaTime);
            LeadingVectorDelta = LeadingPlayerPosition - boss.transform.position;
            
            // Move boss ahead by defined speed
            gameObject.transform.Translate(0, 0, BossSpeed * Time.deltaTime);

            // Maintain target object's position on the model
            boss_t.transform.position = gameObject.transform.position + new Vector3(0f, 3.5f, 4f);

            // Main control
            if (LevelState == LevelStates.HeadOn)
            {
                familiars.transform.position = gameObject.transform.position;
                familiars.transform.rotation = gameObject.transform.rotation;
                BossSpeed = 500f;

                if (PlayerDistance > 3000f)
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, LeadingVectorDelta, 1.0f*Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));
                }
                else
                {
                    ChangeLevelState(LevelStates.LaserAttack);
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Energy surge detected!\nSpecial Laser \"Photon Burst\"");
                }
            }
            else if (LevelState == LevelStates.LaserAttack)    // Head-on laser sweep
            {
                familiars.transform.position = gameObject.transform.position;
                familiars.transform.rotation = gameObject.transform.rotation;
                fa_t.transform.localEulerAngles = Vector3.zero;
                fb_t.transform.localEulerAngles = Vector3.zero;

                // Set up sweep attack
                if (AttackStage == 0)
                {
                    BossSpeed = 300f;
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));
                    laser_f.SetActive(true);
                    laser_r.SetActive(true);
                    if (RelPlayerPosition.z < 0) AttackStage = 1;
                }
                else if (AttackStage == 1)
                {
                    BossSpeed = 100f;
                    TargetPlayerPosition = PlayerPosition + ((PlayerPosition - PreviousPlayerPosition) * (Vector3.Angle(VectorDelta, gameObject.transform.forward) * Mathf.Deg2Rad) / Time.deltaTime);
                    AttackStage = 2;
                }
                else if (AttackStage == 2)    // Turning attack
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, TargetPlayerPosition - gameObject.transform.position, 4.0f * Time.deltaTime, 1.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                }

                laser_r.transform.Rotate(0, 0, 20 * Time.deltaTime, Space.Self);

                if ((PlayerDistance > 4000 && AttackStage == 2) || PlayerDistance > 8000 || AttackTimer > 8)
                {
                    plot_armor.SetActive(false);
                    laser_f.SetActive(false);
                    laser_r.SetActive(false);
                    ChangeLevelState(LevelStates.BossBehind);
                }
            }
            else if (LevelState == LevelStates.BossBehind)
            {
                // Boss out of fuel state
                if (BossAliveTime > 600)
                {
                    BossWeakSpeed = Mathf.Clamp(BossSpeed, 100, 300);
                    AttackStage = -2;
                    AttackTimer = -1;
                }

                // Position familiars
                if (AttackStage != 7)
                {
                    familiars.transform.position = gameObject.transform.position;
                    familiars.transform.rotation = gameObject.transform.rotation;
                }

                // Barrage lead calculation
                if (AttackStage == 2 || AttackStage == 3)    // Optimal Lead setup
                {
                    BarrageLeadingPlayerPosition = PlayerPosition + ((PlayerPosition - PreviousPlayerPosition) * (PlayerDistance / Mathf.Max(700 - PlayerSpeed, 1)) / Time.deltaTime);
                    BarrageLeadingVectorDelta = BarrageLeadingPlayerPosition - boss.transform.position;
                }
                else if (AttackStage >= 4 && AttackStage <= 7)    // Focal Point, Hyper Strike
                {
                    BarrageLeadingPlayerPosition = PlayerPosition + ((PlayerPosition - PreviousPlayerPosition) * (PlayerDistance / Mathf.Max(700 - PlayerSpeed, 1)) * 2 / Time.deltaTime);
                    BarrageLeadingVectorDelta = BarrageLeadingPlayerPosition - boss.transform.position;
                }

                // Boss speed calculation
                if (AttackStage == 0 || AttackStage == 1 || (AttackStage >= 6 && AttackStage <= 8))
                {
                    BossSpeed = Mathf.Clamp(100f + PlayerDistance / 3, 100f, 750f);
                }
                else if (AttackStage >= 2 && AttackStage <= 5)    // Optimal Lead, Focal Point
                {
                    BossSpeed = Mathf.Clamp(100f + BarrageLeadingVectorDelta.magnitude / 10, 100f, 750f);
                }
                else if (AttackStage < 0)    // Weak
                {
                    BossSpeed = BossWeakSpeed;
                }

                if (AttackStage == 0)    // Setup script
                {
                    AttackTimer = Random.Range(-15f, -25f);
                    AttackStage = 1;
                }
                else if (AttackStage == 1)    // Fly towards player
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, 0.8f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));

                    // Check if attack is ready
                    if (AttackTimer > 0)
                    {
                        // Check if player is close enough, run attack if true
                        if (PlayerDistance < 3000)
                        {
                            if (AttackCount % 3 == 0) AttackStage = 2;
                            else if (AttackCount % 3 == 1) AttackStage = 4;
                            else if (AttackCount % 3 == 2) AttackStage = 6;
                        }
                        else
                        {
                            AttackStage = 2;
                        }
                    }
                }
                else if (AttackStage == 2)    // Cut-in turn and leading missile barrage
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, BarrageLeadingVectorDelta, 1.5f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Calculated Barrage \"Perfect Trajectory\"");
                    barrage_c.SetActive(true);
                    barrage_c.transform.localPosition = new Vector3(0, 2, 12);

                    AttackStage = 3;
                }
                else if (AttackStage == 3)
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, BarrageLeadingVectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    if (AttackTimer > 10)
                    {
                        barrage_c.transform.localPosition = Vector3.back * 20000;
                        barrage_c.SetActive(false);
                    }

                    if (AttackTimer > 15)
                    {
                        AttackCount++;
                        AttackStage = -1;
                    }
                }
                else if (AttackStage == 4)    // Focal missile barrage
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, BarrageLeadingVectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Special Barrage \"Point of Convergence\"");
                    barrage_i.SetActive(true);
                    barrage_i.transform.localPosition = new Vector3(0, 3, 12);

                    AttackStage = 5;
                }
                else if (AttackStage == 5)
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, BarrageLeadingVectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    if (AttackTimer > 10)
                    {
                        barrage_i.transform.localPosition = Vector3.back * 20000;
                        barrage_i.SetActive(false);
                    }

                    if (AttackTimer > 15)
                    {
                        AttackCount++;
                        AttackStage = -1;
                    }
                }
                else if (AttackStage == 6)    // Drone command barrage
                {
                    Vector3 RotateDirBoss = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDirBoss);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    ServiceProvider.Instance.GameWorld.ShowStatusMessage("Command Barrage \"Multi-Directional Hyper Strike\"");
                    barrage_f1.SetActive(true);
                    barrage_f2.SetActive(true);
                    barrage_f1.transform.localPosition = new Vector3(0, 3, 0);
                    barrage_f2.transform.localPosition = new Vector3(0, 3, 0);

                    AttackStage = 7;
                }
                else if (AttackStage == 7)
                {
                    Vector3 RotateDirBoss = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDirBoss);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    Vector3 RotateDirFamiliar = Vector3.RotateTowards(familiars.transform.forward, BarrageLeadingPlayerPosition - familiars.transform.position, 1.0f * Time.deltaTime, 3.0f);
                    familiars.transform.rotation = Quaternion.LookRotation(RotateDirFamiliar);
                    familiars.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));
                    familiars.transform.Translate(0, 0, (PlayerSpeed + 150) * Time.deltaTime, Space.Self);

                    if (AttackTimer > 15)
                    {
                        barrage_f1.transform.position = Vector3.forward * 20000;
                        barrage_f2.transform.position = Vector3.forward * 20000;
                        barrage_f1.SetActive(false);
                        barrage_f2.SetActive(false);
                        AttackStage = 8;
                    }
                }
                else if (AttackStage == 8)
                {
                    Vector3 RotateDirBoss = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, 1.0f * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDirBoss);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                    Vector3 RotateDirFamiliar = Vector3.RotateTowards(gameObject.transform.forward, gameObject.transform.position - familiars.transform.position, 3.0f * Time.deltaTime, 3.0f);
                    familiars.transform.rotation = Quaternion.LookRotation(RotateDirFamiliar);
                    familiars.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));
                    familiars.transform.Translate(0, 0, 900 * Time.deltaTime, Space.Self);

                    if ((familiars.transform.position - gameObject.transform.position).magnitude < 200)
                    {
                        AttackCount++;
                        AttackStage = -1;
                    }
                }
                else if (AttackStage == -1)    // Run the check to determine if the boss will be weak
                {
                    if (AttackCount % 3 == 0)
                    {
                        BossWeakSpeed = Mathf.Clamp(BossSpeed, 100, 250);
                        AttackTimer = -30f;
                        AttackStage = -2;
                        ServiceProvider.Instance.GameWorld.ShowStatusMessage("The enemy is weak! Strike now!");
                    }
                    else
                    {
                        AttackStage = 0;
                    }
                }
                else if (AttackStage == -2)
                {
                    if (AttackTimer < 0)
                    {
                        Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, 0.025f * Time.deltaTime, 3.0f);
                        gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));
                    }
                    else
                    {
                        AttackStage = 0;
                    }
                }
            }

            AttackTimer += Time.deltaTime;
            BossAliveTime += Time.deltaTime;
        }

        void LateUpdate()
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.Max(gameObject.transform.position.y, 10 - ServiceProvider.Instance.GameWorld.FloatingOriginOffset.y), gameObject.transform.position.z);
        }
    }
}