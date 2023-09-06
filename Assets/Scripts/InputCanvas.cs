using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class InputCanvas: MonoBehaviour
{
    [SerializeField] private Game GameSF;
    [SerializeField] private Transform DebugPanelSF;
    [SerializeField] private Text FPSTextSF, 
        PlayerMassTextSF, PlayerMassMultiplierTextSF,
        GroundFrictionTextSF, SkyFrictionTextSF, 
        CoinsCountTextSF, AddSegmentFlagTextSF;
    [SerializeField] private Slider PlayerMassSliderSF;
    [SerializeField] private Slider PlayerMassMultiplierSliderSF;
    [SerializeField] private Slider GroundFrictionSliderSF;
    [SerializeField] private Slider SkyFrictionSliderSF;

    public void SetTextFps(int fps) => FPSTextSF.text = fps.ToString();
    public void SetDebug(bool isDebug) => DebugPanelSF.gameObject.SetActive(isDebug);

    public void SetInfo(DebugSettings settings)
    {
        PlayerMassTextSF.text = settings.Mass.ToString();
        PlayerMassMultiplierTextSF.text = settings.MassMultiplier.ToString();
        GroundFrictionTextSF.text = settings.GroundFriction.ToString();
        SkyFrictionTextSF.text = settings.SkyFriction.ToString();
        PlayerMassSliderSF.value = settings.Mass;
        PlayerMassMultiplierSliderSF.value = settings.MassMultiplier;
        GroundFrictionSliderSF.value = settings.GroundFriction;
        SkyFrictionSliderSF.value = settings.SkyFriction;
    }
    public DebugSettings GetDebugSettings()
    {
        return new DebugSettings(
            float.Parse(PlayerMassTextSF.text),
            float.Parse(PlayerMassMultiplierTextSF.text),
            float.Parse(GroundFrictionTextSF.text),
            float.Parse(SkyFrictionTextSF.text));
    }
    public void SetSliderPlayerMass(float value)
    {
        PlayerMassTextSF.text = value.ToString();
    }
    public void SetSliderPlayerMassMultiplier(float value)
    {
        PlayerMassMultiplierTextSF.text = value.ToString();
    }
    public void SetSliderGroundFriction(float value)
    {
        GroundFrictionTextSF.text = value.ToString();
    }
    public void SetSliderSkyFriction(float value)
    {
        SkyFrictionTextSF.text = value.ToString();
    }
    public void UpdateCoinsCount(int count)
    {
        CoinsCountTextSF.text = count.ToString();
    }
    public void Quit()
    {
        GameSF.Quit();
    }
    public IEnumerator ShowInfo()
    {
        AddSegmentFlagTextSF.text = "AddSegment";
        yield return new WaitForSeconds(1);
        AddSegmentFlagTextSF.text = "";
    }

}
