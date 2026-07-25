using UnityEngine;

/// <summary>
/// Attach to the breaker/buttons panel hotspot (x=92,y=65,w=95,h=9 in the sprite).
/// When the power fails, click this repeatedly to rewire/restore it before
/// the restore meter decays back down.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerPanel : MonoBehaviour
{
    [Range(0, 100)] public float restoreProgress = 0f;
    public float restorePerClick = 22f;
    public float decayPerSecond = 5f;

    void Update()
    {
        if (GameManager.Instance.powerOn)
        {
            restoreProgress = 0f;
            return;
        }

        restoreProgress -= decayPerSecond * Time.deltaTime;
        restoreProgress = Mathf.Clamp(restoreProgress, 0f, 100f);

        if (restoreProgress >= 100f)
        {
            GameManager.Instance.RestorePower();
            restoreProgress = 0f;
        }
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.powerOn) return;
        restoreProgress += restorePerClick;
    }

    public void Reset()
    {
        restoreProgress = 0f;
    }
}
