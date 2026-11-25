using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ClientPlugin.Settings.Elements;
using ClientPlugin.Settings.Tools;

namespace ClientPlugin;

/// <summary>
/// Plugin settings. Saved as Config.xml in the plugin data folder and editable in game
/// through Pulsar's plugin settings button (UI is generated from the attributes).
/// </summary>
public class Config : INotifyPropertyChanged
{
    // Windows virtual-key codes for the default bindings
    private const int VkHome = 0x24;
    private const int VkPageUp = 0x21;
    private const int VkPageDown = 0x22;

    #region Options

    private bool enabled = true;
    private bool verboseLog = false;

    private Binding toggleKey = new(VkHome, alt: true);
    private Binding nextKey = new(VkPageDown, alt: true);
    private Binding previousKey = new(VkPageUp, alt: true);

    private bool wheelZoom = true;
    private float wideViewFov = 80f;
    private bool barrelView = true;
    private bool allTurrets = true;

    private bool allConnectors = true;
    private bool controlsFollowCamera = true;

    #endregion

    #region User interface

    [XmlIgnore]
    public readonly string Title = "LW Camera Plugin";

    [Separator("General")]

    [Checkbox(label: "Enabled", description: "Turn the whole plugin on or off")]
    public bool Enabled { get => enabled; set => SetField(ref enabled, value); }

    [Checkbox(label: "Verbose log", description: "Write the camera list and camera switching to the game log (for testing)")]
    public bool VerboseLog { get => verboseLog; set => SetField(ref verboseLog, value); }

    [Separator("Keys")]

    [Keybind(label: "Camera view on/off", description: "In a cockpit: look out of a connector of this ship. Press again to go back")]
    public Binding ToggleKey { get => toggleKey; set => SetField(ref toggleKey, value); }

    [Keybind(label: "Next connector")]
    public Binding NextKey { get => nextKey; set => SetField(ref nextKey, value); }

    [Keybind(label: "Previous connector")]
    public Binding PreviousKey { get => previousKey; set => SetField(ref previousKey, value); }

    [Separator("Zoom")]

    [Checkbox(label: "Mouse wheel zoom", description: "Mouse wheel zooms the dock camera and the turret camera")]
    public bool WheelZoom { get => wheelZoom; set => SetField(ref wheelZoom, value); }


    [Slider(30, 120, 1, SliderAttribute.SliderType.Float, label: "Wide view (FOV °)", description: "Field of view when the dock camera is switched on")]
    public float WideViewFov { get => wideViewFov; set => SetField(ref wideViewFov, value); }

    [Separator("Turret camera")]

    [Checkbox(label: "Barrel view", description: "When you take control of a turret, the view moves to the turret's barrel. Off: the game's own turret view. The wheel zooms in both")]
    public bool BarrelView { get => barrelView; set => SetField(ref barrelView, value); }

    [Checkbox(label: "All turrets", description: "On: every turret gets the barrel view. Off: only turrets with [CAM] in the name")]
    public bool AllTurrets { get => allTurrets; set => SetField(ref allTurrets, value); }

    [Separator("Dock cameras")]

    [Checkbox(label: "All connectors are cameras", description: "On: every connector of the ship is a dock camera, no tag needed. Off: only connectors with [CAM] in the name")]
    public bool AllConnectors { get => allConnectors; set => SetField(ref allConnectors, value); }

    [Checkbox(label: "Controls follow dock camera", description: "On: in the dock camera W flies where the camera looks, A D C Space move left, right, down, up on the screen, the mouse turns around the camera. Off: controls stay as in the cockpit")]
    public bool ControlsFollowCamera { get => controlsFollowCamera; set => SetField(ref controlsFollowCamera, value); }

    #endregion

    #region Property change notification boilerplate

    public static readonly Config Default = new();
    public static Config Current { get; } = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
