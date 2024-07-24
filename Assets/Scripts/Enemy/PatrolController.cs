using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巡逻机制及Enemy1的行为
/// </summary>
public class PatrolController : EnemyController
{
    #region Public Variables

    [Header("移动")]
    public float walkSpeed;
    public float edgeSafeDistance;

    [Header("行为间隔")]
    public float behaveIntervalLeast;
    public float behaveIntervalMost;

    #endregion

    private int _reachEdge;
    private bool _isChasing;
    private bool _isMovable;

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
        CheckEdge();
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
        _currentState = new Patrol();
        _isChasing = false;
        _isMovable = true;
    }
    #endregion

    #region 状态更
    private void UpdatePlayerDistance()
    {
        _playerEnemyDistance = _playerTransform.position.x - _transform.position.x;
    }

    private void CheckEdge()
    {
        Vector2 detectOffset = new Vector2(edgeSafeDistance * _transform.localScale.x, 0);
        _reachEdge = checkGrounded(detectOffset) ? 0 : (_transform.localScale.x > 0 ? 1 : -1);
    }

    private void UpdateState()
    {
        if (!_currentState.checkValid(this))
        {
            _currentState = _isChasing ? new Patrol() : new Chase();
            _isChasing = !_isChasing;
        }
    }

    private void ExecuteCurrentState()
    {
        if (_isMovable)
            _currentState.Execute(this);
    }

    #endregion

    // 目标碰撞层
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            playerController.hurt(1);
        }
    }

    #region 方法重写

    public override float behaveInterval()
    {
        return UnityEngine.Random.Range(behaveIntervalLeast, behaveIntervalMost);
    }

    public override void hurt(int damage)
    {
        health = Math.Max(health - damage, 0);
        _isMovable = false;

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
        _rigidbody.velocity = Vector2.zero;
        gameObject.layer = LayerMask.NameToLayer("Decoration");

        Vector2 newForce = new Vector2(_transform.localScale.x * deathForce.x, deathForce.y);
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(fadeCoroutine());
    }

    #endregion

    #region 移动行为
    public void walk(float move)
    {
        int direction = Math.Sign(move);
        float newWalkSpeed = (direction == _reachEdge) ? 0 : direction * walkSpeed;

        if (direction != 0 && health > 0)
        {
            _transform.localScale = new Vector3(direction, 1, 1);
        }

        _rigidbody.velocity = new Vector2(newWalkSpeed, _rigidbody.velocity.y);
        _animator.SetFloat("Speed", Math.Abs(newWalkSpeed));
    }

    #endregion

    #region 辅助方法

    public int reachEdge()
    {
        return _reachEdge;
    }

    private bool checkGrounded(Vector2 offset)
    {
        Vector2 origin = (Vector2)_transform.position + offset;
        float radius = 0.3f;
        Vector2 direction = Vector2.down;
        float distance = 1.1f;
        LayerMask layerMask = LayerMask.GetMask("Platform");

        return Physics2D.CircleCast(origin, radius, direction, distance, layerMask).collider != null;
    }

    #endregion

    #region 协程

    private IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(hurtRecoilTime);
        _isMovable = true;
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

    #endregion

    #region 状态

    public abstract class PatrolState
    {
        public abstract bool checkValid(PatrolController enemyController);
        public abstract void Execute(PatrolController enemyController);
    }

    public class Patrol : State
    {
        private PatrolState _currentState;
        private int _currentStateCase = 0;
        private bool _isFinished;

        public Patrol()
        {
            _currentState = new Idle();
            _isFinished = true;
        }

        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            PatrolController patrolController = (PatrolController)enemyController;
            if (!_currentState.checkValid(patrolController) || _isFinished)
            {
                int randomStateCase;
                do
                {
                    randomStateCase = UnityEngine.Random.Range(0, 3);
                } while (randomStateCase == _currentStateCase);

                _currentStateCase = randomStateCase;
                switch (_currentStateCase)
                {
                    case 0:
                        _currentState = new Idle();
                        break;
                    case 1:
                        _currentState = new WalkingLeft();
                        break;
                    case 2:
                        _currentState = new WalkingRight();
                        break;
                }

                patrolController.StartCoroutine(executeCoroutine(patrolController.behaveInterval()));
            }

            _currentState.Execute(patrolController);
        }

        private IEnumerator executeCoroutine(float delay)
        {
            _isFinished = false;
            yield return new WaitForSeconds(delay);
            if (!_isFinished)
                _isFinished = true;
        }
    }

    public class Chase : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            PatrolController patrolController = (PatrolController)enemyController;
            float dist = patrolController.playerEnemyDistance();
            patrolController.walk(Math.Abs(dist) < 0.1f ? 0 : dist);
        }
    }

    public class Idle : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() == 0;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(0);
        }
    }

    public class WalkingLeft : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() != -1;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(-1);
        }
    }

    public class WalkingRight : PatrolState
    {
        public override bool checkValid(PatrolController patrolController)
        {
            return patrolController.reachEdge() != 1;
        }

        public override void Execute(PatrolController patrolController)
        {
            patrolController.walk(1);
        }
    }

    #endregion
}