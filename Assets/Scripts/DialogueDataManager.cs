using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ToArray 사용을 위해 필요

public class DialogueDataManager : MonoBehaviour
{
    // 싱글톤 패턴 (시간이 없으므로 가장 빠른 접근 방식 사용)
    public static DialogueDataManager Instance;

    // ID를 키로, 대사 목록을 값으로 저장하는 딕셔너리
    private Dictionary<string, List<string>> dialogueMap = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadDialogueData();
    }

    // CSV 파일 로드 및 파싱
    void LoadDialogueData()
    {
        // Resources 폴더 안의 "DialogueData" 파일을 TextAsset으로 불러옴 (.csv 확장자 제외)
        TextAsset data = Resources.Load<TextAsset>("DialogueData");

        if (data == null)
        {
            Debug.LogError("DialogueData.csv not found in Resources folder.");
            return;
        }

        // 줄바꿈으로 행 분리
        string[] lines = data.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue; // 빈 줄 무시

            // 쉼표로 ID와 대사 분리 (단, 대사 안에 쉼표가 없다는 가정. 있다면 탭 분리 방식 권장)
            // 더 안전하게 하려면 CSV Reader 라이브러리를 써야 하지만, 급하므로 Split 사용
            string[] parts = line.Split(new char[] { '\t' }, 2); // 2개로만 쪼개서 대사 내부 콤마 보존 시도

            if (parts.Length < 2) continue;

            string id = parts[0].Trim();
            string text = parts[1].Trim();

            // 딕셔너리에 추가
            if (!dialogueMap.ContainsKey(id))
            {
                dialogueMap[id] = new List<string>();
            }
            dialogueMap[id].Add(text);
        }
    }

    // 외부에서 호출할 함수: ID를 주면 string 배열 반환
    public string[] GetDialogue(string objectID)
    {
        if (dialogueMap.ContainsKey(objectID))
        {
            return dialogueMap[objectID].ToArray();
        }

        Debug.LogWarning($"Dialogue ID '{objectID}' not found.");
        return new string[] { "..." }; // 기본 대사 반환
    }
}
