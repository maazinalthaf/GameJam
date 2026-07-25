using UnityEngine;

public enum RoomState { Normal, Oxygen, HullLeak, PowerLoss }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Timing")]
    public float surviveDuration = 180f; // 3 minutes
    private float elapsed = 0f;
    public bool GameOver { get; private set; }

    [Header("Water (0 = dry, 100 = flooded/dead)")]
    [Range(0, 100)] public float waterLevel = 15f;
    public float waterRiseRate = 1.6f;        // %/sec baseline seep
    public float waterRiseRateLeaking = 5.5f; // extra %/sec while an active leak is unpatched
    public float waterRiseRateNoPower = 2.0f; // extra %/sec while bilge pumps are offline

    [Header("Oxygen (0 = suffocate, 100 = full)")]
    [Range(0, 100)] public float oxygenLevel = 100f;
    public float oxygenDrainRate = 1.1f;          // %/sec baseline
    public float oxygenDrainRatePowerLoss = 2.4f; // %/sec while scrubber is offline

    [Header("Power")]
    public bool powerOn = true;
    public float powerFaultCheckInterval = 11f;
    [Range(0, 1)] public float powerFaultChance = 0.4f;
    private float powerFaultTimer;

    [Header("Leak")]
    public bool leakActive = false;
    public float leakCheckInterval = 9f;
    [Range(0, 1)] public float leakSpawnChance = 0.55f;
    private float leakSpawnTimer = 6f;

    public RoomState currentState = RoomState.Normal;

    [Header("Wiring - drag scene objects here")]
    public GameUI ui;
    public RoomAnimator roomAnimator;
    public LeakPoint leakPoint;
    public PowerPanel powerPanel;

    void Awake()
    {
        Instance = this;
        powerFaultTimer = powerFaultCheckInterval;
    }

    void Update()
    {
        if (GameOver) return;

        elapsed += Time.deltaTime;

        // --- Water ---
        float rise = waterRiseRate;
        if (leakActive) rise += waterRiseRateLeaking;
        if (!powerOn) rise += waterRiseRateNoPower;
        waterLevel = Mathf.Clamp(waterLevel + rise * Time.deltaTime, 0f, 100f);

        // --- Oxygen ---
        float drain = powerOn ? oxygenDrainRate : oxygenDrainRatePowerLoss;
        oxygenLevel = Mathf.Clamp(oxygenLevel - drain * Time.deltaTime, 0f, 100f);

        // --- Random leak events ---
        if (!leakActive)
        {
            leakSpawnTimer -= Time.deltaTime;
            if (leakSpawnTimer <= 0f)
            {
                leakSpawnTimer = leakCheckInterval;
                if (Random.value < leakSpawnChance) TriggerLeak();
            }
        }

        // --- Random power fault events ---
        if (powerOn)
        {
            powerFaultTimer -= Time.deltaTime;
            if (powerFaultTimer <= 0f)
            {
                powerFaultTimer = powerFaultCheckInterval;
                if (Random.value < powerFaultChance) TriggerPowerLoss();
            }
        }

        UpdateRoomState();
        ui?.Refresh(waterLevel, oxygenLevel, elapsed, surviveDuration, powerOn, leakActive);

        if (waterLevel >= 100f) EndGame(false, "The hull gave out. Flooded.");
        else if (oxygenLevel <= 0f) EndGame(false, "You ran out of air.");
        else if (elapsed >= surviveDuration) EndGame(true, "Depth held. You made it 3 minutes.");
    }

    void UpdateRoomState()
    {
        // Priority: power loss > hull leak > low oxygen warning > normal
        RoomState next;
        if (!powerOn) next = RoomState.PowerLoss;
        else if (leakActive) next = RoomState.HullLeak;
        else if (oxygenLevel < 99f) next = RoomState.Oxygen;
        else next = RoomState.Normal;

        if (next != currentState)
        {
            currentState = next;
            roomAnimator?.SetState(currentState);
        }
    }

    // ----- Called by hotspot scripts -----

    public void TriggerLeak()
    {
        leakActive = true;
        leakPoint?.Reset();
    }

    public void PatchLeak()
    {
        leakActive = false;
        leakSpawnTimer = leakCheckInterval;
    }

    public void TriggerPowerLoss()
    {
        powerOn = false;
        powerPanel?.Reset();
    }

    public void RestorePower()
    {
        powerOn = true;
        powerFaultTimer = powerFaultCheckInterval;
    }

    public void PumpOxygen(float amount)
    {
        oxygenLevel = Mathf.Clamp(oxygenLevel + amount, 0f, 100f);
    }

    void EndGame(bool won, string message)
    {
        GameOver = true;
        ui?.ShowEnd(won, message);
    }

    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
