// Assets/_Game/Scripts/Runtime/Selection/SelectionSlotHud.cs
using UnityEngine;
using _Game.Runtime.Levels;
using _Game.Runtime.Characters.Config;
using TMPro;

#if TMP_PRESENT
using TMPro;
#endif
using UnityEngine.UI;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Worldspace HUD for a single selection slot (archetype).
    /// - Displays Remaining count from LevelRuntimeConfig.
    /// - Displays range, damage and attack direction taken from the archetype config.
    /// - Anchored to a Transform (slotAnchor) at the lineup position with a Y offset.
    /// - Billboards to face the camera (optional toggle).
    /// </summary>
    public sealed class SelectionSlotHud : MonoBehaviour
    {
        // [Header("Bindings (assign any that you use)")]
        // [SerializeField] private Transform slotAnchor;
        // [SerializeField] private float yOffset = 0.2f;
        // [SerializeField] private bool billboard = true;
        // [SerializeField] private Camera faceCamera;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI remainingText;
        [SerializeField] private TextMeshProUGUI statsText;

        private CharacterArchetype _archetype;
        private LevelRuntimeConfig _level;

        public void Initialize(CharacterArchetype archetype, LevelRuntimeConfig level, Transform anchor = null, float yOffsetWorld = 0, Camera cam = null)
        {
            _archetype = archetype;
            _level = level;
            // slotAnchor = anchor;
            // yOffset = yOffsetWorld;
            // faceCamera = cam ?? Camera.main;

            RefreshAll();
        }

        public void RefreshAll()
        {
            if (!_archetype || _level == null) return;

            // Remaining (refill stock left)
            int remaining = _level.GetDefenseRemaining(_archetype);
            SetRemaining($"x{remaining}");

            // Stats
            var stats = BuildStatsLine(_archetype);
            SetStats(stats);
        }

        private void LateUpdate()
        {
            // if (!slotAnchor) return;
            // // Anchor + Y offset
            // var p = slotAnchor.position;
            // p.y += yOffset;
            // transform.position = p;
            //
            // // Face camera if desired
            // if (billboard && faceCamera)
            // {
            //     var fwd = (transform.position - faceCamera.transform.position).normalized;
            //     if (fwd.sqrMagnitude > 0.0001f)
            //         transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            // }
        }

        private void SetRemaining(string text)
        {
            if (remainingText != null) remainingText.text = text;
        }

        private void SetStats(string text)
        {
            if (statsText != null) statsText.text = text;
        }
        
        private static string BuildStatsLine(CharacterArchetype a)
        {
            int range = 0;
            int damage = 0;
            string dir = "Forward";

            // Try to read typical fields if present (defensive null checks).
            // Adjust property names if yours differ.
            try
            {
                // Range
                var weaponCfg = a.weapon;                       // e.g., public WeaponConfig weaponConfig
                if (weaponCfg != null)
                {
                    range = weaponCfg.rangeBlocks;                    // e.g., int rangeBlocks
                    // Damage: projectile or hitscan
                    if (weaponCfg.projectileConfig != null)
                        damage = Mathf.Max(damage, weaponCfg.projectileConfig.damage);
                }

                // Direction
                // e.g., enum AttackDirection { Forward, Omni }
                var ad = a.attackDirection;
                dir = ad.ToString();
            }
            catch
            {
                // Fallback if your archetype structure differs; customize as needed.
            }

            return $"R: {range}  D: {damage}  Dir: {dir}";
        }
    }
}
