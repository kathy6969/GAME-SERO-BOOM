using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pepperPrefab;
    public GameObject snakePrefab;

    int[,] mapData = new int[,]
    {
        {0,0,0,0,0},
        {1,0,0,2,1},
        {1,0,3,0,1},
        {1,1,1,1,1},
    };
    // 0 = floor, 1 = wall, 2 = pepper, 3 = snake start

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        for (int y = 0; y < mapData.GetLength(0); y++)
        {
            for (int x = 0; x < mapData.GetLength(1); x++)
            {
                Vector2 pos = new Vector2(x, -y); // đảo y để đúng hướng
                int tile = mapData[y, x];

                Instantiate(floorPrefab, pos, Quaternion.identity); // luôn có sàn

                if (tile == 1)
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                else if (tile == 2)
                    Instantiate(pepperPrefab, pos, Quaternion.identity);
                else if (tile == 3)
                    Instantiate(snakePrefab, pos, Quaternion.identity);
            }
        }
    }
}