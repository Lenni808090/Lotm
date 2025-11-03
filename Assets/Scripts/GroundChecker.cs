using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private bool grounded;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = false;
        }
    }

    public bool isGrounded()
    {
        return grounded;
    }

}
