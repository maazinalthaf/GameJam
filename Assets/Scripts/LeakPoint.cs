using UnityEngine;

/// <summary>
/// Attach to the leak hotspot GameObject (a BoxCollider2D covering roughly
/// x=108,y=42,w=12,h=42 in the 250x120 sprite - see SETUP.md).
/// Player mashes/holds click on it to fill the patch meter before it decays.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LeakPoint : MonoBehaviour
{
    [Range(0, 100)] public float patchProgress = 0f;
    public float patchPerClick = 16f;
    public float decayPerSecond = 7f;
    public SpriteRenderer warningIcon; // optional flashing icon while active

    void Update()
    {
        if (!GameManager.Instance.leakActive)
        {
            if (warningIcon) warningIcon.enabled = false;
            return;
        }

        if (warningIcon) warningIcon.enabled = true;

        patchProgress -= decayPerSecond * Time.deltaTime;
        patchProgress = Mathf.Clamp(patchProgress, 0f, 100f);

        if (patchProgress >= 100f)
        {
            GameManager.Instance.PatchLeak();
            patchProgress = 0f;
        }
    }

    void OnMouseDown()
    {
        if (!GameManager.Instance.leakActive) return;
        patchProgress += patchPerClick;
    }

    public void Reset()
    {
        patchProgress = 0f;
    }
}
