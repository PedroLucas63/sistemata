using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Coroutine _chunkGenerationProcess;
        
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

            if (_currentChunk.x == chunkId.x && _currentChunk.y == chunkId.y) return;
            if (_chunkGenerationProcess != null)
            {
                StopCoroutine(_chunkGenerationProcess);
            }
            StartCoroutine(GenerateNewChunks(chunkId));
        }

        private void GenerateGrid()
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    var chunk = SpawnChunk();
                    chunk.Initialize(WorldSeed, x, z);
                    _chunks.Add(new Vector2Int(x, z), chunk);
                }
            }

            _currentChunk = new Vector2Int(0, 0);
        }
        
        private Chunk SpawnChunk()
        {
            return Instantiate(
                chunkPrefab,
                _grid.transform
            );
        }

        private static Vector2Int GetChunkIdByPosition(Vector3 position)
        {
            var x = Mathf.FloorToInt(position.x / Chunk.WIDTH);
            var z = Mathf.FloorToInt(position.z / Chunk.HEIGHT);

            return new Vector2Int(x, z);
        }
        
        private IEnumerator GenerateNewChunks(Vector2Int centerChunk)
        {
            HashSet<Vector2Int> requiredChunks = new();

            for (var x = -1; x <= 1; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    requiredChunks.Add(
                        centerChunk + new Vector2Int(x, z)
                    );
                }
            }

            var missingChunks = requiredChunks.Where(chunkId => !_chunks.ContainsKey(chunkId)).ToList();
            var reusableChunks = _chunks.Keys.Where(chunkId => !requiredChunks.Contains(chunkId)).ToList();

            var count = Mathf.Min(missingChunks.Count, reusableChunks.Count);
            
            for (var i = 0; i < count; i++)
            {
                var oldId = reusableChunks[i];
                var newId = missingChunks[i];
                
                if (!_chunks.Remove(oldId, out var chunk))
                    continue;
                _chunks.Add(newId, chunk);

                chunk.Initialize(
                    WorldSeed,
                    newId.x,
                    newId.y
                );
                
                yield return null;
            }

            _currentChunk = centerChunk;
            _chunkGenerationProcess = null;
        }
    }
}
