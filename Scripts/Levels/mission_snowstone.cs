using Jundroo.SimplePlanes.ModTools;
using Assets.Scripts;
using UnityEngine;

/// <summary>
/// A SimplePlanes custom level.
/// </summary>
public class mission_snowstone : Level
{
   /// <summary>
   /// Initializes a new instance of the <see cref="mission_snowstone"/> class.
   /// </summary>
   public mission_snowstone()
      : base("NCS1 - Strategic Strike", MapNames.Default,
          "NCS1 Mission: Strategic Strike\n" +
          "Area: Snowstone\n" +
          "Time: 0700\n" +
          "Cloud Cover: Scattered Clouds\n" +
          "Vehicle: Attacker\n\n" +
          "Destroy the tunnel base and runway to strip Snowstone of its offensive capabilities. Heavy explosives are needed.\n\n" +
          "Optimize your combat strategy to get the best score.\n\n" +
          "Targets:\n" +
          "- 1x Base\n" +
          "- 1x Runway\n\n" +
          "Scoring: \n" +
          "- Mission starts with 10,000 points. 30 points are deducted per second.\n" +
          "- Each destroyer sunk gives 2,500 bonus points.\n" +
          "- No extra points are awarded for destroying any other enemy.")
   {
   }

    protected bool? Initialized { get; set; }

    protected SnowstoneLevelController levelCont { get; set; }
    protected ScoreDispScript scoreDispScript { get; set; }

    protected void Initialize()
    {
        if (!Initialized.HasValue)
        {
            var obj = ServiceProvider.Instance.ResourceLoader.LoadGameObject("SnowstoneLevelObject");

            levelCont = obj.GetComponent<SnowstoneLevelController>();
            scoreDispScript = obj.GetComponent<ScoreDispScript>();

            ServiceProvider.Instance.EnvironmentManager.TimeOfDay = 7f;
            ServiceProvider.Instance.EnvironmentManager.UpdateWeather(WeatherPreset.ScatteredClouds, 0, true);

            Initialized = true;
        }
    }

    protected override LevelStartLocation StartLocation
    {
        get
        {
            return new LevelStartLocation
            {
                Position = new Vector3(2300f, 5000f, 110000f),
                Rotation = Vector3.zero,
                InitialSpeed = 300f,
                InitialThrottle = 1,
                StartOnGround = false
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
        else if (levelCont.TargetsDestroyed == 2)
        {
            EndLevel(true, "Mission Accomplished", scoreDispScript.score);
        }
    }
}