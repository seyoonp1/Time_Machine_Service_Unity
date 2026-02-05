using UnityEngine;

public class ReturnPositionOnLoad : MonoBehaviour
{
    [SerializeField] private string playerObjectName = "player";

    private void Start()
    {
        ApplyReturnPosition();
        ApplyReturnTime();
        SceneReturnState.Clear();
    }

    private void ApplyReturnPosition()
    {
        if (!SceneReturnState.HasReturnPosition)
        {
            return;
        }

        GameObject playerObject = FindPlayerObject();
        if (playerObject == null)
        {
            return;
        }

        playerObject.transform.position = SceneReturnState.ReturnPosition;
    }

    private void ApplyReturnTime()
    {
        if (!SceneReturnState.HasReturnTime)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.SetCurrentTime(SceneReturnState.ReturnTime);
    }

    private GameObject FindPlayerObject()
    {
        GameObject named = GameObject.Find(playerObjectName);
        if (named != null)
        {
            return named;
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        return player != null ? player.gameObject : null;
    }
}
