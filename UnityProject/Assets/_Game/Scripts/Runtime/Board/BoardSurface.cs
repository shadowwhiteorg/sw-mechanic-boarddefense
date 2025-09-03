using UnityEngine;

namespace _Game.Runtime.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardSurface : MonoBehaviour
    {
        [Header("Grid Layout (local space)")]
        [Min(1)] public int rows = 4;
        [Min(1)] public int cols = 8;
        [Min(0.01f)] public float cellSize = 1f;

        [Tooltip("Local-space bottom-left corner of Cell(0,0). Usually (0,0,0).")]
        public Vector3 localOrigin = Vector3.zero;

        /// <summary>World-space plane point (origin) & normal.</summary>
        public Vector3 WorldPlanePoint => transform.TransformPoint(localOrigin);
        public Vector3 WorldPlaneNormal => transform.up; // X/Z board => Y is normal

        public Vector3 LocalToWorld(in Vector3 local) => transform.TransformPoint(local);
        public Vector3 WorldToLocal(in Vector3 world) => transform.InverseTransformPoint(world);

        public float TotalWidth  => cols * cellSize;
        public float TotalHeight => rows * cellSize;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw outer rectangle
            var bl = LocalToWorld(localOrigin);
            var br = LocalToWorld(localOrigin + new Vector3(TotalWidth, 0, 0));
            var tl = LocalToWorld(localOrigin + new Vector3(0, 0, TotalHeight));
            var tr = LocalToWorld(localOrigin + new Vector3(TotalWidth, 0, TotalHeight));

            Gizmos.color = Color.white * 0.9f;
            Gizmos.DrawLine(bl, br); Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl); Gizmos.DrawLine(tl, bl);

            // Grid lines + placeable mask (green bottom half, red top half)
            for (int r = 0; r <= rows; r++)
            {
                float z = r * cellSize;
                var a = LocalToWorld(localOrigin + new Vector3(0, 0, z));
                var b = LocalToWorld(localOrigin + new Vector3(TotalWidth, 0, z));
                Gizmos.color = r <= rows / 2 ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
                Gizmos.DrawLine(a, b);
            }
            Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
            for (int c = 0; c <= cols; c++)
            {
                float x = c * cellSize;
                var a = LocalToWorld(localOrigin + new Vector3(x, 0, 0));
                var b = LocalToWorld(localOrigin + new Vector3(x, 0, TotalHeight));
                Gizmos.DrawLine(a, b);
            }
        }
#endif
    }
}
