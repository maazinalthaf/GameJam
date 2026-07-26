using UnityEngine;

/// <summary>
/// Drop this on a SpriteRenderer child and drag a folder's worth of frames
/// into the Frames array (select all PNGs in e.g. Assets/Sprites/Layers/
/// pipe_leak_drip/ and drag them all into the array slot at once - Unity
/// will fill every element for you). Loops automatically while enabled.
/// No Animator Controller required.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WarningIconPlayer : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 12f;

    SpriteRenderer sr;
    int index;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        index = 0;
        timer = 0f;
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

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
