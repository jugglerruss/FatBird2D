using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Create gameSettings", order = 0)]
public class GameSettings : ScriptableObject
{
    [SerializeField] private float PlayerMassSF,
        PlayerMassMultiplierSF, GroundFrictionSF, 
        SkyFrictionSF, StartSpeedSF;

    public DebugSettings DebugSettings => new (PlayerMassSF, PlayerMassMultiplierSF, GroundFrictionSF, SkyFrictionSF);
    public float StartSpeed => StartSpeedSF;
    public void SetSettingsOnStart()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.SetResolution(1280 , Screen.height * 1280 / Screen.width, true);
    }
    public void UpdateSettings()
    {
        PlayerMassSF = PlayerPrefs.GetFloat("playerMass",PlayerMassSF);
        PlayerMassMultiplierSF = PlayerPrefs.GetFloat("playerMassMultiplier",PlayerMassMultiplierSF);
        GroundFrictionSF = PlayerPrefs.GetFloat("groundFriction",GroundFrictionSF);
        SkyFrictionSF = PlayerPrefs.GetFloat("skyFriction",SkyFrictionSF);
    }
    public void SetSettings(DebugSettings settings)
    {
        PlayerPrefs.SetFloat("playerMass",settings.Mass);
        PlayerPrefs.SetFloat("playerMassMultiplier",settings.MassMultiplier);
        PlayerPrefs.SetFloat("groundFriction",settings.GroundFriction);
        PlayerPrefs.SetFloat("skyFriction",settings.SkyFriction);
        UpdateSettings();
    }
}
