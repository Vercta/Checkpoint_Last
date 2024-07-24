using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 开关：使用J键（攻击键进行互动）
/// </summary>
public class Switch : MonoBehaviour
{
    public Sprite triggered;
    public GameObject obstacle;
    public GameObject trap;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void turnOn()
    {
        _spriteRenderer.sprite = triggered;

        obstacle.GetComponent<Obstacle>().destroy();

        trap.GetComponent<Trap>().trigger();

        gameObject.layer = LayerMask.NameToLayer("Decoration");
    }
}
