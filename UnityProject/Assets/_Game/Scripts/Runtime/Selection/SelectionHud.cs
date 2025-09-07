using UnityEngine;
using _Game.Runtime.Levels;
using _Game.Runtime.Characters.Config;
using TMPro;
using TMPro;
using UnityEngine.UI;

namespace _Game.Runtime.Selection
{
    public sealed class SelectionSlotHud : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI remainingText;
        [SerializeField] private TextMeshProUGUI statsText;

        private CharacterArchetype _archetype;
        private LevelRuntimeConfig _level;

        public void Initialize(CharacterArchetype archetype, LevelRuntimeConfig level, Transform anchor = null, float yOffsetWorld = 0, Camera cam = null)
        {
            _archetype = archetype;
            _level = level;
            RefreshAll();
        }

        public void RefreshAll()
        {
            if (!_archetype || _level == null) return;

            int remaining = _level.GetDefenseRemaining(_archetype);
            SetRemaining($"x{remaining}");

            var stats = BuildStatsLine(_archetype);
            SetStats(stats);
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

            try
            {
                // Range
                var weaponCfg = a.weapon;
                if (weaponCfg != null)
                {
                    range = weaponCfg.rangeBlocks;
                    if (weaponCfg.projectileConfig != null)
                        damage = Mathf.Max(damage, weaponCfg.projectileConfig.damage);
                }

                // Direction
                var ad = a.weapon.attackDirection;
                dir = ad.ToString();
            }
            catch
            {
                // Fallback
            }

            return $"R: {range}  D: {damage}  Dir: {dir}";
        }
    }
}
