using UnityEngine;

namespace Portals.Travellers
{
    [DisallowMultipleComponent]
    public sealed class PlayerTraveller : PortalTraveller
    {
        private CharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public override void Teleport(Vector3 pos, Quaternion rot)
        {
            _characterController.enabled = false;
            
            base.Teleport(pos, rot);
            
            _characterController.enabled = true;
        }
    }
}