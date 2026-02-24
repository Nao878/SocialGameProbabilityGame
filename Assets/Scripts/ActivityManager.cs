using UnityEngine;

/// <summary>
/// 各アクティビティ（コマンド）のロジックを実装する。
/// ボタンクリックから呼ばれ、DataManager のパラメータを操作する。
/// </summary>
public class ActivityManager : MonoBehaviour
{
    [Header("参照")]
    public UIManager uiManager;

    // =========================================================
    // 善行（ボランティア・ゴミ拾い）
    // =========================================================
    public void DoVolunteer()
    {
        var dm = DataManager.Instance;
        float bonus = dm.VolunteerProficiency * 0.5f;
        float karmaGain = Random.Range(5f, 15f) + bonus;
        dm.Karma += karmaGain;
        dm.VolunteerProficiency++;
        dm.AddDesire(0.01f);

        string msg = $"♻ ボランティア完了！ 徳 +{karmaGain:F1}（習熟度 Lv.{dm.VolunteerProficiency}）";
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }

    // =========================================================
    // 労働（アルバイト）
    // =========================================================
    public void DoWork()
    {
        var dm = DataManager.Instance;
        float bonus = dm.WorkProficiency * 10f;
        float earnings = Random.Range(100f, 300f) + bonus;
        dm.Money += earnings;
        dm.WorkProficiency++;
        dm.AddDesire(0.02f);

        string msg = $"💼 バイト完了！ 資金 +{earnings:F0}円（習熟度 Lv.{dm.WorkProficiency}）";
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }

    // =========================================================
    // ギャンブル（パチスロ）
    // =========================================================
    public void DoGamble()
    {
        var dm = DataManager.Instance;

        // 徳を大きく失う
        dm.Karma = Mathf.Max(0f, dm.Karma - 10f);
        dm.AddDesire(0.03f);

        bool win = Random.value < 0.5f;
        string msg;

        if (win)
        {
            float prize = 500f;
            dm.Money += prize;
            dm.LuckBias += 0.005f;
            msg = $"🎰 パチスロ 勝利！ 資金 +{prize:F0}円（でも徳 -10 …）";
        }
        else
        {
            float loss = 300f;
            dm.Money = Mathf.Max(0f, dm.Money - loss);
            float luckGain = Random.Range(0.005f, 0.02f);
            dm.LuckBias += luckGain;
            msg = $"🎰 パチスロ 敗北… 資金 -{loss:F0}円 → 悪運 +{luckGain:F4}（意外と悪くない…かも）";
        }

        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }

    // =========================================================
    // 自己研鑽（勉強）
    // =========================================================
    public void DoStudy()
    {
        var dm = DataManager.Instance;
        dm.HasStudied = true;
        dm.StudyProficiency++;
        dm.AddDesire(0.01f);

        string msg = $"📚 勉強した！ 投資が解禁された（勉強 Lv.{dm.StudyProficiency}）";
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
        uiManager.UpdateInvestButton();
    }

    // =========================================================
    // 自己研鑽（投資）
    // =========================================================
    public void DoInvest()
    {
        var dm = DataManager.Instance;

        if (!dm.HasStudied)
        {
            string warn = "⚠ まず勉強して投資を解禁しよう！";
            uiManager.ShowActivityLog(warn);
            return;
        }

        if (dm.Money <= 0)
        {
            uiManager.ShowActivityLog("⚠ 投資する資金がない！");
            return;
        }

        float successRate = 0.30f + dm.InvestProficiency * 0.02f + dm.StudyProficiency * 0.02f;
        successRate = Mathf.Clamp(successRate, 0f, 0.80f);

        bool success = Random.value < successRate;
        string msg;

        if (success)
        {
            float gain = dm.Money * 0.5f;
            dm.Money += gain;
            dm.InvestProficiency++;
            msg = $"📈 投資成功！ 資金 +{gain:F0}円（成功率 {successRate:P0}）";
        }
        else
        {
            float loss = dm.Money * 0.2f;
            dm.Money -= loss;
            float luckGain = 0.01f;
            dm.LuckBias += luckGain;
            dm.InvestProficiency++;
            msg = $"📉 投資失敗… 資金 -{loss:F0}円 → 悪運 +{luckGain:F4}";
        }

        dm.AddDesire(0.01f);
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }

    // =========================================================
    // 精神統一（瞑想）
    // =========================================================
    public void DoMeditate()
    {
        var dm = DataManager.Instance;
        float before = dm.Desire;
        dm.Desire *= 0.5f;
        float reduced = before - dm.Desire;

        string msg = $"🧘 瞑想完了。欲求値 -{reduced:F3}（現在 {dm.Desire:F3}）";
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }

    // =========================================================
    // 日常（笑顔・感謝）
    // =========================================================
    public void DoDailyGratitude()
    {
        var dm = DataManager.Instance;
        dm.DesireSuppressed = true;
        dm.Karma += 1f;

        string msg = "😊 笑顔で感謝！ 徳 +1、次のアクションの欲求上昇を抑制";
        Debug.Log(msg);
        uiManager.ShowActivityLog(msg);
        uiManager.RefreshStatus();
    }
}
