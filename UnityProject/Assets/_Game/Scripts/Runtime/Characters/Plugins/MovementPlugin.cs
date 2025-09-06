// Assets/_Game/Scripts/Runtime/Characters/Plugins/MovementPlugin.cs
using UnityEngine;
using _Game.Core;                // GameContext
using _Game.Core.Events;         // EnemyReachedBaseEvent
using _Game.Interfaces;          // IMovement
using _Game.Runtime.Board;       // BoardGrid, BoardSurface, GridProjector
using _Game.Runtime.Characters;  // CharacterEntity, CharacterRepository
using _Game.Runtime.Core;        // Cell

namespace _Game.Runtime.Characters.Plugins
{
    /// Moves an enemy one cell per segment toward the base (row-- each step).
    /// - Keeps CharacterRepository's cell index in sync on every step.
    /// - On reaching base, fires EnemyReachedBaseEvent (BaseHealthSystem handles HP & despawn).
    /// - Robust to destroyed views (null-guards before any Transform access).
    public sealed class MovementPlugin : IMovement
    {
        private readonly BoardGrid _grid;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly CharacterRepository _repo;
        private readonly float _blocksPerSecond;

        private CharacterEntity _e;
        private Vector3 _fromWorld;
        private Vector3 _toWorld;
        private float _t;
        private float _segmentTime;

        public MovementPlugin(BoardGrid grid,
                              BoardSurface surface,
                              GridProjector projector,
                              float blocksPerSecond,
                              CharacterRepository repo)
        {
            _grid = grid;
            _surface = surface;
            _projector = projector;
            _repo = repo;
            _blocksPerSecond = Mathf.Max(0f, blocksPerSecond);
        }

        public void OnSpawn(CharacterEntity e)
        {
            _e = e;

            var root = RootOrNull();
            if (root == null) return;

            root.position = _projector.CellToWorldCenter(_e.Cell);
            _t = 0f;
            PrepareNextSegment();
        }

        public void OnDespawn()
        {
            _e = null;
        }

        public void Tick(float dt)
        {
            if (_e == null || _blocksPerSecond <= 0f || _segmentTime <= 0f)
                return;

            var root = RootOrNull();
            if (root == null) return; // view might have been destroyed by lifetime system

            _t += dt / _segmentTime;

            if (_t < 1f)
            {
                root.position = Vector3.Lerp(_fromWorld, _toWorld, _t);
                return;
            }

            // Snap to the target cell center of this segment.
            root.position = _toWorld;

            var c = _e.Cell;
            int nextRow = c.Row - 1; // enemies march downward (toward base)

            if (nextRow < 0)
            {
                OnReachedBase();
                return;
            }

            // Update logical cell and repository index.
            var nextCell = new Cell(nextRow, c.Col);

            // Keep repo mapping consistent (Remove+Add keeps API requirements minimal).
            _repo.Remove(_e);
            _e.Cell = nextCell;
            _repo.Add(_e, nextCell);

            PrepareNextSegment();
        }

        private void PrepareNextSegment()
        {
            var root = RootOrNull();
            if (root == null) { _segmentTime = 0f; _t = 0f; return; }

            var c = _e.Cell;
            _fromWorld = root.position;

            int nextRow = c.Row - 1;
            if (nextRow < 0)
            {
                _toWorld = _fromWorld;
                _segmentTime = 0f;
                _t = 0f;
                return;
            }

            _toWorld = _projector.CellToWorldCenter(new Cell(nextRow, c.Col));

            float dist = Vector3.Distance(_fromWorld, _toWorld);
            float blocks = Mathf.Max(0.0001f, dist / _grid.CellSize);
            _segmentTime = blocks / _blocksPerSecond;
            _t = 0f;
        }

        private void OnReachedBase()
        {
            // Let BaseHealthSystem handle HP and CharacterLifetimeSystem handle despawn.
            if (_e != null)
                GameContext.Events?.Fire(new EnemyReachedBaseEvent(_e.EntityId));
        }

        /// Safe access to the entity's Transform. Returns null if the view is gone.
        private Transform RootOrNull()
        {
            if (_e == null) return null;
            var view = _e.View;
            if (view == null) return null;      // Unity destroyed component compares == null
            var root = view.Root;               // CharacterView.Root returns transform
            if (root == null) return null;      // safety in case the GO is destroyed
            return root;
        }
    }
}
