using UnityEngine;

public class AnimationUI : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void TriggerAnimation()
    {
        animator.SetTrigger("AA");
    }
    public void TriggerAnimation2()
    {
        animator.SetTrigger("AAA");
    }
}
