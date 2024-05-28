using UnityEngine;

[System.Serializable]//可见
public class Wave//struct, ScriptableObject
{
    public bool infinite;

    public int enemyNumber;//每一波【敌人总数】
    public float timeBtwSpawn;//每波当中，前后敌人出现的【间隔时间】

    //新增每一关的【难度】
    public float enemySpeed;
    public int enemyDamage;
    public float enemyHealth;
    public Color enemySkinColor;
}
