using UnityEngine;

public class PortalRenderer : MonoBehaviour
{
    private Portal[] _portals;

    private void Awake()
    {
        _portals = FindObjectsByType<Portal>(FindObjectsInactive.Exclude);
    }

    private void LateUpdate()
    {
        foreach (var portal in _portals)
        {
            portal.Render();
        }

        foreach (var portal in _portals)
        {
            portal.PostPortalRender();
        }
    }
}