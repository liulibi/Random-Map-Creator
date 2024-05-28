using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float shootSpeed;
    [SerializeField] private float damage = 1.0f;
    [SerializeField] private float lifetime;

    public LayerMask collisionMask;//Enemy Layer

    private void Start()
    {
        Destroy(gameObject, lifetime);//发射后N秒内销毁

        #region
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);//防止武器在敌人身体内部不造成伤害
        if(initialCollisions.Length > 0)
        {
            HitEnemy(initialCollisions[0], transform.position);
        }
        #endregion
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * shootSpeed * Time.deltaTime);
        CheckCollision();
    }

    private void CheckCollision()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, shootSpeed * Time.deltaTime, collisionMask, QueryTriggerInteraction.Collide))
        {
            HitEnemy(hitInfo, hitInfo.point);
            //HitEnemy(hitInfo.collider, hitInfo.point);
        }
    }
     
    private void HitEnemy(RaycastHit _hitInfo, Vector3 _hitPoint)
    {
        IDamageable damageableObject = _hitInfo.collider.GetComponent<IDamageable>();
        if (damageableObject != null)
            damageableObject.TakeHit(damage, _hitPoint, transform.forward);
        Destroy(gameObject);//击中敌人后，子弹销毁
    }

    private void HitEnemy(Collider _collider, Vector3 _hitPoint)
    {
        IDamageable damageableObject = _collider.GetComponent<IDamageable>();
        if(damageableObject != null)
            damageableObject.TakeHit(damage, _hitPoint, transform.forward);
        Destroy(gameObject);
    }

}
