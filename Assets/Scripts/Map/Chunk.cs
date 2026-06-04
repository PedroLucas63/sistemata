using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Map
{
    public class Chunk : MonoBehaviour
    {
        public const float WIDTH = 45.5f;
        public const float HEIGHT = 24.5f;

        [SerializeField] private int ChunkX;
        [SerializeField] private int ChunkZ;

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
    }
}
