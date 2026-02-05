using System.Collections.Generic;
using UnityEngine;

public class HideOnFirstLoad : MonoBehaviour
{
    [Tooltip("비워두면 GameObject 이름을 키로 사용합니다.")]
    [SerializeField] private string hideKey;

    private static readonly HashSet<string> HiddenKeys = new HashSet<string>();

    private void Start()
    {
        string key = string.IsNullOrWhiteSpace(hideKey) ? gameObject.name : hideKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (HiddenKeys.Contains(key))
        {
            return;
        }

        HiddenKeys.Add(key);
        gameObject.SetActive(false);
    }
}
