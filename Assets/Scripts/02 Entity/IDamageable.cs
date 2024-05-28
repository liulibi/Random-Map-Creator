using UnityEngine;

public interface IDamageable
{
    void TakeHit(float _damageAmount, Vector3 hitPoint, Vector3 hitDirection);//用作敌人阵亡效果
    void TakenDamage(float _damageAmount);
}
