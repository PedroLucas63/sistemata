using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public class Chunk : MonoBehaviour
    {
        public const float WIDTH = 45.5f;
        public const float HEIGHT = 24.5f;

        private const int TILES_X = 47;
        private const int TILES_Z = 26;
        private const int TOTAL_TILES = TILES_X * TILES_Z;

        [Header("Tilemaps configuration")]
        [SerializeField] private Tilemap areiaEscura;
        [SerializeField] private Tilemap areiaClara;

        [Header("Tilebase configuration")]
        [SerializeField] private TileBase darkTile;
        [SerializeField] private TileBase lightTile;

        [Header("Lighting Noise")]
        [SerializeField] private float LightTileNoise = 0.15f;
        [SerializeField] private float LightTilePercentage = 0.40f;

        [Header("Decoration Tilemaps")]
        [SerializeField] private Tilemap rocksTilemap;
        [SerializeField] private Tilemap plantsTilemap;
        [SerializeField] private Tilemap cactiTilemap;

        [Header("Decoration Tiles")]
        [SerializeField] private WeightedTile[] rockTiles;
        [SerializeField] private WeightedTile[] plantTiles;
        [SerializeField] private WeightedTile[] cactiTiles;

        [Header("Spawn Chances")]
        [Range(0f, 1f)]
        [SerializeField] private float rockChance = 0.08f;

        [Range(0f, 1f)]
        [SerializeField] private float plantChance = 0.04f;

        [Range(0f, 1f)]
        [SerializeField] private float cactusChance = 0.03f;
        

        [Header("Decoration Chance")]
        [SerializeField] private float DecorationChance = 0.3f;

        private int ChunkX;
        private int ChunkZ;
        private bool _firstGeneration = true;
        
        private static TileBase[] _darkTilesCache;
        private static TileBase[] _lightTilesBuffer;
        private static TileBase[] _rocksBuffer;
        private static TileBase[] _plantsBuffer;
        private static TileBase[] _cactiBuffer;
        
        private float _totalDecorationChanceWeight;
        private float _totalRockWeight;
        private float _totalPlantWeight;
        private float _totalCactusWeight;
        
        private Coroutine _generationCoroutine;

        public void SetChunk(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;
        }

        public int GetChunkX()
        {
            return ChunkX;
        }

        public int GetChunkZ()
        {
            return ChunkZ;
        }
        
        private void Start()
        {
            InitializeWeightsCache();
        }
        
        private void OnValidate()
        {
            InitializeWeightsCache();
        }
        
        private void InitializeWeightsCache()
        {
            _totalDecorationChanceWeight = rockChance + plantChance + cactusChance;

            _totalRockWeight = CalculateTilesTotalWeight(rockTiles);
            _totalPlantWeight = CalculateTilesTotalWeight(plantTiles);
            _totalCactusWeight = CalculateTilesTotalWeight(cactiTiles);
        }
        
        private static float CalculateTilesTotalWeight(WeightedTile[] tiles)
        {
            if (tiles == null || tiles.Length == 0) return 0f;
            return tiles.Sum(tile => tile.Weight);
        }

        public void Initialize(int seed, int chunkX, int chunkZ)
        {
            if (_generationCoroutine != null)
            {
                StopCoroutine(_generationCoroutine);
            }
            
            transform.position = new Vector3(
                chunkX * WIDTH,
                0,
                chunkZ * HEIGHT
            );

            ChunkX = chunkX;
            ChunkZ = chunkZ;

            _generationCoroutine = StartCoroutine(GenerateRoutine(seed));
        }

        private IEnumerator GenerateRoutine(int seed)
        {
            GenerateDarkTiles();
            yield return null;
            GenerateLightTiles(seed);
            yield return null;
            GenerateDecorations(seed);
            _generationCoroutine = null;
        }

        private void GenerateDarkTiles()
        {
            if (!_firstGeneration)
                return;
            
            areiaEscura.ClearAllTiles();
            
            var bounds = new BoundsInt(0, 0, 0, TILES_X, TILES_Z, 1);
            if (_darkTilesCache is not { Length: TOTAL_TILES })
            {
                _darkTilesCache = new TileBase[TOTAL_TILES];
                for (var i = 0; i < TOTAL_TILES; i++)
                {
                    _darkTilesCache[i] = darkTile;
                }
            }

            areiaEscura.SetTilesBlock(bounds, _darkTilesCache);
            _firstGeneration = false;
        }
        
        private void GenerateLightTiles(int seed)
        {
            areiaClara.ClearAllTiles();
            _lightTilesBuffer ??= new TileBase[TOTAL_TILES];
            
            var index = 0;
            for (var z = 0; z < TILES_Z; z++)
            {
                for (var x = 0; x < TILES_X; x++)
                {
                    var noise = Mathf.PerlinNoise(
                        (ChunkX * TILES_X + x + seed) * LightTileNoise,
                        (ChunkZ * TILES_Z + z + seed) * LightTileNoise
                    );

                    _lightTilesBuffer[index] = (noise > LightTilePercentage) ? lightTile : null;
                    index++;
                }
            }
            
            var bounds = new BoundsInt(0, 0, 0, TILES_X, TILES_Z, 1);
            areiaClara.SetTilesBlock(bounds, _lightTilesBuffer);
        }

        private void GenerateDecorations(int seed)
        {
            rocksTilemap.ClearAllTiles();
            plantsTilemap.ClearAllTiles();
            cactiTilemap.ClearAllTiles();
            
            _rocksBuffer ??= new TileBase[TOTAL_TILES];
            _plantsBuffer ??= new TileBase[TOTAL_TILES];
            _cactiBuffer ??= new TileBase[TOTAL_TILES];
            
            System.Array.Clear(_rocksBuffer, 0, TOTAL_TILES);
            System.Array.Clear(_plantsBuffer, 0, TOTAL_TILES);
            System.Array.Clear(_cactiBuffer, 0, TOTAL_TILES);
            
            var index = 0;
            for (var z = 0; z < TILES_Z; z++)
            {
                for (var x = 0; x < TILES_X; x++)
                {
                    var worldX = ChunkX * TILES_X + x;
                    var worldZ = ChunkZ * TILES_Z + z;
                    
                    var spawnRoll = Hash(worldX, worldZ, seed);
                    if (spawnRoll > DecorationChance)
                    {
                        index++;
                        continue;
                    }

                    EvaluateDecorationBuffers(index, worldX, worldZ, seed);
                    index++;
                }
            }

            var bounds = new BoundsInt(0, 0, 0, TILES_X, TILES_Z, 1);
            rocksTilemap.SetTilesBlock(bounds, _rocksBuffer);
            plantsTilemap.SetTilesBlock(bounds, _plantsBuffer);
            cactiTilemap.SetTilesBlock(bounds, _cactiBuffer);
        }
        
        private void EvaluateDecorationBuffers(int index, int worldX, int worldZ, int seed)
        {
            var selection = Hash(worldX + 9999, worldZ - 9999, seed) * _totalDecorationChanceWeight;

            if (selection < rockChance)
            {
                _rocksBuffer[index] = GetRandomTile(rockTiles, worldX, worldZ, seed, _totalRockWeight);
                return;
            }

            selection -= rockChance;

            if (selection < plantChance)
            {
                _plantsBuffer[index] = GetRandomTile(plantTiles, worldX, worldZ, seed, _totalPlantWeight);
                return;
            }

            _cactiBuffer[index] = GetRandomTile(cactiTiles, worldX, worldZ, seed, _totalCactusWeight);
        }

        private static float Hash(
            int x,
            int z,
            int seed)
        {
            var h = (uint)(
                x * 374761393 +
                z * 668265263 +
                seed * 1442695041
            );

            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;

            return h / (float)uint.MaxValue;
        }

        private TileBase GetRandomTile(
            WeightedTile[] tiles,
            int worldX,
            int worldZ,
            int seed,
            float weight
        )
        {
            if (tiles == null || tiles.Length == 0)
                return null;

            var roll =
                Hash(
                    worldX + 12345,
                    worldZ + 54321,
                    seed
                ) * weight;

            foreach (var tile in tiles)
            {
                roll -= tile.Weight;

                if (roll <= 0)
                    return tile.Tile;
            }

            return tiles[^1].Tile;
        }
    }
}