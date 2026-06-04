using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private Chunk chunkPrefab;

        private const int GRID_SIZE = 3;
        private GameObject _grid;
        private void Awake()
        {
            _grid = transform.Find("Grid").gameObject;
        }

        private void Start()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Chunk chunk = SpawnChunk();
                    chunk.SetChunk(x, z);

                    chunk.transform.position = new Vector3(
                        x * Chunk.WIDTH,
                        0,
                        z * Chunk.HEIGHT
                    );

                    chunk.name = $"Chunk ({x},{z})";
                    Debug.Log($"Chunk {chunk.name} created");
                    Debug.Log(chunk.transform.rotation.eulerAngles);
                    Debug.Log(chunk.transform.localRotation.eulerAngles);
                }
            }
        }
        
        private Chunk SpawnChunk()
        {
            return Instantiate(
                chunkPrefab,
                _grid.transform
            );
        }
    }
}
