using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadeImage;
    public Color fromColor, toColor;
    public GameObject gameOverUI;

    public RectTransform waveBannerTrans;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemyText;

    Spawner spawner;
    public RectTransform healthBarTrans;//血量底部显示
    public RectTransform healthBarShadowTrans;//缓冲效果
    [SerializeField] private float shadowSpeed = 0.1f;
    private PlayerController player;
    public Text healthText;
    public Text gameoverScoreText;


    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        player.onDeath += GameOver;

        healthText.text = player.health + "/" + player.maxHealth;
    }

    //这个方法将会在人物受伤时调用，用来显示玩家血量
    public void UpdateHealth()
    {
        if(player != null)
        {
            float healthPointPercent = player.health / player.maxHealth;
            healthBarTrans.localScale = new Vector3(healthPointPercent, 1, 1);

            healthText.text = player.health + "/" + player.maxHealth;
            StartCoroutine(ShadowEffectCo());
        }
    }

    IEnumerator ShadowEffectCo()
    {
        float shadowX = healthBarShadowTrans.localScale.x;
        while(shadowX > healthBarTrans.localScale.x)
        {
            shadowX -= shadowSpeed * Time.deltaTime;
            healthBarShadowTrans.localScale = new Vector3(shadowX, 1, 1);
            yield return null;
        }
    }

    public void NewWaveBannerUI(int _waveIndex)
    {
        string[] numbers = { "I", "II", "III", "IV", "V" };
        waveText.text = "- Wave " + numbers[_waveIndex - 1] + "-";
        enemyText.text = "Enemies: " + spawner.waves[_waveIndex - 1].enemyNumber;

        StartCoroutine(AnimateWaveBanner());
    }

    IEnumerator AnimateWaveBanner()
    {
        float delayTime = 2.0f;
        float floatSpeed = 2.5f;
        float animatePercent = 0;
        int direction = 1;

        float endDelayTime = Time.time + 1 / floatSpeed + delayTime;

        while(animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * floatSpeed * direction;

            if(animatePercent >= 1)
            {
                animatePercent = 1;
                if(Time.time > endDelayTime)
                {
                    direction = -1;
                }
            }

            waveBannerTrans.anchoredPosition = Vector2.up * Mathf.Lerp(140, 15, animatePercent);
            yield return null;
        }
    }

    void GameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(fromColor, toColor, 1));
        gameoverScoreText.text = FindObjectOfType<ScoreKeeper>().score.ToString();
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color _from, Color _to, float _time)
    {
        float speed = 1 / _time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadeImage.color = Color.Lerp(_from, _to, percent);
            yield return null;
        }
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("00_Game");
    }
}
