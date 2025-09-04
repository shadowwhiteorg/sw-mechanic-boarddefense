using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters.Plugins
{
    /// <summary>Simple waypoint movement for enemies.</summary>
    public sealed class MovementPlugin : IMovement
    {
        private readonly float _speed;
        private readonly Vector3[] _path;
        private int _index;
        private CharacterEntity _e;

        public MovementPlugin(float speed, Vector3[] path)
        {
            _speed = Mathf.Max(0f, speed);
            _path = path ?? new Vector3[0];
        }

        public void OnSpawn(CharacterEntity e) { _e = e; _index = 0; }
        public void OnDespawn() { _e = null; }

        public void Tick(float dt)
        {
            if (_path.Length == 0 || _e?.View == null || _speed <= 0f) return;

            var t = _e.View.Root;
            var target = _path[_index];
            var delta = target - t.position;
            var dist = delta.magnitude;

            if (dist < 0.05f)
            {
                if (_index < _path.Length - 1) _index++;
                return;
            }

            t.position += (delta / dist) * (_speed * dt);
        }
    }
}