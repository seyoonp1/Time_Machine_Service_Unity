using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionProgressTracker : MonoBehaviour
{
    public static InteractionProgressTracker Instance { get; private set; }

    [Header("Required Interactions")]
    [SerializeField] private string[] requiredIds;

    public bool IsAllInteracted => _required.Count > 0 && _completed.Count >= _required.Count;

    public event Action OnAllInteracted;

    private readonly HashSet<string> _required = new HashSet<string>();
    private readonly HashSet<string> _completed = new HashSet<string>();
    private bool _completedRaised;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshRequired();
    }

    private void RefreshRequired()
    {
        _required.Clear();
        if (requiredIds == null)
        {
            return;
        }

        foreach (string id in requiredIds)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }
            _required.Add(id.Trim());
        }
    }

    public void RegisterInteraction(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        if (_required.Count == 0)
        {
            RefreshRequired();
        }

        if (!_required.Contains(id))
        {
            return;
        }

        if (_completed.Add(id))
        {
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (_completedRaised || !IsAllInteracted)
        {
            return;
        }

        _completedRaised = true;
        OnAllInteracted?.Invoke();
    }
}
