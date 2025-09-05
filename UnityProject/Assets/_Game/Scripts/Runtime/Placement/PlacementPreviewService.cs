using System;
using UnityEngine;

using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Core;

namespace _Game.Runtime.Placement
{
    /// <summary>
    /// Visual "ghost" preview during placement.
    /// - UpdateTo(cell): snaps ghost to a specific cell (or hides if null)
    /// - UpdateFromPointer(ray): follows cursor on board plane (optional)
    /// - Tints valid/invalid via PlacementValidator
    /// </summary>
    public sealed class PlacementPreviewService
    {
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly PlacementValidator _validator;
        private readonly Transform _parent;
        private readonly float _lift;

        private GameObject _ghost;
        private Renderer[] _ghostRenderers;
        private CharacterArchetype _current;
        private bool _active;

        private static readonly Color kValid   = new Color(0.35f, 1.00f, 0.35f, 0.55f);
        private static readonly Color kInvalid = new Color(1.00f, 0.35f, 0.35f, 0.55f);

        public PlacementPreviewService(
            CharacterFactory factory,       // kept for future extensions, not required here
            BoardSurface surface,
            GridProjector projector,
            PlacementValidator validator,
            Transform parent,
            float lift = 0.01f)
        {
            _surface   = surface;
            _projector = projector;
            _validator = validator;
            _parent    = parent;
            _lift      = Mathf.Max(0f, lift);
        }

        public bool IsActive => _active && _ghost != null;
        public CharacterArchetype CurrentArchetype => _current;

        /// <summary>Create the ghost for an archetype.</summary>
        public void Begin(CharacterArchetype archetype)
        {
            End(); // cleanup previous
            _current = archetype;
            _active  = true;

            _ghost = CreateGhostInstance(archetype, _parent, out _ghostRenderers);
            if (_ghost) _ghost.SetActive(false);
        }

        /// <summary>Dispose the ghost and stop previewing.</summary>
        public void End()
        {
            _active = false;
            _current = null;

            if (_ghost)
            {
                UnityEngine.Object.Destroy(_ghost);
                _ghost = null;
                _ghostRenderers = null;
            }
        }

        /// <summary>
        /// Snap the ghost to a specific cell (null hides it).
        /// </summary>
        public void UpdateTo(Cell? cell)
        {
            if (!_active || _ghost == null)
                return;

            if (!cell.HasValue)
            {
                _ghost.SetActive(false);
                return;
            }

            var c = cell.Value;
            var valid = _validator.IsValid(c);

            var pos = _projector.CellToWorldCenter(c) + _surface.WorldPlaneNormal * _lift;
            _ghost.transform.position = pos;

            _ghost.SetActive(true);
            TintGhost(valid ? kValid : kInvalid);
        }

        /// <summary>
        /// Optional: follow the cursor on the board plane (for free-move previews).
        /// </summary>
        public void UpdateFromPointer(Ray ray)
        {
            if (!_active || _ghost == null)
                return;

            if (!InputProjectionMath.TryRayPlane(ray, _surface.WorldPlanePoint, _surface.WorldPlaneNormal, out var hit))
            {
                _ghost.SetActive(false);
                return;
            }

            bool valid = _projector.TryWorldToCell(hit, out var derived) && _validator.IsValid(derived);

            var pos = hit + _surface.WorldPlaneNormal * _lift;
            _ghost.transform.position = pos;

            _ghost.SetActive(true);
            TintGhost(valid ? kValid : kInvalid);
        }


        private static GameObject CreateGhostInstance(
            CharacterArchetype archetype,
            Transform parent,
            out Renderer[] renderers)
        {
            GameObject go;
            if (archetype != null && archetype.viewPrefab != null)
            {
                go = UnityEngine.Object.Instantiate(archetype.viewPrefab, parent, worldPositionStays: true);
            }
            else
            {
                go = new GameObject("PlacementGhost");
                go.transform.SetParent(parent, worldPositionStays: true);
            }

            renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);

            foreach (var r in renderers)
            {
                if (!r) continue;
                foreach (var mat in r.materials)
                {
                    if (!mat) continue;
                    SetupMaterialForTransparency(mat);
                }
            }

            return go;
        }

        private void TintGhost(Color c)
        {
            if (_ghostRenderers == null) return;
            for (int i = 0; i < _ghostRenderers.Length; i++)
            {
                var r = _ghostRenderers[i];
                if (!r) continue;

                var mats = r.materials;
                for (int m = 0; m < mats.Length; m++)
                {
                    var mat = mats[m];
                    if (!mat) continue;

                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                    else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                }
            }
        }

        private static void SetupMaterialForTransparency(Material mat)
        {
            if (!mat) return;

            if (mat.HasProperty("_Surface"))            // URP Lit
            {
                mat.SetFloat("_Surface", 1f);          // Transparent
                mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
            }
            else if (mat.HasProperty("_Mode"))         // Standard
            {
                mat.SetFloat("_Mode", 3f);             // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            var baseColor = mat.HasProperty("_Color") ? mat.color :
                            mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") :
                            Color.white;
            baseColor.a = 0.5f;
            if (mat.HasProperty("_Color")) mat.color = baseColor;
            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        }
    }
}
