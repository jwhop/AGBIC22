using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    public GameObject currentIngredient;

    public static MouseHandler Instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        currentIngredient = null;
    }



}
