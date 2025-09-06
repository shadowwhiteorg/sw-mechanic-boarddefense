// Assets/_Game/Scripts/Runtime/Characters/CharacterSystem.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;

namespace _Game.Runtime.Characters
{
    /// Ticks character plugins. Robust against views destroyed mid-frame.
    public sealed class CharacterSystem : IUpdatableSystem
    {
        private readonly List<CharacterEntity> _entities = new List<CharacterEntity>(128);
        private readonly HashSet<int> _pendingRemove = new HashSet<int>(64);
        private bool _isIterating;

        public void Register(CharacterEntity e)
        {
            if (e == null) return;
            if (_entities.Contains(e)) return;

            _entities.Add(e);

            // Spawn lifecycle
            var plugins = e.Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].OnSpawn(e);
        }

        public void Unregister(CharacterEntity e)
        {
            if (e == null) return;

            if (_isIterating)
            {
                _pendingRemove.Add(e.EntityId);
                return;
            }

            // Despawn lifecycle
            var plugins = e.Plugins;
            for (int i = 0; i < plugins.Count; i++)
            {
                try { plugins[i].OnDespawn(); }
                catch (System.Exception ex) { Debug.LogException(ex); }
            }

            _entities.Remove(e);
        }

        public void Tick()
        {
            _isIterating = true;

            float dt = Time.deltaTime;

            for (int i = 0; i < _entities.Count; i++)
            {
                var e = _entities[i];
                if (e == null) continue;

                // If the view or its transform is gone, schedule removal and skip.
                var view = e.View;
                if (view == null || view.Root == null)
                {
                    _pendingRemove.Add(e.EntityId);
                    continue;
                }

                var plugins = e.Plugins;
                for (int p = 0; p < plugins.Count; p++)
                {
                    try { plugins[p].Tick(dt); }
                    catch (System.Exception ex) { Debug.LogException(ex); }
                }
            }

            _isIterating = false;

            // Process any removals requested during iteration
            if (_pendingRemove.Count > 0)
            {
                for (int i = _entities.Count - 1; i >= 0; i--)
                {
                    var e = _entities[i];
                    if (e != null && _pendingRemove.Contains(e.EntityId))
                    {
                        // Run despawn lifecycle now
                        var plugins = e.Plugins;
                        for (int p = 0; p < plugins.Count; p++)
                        {
                            try { plugins[p].OnDespawn(); }
                            catch (System.Exception ex) { Debug.LogException(ex); }
                        }
                        _entities.RemoveAt(i);
                    }
                }
                _pendingRemove.Clear();
            }
        }
    }
}
