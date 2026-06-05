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

        public void Initialize(int seed, int chunkX, int chunkZ)
        {
            transform.position = new Vector3(
                chunkX * WIDTH,
                0,
                chunkZ * HEIGHT
            );

            ChunkX = chunkX;
            ChunkZ = chunkZ;

            Generate(seed);
        }

        private void Generate(int seed)
        {
            areiaEscura.ClearAllTiles();
            areiaClara.ClearAllTiles();

            for (int x = 0; x < TILES_X; x++)
            {
                for (int z = 0; z < TILES_Z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, z, 0);

                    areiaEscura.SetTile(pos, darkTile);

                    float noise = Mathf.PerlinNoise(
                        (ChunkX * TILES_X + x + seed) * LightTileNoise,
                        (ChunkZ * TILES_Z + z + seed) * LightTileNoise
                    );

                    if (noise > LightTilePercentage)
                    {
                        areiaClara.SetTile(pos, lightTile);
                    }
                }
            }

            GenerateDecorations(seed);
        }

        private void GenerateDecorations(int seed)
        {
            rocksTilemap.ClearAllTiles();
            plantsTilemap.ClearAllTiles();
            cactiTilemap.ClearAllTiles();

            for (int x = 0; x < TILES_X; x++)
            {
                for (int z = 0; z < TILES_Z; z++)
                {
                    int worldX = ChunkX * TILES_X + x;
                    int worldZ = ChunkZ * TILES_Z + z;

                    Vector3Int pos = new Vector3Int(x, z, 0);

                    TrySpawnDecoration(
                        pos,
                        worldX,
                        worldZ,
                        seed
                    );
                }
            }
        }

        private void TrySpawnDecoration(
            Vector3Int pos,
            int worldX,
            int worldZ,
            int seed)
        {
            float spawnRoll = Hash(worldX, worldZ, seed);
            
            if (spawnRoll > DecorationChance)
                return;

            float totalWeight =
                rockChance +
                plantChance +
                cactusChance;

            float selection =
                Hash(worldX + 9999, worldZ - 9999, seed)
                * totalWeight;

            if (selection < rockChance)
            {
                rocksTilemap.SetTile(
                    pos,
                    GetRandomTile(rockTiles, worldX, worldZ, seed)
                );

                return;
            }

            selection -= rockChance;

            if (selection < plantChance)
            {
                plantsTilemap.SetTile(
                    pos,
                    GetRandomTile(plantTiles, worldX, worldZ, seed)
                );

                return;
            }

            cactiTilemap.SetTile(
                pos,
                GetRandomTile(cactiTiles, worldX, worldZ, seed)
            );
        }

        private float Hash(
            int x,
            int z,
            int seed)
        {
            uint h = (uint)(
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
            int seed)
        {
            if (tiles == null || tiles.Length == 0)
                return null;

            float totalWeight = 0f;

            foreach (var tile in tiles)
            {
                totalWeight += tile.Weight;
            }

            float roll =
                Hash(
                    worldX + 12345,
                    worldZ + 54321,
                    seed
                ) * totalWeight;

            foreach (var tile in tiles)
            {
                roll -= tile.Weight;

                if (roll <= 0)
                    return tile.Tile;
            }

            return tiles[tiles.Length - 1].Tile;
        }
    }
}