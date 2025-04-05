using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUIController : MonoBehaviour
{
    public WeaponItemController[] weapons;
    private GameObject[] boxes;
    public Color highlightedBoxColor = new Color(.9f, .9f, .9f);
    public Color otherBoxColor = new Color(.3f, .3f, .3f);
    public int boxSelected;
    public GameObject boxPrefab;
    private Transform hotbarParent;
    public int hotbarSize;
    private bool boxesDrawn = false;

    private GameObject playerObject;
    private Player player;
    public static HotbarUIController instance;

    void Start()
    {
        instance = this;
        playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.GetComponent<Player>();
        hotbarParent = transform.parent.transform;
        weapons = new WeaponItemController[hotbarSize];
        DoHotbar();
        HighlightBox(boxSelected, highlightedBoxColor, otherBoxColor);
        boxSelected = 1;
    }

    /// <summary>
    /// Checks for item selection change
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown("1") && boxSelected != 1)
        {
            boxSelected = 1;
            HighlightBox(0, highlightedBoxColor, otherBoxColor);
            player.ChangeWeapon(0);
        } else if (Input.GetKeyDown("2") && boxSelected != 2)
        {
            boxSelected = 2;
            HighlightBox(1, highlightedBoxColor, otherBoxColor);
            player.ChangeWeapon(1);
        } else if (Input.GetKeyDown("3")  && boxSelected != 3)
        {
            boxSelected = 3;
            HighlightBox(2, highlightedBoxColor, otherBoxColor);
            player.ChangeWeapon(2);
        } else if (Input.GetKeyDown("4") && boxSelected != 4)
        {
            boxSelected = 4;
            HighlightBox(3, highlightedBoxColor, otherBoxColor);
            player.ChangeWeapon(3);
        } else if (Input.GetKeyDown("5") && boxSelected != 5)
        {
            boxSelected = 5;
            HighlightBox(4, highlightedBoxColor, otherBoxColor);
            player.ChangeWeapon(4);
        }
    }

    /// <summary>
    /// Colors all the hotbar boxes
    /// </summary>
    /// <param name="boxNum">Index of box to color special color</param>
    /// <param name="c">Color to color special box</param>
    /// <param name="other">Color to color other boxes</param>
    private void HighlightBox(int boxNum, Color c, Color other)
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            SpriteRenderer sr = boxes[i].GetComponent<SpriteRenderer>();
            if (i == boxNum)
            {
                sr.color = c;
                sr.sortingOrder = 1;
            }
            else 
            {
                sr.color = other;
                sr.sortingOrder = 0;
            }
        }
    }

    /// <summary>
    /// Draws a box in the hotbar and titles and places it as directed by which number it is
    /// </summary>
    /// <param name="numBox">The number that the box is in the hotbar (0-indexed)</param>
    public void DrawBoxes(int numBox)
    {
        float boxSpacing = 10f; // Adjust this value to control the spacing between hearts
        float boxSize = 32f;
        float xAdjust = boxSize + 10f;
        float yAdjust = -(boxSize-32);
        // Calculate heart position
        float xPos = (-numBox - 2) * (boxSpacing + boxSize);
        // Instantiate box and item inside
        GameObject box = Instantiate(boxPrefab, hotbarParent);
        box.name = "Hotbar Box " + (hotbarSize - 1 - numBox);
        box.transform.localPosition = new Vector3(xPos +  xAdjust, yAdjust, 0f);
        box.transform.localScale = new Vector2(32, 32);
        box.GetComponent<SpriteRenderer>().color = otherBoxColor;
        boxes[hotbarSize - 1 - numBox] = box;
    }

    /// <summary>
    /// Places the item sprites in the hotbar and highlights the currently selected one
    /// </summary>
    public void DoHotbar()
    {
        if (!boxesDrawn) boxes = new GameObject[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            if (!boxesDrawn) DrawBoxes(i);
            WeaponItemController currWeapon = weapons[i];
            
            if (currWeapon)
            {
                GameObject weaponSpot = new GameObject(currWeapon.info.name);
                weaponSpot.transform.position = boxes[i].transform.position;
                weaponSpot.transform.parent = boxes[i].transform;

                SpriteRenderer weaponRenderer = weaponSpot.AddComponent<SpriteRenderer>();
                weaponRenderer.sortingLayerName = "UI";
                weaponRenderer.sprite = currWeapon.info.itemImage;
                weaponRenderer.material = GameController.instance.defaultMaterial;
                weaponSpot.transform.localScale = new Vector2(1, 1);

                if (i == boxSelected - 1) player.GetComponent<Player>().ChangeWeapon(i);
            }
        }
        boxesDrawn = true;
    }
}
