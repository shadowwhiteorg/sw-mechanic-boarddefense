using UnityEngine;
using _Game.Core;                
using _Game.Core.Events;         
using _Game.Interfaces;          
using _Game.Runtime.Board;       
using _Game.Runtime.Core;        

namespace _Game.Runtime.Characters.Plugins
{
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
        private bool _baseReached;

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
            _baseReached = false;

            var root = RootOrNull();
            if (!root) return;

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
            if (_e == null || _baseReached || _blocksPerSecond <= 0f) return;

            var root = RootOrNull();
            if (!root) return;

            if (_segmentTime <= 0f)
            {
                
                PrepareNextSegment();
                if (_baseReached) return;
                if (_segmentTime <= 0f) return;
            }

            _t += dt / _segmentTime;

            if (_t < 1f)
            {
                root.position = Vector3.Lerp(_fromWorld, _toWorld, _t);
                return;
            }

            root.position = _toWorld;

            var c = _e.Cell;
            int nextRow = c.Row - 1; 

            if (nextRow < 0)
            {
                FireBaseReached();
                return;
            }

            var nextCell = new Cell(nextRow, c.Col);
            _repo.Remove(_e);
            _e.Cell = nextCell;
            _repo.Add(_e, nextCell);

            PrepareNextSegment();
        }

        private void PrepareNextSegment()
        {
            var root = RootOrNull();
            if (!root) { _segmentTime = 0f; _t = 0f; return; }

            var c = _e.Cell;
            _fromWorld = root.position;

            int nextRow = c.Row - 1;
            if (nextRow < 0)
            {
                _toWorld = _fromWorld;
                _segmentTime = 0f;
                _t = 0f;
                FireBaseReached();
                return;
            }

            _toWorld = _projector.CellToWorldCenter(new Cell(nextRow, c.Col));

            float dist = Vector3.Distance(_fromWorld, _toWorld);
            float blocks = Mathf.Max(0.0001f, dist / _grid.CellSize);
            _segmentTime = blocks / _blocksPerSecond;
            _t = 0f;
        }

        private void FireBaseReached()
        {
            if (_baseReached) return;
            _baseReached = true;

            if (_e != null)
                GameContext.Events?.Fire(new EnemyReachedBaseEvent(_e.EntityId));
        }

        private Transform RootOrNull()
        {
            if (_e == null) return null;
            var view = _e.View;
            if (view == null) return null; 
            var root = view.Root;
            return root ? root : null;
        }
    }
}
