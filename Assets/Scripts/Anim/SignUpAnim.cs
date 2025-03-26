using UnityEngine;

public class SignUpAnim : MonoBehaviour
{
    private Animator signUpAnim;
    
    void Start()
    {
        signUpAnim = GetComponent<Animator>();
    }

    public void SignUp()
    {
        signUpAnim.SetBool("IsClick", true);
        signUpAnim.SetBool("IsClose", false);
    }

    public void IsClose()
    {
        signUpAnim.SetBool("IsClick", false);
        signUpAnim.SetBool("IsClose", true);
    }
}
