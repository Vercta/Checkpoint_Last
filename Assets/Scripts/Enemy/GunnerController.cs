using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gunner（Enemy2）的实现类
/// </summary>
public class GunnerController : EnemyController
{

    [Header("基本参数（攻击）")]
    public float shootInterval;
    public GameObject projectilePrefab;

    private bool _isShooting;
    private bool _isShootable;

    private Transform _playerTransform;
    private Transform _transform;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    #region Unity周期

    void Start()
    {
        InitializeComponents();
        InitializeState();
    }

    void Update()
    {
        UpdatePlayerDistance();
        UpdateFacing();
        UpdateState();
        ExecuteCurrentState();
    }

    #endregion

    #region 初始化

    private void InitializeComponents()
    {
        _playerTransform = GlobalController.Instance.player.GetComponent<Transform>();
        _transform = gameObject.GetComponent<Transform>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void InitializeState()
    {
        _isShootable = true;
        _isShooting = false;
        _currentState = new Idle();
    }

    #endregion

    #region 状态更新

    private void UpdatePlayerDistance()
    {
        _playerEnemyDistance = _playerTransform.position.x - _transform.position.x;
    }

    private void UpdateFacing()
    {
        int direction = _playerEnemyDistance > 0 ? 1 : _playerEnemyDistance < 0 ? -1 : 0;

        if (direction != 0 && health > 0)
        {
            Vector3 newScale = _transform.localScale;
            newScale.x = direction;
            _transform.localScale = newScale;
        }
    }

    private void UpdateState()
    {
        if (!_currentState.checkValid(this))
        {
            _currentState = _isShooting ? new Shooting() : new Idle();
            _isShooting = !_isShooting;
        }
    }

    private void ExecuteCurrentState()
    {
        if (health > 0)
            _currentState.Execute(this);
    }

    #endregion

    #region 针对性重写

    public override float behaveInterval()
    {
        return shootInterval;
    }

    public override void hurt(int damage)
    {
        health = Math.Max(health - damage, 0);

        if (health == 0)
        {
            die();
            return;
        }

        Vector2 newVelocity = hurtRecoil;
        newVelocity.x *= _transform.localScale.x;

        _rigidbody.velocity = newVelocity;

        StartCoroutine(hurtCoroutine());
    }

    protected override void die()
    {
        _animator.SetTrigger("isDead");
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.velocity = Vector2.zero;
        gameObject.layer = LayerMask.NameToLayer("Decoration");

        Vector2 newForce = new Vector2(_transform.localScale.x * deathForce.x, deathForce.y);
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(fadeCoroutine());
    }

    #endregion

    #region 协程

    private IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(hurtRecoilTime);
        _rigidbody.velocity = Vector2.zero;
    }

    private IEnumerator fadeCoroutine()
    {
        float elapsedTime = 0f;
        Color originalColor = _spriteRenderer.color;

        while (elapsedTime < destroyDelay)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Max(0, 1 - (elapsedTime / destroyDelay));
            _spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator shootPlayerCoroutine(Vector2 direction, float shootInterval)
    {
        yield return new WaitForSeconds(0.2f);

        GameObject projectileObj = Instantiate(projectilePrefab, _transform.position, _transform.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.direction = direction;
        projectile.trigger();

        yield return new WaitForSeconds(shootInterval);
        _isShootable = true;
    }

    #endregion

    #region 射击机制

    private void shootPlayer()
    {
        if (_isShootable)
        {
            _animator.SetTrigger("attack");
            _isShootable = false;
            Vector2 direction = _playerTransform.position - _transform.position;
            StartCoroutine(shootPlayerCoroutine(direction, shootInterval));
        }
    }

    #endregion

    #region 状态

    public class Idle : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
        }
    }

    public class Shooting : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            GunnerController gunnerController = (GunnerController)enemyController;
            gunnerController.shootPlayer();
        }
    }
    #endregion
}