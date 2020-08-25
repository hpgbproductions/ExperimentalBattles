namespace Assets.Scripts
{
    using Jundroo.SimplePlanes.ModTools.PrefabProxies;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FighterScript : MonoBehaviour
    {
        public GameObject f_m;

        public RotatingMissileLauncherProxy f_t;

        // Enemy AI behavior state
        // This program is not allowed to change the state, only the level controller may do so.
        public enum EnemyStates { Start, HeadOn, Ahead, Behind, Bomber, Destroyed }
        public EnemyStates EnemyState = EnemyStates.Start;

        public GameObject HeadOnTarget;

        public int AttackStage = 0;

        // Standard player position data
        private Vector3 VectorDelta;           // Points from enemy to player in world space
        private Vector3 PlayerPosition;
        public Vector3 RelPlayerPosition;      // Points from enemy to player in local space
        private Vector3 RelPlayerDirection;
        private float RelPlayerPitchAngle;     // Relative pitch from enemy to player
        public float PlayerDistance = 100000;

        // Leading system data for missile launch
        private Vector3 LeadingVectorDelta;        // Points from enemy to predicted location in world space
        private Vector3 LeadingPlayerPosition;     // Predicted location that chasing fighters point to
        private Vector3 PreviousPlayerPosition;    // Previous frame data
        private float LeadingPlayerDistance;
        private float PreviousPlayerDistance;

        private float PlayerSpeed;

        public float EnemyMinIAS = 80f;           // Constants that will be used to calculate TAS limit
        public float EnemyMaxIAS = 400f;
        public float EnemyBaseAccel = 15f;
        public float EnemyBaseDecel = 25f;
        public float EnemyOptimalSpeed = 225f;    // Special turn speed
        public float EnemyMaxAc = 89;
        public float EnemyTurnConstant = 0.00001f;
        public float EnemyWeakConstant = 0.4f;

        private float EnemySpeed = 150f;          // Current speed (TAS)
        private float EnemyIAS;                   // Current speed (IAS) for turn calculation
        private float EnemySpeedTarget = 150f;    // Target speed (TAS)
        private float EnemyMinSpeed;              // Variable TAS limit
        private float EnemyMaxSpeed;
        private float EnemyMaxAccel;
        private float EnemyMaxDecel;

        private float EnemyMaxAngVel;    // Turn rate in radians

        private bool LeadingMode = false;

        public float AttackTimer = 0;    // Handles timing of attack states

        // Start is called before the first frame update
        void Start()
        {
            Random.InitState(System.DateTime.Now.Millisecond + AttackStage);
        }

        // Update is called once per frame
        void Update()
        {
            // Prevents the program from breaking when the game is paused
            if (ServiceProvider.Instance.GameState.IsPaused)
            {
                return;
            }

            if (f_t.IsDisabled)
            {
                f_m.SetActive(false);
                f_t.gameObject.SetActive(false);
                EnemyState = EnemyStates.Destroyed;
                return;
            }

            // Store previous frame values before new values are written
            PreviousPlayerPosition = PlayerPosition;
            PreviousPlayerDistance = PlayerDistance;

            // Player variables
            PlayerPosition = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
            PlayerSpeed = ServiceProvider.Instance.PlayerAircraft.Airspeed;

            // Current player location variables
            VectorDelta = PlayerPosition - gameObject.transform.position;
            RelPlayerPosition = transform.InverseTransformDirection(VectorDelta);
            RelPlayerDirection = RelPlayerPosition.normalized;
            RelPlayerPitchAngle = Mathf.Rad2Deg * Mathf.Atan(RelPlayerDirection.y / RelPlayerDirection.z);
            PlayerDistance = VectorDelta.magnitude;

            // Simulate TAS limits
            EnemyMinSpeed = EnemyMinIAS * (1 + gameObject.transform.position.y / 305 * 0.02f);
            EnemyMaxSpeed = EnemyMaxIAS * (1 + gameObject.transform.position.y / 305 * 0.02f);

            // Simulate acceleration stats
            EnemyMaxAccel = EnemyBaseAccel + 9.8f * Mathf.Sin(gameObject.transform.eulerAngles.x);
            EnemyMaxDecel = EnemyBaseDecel - 9.8f * Mathf.Sin(gameObject.transform.eulerAngles.x);

            // Simulate accel/decel
            if (EnemySpeed < EnemySpeedTarget)
            {
                EnemySpeed = Mathf.Clamp(EnemySpeed + EnemyMaxAccel * Time.deltaTime, EnemyMinSpeed, EnemyMaxSpeed);
            }
            else if (EnemySpeed > EnemySpeedTarget)
            {
                EnemySpeed = Mathf.Clamp(EnemySpeed - EnemyMaxDecel * Time.deltaTime, EnemyMinSpeed, EnemyMaxSpeed);
            }

            // Get IAS
            EnemyIAS = EnemySpeed / (1 + gameObject.transform.position.y / 305 * 0.02f);

            // Calculate turn angular velocity
            EnemyMaxAngVel = Mathf.Min(0.00001f * EnemyIAS * EnemyIAS, EnemyMaxAc / EnemySpeed);

            // Move enemy ahead by defined speed
            gameObject.transform.Translate(0, 0, EnemySpeed * Time.deltaTime);

            // Main control
            if (EnemyState == EnemyStates.HeadOn)
            {
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));

                if (PlayerDistance > 3000)
                {
                    EnemySpeedTarget = EnemyMaxSpeed;

                    Vector3 HeadOnVectorDelta = HeadOnTarget.transform.position - gameObject.transform.position;

                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, HeadOnVectorDelta, EnemyMaxAngVel * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(HeadOnVectorDelta.x / HeadOnVectorDelta.y));
                }
                else
                {
                    EnemySpeedTarget = EnemyOptimalSpeed;
                }
            }
            else if (EnemyState == EnemyStates.Behind)
            {
                if (AttackTimer <= 0)
                {
                    AttackTimer = Random.Range(8f, 30f);
                    LeadingMode = !LeadingMode;
                }

                if (RelPlayerPosition.z > 200)
                {
                    if (LeadingMode)
                    {
                        // Missile lead prediction variables
                        LeadingPlayerPosition = PlayerPosition + ((PlayerPosition - PreviousPlayerPosition) * (PlayerDistance / Mathf.Max(700 - PlayerSpeed, 1)) / Time.deltaTime);
                        LeadingVectorDelta = LeadingPlayerPosition - gameObject.transform.position;
                        LeadingPlayerDistance = LeadingVectorDelta.magnitude;

                        // Fly towards target location
                        Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, LeadingVectorDelta, EnemyMaxAngVel * Time.deltaTime, 3.0f);
                        gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(LeadingVectorDelta.x / LeadingVectorDelta.y));

                        EnemySpeedTarget = Mathf.Clamp(EnemyMinSpeed + LeadingPlayerDistance / 4, EnemyMinSpeed, EnemyMaxSpeed);
                    }
                    else
                    {
                        // Fly directly towards player
                        Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, EnemyMaxAngVel * Time.deltaTime, 3.0f);
                        gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));

                        EnemySpeedTarget = Mathf.Clamp(EnemyMinSpeed + RelPlayerPosition.z / 4, EnemyMinSpeed, EnemyMaxSpeed);
                    }
                }
                else if (RelPlayerPosition.z > -50)
                {
                    // Fly in a straight line
                    EnemySpeedTarget = EnemyOptimalSpeed;
                }
                else
                {
                    Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, EnemyMaxAngVel * Time.deltaTime, 3.0f);
                    gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                    gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));

                    EnemySpeedTarget = EnemyOptimalSpeed;
                }

                AttackTimer -= Time.deltaTime;
            }
            else if (EnemyState == EnemyStates.Ahead)
            {
                Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, VectorDelta, EnemyMaxAngVel * Time.deltaTime * EnemyWeakConstant, 3.0f);
                gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Mathf.Rad2Deg * -Mathf.Atan(VectorDelta.x / VectorDelta.y));

                EnemySpeedTarget = EnemyOptimalSpeed;
            }
            else if (EnemyState == EnemyStates.Bomber)
            {
                EnemySpeedTarget = EnemyMaxSpeed;

                Vector3 BomberVectorDelta = HeadOnTarget.transform.position - gameObject.transform.position;

                Vector3 RotateDir = Vector3.RotateTowards(gameObject.transform.forward, BomberVectorDelta, EnemyMaxAngVel * Time.deltaTime, 3.0f);
                gameObject.transform.rotation = Quaternion.LookRotation(RotateDir);
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, 0f);
            }

        }

        void LateUpdate()
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.Max(gameObject.transform.position.y, 10 - ServiceProvider.Instance.GameWorld.FloatingOriginOffset.y), gameObject.transform.position.z);
        }
    }
}