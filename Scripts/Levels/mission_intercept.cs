using Jundroo.SimplePlanes.ModTools;
using Assets.Scripts;
using UnityEngine;

/// <summary>
/// A SimplePlanes custom level.
/// </summary>
public class mission_intercept : Level
{
   /// <summary>
   /// Initializes a new instance of the <see cref="mission_intercept"/> class.
   /// </summary>
   public mission_intercept()
      : base("NCS1 - Bomber Intercept", MapNames.Default,
          "NCS1 Mission: Bomber Intercept\n" +
          "Area: Wright Isles\n" +
          "Time: 1100\n" +
          "Cloud Cover: Broken Clouds\n" +
          "Vehicle: Fighter\n\n" +
          "The lads over at Bandit aren't happy that we bombed their airfield. In fact, they've sent a bomber and three escort fighters, and are attempting a low-altitude attack. Show them again what our forces are worth.\n\n" +
          "This level is designed for realistic/semi-realistic fighters like Wasp and Hellkeska.\n\n" +
          "Targets:\n" +
          "- 1x Bomber\n" +
          "- 3x Fighter\n\n" +
          "Tips:\n" +
          "- The bomber is the only time-sensitive target. Prioritize it.\n\n" +
          "Scoring: \n" +
           "- Mission starts with 20,000 points. 30 points are deducted per second.\n\n" +
          "<color=orange>IMPORTANT NOTES:\n" +
          "- Use air to ground missiles and targeting mode.</color>")
   {
   }

    protected bool? Initialized { get; set; }

    protected BomberScript bomberScript { get; set; }
    protected InterceptLevelController levelCont { get; set; }
    protected ScoreDispScript scoreDispScript { get; set; }

    protected void Initialize()
    {
        if (!Initialized.HasValue)
        {
            var obj = ServiceProvider.Instance.ResourceLoader.LoadGameObject("InterceptLevelObject");

            bomberScript = obj.GetComponentInChildren<BomberScript>();
            levelCont = obj.GetComponent<InterceptLevelController>();
            scoreDispScript = obj.GetComponent<ScoreDispScript>();

            ServiceProvider.Instance.EnvironmentManager.TimeOfDay = 11f;
            ServiceProvider.Instance.EnvironmentManager.UpdateWeather(WeatherPreset.BrokenClouds, 0, true);

            ServiceProvider.Instance.GameWorld.ShowStatusMessage("Fly straight ahead and intercept the targets at 2000 m (6600 ft) ASL.");

            Initialized = true;
        }
    }

    protected override LevelStartLocation StartLocation
    {
        get
        {
            return new LevelStartLocation
            {
                Position = new Vector3(6020f, 73f, -5592f),
                Rotation = Vector3.zero,
                InitialSpeed = 0f,
                InitialThrottle = 0f,
                StartOnGround = true
            };
        }
    }

    protected override string FormatScore(float score)
    {
        return string.Format("Score: {0}", score);
    }

    protected override void Update()
    {
        base.Update();

        if (!(Initialized ?? false))
        {
            Initialize();
            return;
        }

        if (ServiceProvider.Instance.PlayerAircraft.CriticallyDamaged)
        {
            EndLevel(false, "Mission Failed", 0);
        }
        else if (bomberScript.FailMission)
        {
            EndLevel(false, "Mission Failed", 0);
        }
        else if (levelCont.TargetsDestroyed == 4)
        {
            EndLevel(true, "Mission Accomplished", scoreDispScript.score);
        }
    }
}