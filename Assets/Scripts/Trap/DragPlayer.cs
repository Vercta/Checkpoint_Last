using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动平台的player跟随
/// </summary>
public class DragPlayer : MonoBehaviour
{
    private MovingTrap _movingTrap;

    void Start()
    {
        _movingTrap = gameObject.GetComponent<MovingTrap>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject != GlobalController.Instance.player)
            return;

        Transform playerTransform = GlobalController.Instance.player.GetComponent<Transform>();
        Vector3 playerNewPosition = playerTransform.position;
        playerNewPosition.x += _movingTrap.movingSpeed * Time.deltaTime;
        playerTransform.position = playerNewPosition;
    }
}
