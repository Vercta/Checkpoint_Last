using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 直接导致player或enemy死亡的陷阱类 用于spike;saw
/// </summary>
public class Deadly : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName == "Player")
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            playerController.hurt(playerController.health);
        }
        else if (layerName == "Enemy")
        {
            EnemyController enemyController = collision.collider.GetComponent<EnemyController>();
            enemyController.hurt(enemyController.health);
        }
    }
}
