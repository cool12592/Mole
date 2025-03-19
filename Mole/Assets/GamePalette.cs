using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewPalette", menuName = "Custom/PaletteObject")]
public class GamePalette : ScriptableObject
{
    public Color[] colors; // 🔴 색상 리스트 (Unity Inspector에서 설정)

    public int MaxColors => colors.Length; // 🔴 색상 개수 반환
    int index = 0;

   
    public void Init()
    {
        index = 0;
    }
    // 🔴 인덱스를 받아 색상을 반환 (범위 초과 방지)
    public Color GetColor()
    {
        if (colors == null || colors.Length == 0)
        {
            Debug.LogWarning("Palette is empty!");
            return Color.white; // 기본값: 흰색
        }

        return colors[index++ % colors.Length]; // 🔄 순환 구조 (넘어가면 처음으로)
    }
}

