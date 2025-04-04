using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemInfo))]
[CanEditMultipleObjects]
public class ItemDetailsEditor : Editor
{
    private Sprite lastItemImage;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        if (targets.Length == 1) // Ensure single-object editing
        {
            ItemInfo item = (ItemInfo)target;

            if (item.itemImage != lastItemImage) // Only update if the sprite changed
            {
                lastItemImage = item.itemImage;
                if (item.itemImage != null)
                {
                    EditorApplication.delayCall += () => SetThumbnail(item, item.itemImage.texture);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SetThumbnail(ItemInfo item, Texture2D texture)
    {
        if (texture != null)
        {
            EditorGUIUtility.SetIconForObject(item, texture);
            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
        }
    }
}
