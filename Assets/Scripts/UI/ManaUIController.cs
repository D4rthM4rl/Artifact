using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaUIController : MonoBehaviour
{
    public static ManaUIController instance;
    public GameObject manaFill;
    private float fillValue;

    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    public void UpdateManaUI(float mana)
    {
        fillValue = mana / 100f;
        manaFill.GetComponent<Image>().fillAmount = fillValue;
    }
}
