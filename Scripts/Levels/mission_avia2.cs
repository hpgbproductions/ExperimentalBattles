using Assets.Scripts;
using Jundroo.SimplePlanes.ModTools;
using Jundroo.SimplePlanes.ModTools.PrefabProxies;
using UnityEngine;

/// <summary>
/// A SimplePlanes custom level.
/// </summary>
public class mission_avia2 : Level
{
    /// <summary>
    /// Initializes a new instance of the <see cref="mission6_avia2"/> class.
    /// </summary>
    public mission_avia2()
        : base("NCS1 - Piloted", "MapAvia2",
            "NCS1 Mission: Piloted\n" +
            "Area: Open Water\n" +
            "Time: 1830\n" +
            "Cloud Cover: Few Clouds\n" +
            "Vehicle: Fighter\n\n" +
            "Engage in a duel against one of the most advanced drones of the 21st Century, the AQF-21 EX. Learn its combat pattern and put your piloting skills to the test.\n\n" +
            "This level is designed for realistic/semi-realistic fighters like Wasp and Hellkeska. Better missiles may be required.\n\n" +
            "Scoring: \n" +
            "- Mission starts with 30,000 points. 60 points are deducted per second.\n" +
            "- Each support drone destroyed gives 5,000 bonus points.\n\n" +
            "<color=orange>IMPORTANT NOTES:\n" +
            "- Use air to ground missiles and targeting mode.\n" +
            "- If the enemy does not appear on screen within about 30 seconds, you should restart the level.</color>")
    {
    }

    protected override bool StartTimerWithThrottle
    {
        get
        {
            return false;
        }
    }

    protected override string FormatScore(float score)
    {
        return string.Format("Score: {0}", score);
    }

    protected override void Update()
    {
        base.Update();

        ServiceProvider.Instance.EnvironmentManager.TimeOfDay = 18.3f;
        ServiceProvider.Instance.EnvironmentManager.UpdateWeather(WeatherPreset.FewClouds, 0, true);

        ShipProxy Boss = Map.gameObject.GetComponentInChildren<ShipProxy>();

        if (ServiceProvider.Instance.PlayerAircraft.CriticallyDamaged)
        {
            EndLevel(false, "Mission Failed", 0);
        }
        else if (Boss.IsCriticallyDamaged)
        {
            EndLevel(true, "Mission Accomplished", Map.gameObject.GetComponentInChildren<ScoreDispScript>().score);
        }
    }
}