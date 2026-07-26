using UnityEngine;

public enum RoomState { Normal, Oxygen, HullLeak, PowerLoss }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Timing")]
    public float surviveDuration = 180f;
    private float elapsed = 0f;
    public bool GameOver { get; private set; }

    [Header("Difficulty ramp")]
    public float maxDifficultyMultiplier = 2.5f;
    float DifficultyMultiplier => Mathf.Lerp(1f, maxDifficultyMultiplier, Mathf.Clamp01(elapsed / surviveDuration));

    [Header("Water")]
    [Range(0, 100)] public float waterLevel = 15f;
    public float waterRiseRateLeaking = 5.5f;
    public float waterRiseRateNoPower = 2.0f;
    public float waterRecedeRate = 20.0f;
    public float waterNormalLevel = 15f;

    [Header("Oxygen")]
    [Range(0, 100)] public float oxygenLevel = 100f;
    public float oxygenDrainRate = 1.1f;
    public float oxygenDrainRatePowerLoss = 1.6f;

    [Header("Oxygen Capacity")]
    [Range(0, 100)] public float maxOxygenCapacity = 100f;
    
    [Header("Permanent Capacity Loss")]
    public float permanentCapacityLossPerSecond = 2.0f;
    public float postRecoveryCapacityLossPerSecond = 0.5f;
    public float postRecoveryDegradationDuration = 10f;
    public float minOxygenCapacity = 25f;
    
    private float powerOutageDuration = 0f;
    private float postRecoveryTimer = 0f;
    private bool isPostRecoveryDegrading = false;
    
    private float totalPowerOutageTime = 0f;
    public float TotalPowerOutageTime => totalPowerOutageTime;

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

    [Header("Wiring")]
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
        float difficulty = DifficultyMultiplier;

        if (!powerOn)
        {
            totalPowerOutageTime += Time.deltaTime;
            powerOutageDuration += Time.deltaTime;
            
            maxOxygenCapacity = Mathf.Max(minOxygenCapacity, 
                maxOxygenCapacity - permanentCapacityLossPerSecond * Time.deltaTime);
            
            postRecoveryTimer = 0f;
            isPostRecoveryDegrading = false;
        }
        else if (isPostRecoveryDegrading)
        {
            postRecoveryTimer += Time.deltaTime;
            
            if (postRecoveryTimer <= postRecoveryDegradationDuration)
            {
                maxOxygenCapacity = Mathf.Max(minOxygenCapacity, 
                    maxOxygenCapacity - postRecoveryCapacityLossPerSecond * Time.deltaTime);
            }
            else
            {
                isPostRecoveryDegrading = false;
                postRecoveryTimer = 0f;
                powerOutageDuration = 0f;
            }
        }

        oxygenLevel = Mathf.Min(oxygenLevel, maxOxygenCapacity);

        float rise = 0f;
        if (leakActive)
        {
            rise = waterRiseRateLeaking * difficulty;
            if (!powerOn) rise += waterRiseRateNoPower * difficulty;
        }
        else if (waterLevel > waterNormalLevel)
        {
            if (powerOn)
            {
                rise = -waterRecedeRate;
            }
            else
            {
                rise = -waterRecedeRate * 0.3f;
            }
        }
        
        waterLevel = Mathf.Clamp(waterLevel + rise * Time.deltaTime, 0f, 100f);
        
        if (!leakActive && waterLevel < waterNormalLevel) 
            waterLevel = waterNormalLevel;

        float drain = (powerOn ? oxygenDrainRate : oxygenDrainRatePowerLoss) * difficulty;
        oxygenLevel = Mathf.Clamp(oxygenLevel - drain * Time.deltaTime, 0f, maxOxygenCapacity);

        if (!leakActive)
        {
            leakSpawnTimer -= Time.deltaTime;
            if (leakSpawnTimer <= 0f)
            {
                leakSpawnTimer = leakCheckInterval / difficulty;
                if (Random.value < Mathf.Clamp01(leakSpawnChance * difficulty)) TriggerLeak();
            }
        }

        if (powerOn)
        {
            powerFaultTimer -= Time.deltaTime;
            if (powerFaultTimer <= 0f)
            {
                powerFaultTimer = powerFaultCheckInterval / difficulty;
                if (Random.value < Mathf.Clamp01(powerFaultChance * difficulty)) TriggerPowerLoss();
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
        RoomState next;
        if (!powerOn) next = RoomState.PowerLoss;
        else if (leakActive) next = RoomState.HullLeak;
        else if (oxygenLevel < 30f) next = RoomState.Oxygen;
        else next = RoomState.Normal;

        if (next != currentState)
        {
            currentState = next;
            roomAnimator?.SetState(currentState);
        }
    }

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
        
        isPostRecoveryDegrading = false;
        postRecoveryTimer = 0f;
    }

    public void RestorePower()
    {
        powerOn = true;
        powerFaultTimer = powerFaultCheckInterval;
        
        if (powerOutageDuration > 0f)
        {
            isPostRecoveryDegrading = true;
            postRecoveryTimer = 0f;
        }
    }

    public void PumpOxygen(float amount)
    {
        oxygenLevel = Mathf.Clamp(oxygenLevel + amount, 0f, maxOxygenCapacity);
    }

    public float GetTotalCapacityLost()
    {
        return 100f - maxOxygenCapacity;
    }
    
    public float GetCurrentPowerOutageDuration()
    {
        return powerOutageDuration;
    }
    
    public bool IsPostRecoveryDegrading()
    {
        return isPostRecoveryDegrading;
    }
    
    public float GetPostRecoveryTimeRemaining()
    {
        if (!isPostRecoveryDegrading) return 0f;
        return Mathf.Max(0f, postRecoveryDegradationDuration - postRecoveryTimer);
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