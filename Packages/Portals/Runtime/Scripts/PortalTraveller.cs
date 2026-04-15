using UnityEngine;

[DisallowMultipleComponent]
public class PortalTraveller : MonoBehaviour
{
    public Vector3 PreviousOffsetFromPortal { get; set; }

    public virtual void Teleport(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
    }
        
    public void EnterPortalThreshold() {}
    public void ExitPortalThreshold() {}
}