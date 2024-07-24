using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{

    [Header("基本属性")]
    public int health;
    public float detectDistance;
    public int damageToPlayer;

    [Header("受伤及死亡")]
    public Vector2 hurtRecoil;
    public float hurtRecoilTime;
    public Vector2 deathForce;
    public float destroyDelay;


    protected State _currentState;
    protected float _playerEnemyDistance;

    #region 碰撞目标层

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName == "Player")
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            playerController.hurt(damageToPlayer);
        }
    }

    #endregion

    #region 公共方法

    public float playerEnemyDistance()
    {
        return _playerEnemyDistance;
    }

    public abstract float behaveInterval();

    public abstract void hurt(int damage);

    #endregion


    protected abstract void die();


    #region 状态类
    public abstract class State
    {
        public abstract bool checkValid(EnemyController enemyController);
        public abstract void Execute(EnemyController enemyController);
    }

    #endregion
}