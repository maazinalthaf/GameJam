using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OxygenGaugeDisplay : MonoBehaviour
{
    public Sprite[] frames;

    [Tooltip("If checked, frame 0 = full oxygen and the last frame = empty. Uncheck to reverse.")]
    public bool fullAtFrameZero = true;

    [Tooltip("Remaps oxygen % (0-1, X axis) to how far through the frame " +
             "sequence to display (0-1, Y axis). The needle's hand-drawn " +
             "sweep usually isn't perfectly linear, so if e.g. 50% oxygen " +
             "visually looks like 80% on the dial, drag this curve so the " +
             "point at X=0.5 sits at the Y value that actually looks right, " +
             "instead of assuming a straight line.")]
    public AnimationCurve responseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || frames == null || frames.Length == 0) return;

        float raw = Mathf.Clamp01(gm.oxygenLevel / 100f);
        float t = Mathf.Clamp01(responseCurve.Evaluate(raw));
        if (!fullAtFrameZero) t = 1f - t;

        int index = Mathf.Clamp(Mathf.RoundToInt(t * (frames.Length - 1)), 0, frames.Length - 1);
        sr.sprite = frames[index];
    }
}