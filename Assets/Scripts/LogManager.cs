using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LogManager : MonoBehaviour
{
    [SerializeField]
    private InteractionManager interactionManager;
    [SerializeField]
    private TMP_Text logField, hpField, expField, floorField;
    private List<string> turnLog;
    private int turnLogIndex;
    private List<Interaction> interactions;

    private InputAction browse;
    private InputAction confirm;

    private void Awake() {
        turnLog = new();
        interactions = new();
        turnLogIndex = 0;
        interactionManager.RegisterLogManager(this);
        logField.text = "";
    }

    private void Update() {
        browse = InputSystem.actions.FindAction("Browse");
        confirm = InputSystem.actions.FindAction("Confirm");
        if (browse.WasPressedThisFrame()) {
            BrowseTurnLog();
        }
        if (confirm.WasPressedThisFrame()) {
            Interaction[] possibleInteraction = interactions.Where(i => i.logIndex == turnLogIndex).ToArray();
            if (possibleInteraction.Length > 0) {
                possibleInteraction.First().affirmation.Invoke();
            }
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
        if (interactions.Select(i => i.logIndex).Contains(turnLogIndex)) {
            logField.text += "[(C)onfirm]";
        }
        if (turnLog.Count > 1) {
            logField.text += " [(B)rowse]";
        }
    }

    public void SetHP(float current, float max) {
        hpField.text = $"HP {current:0}/{max:0}";
    }

    public void SetEXP(int currentLevel) {
        expField.text = $"Level {currentLevel}";
    }

    public void SetFloor(int floor) {
        floorField.text = $"Floor {floor}";
    }

    public void Reset() {
        logField.text = "";
        turnLog.Clear();
        interactions.Clear();
        turnLogIndex = 0;
    }

    public void QueueInteraction(string text, Action affirmation) {
        int index = turnLog.Count;
        interactions.Add(new Interaction() {
            text = text,
            affirmation = affirmation,
            logIndex = index,
        });

        SendToLog(text);
    }

    private class Interaction
    {
        public string text;
        public Action affirmation;
        public int logIndex;
    }
}