using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Core;
using UnityEngine;

namespace _Game.Runtime.Characters.Plugins
{
    /// <summary>
    /// COLUMN-BASED movement (no waypoint array).
    /// Enemies spawn at the TOP row and march straight DOWN their current column.
    /// Speed is in blocks/sec (uses BoardGrid.CellSize as block length).
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
        private float _t;           // 0..1 progress along segment
        private float _segmentTime; // secs to traverse one cell at speed

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
            // Snap to current cell center, then prepare first segment downwards.
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

            // Arrived at next cell center
            _e.View.Root.position = _toWorld;

            // Update to the next row (downwards toward base)
            var c = _e.Cell;
            var nextRow = c.Row - 1; // moving toward row 0
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

            var nextRow = c.Row - 1;
            if (nextRow < 0)
            {
                _toWorld = _fromWorld;
                _segmentTime = 0f;
                _t = 0f;
                return;
            }

            _toWorld = _projector.CellToWorldCenter(new Cell(nextRow, c.Col));

            // distance≈one cell; compute time from blocks/sec (use grid cell size as block length)
            var dist = Vector3.Distance(_fromWorld, _toWorld);
            var blocks = Mathf.Max(0.0001f, dist / _grid.CellSize);
            _segmentTime = blocks / _blocksPerSecond;
            _t = 0f;
        }

        private void OnReachedBase()
        {
            // TODO: fire BaseDamagedEvent + proper despawn pipeline if you have one.
            if (_e?.View?.Root)
                Object.Destroy(_e.View.Root.gameObject);
        }
    }
}
