using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectilePrefab;
    [SerializeField] private float fireRate = 0.5f;

    public GameObject shellPrefab;
    public Transform shellTrans;

    private float timer;
    private MuzzleFlash muzzleFlash;

    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
            Shot();
    }

    public void Shot()//经典的计时器限制频率问题
    {
        timer += Time.deltaTime;
        if(timer > fireRate)
        {
            timer = 0;
            GameObject spawnProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            Instantiate(shellPrefab, shellTrans.position, shellTrans.rotation);//弹壳
            muzzleFlash.Activate();//火花
        }
    }

    public float GetHeight
    {
        get
        {
            return firePoint.position.y;
        }
    }
}
