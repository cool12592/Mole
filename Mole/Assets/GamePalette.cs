using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewPalette", menuName = "Custom/PaletteObject")]
public class GamePalette : ScriptableObject
{
    [Serializable]
    public class ColorInfo
    {
        public Color color; // 🔴 색상 리스트 (Unity Inspector에서 설정)
        public Sprite[] spries;
    }
    [SerializeField] ColorInfo[] colorInfos; // 🔴 색상 리스트 (Unity Inspector에서 설정)


    // 🔴 인덱스를 받아 색상을 반환 (범위 초과 방지)
    public ColorInfo GetColorInfo(int index)
    {
        if (colorInfos == null || colorInfos.Length == 0)
        {
            Debug.LogError("Palette is empty!");
            return null; // 기본값: 흰색
        }

        if(colorInfos.Length <= index)
        {
            Debug.LogError("Palette Invalid Index!");
        }

        return colorInfos[index];
    }

    public int GetInfoLength()
    {
        return colorInfos.Length;
    }
}

