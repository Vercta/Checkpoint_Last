using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 障碍物
/// </summary>
public class Obstacle : MonoBehaviour
{
    public void destroy()
    {
        Destroy(gameObject);
    }
}
