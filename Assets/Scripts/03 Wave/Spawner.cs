using System.Collections;
using UnityEngine;
using System;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Wave[] waves;//List OJBK

    [Header("JUST FOR CHECK ! ! !")]
    public Wave currentWave;//当前波
    public int currentWaveIndex;//当前波数【序列、索引】

    public int waitSpawnNum;//这一波还剩下多少敌人没有生成，等于0以后不再生成新的敌人
    public int spawnAliveNum;//这一波的敌人还存活了多少个，少于0的话，进行下一波
    public float nextSpawnTime;

    private MapGenerator mapGenerator;

    private bool isDisabled;
    private LivingEntity playerEntity;

    public event Action<int> onNewWave;//这个事件，将会在NextWave内部逻辑这个方法中触发。由于每一波的序号不同，所以这里使用的是Action<int>

    private void Start()
    {
        playerEntity = FindObjectOfType<LivingEntity>();
        playerEntity.onDeath += PlayerDeath;
        mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void ResetPlayerPos()
    {
        playerEntity.transform.position = Vector3.zero + Vector3.up * 1;//让玩家从上面掉下来
    }

    private void Update()
    {
        if(!isDisabled)
        {
            if ((waitSpawnNum > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                waitSpawnNum--;
                nextSpawnTime = Time.time + currentWave.timeBtwSpawn;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    private void NextWave()
    {
        currentWaveIndex++;
        Debug.Log(string.Format("Current Wave : {0}", currentWaveIndex));

        if (currentWaveIndex - 1 < waves.Length)//MARKER 最后一步，这次currentIndex是从1开始的，所以第三波的时候，index实际上等于3已经超过了Length的范围，所以再下一波的时候报错，我们可以限定范围
        {
            currentWave = waves[currentWaveIndex - 1];
            waitSpawnNum = currentWave.enemyNumber;
            spawnAliveNum = currentWave.enemyNumber;

            if (onNewWave != null)
            {
                onNewWave(currentWaveIndex);
            }

            ResetPlayerPos();
            FindObjectOfType<GameUI>().NewWaveBannerUI(currentWaveIndex);
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1.0f;
        float tileFlashSpeed = 4;

        Transform randomTile = mapGenerator.GetRandomOpenTile();

        #region
        Material tileMat = randomTile.GetComponent<MeshRenderer>().material;
        Color originalColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;
        #endregion

        while(spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        //GameObject spawnEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        GameObject spawnEnemy = Instantiate(enemyPrefab, randomTile.position + Vector3.up, Quaternion.identity);
        spawnEnemy.GetComponent<Enemy>().onDeath += EnemyDeath;//CORE

        spawnEnemy.GetComponent<Enemy>().SetDifficulty(currentWave.enemySpeed, currentWave.enemyDamage, currentWave.enemyHealth, currentWave.enemySkinColor);
    }

    private void EnemyDeath()
    {
        spawnAliveNum--;
        if (spawnAliveNum <= 0)
            NextWave();
    }

    private void PlayerDeath()
    {
        isDisabled = true;
    }

}
