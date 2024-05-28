using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    #region 生成瓦片
    public GameObject tilePrefab;
    //public Vector2 mapSize;
    //public Transform mapHolder;
    [Range(0, 1)] public float outlinePercent;
    #endregion

    #region 生态障碍物
    List<Coord> allTileCoords;//CORE 卧槽，千万不能写成【List<Coord> allTileCoords = new List<Coord>();】的形式
    Queue<Coord> shuffleTileCoords;
    //public int seed;
    public GameObject obsPrefab;
    #endregion

    #region 地图的连接性
    [Range(0, 1)] public float obsPercent;
    //Coord mapCenter;
    #endregion

    #region navMesh
    public float tileSize;//OPTIONAL 用来调整整体场景地图的大小，可不加
    public GameObject mapFloor;
    public GameObject navMeshFloor;
    public Vector2 maxMapSize;
    public GameObject navMeshObsPrefab;
    #endregion

    #region
    public Map[] maps;
    public int mapIndex;
    Map currentMap;
    #endregion

    Transform[,] tilemap;
    Queue<Coord> shuffleOpenTileCoords;

    private void Awake()
    {
        GenerateMap();
        //FindObjectOfType<Spawner>().onNewWave += NewWave;
    }

    private void NewWave(int _waveIndex)
    {
        mapIndex = _waveIndex - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tilemap = new Transform[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        //GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        allTileCoords = new List<Coord>();//每次开启一个新的地图，都会进行一次初始化！「遗弃过去」

        #region 必须写
        string holderName = "mapHolder";
        if(transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        #endregion

        #region 地图瓦片的生成
        for (int i = 0; i < currentMap.mapSize.x; i++)
        {
            for(int j = 0; j < currentMap.mapSize.y; j++)
            {
                //Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + i, 0, -mapSize.y / 2 + 0.5f + j);
                Vector3 newPos = CoordToPosition(i, j);
                GameObject spawnTile = Instantiate(tilePrefab, newPos, Quaternion.Euler(90, 0, 0));
                spawnTile.transform.parent = mapHolder;
                spawnTile.transform.localScale *= (1 - outlinePercent) * tileSize;

                allTileCoords.Add(new Coord(i, j));
                tilemap[i, j] = spawnTile.transform;
            }
        }
        #endregion

        #region 障碍物的生成
        shuffleTileCoords = new Queue<Coord>(Utilities.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //mapCenter = new Coord((int)currentMap.mapSize.x / 2, (int)currentMap.mapSize.y / 2);//中心点先获取了，中心点是玩家生成位置，不允许生成任何的障碍物
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        //int obsCount = 10;//改成了下面这句
        int obsCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obsPercent);
        int currentObsCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);//一开始肯定等于allTileCoords

        for(int i = 0; i < obsCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObsCount++;//每次生成障碍物，currentObsCount 累加加一

            //所以我们要在这里，通过MapIsFullyAccessible方法，判断一下
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObsCount))
            {
                float obsHeight = Mathf.Lerp(currentMap.minObsHeight, currentMap.maxObsHeight, (float)prng.NextDouble());

                Vector3 newPos = CoordToPosition(randomCoord.x, randomCoord.y);
                GameObject spawnObs = Instantiate(obsPrefab, newPos + Vector3.up * obsHeight / 2, Quaternion.identity);
                spawnObs.transform.parent = mapHolder;
                //spawnObs.transform.localScale *= (1 - outlinePercent) * tileSize;
                spawnObs.transform.localScale = new Vector3((1- outlinePercent), obsHeight, (1- outlinePercent)) * tileSize;

                MeshRenderer render = spawnObs.GetComponent<MeshRenderer>();
                Material material = render.material;
                float colorPercent = randomCoord.y / currentMap.mapSize.y;
                material.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                render.material = material;

                allOpenCoords.Remove(randomCoord);
            }
            else//如果方法返回值为False，说明你不能够在这里生成障碍物
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObsCount--;
            }

        }
        #endregion

        shuffleOpenTileCoords = new Queue<Coord>(Utilities.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        #region 边缘navmesh导航的限制，动态的创建navmeshObs

        navMeshFloor.transform.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.transform.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);

        GameObject obsLeft = Instantiate(navMeshObsPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        obsLeft.transform.parent = mapHolder;
        obsLeft.transform.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        GameObject obsRight = Instantiate(navMeshObsPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        obsRight.transform.parent = mapHolder;
        obsRight.transform.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        GameObject obsTop = Instantiate(navMeshObsPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        obsTop.transform.parent = mapHolder;
        obsTop.transform.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        GameObject obsBottom = Instantiate(navMeshObsPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        obsBottom.transform.parent = mapHolder;
        obsBottom.transform.localScale = new Vector3(currentMap.mapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        #endregion
    }

    public bool MapIsFullyAccessible(bool[,] _obstacleMap, int _currentObsCount)
    {
        bool[,] mapFlags = new bool[_obstacleMap.GetLength(0), _obstacleMap.GetLength(1)];//标记是否已被【检查】
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;//中心点被标记为【检查】

        int accessibleTileCount = 1;//不含邮障碍物的瓦片的数量，一开始为1因为中心点肯定不是障碍物

        while(queue.Count > 0)
        {
            Coord currentTile = queue.Dequeue();

            for(int x = -1; x <= 1; x++)//遍历currentTile周边的四个相邻瓦片
            {
                for(int y = -1; y <= 1; y++)
                {
                    int neightbourX = currentTile.x + x;//这一步会遍历周围的八个
                    int neightbourY = currentTile.y + y;//这一步会遍历周围的八个

                    if(x == 0 || y == 0)//排除对角线
                    {
                        if(neightbourX >= 0 && neightbourX < _obstacleMap.GetLength(0) && neightbourY >= 0 && neightbourY < _obstacleMap.GetLength(1))//不出现在地图之外
                        {
                            //检查这个Tile是否我们已经检测过了 && 相邻的这个瓦片并没有obstacle瓦片
                            if(mapFlags[neightbourX, neightbourY] == false && _obstacleMap[neightbourX, neightbourY] == false)
                            {
                                mapFlags[neightbourX, neightbourY] = true;
                                queue.Enqueue(new Coord(neightbourX, neightbourY));

                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - _currentObsCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffleTileCoords.Dequeue();
        shuffleTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffleOpenTileCoords.Dequeue();
        shuffleOpenTileCoords.Enqueue(randomCoord);
        return tilemap[randomCoord.x, randomCoord.y];
    }

    public Vector3 CoordToPosition(int _x, int _y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + _x, 0, -currentMap.mapSize.y / 2f + 0.5f + _y) * tileSize;
    }

}

[System.Serializable]
public class Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Coord a, Coord b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            return false;

        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Coord other = (Coord)obj;
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            return hash;
        }
    }
}

[System.Serializable]
public class Map
{
    public Vector2 mapSize;
    [Range(0, 1)] public float obsPercent;
    public int seed;
    public float minObsHeight, maxObsHeight;
    public Color foregroundColor, backgroundColor;

    public Coord mapCenter
    {
        get
        {
            return new Coord((int)(mapSize.x / 2), (int)(mapSize.y / 2));
        }
    }

}

