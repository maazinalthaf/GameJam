using UnityEngine;

/// <summary>
/// Attach to the oxygen gauge hotspot (x=138,y=52,w=13,h=13 in the sprite).
/// Click it to manually crank the air pump - gives the player a small,
/// always-available way to buy oxygen back at the cost of attention.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OxygenSystem : MonoBehaviour
{
    public float pumpBoost = 5f;
    public float pumpCooldown = 0.35f;
    float cooldownTimer = 0f;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    void OnMouseDown()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = pumpCooldown;
        GameManager.Instance.PumpOxygen(pumpBoost);
    }
}
