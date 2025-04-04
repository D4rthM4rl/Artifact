using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterInfo))]
[CanEditMultipleObjects]
public class CharacterEditor : Editor
{
    private Sprite lastCharacterImage;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        if (targets.Length == 1) // Ensure single-object editing
        {
            CharacterInfo info = (CharacterInfo)target;

            if (info.sprite != lastCharacterImage) // Only update if the sprite changed
            {
                lastCharacterImage = info.sprite;
                if (info.sprite != null)
                {
                    EditorApplication.delayCall += () => SetThumbnail(info, info.sprite.texture);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SetThumbnail(CharacterInfo character, Texture2D texture)
    {
        if (texture != null)
        {
            EditorGUIUtility.SetIconForObject(character, texture);
            EditorUtility.SetDirty(character);
            AssetDatabase.SaveAssets();
        }
    }
}
