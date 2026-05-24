using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LogManager : MonoBehaviour
{
    [SerializeField]
    private InteractionManager interactionManager;
    [SerializeField]
    private TMP_Text logField, hpField;
    [SerializeField]
    private List<string> turnLog;
    private int turnLogIndex;

    private InputAction browse;

    private void Awake() {
        interactionManager.RegisterLogManager(this);
        logField.text = "";
    }

    private void Update() {
        browse = InputSystem.actions.FindAction("Browse");
        if (browse.WasPressedThisFrame()) {
            BrowseTurnLog();
        }
    }

    public void SendToLog(string log) {
        turnLogIndex = turnLog.Count;
        turnLog.Add(log);
        ShowLog();
    }

    public void BrowseTurnLog() {
        if (turnLog.Count == 0) return;
        turnLogIndex += 1;
        turnLogIndex %= turnLog.Count;
        ShowLog();
    }

    private void ShowLog() {
        logField.text = turnLog[turnLogIndex];
        if (turnLog.Count > 1) {
            logField.text += " [(B)rowse]";
        }
    }

    public void SetHP(float current, float max) {
        hpField.text = $"HP {current:0}/{max:0}";
    }

    public void Reset() {
        logField.text = "";
        turnLog.Clear();
    }
}