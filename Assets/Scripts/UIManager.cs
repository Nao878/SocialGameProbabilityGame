using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// uGUI を制御する。メインパネル・ガチャパネル・結果パネルの管理、
/// ステータス表示の更新、ガチャ演出を担当する。
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("=== パネル ===")]
    public GameObject MainPanel;
    public GameObject GachaPanel;
    public GameObject ResultPanel;

    [Header("=== ステータス表示 (MainPanel) ===")]
    public Text MoneyText;
    public Text KarmaText;
    public Text DesireText;
    public Text LuckBiasText;
    public Text GachaCountText;

    [Header("=== アクティビティログ (MainPanel) ===")]
    public Text ActivityLogText;

    [Header("=== ガチャUI (GachaPanel) ===")]
    public Text ProbabilityLabel;
    public Button PullButton;
    public Text PullButtonText;
    public Text GachaCostText;

    [Header("=== 結果UI (ResultPanel) ===")]
    public Text ResultTitleText;
    public Text ResultMessageText;
    public Image ResultBackgroundImage;

    [Header("=== ボタン参照 ===")]
    public Button InvestButton;

    [Header("=== 参照 ===")]
    public GachaSystem gachaSystem;
    public ActivityManager activityManager;

    // ---- カラー定数 ----
    private readonly Color ColorGold = new Color(1f, 0.843f, 0f);          // #FFD700
    private readonly Color ColorRainbow = new Color(1f, 0.4f, 0.7f);       // ピンク系
    private readonly Color ColorDarkPanel = new Color(0.086f, 0.129f, 0.243f); // #16213E
    private readonly Color ColorLose = new Color(0.4f, 0.4f, 0.5f);

    private void Start()
    {
        ShowMainPanel();
        RefreshStatus();
        UpdateInvestButton();
    }

    // =========================================================
    // パネル切替
    // =========================================================

    public void ShowMainPanel()
    {
        MainPanel.SetActive(true);
        GachaPanel.SetActive(false);
        ResultPanel.SetActive(false);
    }

    public void ShowGachaPanel()
    {
        MainPanel.SetActive(false);
        GachaPanel.SetActive(true);
        ResultPanel.SetActive(false);
        RefreshGachaUI();
    }

    public void ShowResultPanel(bool won, int starRank)
    {
        ResultPanel.SetActive(true);
        GachaPanel.SetActive(false);

        if (won)
        {
            ResultTitleText.text = "★★★★★";
            ResultTitleText.color = ColorGold;
            ResultMessageText.text = "限定キャラ降臨！！\nおめでとう！！！";
            ResultMessageText.color = Color.white;
            if (ResultBackgroundImage != null)
                ResultBackgroundImage.color = ColorRainbow;
        }
        else
        {
            string stars = new string('★', starRank);
            ResultTitleText.text = stars;
            ResultTitleText.color = ColorLose;
            ResultMessageText.text = starRank == 4
                ? "星4…惜しい！でもハズレ！"
                : "星3…またお前か…";
            ResultMessageText.color = new Color(0.8f, 0.8f, 0.8f);
            if (ResultBackgroundImage != null)
                ResultBackgroundImage.color = ColorDarkPanel;
        }
    }

    // =========================================================
    // ステータス更新
    // =========================================================

    public void RefreshStatus()
    {
        var dm = DataManager.Instance;
        if (dm == null) return;

        MoneyText.text = $"💰 資金: {dm.Money:F0} 円";
        KarmaText.text = $"☯ 徳: {dm.Karma:F1}";
        DesireText.text = $"🔥 欲求: {dm.Desire:P1}";
        LuckBiasText.text = $"🍀 悪運: {dm.LuckBias:F4}";
        GachaCountText.text = $"🎰 ガチャ累計: {dm.GachaCount} 回";
    }

    public void RefreshGachaUI()
    {
        if (gachaSystem == null) return;
        ProbabilityLabel.text = $"現在の運勢: {gachaSystem.GetProbabilityLabel()}";

        bool canPull = gachaSystem.CanPull();
        PullButton.interactable = canPull;
        PullButtonText.text = canPull ? "ガチャを回す！" : "資金不足…";
        GachaCostText.text = $"1回 {GachaSystem.GachaCost:F0} 円";
    }

    // =========================================================
    // アクティビティログ
    // =========================================================

    public void ShowActivityLog(string message)
    {
        ActivityLogText.text = message;
    }

    // =========================================================
    // 投資ボタン更新
    // =========================================================

    public void UpdateInvestButton()
    {
        if (InvestButton != null)
        {
            InvestButton.interactable = DataManager.Instance.HasStudied;
        }
    }

    // =========================================================
    // ガチャ実行（ボタンから呼ばれる）
    // =========================================================

    public void OnPullButtonClicked()
    {
        if (gachaSystem == null || !gachaSystem.CanPull()) return;
        StartCoroutine(GachaSequence());
    }

    private IEnumerator GachaSequence()
    {
        PullButton.interactable = false;
        ProbabilityLabel.text = "抽選中...";

        // 演出: テキストが点滅するシンプルな待ち時間
        for (int i = 0; i < 6; i++)
        {
            ProbabilityLabel.text = i % 2 == 0 ? "✦ ✦ ✦" : "✧ ✧ ✧";
            yield return new WaitForSeconds(0.3f);
        }

        bool won = gachaSystem.Pull();
        int rank = won ? 5 : gachaSystem.GetLoserRank();

        // 星5なら追加演出
        if (won)
        {
            ProbabilityLabel.text = "！！！！！";
            ProbabilityLabel.color = ColorGold;
            yield return new WaitForSeconds(1.0f);
        }

        RefreshStatus();
        ShowResultPanel(won, rank);
        ProbabilityLabel.color = Color.white;
    }

    // =========================================================
    // 結果パネルから戻る
    // =========================================================

    public void OnResultCloseClicked()
    {
        ResultPanel.SetActive(false);
        ShowGachaPanel();
    }

    // =========================================================
    // メインパネルへ戻る（ガチャパネルから）
    // =========================================================

    public void OnBackToMainClicked()
    {
        ShowMainPanel();
    }
}
