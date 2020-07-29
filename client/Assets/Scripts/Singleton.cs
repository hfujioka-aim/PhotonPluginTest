using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    static GameObject Instance;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this.gameObject;
            GameObject.DontDestroyOnLoad(this.gameObject);
        } else {
            GameObject.Destroy(this.gameObject);
        }
    }
}
