using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("HUD", LoadSceneMode.Additive);
    }
}
