using UnityEngine;

/// <summary>
/// ガチャの確率計算・抽選ロジックを担当する MonoBehaviour。
/// DataManager のパラメータを参照して最終確率を算出し、抽選を行う。
/// </summary>
public class GachaSystem : MonoBehaviour
{
    /// <summary>ガチャ1回あたりのコスト</summary>
    public const float GachaCost = 300f;

    /// <summary>基本排出確率 0.01%</summary>
    private const float BaseRate = 0.0001f;

    /// <summary>
    /// 現在のパラメータから最終的なガチャ確率を算出する。
    /// 最終確率 = (基本確率 + 徳×0.001) × (1.0 - 欲求値) + 乱数調整値
    /// </summary>
    public float CalculateProbability()
    {
        var dm = DataManager.Instance;
        float karmaBonus = dm.Karma * 0.001f;
        float desireDebuff = 1.0f - Mathf.Clamp01(dm.Desire);
        float prob = (BaseRate + karmaBonus) * desireDebuff + dm.LuckBias;
        return Mathf.Clamp(prob, 0f, 1f);
    }

    /// <summary>
    /// ガチャを1回実行する。
    /// 戻り値: true=当選（星5）、false=落選
    /// </summary>
    public bool Pull()
    {
        var dm = DataManager.Instance;

        // コスト消費
        dm.Money -= GachaCost;
        dm.GachaCount++;

        // 欲求値上昇（ガチャへの執着）
        dm.AddDesire(0.05f);

        // 確率計算
        float prob = CalculateProbability();

        // LuckBias はガチャ実行時に半減消費
        dm.LuckBias *= 0.5f;

        // 抽選
        float roll = Random.value;
        bool won = roll < prob;

        Debug.Log($"[Gacha] 確率={prob:P4} Roll={roll:F6} 結果={( won ? "★5 当選！！" : "落選…")} (累計{dm.GachaCount}回目)");

        return won;
    }

    /// <summary>
    /// 資金がガチャコスト以上あるかチェックする。
    /// </summary>
    public bool CanPull()
    {
        return DataManager.Instance.Money >= GachaCost;
    }

    /// <summary>
    /// 現在の確率を曖昧なテキストとして返す。
    /// </summary>
    public string GetProbabilityLabel()
    {
        float p = CalculateProbability() * 100f; // パーセント

        if (p < 0.1f) return "絶望的…";
        if (p < 0.5f) return "激渋…";
        if (p < 1.0f) return "ワンチャンあるかも…";
        if (p < 3.0f) return "いい感じかも！";
        return "今なら来る！！";
    }

    /// <summary>
    /// ハズレ時の星ランクを返す（演出用）。
    /// </summary>
    public int GetLoserRank()
    {
        float r = Random.value;
        if (r < 0.7f) return 3;  // 70% で星3
        return 4;                // 30% で星4
    }
}
