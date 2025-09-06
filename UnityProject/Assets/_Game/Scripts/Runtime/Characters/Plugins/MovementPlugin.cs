using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters.Plugins
{
    /// <summary>
    /// COLUMN-BASED movement with no waypoints.
    /// Enemies spawn on the TOP row and march straight DOWN their current column.
    /// Speed is in blocks/second (uses BoardGrid.CellSize as the size of one "block").
    /// </summary>
    public sealed class MovementPlugin : IMovement
    {
        private readonly BoardGrid _grid;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly float _blocksPerSecond;

        private CharacterEntity _e;
        private Vector3 _fromWorld;
        private Vector3 _toWorld;
        private float _t;           // 0..1 progress along the current cell-to-cell segment
        private float _segmentTime; // seconds to traverse one grid cell at current speed

        public MovementPlugin(BoardGrid grid, BoardSurface surface, GridProjector projector, float blocksPerSecond)
        {
            _grid = grid;
            _surface = surface;
            _projector = projector;
            _blocksPerSecond = Mathf.Max(0f, blocksPerSecond);
        }

        public void OnSpawn(CharacterEntity e)
        {
            _e = e;
            // Snap to the current cell center, then prepare the first step.
            var c = _e.Cell;
            _e.View.Root.position = _projector.CellToWorldCenter(c);
            PrepareNextSegment();
        }

        public void OnDespawn()
        {
            _e = null;
        }

        public void Tick(float dt)
        {
            if (_e == null || _blocksPerSecond <= 0f) return;

            if (_segmentTime <= 0f)
                return;

            _t += dt / _segmentTime;

            if (_t < 1f)
            {
                _e.View.Root.position = Vector3.Lerp(_fromWorld, _toWorld, _t);
                return;
            }

            // Arrived at next cell center.
            _e.View.Root.position = _toWorld;

            // Move logical cell down by one row (toward the base).
            var c = _e.Cell;
            int nextRow = c.Row - 1;
            if (nextRow < 0)
            {
                OnReachedBase();
                return;
            }

            _e.Cell = new Cell(nextRow, c.Col);
            PrepareNextSegment();
        }

        private void PrepareNextSegment()
        {
            var c = _e.Cell;
            _fromWorld = _e.View.Root.position;

            int nextRow = c.Row - 1;
            if (nextRow < 0)
            {
                _toWorld = _fromWorld;
                _segmentTime = 0f;
                _t = 0f;
                return;
            }

            _toWorld = _projector.CellToWorldCenter(new Cell(nextRow, c.Col));

            // Distance between cell centers is ~one cell; compute time from blocks/sec.
            float dist = Vector3.Distance(_fromWorld, _toWorld);
            float blocks = Mathf.Max(0.0001f, dist / _grid.CellSize);
            _segmentTime = blocks / _blocksPerSecond;
            _t = 0f;
        }

        private void OnReachedBase()
        {
            // TODO: Fire a BaseDamagedEvent (preferred) then despawn via lifetime system.
            // Minimal fallback: destroy the view so it disappears.
            if (_e?.View?.Root)
                Object.Destroy(_e.View.Root.gameObject);
        }
    }
}
