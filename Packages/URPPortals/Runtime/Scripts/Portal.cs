using System;
using System.Collections.Generic;
using UnityEngine;
using URPPortals.Runtime.Scripts.Utilities;

namespace URPPortals.Runtime.Scripts
{
    [DisallowMultipleComponent]
    public sealed class Portal : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int DisplayMask = Shader.PropertyToID("displayMask");
        
        [Header("Main Settings")]
        public Portal linkedPortal;
        [SerializeField, Tooltip("Maximum recursions before rendering the inactive color.")] 
        private int maxRecursions = 5;
        private MeshRenderer _screen;
        private MeshFilter _screenFilter;

        [Header("Advanced Settings")] 
        [SerializeField] private float nearClipOffset = 0.05f;
        [SerializeField] private float nearClipLimit = 0.2f;

        private List<PortalTraveller> _trackedTravellers;
        
        private Camera _playerCam;
        private Camera _portalCam;
        private RenderTexture _viewTexture;

        private void Awake()
        {
            _playerCam = Camera.main;
            
            _portalCam = GetComponentInChildren<Camera>();
            _portalCam.enabled = false;
            
            _screen = GetComponentInChildren<MeshRenderer>();
            _screenFilter = GetComponentInChildren<MeshFilter>();
            
            _trackedTravellers = new List<PortalTraveller>();
        }

        private void LateUpdate()
        {
            HandleTravellers();
        }
        
        private void HandleTravellers()
        {
            for (var i = 0; i < _trackedTravellers.Count; i++)
            {
                var traveller = _trackedTravellers[i];

                // Check for the traveller existence
                if (!traveller) continue;
                
                var travellerT = traveller.transform;
                var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix *
                        travellerT.localToWorldMatrix;

                var offsetFromPortal = travellerT.position - transform.position;
                var portalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
                var portalSideOld = Math.Sign(Vector3.Dot(traveller.PreviousOffsetFromPortal, transform.forward));

                if (portalSide != portalSideOld)
                {
                    var positionOld = travellerT.position;
                    var rotOld = travellerT.rotation;
                    traveller.Teleport( m.GetColumn(3), m.rotation);
                    // traveller.graphicsClone.transform.SetPositionAndRotation (positionOld, rotOld);
                    linkedPortal.OnTravellerEnterPortal(traveller);
                    _trackedTravellers.RemoveAt(i);
                    i--;
                }
                else
                {
                    // traveller.graphicsClone.transform.SetPositionAndRotation (m.GetColumn(3), m.rotation);
                    traveller.PreviousOffsetFromPortal = offsetFromPortal;
                }
            }
        }

        private void CreateViewTexture()
        {
            if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
            {
                if (_viewTexture != null) _viewTexture.Release();
                
                _viewTexture = new RenderTexture(Screen.width, Screen.height, 24);
                
                _portalCam.targetTexture = _viewTexture;
                linkedPortal._screen.material.SetTexture(MainTex, _viewTexture);
            }
        }

        /// <summary>
        /// Renders the portal
        /// </summary>
        public void Render()
        {
            if (!PortalUtility.IsVisibleFromCamera(linkedPortal._screen, _playerCam)) return;
            
            CreateViewTexture();
            
            var localToWorldMatrix = _playerCam.transform.localToWorldMatrix;
            var renderPositions = new Vector3[maxRecursions];
            var renderRotations = new Quaternion[maxRecursions];

            int startIndex = 0;
            _portalCam.projectionMatrix = _playerCam.projectionMatrix;

            for (int i = 0; i < maxRecursions; i++)
            {
                if (i > 0)
                {
                    if (!PortalUtility.BoundsOverlap(_screenFilter, linkedPortal._screenFilter, _portalCam)) break;
                }
                
                localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
                int renderOrderIndex = maxRecursions - i - 1;
                renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
                renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;
                
                _portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
                startIndex = renderOrderIndex;
            }
            
            _screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            linkedPortal._screen.material.SetInt(DisplayMask, 0);

            for (int i = startIndex; i < maxRecursions; i++)
            {
                _portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
                SetNearClipPlane();

                try
                {
                    _portalCam.Render();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                
                if (i == startIndex) linkedPortal._screen.material.SetInt(DisplayMask, 1);
            }
            
            _screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        public void PostPortalRender()
        {
            ProtectScreenFromClipping();
        }

        private void SetNearClipPlane()
        {
            var clipPlane = transform;
            var dot = Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - _portalCam.transform.position));

            var camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
            var camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
            var camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + nearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs (camSpaceDst) > nearClipLimit) {
                var clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                _portalCam.projectionMatrix = _playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
            } else {
                _portalCam.projectionMatrix = _playerCam.projectionMatrix;
            }
        }

        private void ProtectScreenFromClipping()
        {
            var halfHeight = _playerCam.nearClipPlane * Mathf.Tan (_playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var halfWidth = halfHeight * _playerCam.aspect;
            var dstToNearClipPlaneCorner = new Vector3 (halfWidth, halfHeight, _playerCam.nearClipPlane).magnitude;

            var screenT = _screen.transform;
            
            var camFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - _playerCam.transform.position) > 0;
            screenT.localScale = new Vector3 (screenT.localScale.x, screenT.localScale.y, dstToNearClipPlaneCorner);
            screenT.localPosition = Vector3.forward * (dstToNearClipPlaneCorner * (camFacingSameDirAsPortal ? 0.5f : -0.5f));
        }

        private void OnTravellerEnterPortal(PortalTraveller traveller)
        {
            if (_trackedTravellers.Contains(traveller)) return;
            
            traveller.EnterPortalThreshold();
            traveller.PreviousOffsetFromPortal = traveller.transform.position - transform.position;
            _trackedTravellers.Add(traveller);
        }

        private void OnTriggerEnter(Collider other)
        {
            var traveller = other.GetComponent<PortalTraveller>();
            if (traveller) OnTravellerEnterPortal(traveller);
        }

        private void OnTriggerExit(Collider other)
        {
            var traveller = other.GetComponent<PortalTraveller>();
            if (traveller&& _trackedTravellers.Contains(traveller))
            {
                traveller.ExitPortalThreshold();
                _trackedTravellers.Remove(traveller);
            }
        }
    }
}