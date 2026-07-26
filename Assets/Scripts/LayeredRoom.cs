using UnityEngine;

/// <summary>
/// Drives all room layers directly from GameManager state. No Animator
/// Controller, no state machine, no "Any State" retrigger bugs.
///
/// Attach to an empty "Room" GameObject. Add one child SpriteRenderer per
/// layer listed in SETUP.md and drag it into the matching field here.
/// </summary>
public class LayeredRoom : MonoBehaviour
{
    [Header("Static layers (assign once, never change)")]
    public SpriteRenderer interiorLayer; // Assets/Sprites/Layers/interior_static
    public SpriteRenderer buttonsLayer;  // Assets/Sprites/Layers/buttons_static

    [Header("Oxygen gauge - handles itself via OxygenGaugeDisplay, nothing to wire here")]
    public OxygenGaugeDisplay oxygenGauge; // kept for reference/inspection only

    [Header("Pipe leak - only visible while a leak is active")]
    public FramePlayer pipeLeak;         // Assets/Sprites/Layers/pipe_leak_drip

    [Header("Power warning vignette - only visible while power is out")]
    public FramePlayer errorVignette;    // Assets/Sprites/Layers/error_blink
    public SpriteRenderer graphLayer;    // Assets/Sprites/Layers/graph_static (only during power loss)

    [Header("Lights out overlay - real art from the .aseprite 'lights' layer")]
    public SpriteRenderer lightsOutLayer; // Assets/Sprites/Layers/lights_out_static/lights_out.png

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        bool leak = gm.leakActive;
        bool powerOut = !gm.powerOn;

        if (pipeLeak) pipeLeak.SetVisible(leak);
        if (errorVignette) errorVignette.SetVisible(powerOut);
        if (graphLayer) graphLayer.enabled = powerOut;

        // Lights physically go dark using the real overlay art, instead of a color tint
        if (lightsOutLayer) lightsOutLayer.enabled = powerOut;

        // OxygenGaugeDisplay reads GameManager.oxygenLevel itself every frame -
        // nothing to drive from here.
    }
}
