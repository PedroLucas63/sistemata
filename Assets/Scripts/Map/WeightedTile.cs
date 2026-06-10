using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sistemata.Map
{
    [System.Serializable]
    public class WeightedTile
    {
        public TileBase Tile;

        [Min(0.001f)]
        public float Weight = 1f;
    }
}