using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Tools > Setup Gacha Savior メニューから実行。
/// Canvas・EventSystem・全パネル・全ボタン・GameManagerをシーンに自動生成する。
/// </summary>
public class SceneSetupTool
{
    // ---- カラーパレット ----
    private static readonly Color ColBG      = new Color32(26, 26, 46, 255);
    private static readonly Color ColPanel   = new Color32(22, 33, 62, 255);
    private static readonly Color ColButton  = new Color32(15, 52, 96, 255);
    private static readonly Color ColBtnHL   = new Color32(233, 69, 96, 255);
    private static readonly Color ColGold    = new Color32(255, 215, 0, 255);
    private static readonly Color ColText    = new Color32(240, 240, 240, 255);
    private static readonly Color ColTextDim = new Color32(180, 180, 200, 255);
    private static readonly Color ColGacha   = new Color32(40, 20, 60, 255);
    private static readonly Color ColGreen   = new Color32(80, 200, 120, 255);
    private static readonly Color ColYellow  = new Color32(255, 200, 50, 255);
    private static readonly Color ColRed     = new Color32(200, 60, 60, 255);

    [MenuItem("Tools/Setup Gacha Savior")]
    public static void Setup()
    {
        // ========== シーンクリア ==========
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = ColBG;

        // ========== EventSystem ==========
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ========== Canvas ==========
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ========== GameManager ==========
        var gmGO = new GameObject("GameManager");
        var dataManager = gmGO.AddComponent<DataManager>();
        var gachaSystem = gmGO.AddComponent<GachaSystem>();
        var activityManager = gmGO.AddComponent<ActivityManager>();
        var uiManager = gmGO.AddComponent<UIManager>();
        activityManager.uiManager = uiManager;
        uiManager.gachaSystem = gachaSystem;
        uiManager.activityManager = activityManager;

        // =============================================
        //  MAIN PANEL
        // =============================================
        var mainPanel = MakePanel("MainPanel", canvasGO, ColBG);
        Stretch(mainPanel);
        uiManager.MainPanel = mainPanel;

        // タイトル
        MakeText("TitleText", mainPanel, "🎮 Gacha Savior", 40, ColGold, TextAnchor.MiddleCenter,
            0f, 1f, 1f, 1f, 10f, -80f, -10f, -10f);

        // ---- ステータスバー ----
        var statusBar = MakePanel("StatusBar", mainPanel, new Color32(10, 15, 30, 230));
        SetRect(statusBar, 0f, 1f, 1f, 1f, 10f, -210f, -10f, -90f);

        uiManager.MoneyText = MakeText("MoneyText", statusBar, "💰 資金: 500 円", 24, ColGold,
            TextAnchor.MiddleLeft, 0f, 0.5f, 0.5f, 1f, 15f, -35f, 0f, 0f).GetComponent<Text>();

        uiManager.KarmaText = MakeText("KarmaText", statusBar, "☯ 徳: 0.0", 24, ColGreen,
            TextAnchor.MiddleLeft, 0.5f, 0.5f, 1f, 1f, 15f, -35f, -10f, 0f).GetComponent<Text>();

        uiManager.DesireText = MakeText("DesireText", statusBar, "🔥 欲求: 0.0%", 24, ColRed,
            TextAnchor.MiddleLeft, 0f, 0f, 0.5f, 0.5f, 15f, 0f, 0f, 35f).GetComponent<Text>();

        uiManager.LuckBiasText = MakeText("LuckBiasText", statusBar, "🍀 悪運: 0.0000", 24, ColYellow,
            TextAnchor.MiddleLeft, 0.5f, 0f, 1f, 0.5f, 15f, 0f, -10f, 35f).GetComponent<Text>();

        uiManager.GachaCountText = MakeText("GachaCountText", mainPanel, "🎰 ガチャ累計: 0 回", 22, ColTextDim,
            TextAnchor.MiddleCenter, 0f, 1f, 1f, 1f, 10f, -250f, -10f, -215f).GetComponent<Text>();

        // ---- 棒人間 ----
        var stickFigure = MakePanel("StickFigure", mainPanel, new Color32(60, 60, 80, 200));
        SetRect(stickFigure, 0.25f, 0.52f, 0.75f, 0.75f, 0f, 0f, 0f, 0f);
        MakeText("StickLabel", stickFigure, "( ˘ω˘ )\n棒人間", 36, ColText,
            TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

        // ---- アクティビティボタン群 ----
        // ボタンは mainPanel の anchor top=1 から下方向に配置
        // 行1: 善行 | 労働
        float bTop = -270f; float bH = 65f; float bGap = 8f;

        var btn1 = MakeButton("BtnVolunteer", mainPanel, "♻ 善行", 22);
        SetRect(btn1, 0.02f, 1f, 0.49f, 1f, 0f, bTop - bH, 0f, bTop);
        btn1.GetComponent<Button>().onClick.AddListener(activityManager.DoVolunteer);

        var btn2 = MakeButton("BtnWork", mainPanel, "💼 労働", 22);
        SetRect(btn2, 0.51f, 1f, 0.98f, 1f, 0f, bTop - bH, 0f, bTop);
        btn2.GetComponent<Button>().onClick.AddListener(activityManager.DoWork);

        bTop -= (bH + bGap);
        // 行2: ギャンブル | 勉強
        var btn3 = MakeButton("BtnGamble", mainPanel, "🎰 ギャンブル", 22);
        SetRect(btn3, 0.02f, 1f, 0.49f, 1f, 0f, bTop - bH, 0f, bTop);
        btn3.GetComponent<Button>().onClick.AddListener(activityManager.DoGamble);

        var btn4 = MakeButton("BtnStudy", mainPanel, "📚 勉強", 22);
        SetRect(btn4, 0.51f, 1f, 0.98f, 1f, 0f, bTop - bH, 0f, bTop);
        btn4.GetComponent<Button>().onClick.AddListener(activityManager.DoStudy);

        bTop -= (bH + bGap);
        // 行3: 投資 | 瞑想
        var btn5 = MakeButton("BtnInvest", mainPanel, "📈 投資", 22);
        SetRect(btn5, 0.02f, 1f, 0.49f, 1f, 0f, bTop - bH, 0f, bTop);
        btn5.GetComponent<Button>().onClick.AddListener(activityManager.DoInvest);
        uiManager.InvestButton = btn5.GetComponent<Button>();

        var btn6 = MakeButton("BtnMeditate", mainPanel, "🧘 瞑想", 22);
        SetRect(btn6, 0.51f, 1f, 0.98f, 1f, 0f, bTop - bH, 0f, bTop);
        btn6.GetComponent<Button>().onClick.AddListener(activityManager.DoMeditate);

        bTop -= (bH + bGap);
        // 行4: 日常（笑顔・感謝） - 全幅
        var btn7 = MakeButton("BtnGratitude", mainPanel, "😊 日常（笑顔・感謝）", 22);
        SetRect(btn7, 0.02f, 1f, 0.98f, 1f, 0f, bTop - bH, 0f, bTop);
        btn7.GetComponent<Button>().onClick.AddListener(activityManager.DoDailyGratitude);

        bTop -= (bH + bGap + 5);
        // 行5: ガチャ画面へ - 特大ボタン
        var btnGacha = MakeButton("BtnOpenGacha", mainPanel, "🌟 ガチャ画面へ 🌟", 28);
        SetRect(btnGacha, 0.05f, 1f, 0.95f, 1f, 0f, bTop - (bH + 15), 0f, bTop);
        btnGacha.GetComponent<Image>().color = ColBtnHL;
        var gachaColors = btnGacha.GetComponent<Button>().colors;
        gachaColors.normalColor = ColBtnHL;
        gachaColors.highlightedColor = new Color32(255, 100, 120, 255);
        btnGacha.GetComponent<Button>().colors = gachaColors;
        btnGacha.GetComponent<Button>().onClick.AddListener(uiManager.ShowGachaPanel);

        // ---- アクティビティログ ----
        var logBG = MakePanel("LogBG", mainPanel, new Color32(10, 10, 20, 200));
        SetRect(logBG, 0.02f, 0.02f, 0.98f, 0.08f, 0f, 0f, 0f, 0f);
        uiManager.ActivityLogText = MakeText("ActivityLogText", logBG, "何かアクションを選んでください…", 20, ColTextDim,
            TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f, 10f, 0f, -10f, 0f).GetComponent<Text>();

        // =============================================
        //  GACHA PANEL
        // =============================================
        var gachaPanel = MakePanel("GachaPanel", canvasGO, ColGacha);
        Stretch(gachaPanel);
        uiManager.GachaPanel = gachaPanel;

        // タイトル
        MakeText("GachaTitleText", gachaPanel, "✨ 限定ガチャ ✨\n〜伝説の星5を求めて〜", 36, ColGold,
            TextAnchor.MiddleCenter, 0f, 1f, 1f, 1f, 10f, -130f, -10f, -20f);

        // 確率ラベル
        uiManager.ProbabilityLabel = MakeText("ProbabilityLabel", gachaPanel, "現在の運勢: ---", 28, ColText,
            TextAnchor.MiddleCenter, 0.1f, 0.6f, 0.9f, 0.68f, 0f, 0f, 0f, 0f).GetComponent<Text>();

        // 演出エリア
        var gachaVisual = MakePanel("GachaVisual", gachaPanel, new Color32(20, 10, 40, 200));
        SetRect(gachaVisual, 0.15f, 0.35f, 0.85f, 0.58f, 0f, 0f, 0f, 0f);
        MakeText("GachaVisualEmoji", gachaVisual, "🎁", 80, ColGold,
            TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

        // コスト表示
        uiManager.GachaCostText = MakeText("GachaCostText", gachaPanel, "1回 300 円", 22, ColTextDim,
            TextAnchor.MiddleCenter, 0.2f, 0.26f, 0.8f, 0.32f, 0f, 0f, 0f, 0f).GetComponent<Text>();

        // ガチャを回すボタン
        var pullBtn = MakeButton("PullButton", gachaPanel, "ガチャを回す！", 30);
        SetRect(pullBtn, 0.15f, 0.12f, 0.85f, 0.24f, 0f, 0f, 0f, 0f);
        pullBtn.GetComponent<Image>().color = ColBtnHL;
        var pullColors = pullBtn.GetComponent<Button>().colors;
        pullColors.normalColor = ColBtnHL;
        pullColors.highlightedColor = new Color32(255, 100, 120, 255);
        pullBtn.GetComponent<Button>().colors = pullColors;
        pullBtn.GetComponent<Button>().onClick.AddListener(uiManager.OnPullButtonClicked);
        uiManager.PullButton = pullBtn.GetComponent<Button>();
        uiManager.PullButtonText = pullBtn.GetComponentInChildren<Text>();

        // 戻るボタン
        var backBtn = MakeButton("BackButton", gachaPanel, "← 戻る", 22);
        SetRect(backBtn, 0.3f, 0.03f, 0.7f, 0.10f, 0f, 0f, 0f, 0f);
        backBtn.GetComponent<Button>().onClick.AddListener(uiManager.OnBackToMainClicked);

        // =============================================
        //  RESULT PANEL
        // =============================================
        var resultPanel = MakePanel("ResultPanel", canvasGO, ColPanel);
        Stretch(resultPanel);
        uiManager.ResultPanel = resultPanel;
        uiManager.ResultBackgroundImage = resultPanel.GetComponent<Image>();

        uiManager.ResultTitleText = MakeText("ResultTitleText", resultPanel, "★★★", 60, ColGold,
            TextAnchor.MiddleCenter, 0.1f, 0.55f, 0.9f, 0.75f, 0f, 0f, 0f, 0f).GetComponent<Text>();

        uiManager.ResultMessageText = MakeText("ResultMessageText", resultPanel, "結果はいかに…", 28, ColText,
            TextAnchor.MiddleCenter, 0.1f, 0.35f, 0.9f, 0.55f, 0f, 0f, 0f, 0f).GetComponent<Text>();

        var closeBtn = MakeButton("CloseResultButton", resultPanel, "OK", 28);
        SetRect(closeBtn, 0.25f, 0.12f, 0.75f, 0.24f, 0f, 0f, 0f, 0f);
        closeBtn.GetComponent<Image>().color = ColBtnHL;
        var closeColors = closeBtn.GetComponent<Button>().colors;
        closeColors.normalColor = ColBtnHL;
        closeBtn.GetComponent<Button>().colors = closeColors;
        closeBtn.GetComponent<Button>().onClick.AddListener(uiManager.OnResultCloseClicked);

        // ========== 初期状態 ==========
        gachaPanel.SetActive(false);
        resultPanel.SetActive(false);

        // ========== シーン保存 ==========
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        Debug.Log("[SceneSetupTool] ✅ Gacha Savior シーンのセットアップが完了しました！");
    }

    // =============================================
    //  ヘルパーメソッド
    // =============================================

    private static GameObject MakePanel(string name, GameObject parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetRect(GameObject go, float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(aMinX, aMinY);
        rt.anchorMax = new Vector2(aMaxX, aMaxY);
        rt.offsetMin = new Vector2(oMinX, oMinY);
        rt.offsetMax = new Vector2(oMaxX, oMaxY);
    }

    private static GameObject MakeText(string name, GameObject parent, string text, int fontSize, Color color,
        TextAnchor align, float aMinX, float aMinY, float aMaxX, float aMaxY,
        float oMinX, float oMinY, float oMaxX, float oMaxY)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        SetRect(go, aMinX, aMinY, aMaxX, aMaxY, oMinX, oMinY, oMaxX, oMaxY);

        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        return go;
    }

    private static GameObject MakeButton(string name, GameObject parent, string label, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);

        var img = go.AddComponent<Image>();
        img.color = ColButton;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = ColButton;
        colors.highlightedColor = ColBtnHL;
        colors.pressedColor = new Color(ColButton.r * 0.7f, ColButton.g * 0.7f, ColButton.b * 0.7f);
        btn.colors = colors;

        // テキスト子要素
        var txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(go.transform, false);
        Stretch(txtGO);

        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = fontSize;
        txt.color = ColText;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.supportRichText = true;

        return go;
    }
}
