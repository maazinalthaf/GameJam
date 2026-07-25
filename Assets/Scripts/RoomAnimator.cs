using UnityEngine;

/// <summary>
/// Sits on the same GameObject as the room's Animator component.
/// The Animator Controller should have an Int parameter called "State"
/// with 4 states (Normal=0, Oxygen=1, HullLeak=2, PowerLoss=3), each
/// playing the matching animation clip built from the Sprites/ folders.
/// See SETUP.md for exact steps.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RoomAnimator : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetState(RoomState state)
    {
        animator.SetInteger("State", (int)state);
    }
}
