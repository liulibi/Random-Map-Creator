using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
    public int score { get; private set; }//外界只能获取，不能更改
    [SerializeField] float lastEnemyKillTime;
    [SerializeField] int streakCount;
    float streakExpiredTime = 2.0f;

    public Text scoreText;

    private void Start()
    {
        Enemy.onDeathStatic += EnemyKilled;
        FindObjectOfType<PlayerController>().onDeath += PlayerDeath;
    }

    private void EnemyKilled()
    {
        if (Time.time < lastEnemyKillTime + streakExpiredTime)
            streakCount++;
        else
            streakCount = 0;

        lastEnemyKillTime = Time.time;

        score += 5 + (int)Mathf.Pow(2, streakCount);//连击得分的奖励

        scoreText.text = score.ToString("D6");
    }

    void PlayerDeath()
    {
        Enemy.onDeathStatic -= EnemyKilled;
    }

}
