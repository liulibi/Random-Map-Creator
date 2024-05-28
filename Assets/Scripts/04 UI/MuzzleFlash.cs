using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject MuzzleHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer[] spriteRenderers;

    public float flashTime;

    private void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        MuzzleHolder.SetActive(true);
        int randomNum = Random.Range(0, flashSprites.Length);
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[randomNum];
        }

        Invoke("Deactivate", flashTime);
    }

    private void Deactivate()
    {
        MuzzleHolder.SetActive(false);
    }
}
