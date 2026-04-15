using UnityEngine;

namespace Portals.Utilities
{
    public static class PortalUtility
    {
        private static readonly Vector3[] CubeCornerOffsets = {
            new(1, 1, 1),
            new(-1, 1, 1),
            new(-1, -1, 1),
            new(-1, -1, -1),
            new(-1, 1, -1),
            new(1, -1, -1),
            new(1, 1, -1),
            new(1, -1, 1),
        };
        
        /// <summary>
        /// Checks if the portal's screen is visible from the camera
        /// </summary>
        /// <param name="screen">The portal's screen renderer</param>
        /// <param name="cam">The camera to check the visibility for</param>
        /// <returns>True if the portal's screen is visible from the camera</returns>
        public static bool IsVisibleFromCamera(Renderer screen, Camera cam) {
            var frustumPlanes = GeometryUtility.CalculateFrustumPlanes (cam);
            return GeometryUtility.TestPlanesAABB (frustumPlanes, screen.bounds);
        }

        /// <summary>
        /// Determines whether the bounds of two objects overlap when projected onto the camera's screen space.
        /// </summary>
        /// <param name="nearObject">The mesh filter of the near object to check.</param>
        /// <param name="farObject">The mesh filter of the far object to check.</param>
        /// <param name="camera">The camera used to project the objects' bounds onto screen space.</param>
        /// <returns>True if the objects' bounds overlap in screen space; otherwise, false.</returns>
        public static bool BoundsOverlap(MeshFilter nearObject, MeshFilter farObject, Camera camera)
        {
            var near = GetScreenRectFromBounds(nearObject, camera);
            var far = GetScreenRectFromBounds(farObject, camera);

            if (far.ZMax > near.ZMin)
            {
                if (far.XMax < near.XMin || far.XMin > near.XMax) return false;
                
                if (far.YMax < near.YMin || far.YMin > near.YMax) return false;
                
                return true;
            }

            return false;
        }
        
        private static MinMax3D GetScreenRectFromBounds (MeshFilter renderer, Camera mainCamera) {
            MinMax3D minMax = new MinMax3D (float.MaxValue, float.MinValue);

            var localBounds = renderer.sharedMesh.bounds;
            bool anyPointIsInFrontOfCamera = false;

            for (int i = 0; i < 8; i++) {
                Vector3 localSpaceCorner = localBounds.center + Vector3.Scale (localBounds.extents, CubeCornerOffsets[i]);
                Vector3 worldSpaceCorner = renderer.transform.TransformPoint (localSpaceCorner);
                Vector3 viewportSpaceCorner = mainCamera.WorldToViewportPoint (worldSpaceCorner);

                if (viewportSpaceCorner.z > 0) {
                    anyPointIsInFrontOfCamera = true;
                } else {
                    // If point is behind camera, it gets flipped to the opposite side
                    // So clamp to opposite edge to correct for this
                    viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                    viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
                }

                // Update bounds with new corner point
                minMax.AddPoint (viewportSpaceCorner);
            }

            // All points are behind camera so just return empty bounds
            if (!anyPointIsInFrontOfCamera) {
                return new MinMax3D ();
            }

            return minMax;
        }
    }
}