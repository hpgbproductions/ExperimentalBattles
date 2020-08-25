using Assets.Scripts;
using Jundroo.SimplePlanes.ModTools;
using UnityEngine;
/// <summary>
/// A SimplePlanes custom level.
/// </summary>
public class mission_hellkeska : Level
{
    /// <summary>
    /// Initializes a new instance of the <see cref="mission_hellkeska"/> class.
    /// </summary>
    public mission_hellkeska()
       : base("NCS1 - Cat and Mouse", MapNames.Default,
           "NCS1 Mission: Cat and Mouse\n" +
           "Area: Wright Isles\n" +
           "Time: 1300\n" +
           "Cloud Cover: Clear\n" +
           "Vehicle: Fighter\n\n" +
           "A well-trained enemy squadron is hell-bent on bombing our airfield. They are even willing to sacrifice some of their own. Take them all out before our airfield suffers any damage.\n\n" +
           "This level is designed for realistic/semi-realistic fighters like Wasp and Hellkeska.\n\n" +
           "Targets:\n" +
           "- 5x Fighter-Bomber\n\n" +
           "Tips:\n" +
           "- Watch the enemies' movements to see when they are trying to attack the airfield. The airfield's radar can only identify an attack a few seconds later.\n\n" +
           "Scoring: \n" +
           "- Mission starts with 20,000 points. 30 points are deducted per second.\n\n" +
           "<color=orange>IMPORTANT NOTES:\n" +
           "- Use air to ground missiles and targeting mode.</color>")
    {
    }

    protected bool? Initialized { get; set; }

    protected HellkeskaLevelController levelCont { get; set; }
    protected ScoreDispScript scoreDispScript { get; set; }

    protected void Initialize()
    {
        if (!Initialized.HasValue)
        {
            var obj = ServiceProvider.Instance.ResourceLoader.LoadGameObject("HellkeskaLevelObject");

            levelCont = obj.GetComponent<HellkeskaLevelController>();
            scoreDispScript = obj.GetComponent<ScoreDispScript>();

            ServiceProvider.Instance.EnvironmentManager.TimeOfDay = 13f;
            ServiceProvider.Instance.EnvironmentManager.UpdateWeather(WeatherPreset.Clear, 0, true);

            ServiceProvider.Instance.GameWorld.ShowStatusMessage("Fly straight ahead and intercept the targets at medium altitude.");

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
        else if (levelCont.FailMission)
        {
            EndLevel(false, "Mission Failed", 0);
        }
        else if (levelCont.TargetsDestroyed == 5)
        {
            EndLevel(true, "Mission Accomplished", scoreDispScript.score);
        }
    }
}
