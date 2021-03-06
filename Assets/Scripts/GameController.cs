﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private TextMeshProUGUI weekValue;
    private TextMeshProUGUI moneyValue;
    private Transform effectList;

    private int weekNumber;
    private PillManager pillManager;
    private PlayerManager playerManager;
    private EventManager eventManager;
    private List<string> effectsNames;
    DoctorData currentDoctorData;

    [SerializeField]
    PillImagesData pillImages;
    [SerializeField]
    PillImagesData fruitImages;

    private HospitalController hospitalController;
    private HospitalHandler hospitalHandler;

    // Start is called before the first frame update
    private void Start()
    {
        InitializeGame();
        InitTurn();
    }
    /// <summary>
    /// Initialize game data
    /// </summary>
    private void InitializeGame()
    {
        InitGameObjectReferences();
        weekNumber = 0;
        effectsNames = new List<string>();
        GameContext.pills.Pills.ForEach(pill => pill.alreadyTaken = false);
        hospitalHandler.ToggleBlue(true);       
        pillManager = new PillManager(pillImages,fruitImages);
        playerManager = new PlayerManager();
        eventManager = new EventManager();

        ChangeStatementIfNeeded(true);
    }

    /// <summary>
    /// Game objects initialization, if found
    /// </summary>
    private void InitGameObjectReferences()
    {
        GameObject weekGameObject = GameObject.FindGameObjectWithTag("WeekText");
        if (weekGameObject != null)
        {
            weekValue = weekGameObject.GetComponent<TextMeshProUGUI>();
        }
        GameObject moneyGameObject = GameObject.FindGameObjectWithTag("MoneyValue");
        moneyValue = moneyGameObject.GetComponent<TextMeshProUGUI>();

        GameObject effectListGameObject = GameObject.FindGameObjectWithTag("EffectList");
        if (effectListGameObject != null)
        {
            effectList = effectListGameObject.transform;
        }

        GameObject hospitalGameObject = GameObject.FindGameObjectWithTag("HospitalController");
        if (hospitalGameObject != null)
        {
            hospitalController = hospitalGameObject.GetComponent<HospitalController>();
        }

        GameObject moneyPanelGameObject = GameObject.FindGameObjectWithTag("MoneyPanel");
        if (moneyPanelGameObject != null)
        {
            hospitalHandler = moneyPanelGameObject.GetComponent<HospitalHandler>();
        }
    }

    /// <summary>
    /// Initialize new turn
    /// </summary>
    private void InitTurn()
    {
        playerManager.DecreaseSideEffects();
        weekNumber++;
        ChangeStatementIfNeeded(false);
        pillManager.FetchPills(playerManager.level);
        playerManager.level += 2;
        UpdateGUI();
        eventManager.update(playerManager.player);
    }

    internal void Transplant(ShopItem shopItem)
    {
        playerManager.Transplant(shopItem);
        UpdateGUI();
    }
    void ChangeStatementIfNeeded(bool force)
    {
        if (weekNumber % 5 == 0 || force)
        {
            DoctorData doctorData;
            int tryCount = 0;
            do
            {
                int statementNumber = UnityEngine.Random.Range(0, GameContext.doctorStatements.Count);
                doctorData = GameContext.doctorStatements[statementNumber];
                tryCount++;
            } while (tryCount < 5 && (currentDoctorData == null || currentDoctorData.statement == doctorData.statement));
            currentDoctorData = doctorData;
            GameObject.FindGameObjectWithTag("MainCanvas").GetComponentInChildren<CommentController>().SetCatchPhrase(doctorData != null ? doctorData.statement : "Hello.");
        }
    }

    /// <summary>
    /// Trigger the game over event
    /// </summary>
    public void TriggerGameOver()
    {
        Debug.Log("GAME OVER");
        GameObject.FindGameObjectWithTag("MainCanvas").GetComponentInChildren<CommentController>().ToggleGameOverComment(true);
        GameObject.FindGameObjectWithTag("MainCanvas").GetComponentInChildren<CommentController>().PlayAnimationForward();
    }

    /// <summary>
    /// Update different GUI elements with data
    /// </summary>
    public void UpdateGUI()
    {
        if (weekValue != null)
        {
            weekValue.SetText(weekNumber.ToString());
        }        
        else
        {
            Debug.LogWarning("Week text element is not defined !");
        }
        moneyValue.SetText(playerManager.player.getCurrency());

        pillManager.UpdateGUI(playerManager.player.currentEffects);
        playerManager.UpdateGUI();
        hospitalController.UpdateGUI(playerManager.player);
        hospitalHandler.UpdateGUI(playerManager.player.body);
    }

    /// <summary>
    /// Button action to use a pill
    /// </summary>
    /// <param name="position">Position of the button, from 0 to 2</param>
    public void UsePill(int position)
    {
        PillData pill = pillManager.ConsumePill(position);
        if (playerManager.EatPill(pill))
        {
            InitTurn();
        }
    }
}
