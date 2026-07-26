using UnityEngine;

/// <summary>
/// Drop on a SpriteRenderer child, drag a folder's worth of PNGs into
/// Frames (or use the "Load Frames From Folder" button added by
/// FramePlayerEditor.cs). Loops continuously while visible. Call
/// SetVisible(false) to hide + pause, SetVisible(true) to show + resume
/// from frame 0.
///
/// Used for: pipe leak drip, power-loss warning vignette. For the oxygen
/// gauge, use OxygenGaugeDisplay.cs instead - that one reflects the real
/// oxygen value rather than looping on its own.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FramePlayer : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 12f;

    SpriteRenderer sr;
    int index;
    float timer;
    bool visible;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetVisible(false);
    }

    public void SetVisible(bool show)
    {
        if (show == visible) return;
        visible = show;
        sr.enabled = show;
        if (show)
        {
            index = 0;
            timer = 0f;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        }
    }

    void Update()
    {
        if (!visible || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        if (timer >= frameTime)
        {
            timer -= frameTime;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
