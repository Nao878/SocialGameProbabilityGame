using UnityEngine;

/// <summary>
/// ゲーム全体のパラメータを管理するシングルトン。
/// 資金、徳、悪運、欲求値、各習熟度を一元管理する。
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("=== 基本パラメータ ===")]
    [Tooltip("ガチャや投資の原資")]
    public float Money = 500f;

    [Tooltip("善行で上昇。高いほどガチャ確率にプラス補正")]
    public float Karma = 0f;

    [Tooltip("悪運/乱数調整値。失敗で蓄積し、ガチャの揺らぎを増大させる")]
    public float LuckBias = 0f;

    [Tooltip("欲求値。高いほどガチャ確率にデバフ（物欲センサー）")]
    public float Desire = 0f;

    [Header("=== 習熟度 ===")]
    public int VolunteerProficiency = 0;
    public int WorkProficiency = 0;
    public int StudyProficiency = 0;
    public int InvestProficiency = 0;

    [Header("=== フラグ・カウンター ===")]
    public int GachaCount = 0;
    public bool HasStudied = false;

    /// <summary>欲求上昇抑制フラグ（日常アクション用）</summary>
    public bool DesireSuppressed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 欲求値を蓄積させる（ガチャ実行やアクション時に呼ぶ）。
    /// DesireSuppressed フラグがONの場合は上昇量を1/3にする。
    /// </summary>
    public void AddDesire(float amount)
    {
        if (DesireSuppressed)
        {
            amount *= 0.33f;
            DesireSuppressed = false; // 1回で効果切れ
        }
        Desire = Mathf.Clamp01(Desire + amount);
    }

    /// <summary>
    /// パラメータをリセットして最初からやり直す。
    /// </summary>
    public void ResetAll()
    {
        Money = 500f;
        Karma = 0f;
        LuckBias = 0f;
        Desire = 0f;
        VolunteerProficiency = 0;
        WorkProficiency = 0;
        StudyProficiency = 0;
        InvestProficiency = 0;
        GachaCount = 0;
        HasStudied = false;
        DesireSuppressed = false;
    }
}
