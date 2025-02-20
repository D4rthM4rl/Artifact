using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaUIController : MonoBehaviour
{
    public static GameObject manaContainer;
    private static float fillValue;

    void Awake()
    {
        if (manaContainer == null)
        {
            manaContainer = GameObject.Find("ManaFill");
        }
    }

    // Update is called once per frame
    public static void UpdateManaUI(float mana)
    {
        fillValue = mana / 100f;
        manaContainer.GetComponent<Image>().fillAmount = fillValue;
    }
}
