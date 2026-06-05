using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public class WorldManager : MonoBehaviour
    {
        private const int GRID_SIZE = 3;
        
        [SerializeField] private Chunk chunkPrefab;
        [SerializeField] private CharacterController _player;
        [SerializeField] private int WorldSeed = 12345;
        
        private Dictionary<Vector2Int, Chunk> _chunks = new();
        private Vector2Int _currentChunk;
        private GameObject _grid;
        
        private void Awake()
        {
            _grid = transform.Find("Grid").gameObject;
        }

        private void Start()
        {
            GenerateGrid();
        }

        private void Update()
        {
            var playerPos = _player.transform.position;
            var chunkId = GetChunkIdByPosition(playerPos);

            if (_currentChunk.x != chunkId.x || _currentChunk.y != chunkId.y)
                GenerateNewChunks(chunkId);
        }

        private void GenerateGrid()
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Chunk chunk = SpawnChunk();
                    chunk.Initialize(WorldSeed, x, z);
                    _chunks.Add(new Vector2Int(x, z), chunk);
                }
            }

            _currentChunk = new(0, 0);
        }
        
        private Chunk SpawnChunk()
        {
            return Instantiate(
                chunkPrefab,
                _grid.transform
            );
        }

        private Vector2Int GetChunkIdByPosition(Vector3 position)
        {
            var x = Mathf.FloorToInt(position.x / Chunk.WIDTH);
            var z = Mathf.FloorToInt(position.z / Chunk.HEIGHT);

            return new Vector2Int(x, z);
        }
        
        private void GenerateNewChunks(Vector2Int centerChunk)
        {
            HashSet<Vector2Int> requiredChunks = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    requiredChunks.Add(
                        centerChunk + new Vector2Int(x, z)
                    );
                }
            }

            List<Vector2Int> missingChunks = new();
            List<Vector2Int> reusableChunks = new();

            foreach (var chunkId in requiredChunks)
            {
                if (!_chunks.ContainsKey(chunkId))
                {
                    missingChunks.Add(chunkId);
                }
            }

            foreach (var chunkId in _chunks.Keys)
            {
                if (!requiredChunks.Contains(chunkId))
                {
                    reusableChunks.Add(chunkId);
                }
            }

            for (int i = 0; i < missingChunks.Count; i++)
            {
                Vector2Int oldId = reusableChunks[i];
                Vector2Int newId = missingChunks[i];

                Chunk chunk = _chunks[oldId];

                chunk.Initialize(
                    WorldSeed,
                    newId.x,
                    newId.y
                );

                _chunks.Remove(oldId);
                _chunks.Add(newId, chunk);
            }

            _currentChunk = centerChunk;
        }
    }
}
