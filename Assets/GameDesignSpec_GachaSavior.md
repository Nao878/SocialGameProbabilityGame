## ゲーム概要

- **タイトル**: Gacha Savior（仮）
- **ジャンル**: 確率・欲求・行動の関係を可視化する「ソシャゲ風ガチャ×ライフシミュレーション」
- **プレイ時間想定**: 1セッション数分〜
- **対象**: ガチャの確率・射幸性・欲求の関係を体験的に学びたいプレイヤー／実験・検証用途

### コンセプト

- **「徳」「悪運」「欲求」がガチャ確率にどう影響するかを、行動選択を通じて体験させるミニゲーム**。
- プレイヤーは資金と時間を使いながら
  - 善行・労働・投資・ギャンブル・勉強・瞑想・日常の笑顔/感謝
  を行い、その結果として「徳 (Karma)」「悪運 (LuckBias)」「欲求 (Desire)」などのパラメータが変化する。
- 最終的なガチャ当選確率は、これらのパラメータから算出される数式で決まり、「物欲センサー」や「天井／悪運の蓄積」的な感覚を再現する。

### コアゲームループ（高レベル）

1. **メインパネルでアクティビティを選択**  
   - 善行・労働・ギャンブル・勉強・投資・瞑想・日常（笑顔/感謝）を実行。
2. **各アクティビティでパラメータが変動**  
   - `Money`・`Karma`・`LuckBias`・`Desire`・各種習熟度が増減する。
3. **十分な資金が貯まったらガチャ画面に移動**  
   - 現在のパラメータから**ガチャ当選確率**が計算され、「現在の運勢」テキストとして表示。
4. **ガチャを回す**  
   - ガチャコストを支払い、抽選を行い、結果パネルに演出付きで表示。
   - 結果に応じて一部パラメータ（特に`LuckBias`や`Desire`）が再び変動。
5. **再びメインパネルに戻り、アクティビティとガチャを繰り返す**。

---

## 実装レイヤ構成（クラス対応表）

- **`DataManager`**: ゲーム全体の状態（資金・徳・悪運・欲求・習熟度など）のシングルトン管理。
- **`GachaSystem`**: ガチャの確率計算・抽選ロジック。
- **`ActivityManager`**: 各アクティビティ（善行/労働/ギャンブル/勉強/投資/瞑想/日常）のロジック。
- **`UIManager`**: 3つのUIパネル（Main / Gacha / Result）の表示切替とテキスト更新・簡易演出。
- **`SceneSetupTool` (Editor)**: メニュー `Tools/Setup Gacha Savior` から実行するシーン自動構築ツール。

この仕様書では、上記クラス名を**単一の情報源**として扱う。  
以後、各項目で「対応クラス: `Xxx.cs`」を明示する。

---

## パラメータ仕様（DataManager）

- **対応クラス**: `DataManager.cs`

### シングルトン管理

- `public static DataManager Instance { get; private set; }`
  - シーン内に1つだけ存在する前提。複数存在した場合は`Awake`で自動的に自壊。

### 基本パラメータ

- **`Money : float`**
  - 初期値: `500`
  - 意味: ガチャや投資の原資。  
  - 主な変化要因:
    - `DoWork` で増加。
    - `DoGamble` で増減。
    - `DoInvest` で増減。
    - `GachaSystem.Pull` 実行時に `GachaCost` 分減少。

- **`Karma : float`**
  - 初期値: `0`
  - 意味: 善行によって上昇する「徳」。**ガチャ当選確率にプラス補正**として効く。
  - 主な変化要因:
    - `DoVolunteer`: ランダムに増加（習熟度ボーナスあり）。
    - `DoGamble`: 一律 `-10`（最低 0）。
    - `DoDailyGratitude`: `+1`。

- **`LuckBias : float`**
  - 初期値: `0`
  - 意味: 「悪運 / 乱数調整値」。失敗やギャンブル・投資の結果により変化し、ガチャ確率に加算される揺らぎ。
  - 主な変化要因:
    - `DoGamble`: 勝利時 `+0.005`、敗北時 `+[0.005, 0.02]` ランダム加算。
    - `DoInvest`: 失敗時 `+0.01`。
    - `GachaSystem.Pull`: 抽選時に `LuckBias *= 0.5`（半減して消費される）。

- **`Desire : float`**
  - 初期値: `0`
  - 範囲: \([0, 1]\) にClamp。
  - 意味: ガチャや行動への「欲求値」。**高いほどガチャ確率にデバフ（物欲センサー）**として働く。
  - 主な変化要因（`AddDesire`を通じて増加）:
    - `GachaSystem.Pull`: `+0.05`
    - `DoVolunteer`: `+0.01`
    - `DoWork`: `+0.02`
    - `DoGamble`: `+0.03`
    - `DoStudy`: `+0.01`
    - `DoInvest`: `+0.01`
    - `DoMeditate`: `Desire *= 0.5` で減少。
    - `DoDailyGratitude`: 次回 `AddDesire` の上昇量を抑制（下記参照）。

### 習熟度

- **`VolunteerProficiency : int`**
  - 意味: 善行（ボランティア）の習熟度。高いほど徳の獲得量が増える。
  - 使用箇所: `DoVolunteer`

- **`WorkProficiency : int`**
  - 意味: 労働（バイト）の習熟度。高いほど収入額が増える。
  - 使用箇所: `DoWork`

- **`StudyProficiency : int`**
  - 意味: 勉強の習熟度。投資の成功率に寄与。
  - 使用箇所: `DoStudy`, `DoInvest`

- **`InvestProficiency : int`**
  - 意味: 投資の習熟度。投資成功率に寄与。
  - 使用箇所: `DoInvest`

### フラグ・カウンタ

- **`GachaCount : int`**
  - ガチャ実行回数の累計。
  - 使用箇所: `GachaSystem.Pull` 実行時にインクリメントし、ログにも表示。

- **`HasStudied : bool`**
  - 勉強を一度でも行ったかどうか。`true` になると投資アクションが解禁される。
  - 使用箇所: `DoStudy`（trueに設定）、`DoInvest`（前提条件チェック）、`UIManager.UpdateInvestButton`

- **`DesireSuppressed : bool`**
  - 日常アクション（感謝）による「次回アクションの欲求上昇抑制フラグ」。
  - 使用箇所:
    - `DoDailyGratitude`: `true` に設定。
    - `AddDesire`: `true` の場合、上昇量を1/3にし、1回適用後に`false`へ戻す。

### リセット処理

- **`ResetAll()`**
  - 全パラメータを初期値に戻す。
  - 現状UIからは直接呼ばれていないため、将来「リセット」ボタンを追加する場合はここを利用する。

---

## ガチャシステム仕様（GachaSystem）

- **対応クラス**: `GachaSystem.cs`

### ガチャ基本情報

- **1回あたりコスト**: `GachaCost = 300`（通貨単位は円想定）
- **基本排出確率**: `BaseRate = 0.0001`（0.01%）
- **当選判定**:
  - `Pull()` 戻り値: `true` → 星5 当選、`false` → 外れ（星3 or 星4）

### 確率計算式

- 関数: `CalculateProbability()`

- 数式（コード準拠）:

  - \( \text{karmaBonus} = \text{Karma} \times 0.001 \)
  - \( \text{desireDebuff} = 1.0 - \text{Clamp01(Desire)} \)
  - \( \text{prob} = (\text{BaseRate} + \text{karmaBonus}) \times \text{desireDebuff} + \text{LuckBias} \)
  - その後、\(\text{prob}\) を `[0, 1]` にClamp。

- 直感的な意味:
  - **徳 (Karma)** が高いほど、線形に確率へプラス補正。
  - **欲求 (Desire)** が高いほど、`(1 - Desire)` による乗算で全体を下げる（物欲センサー）。
  - **悪運 (LuckBias)** は加算項として直接プラスされるが、ガチャを回すたびに半減する「一時的なブースト/揺らぎ」。

### ガチャ実行フロー

- `bool Pull()` の処理順:
  1. `Money -= GachaCost`  
     `GachaCount++`
  2. `AddDesire(0.05)` で欲求値上昇。
  3. `CalculateProbability()` で最終確率を算出。
  4. `LuckBias *= 0.5` で「悪運」を半減（消費）。
  5. `Random.value` と確率を比較し、当否を決定。
  6. ログ出力（確率・Roll値・結果・累計回数）。

### 結果種別

- **当選 (`true`)**
  - 結果パネル上では固定で「★★★★★」として扱う（UI側の仕様参照）。

- **外れ (`false`)**
  - `GetLoserRank()` で星ランクを決定。
    - 70%: 星3
    - 30%: 星4
  - **星3/星4のどちらも「外れ」として扱う**（UIメッセージで明示）。

### 確率テキスト（運勢ラベル）

- 関数: `GetProbabilityLabel()`
- 内部で `CalculateProbability() * 100` をパーセントとして扱い、範囲に応じてラベルを返す:
  - `< 0.1%` : 「絶望的…」
  - `< 0.5%` : 「激渋…」
  - `< 1.0%` : 「ワンチャンあるかも…」
  - `< 3.0%` : 「いい感じかも！」
  - `>= 3.0%` : 「今なら来る！！」

---

## アクティビティ仕様（ActivityManager）

- **対応クラス**: `ActivityManager.cs`
- 各メソッドは**ボタン1つに対する行動コマンド**として、`UIManager` からボタンの `onClick` に紐づく。
- すべてのアクティビティは、実行後に
  - `uiManager.ShowActivityLog(msg);`
  - `uiManager.RefreshStatus();`
  を呼び出し、UIを即時更新する（例外: 投資の事前条件での警告メッセージなど）。

### 善行（ボランティア） — `DoVolunteer()`

- 効果:
  - 徳の増加: `Karma += Random.Range(5, 15) + VolunteerProficiency * 0.5`
  - 習熟度: `VolunteerProficiency++`
  - 欲求: `AddDesire(0.01)`
- ログ例:
  - 「♻ ボランティア完了！ 徳 +X.X（習熟度 Lv.N）」

### 労働（アルバイト） — `DoWork()`

- 効果:
  - 収入: `Money += Random.Range(100, 300) + WorkProficiency * 10`
  - 習熟度: `WorkProficiency++`
  - 欲求: `AddDesire(0.02)`
- ログ例:
  - 「💼 バイト完了！ 資金 +XXX円（習熟度 Lv.N）」

### ギャンブル（パチスロ） — `DoGamble()`

- 効果（共通）:
  - 徳: `Karma = max(0, Karma - 10)`（大きく減少）
  - 欲求: `AddDesire(0.03)`
- 勝利時:
  - 資金 `+500`
  - `LuckBias += 0.005`
  - ログ例: 「🎰 パチスロ 勝利！ 資金 +500円（でも徳 -10 …）」
- 敗北時:
  - 資金 `-300`（残高がマイナスにならないよう `max(0)`）
  - `LuckBias += Random.Range(0.005, 0.02)`
  - ログ例: 「🎰 パチスロ 敗北… 資金 -300円 → 悪運 +0.XXXX（意外と悪くない…かも）」

### 自己研鑽（勉強） — `DoStudy()`

- 効果:
  - `HasStudied = true`（投資アクション解禁）
  - `StudyProficiency++`
  - 欲求: `AddDesire(0.01)`
  - 投資ボタン状態更新: `uiManager.UpdateInvestButton()`
- ログ例:
  - 「📚 勉強した！ 投資が解禁された（勉強 Lv.N）」

### 自己研鑽（投資） — `DoInvest()`

- 前提条件:
  - `HasStudied == true` でなければ実行不可。
    - ログ: 「⚠ まず勉強して投資を解禁しよう！」
  - `Money > 0` でなければ実行不可。
    - ログ: 「⚠ 投資する資金がない！」

- 成功率:

  - \( \text{successRate} = 0.30 + 0.02 \times \text{InvestProficiency} + 0.02 \times \text{StudyProficiency} \)
  - \([0, 0.80]\) にClamp（最大80%）。

- 成功時:
  - 資金: `Money += Money * 0.5`（+50%）
  - `InvestProficiency++`
  - 欲求: `AddDesire(0.01)`
  - ログ例: 「📈 投資成功！ 資金 +XXX円（成功率 YY%）」

- 失敗時:
  - 資金: `Money -= Money * 0.2`（-20%）
  - `LuckBias += 0.01`
  - `InvestProficiency++`
  - 欲求: `AddDesire(0.01)`
  - ログ例: 「📉 投資失敗… 資金 -XXX円 → 悪運 +0.0100」

### 精神統一（瞑想） — `DoMeditate()`

- 効果:
  - `Desire *= 0.5`（欲求値を半減）
  - ログ例:
    - 「🧘 瞑想完了。欲求値 -Δ（現在 D）」

### 日常（笑顔・感謝） — `DoDailyGratitude()`

- 効果:
  - `DesireSuppressed = true`（次回 `AddDesire` の上昇量を1/3に抑制）
  - `Karma += 1`
  - ログ例:
    - 「😊 笑顔で感謝！ 徳 +1、次のアクションの欲求上昇を抑制」

---

## UI・シーン構成（UIManager / SceneSetupTool）

- **対応クラス**: `UIManager.cs`, `SceneSetupTool.cs`

### パネル構成

- **MainPanel**
  - ステータス表示:
    - 資金 (`MoneyText`)
    - 徳 (`KarmaText`)
    - 欲求 (`DesireText`)
    - 悪運 (`LuckBiasText`)
    - ガチャ累計回数 (`GachaCountText`)
  - 棒人間表示用パネル（キャラクター的な視覚表現）
  - 各アクティビティ用ボタン7種:
    - 善行 / 労働 / ギャンブル / 勉強 / 投資 / 瞑想 / 日常（笑顔・感謝）
  - ガチャ画面への遷移ボタン:
    - 「🌟 ガチャ画面へ 🌟」
  - アクティビティログ表示:
    - `ActivityLogText`

- **GachaPanel**
  - タイトル: 「✨ 限定ガチャ ✨〜伝説の星5を求めて〜」
  - 現在の運勢ラベル: `ProbabilityLabel`
  - 演出エリア（簡易な箱・絵文字など）
  - コスト表示: `GachaCostText`（例: 「1回 300 円」）
  - ガチャ実行ボタン: `PullButton` / `PullButtonText`
  - メインパネルへ戻るボタン: 「← 戻る」

- **ResultPanel**
  - 背景色: 当選時はレインボー、外れ時は暗めのカラー。
  - 結果タイトル: `ResultTitleText`
    - 当選時: 「★★★★★」 + ゴールドカラー
    - 外れ時: 星3〜4の数に応じた「★★★」 or 「★★★★」
  - 結果メッセージ: `ResultMessageText`
    - 星5: 「限定キャラ降臨！！\nおめでとう！！！」
    - 星4: 「星4…惜しい！でもハズレ！」
    - 星3: 「星3…またお前か…」
  - OKボタン（結果を閉じてガチャ画面に戻る）

### 画面遷移

- 起動時:
  - `UIManager.Start()` にて
    - `ShowMainPanel()`
    - `RefreshStatus()`
    - `UpdateInvestButton()`

- メイン → ガチャ:
  - MainPanel内「ガチャ画面へ」ボタン → `UIManager.ShowGachaPanel()`

- ガチャ → 結果:
  - `OnPullButtonClicked()` → `GachaSequence()` コルーチンで演出 → `ShowResultPanel(won, rank)`

- 結果 → ガチャ:
  - 結果パネルのOKボタン → `OnResultCloseClicked()` → `ShowGachaPanel()`

- ガチャ → メイン:
  - GachaPanelの戻るボタン → `OnBackToMainClicked()` → `ShowMainPanel()`

### SceneSetupTool による自動構築

- メニュー: `Tools/Setup Gacha Savior`
- 実行時の処理概要:
  - 新規シーン作成（デフォルトGameObjects付き）。
  - カメラ背景色などをコンセプトカラーに設定。
  - `EventSystem` がなければ自動生成。
  - `Canvas` および `GameManager` GameObject を生成し、
    - `DataManager`, `GachaSystem`, `ActivityManager`, `UIManager` を `GameManager` にアタッチ。
    - 各UIコンポーネントとクラス参照を自動で紐づけ。
  - MainPanel, GachaPanel, ResultPanel およびすべての表示テキスト・ボタンをコードで自動生成。
  - シーンを `Assets/Scenes/MainScene.unity` として保存。

---

## ログ・デバッグ観点

- 主要なアクションは `Debug.Log` を通じてログ出力される。
- ログに含まれる主な情報:
  - ガチャ: 確率・Roll値・結果・累計回数。
  - 各アクティビティ: 資金増減量・徳増減量・悪運増減量・習熟度レベル・成功率など。
- Unity コンソールのログを追うことで、パラメータ変化とガチャ確率の関係を検証できる。

---

## 仕様更新ポリシー（AI向け運用ルール）

このセクションは、**他のAI／将来のエージェントが仕様書を更新する際のガイドライン**を定義する。
仕様が重複しないよう、必ず以下のポリシーに従って改訂を行うこと。

### 1. クラスと仕様セクションの1対1対応

- 各クラスに対応する仕様セクションは**1か所だけ**持つ。
  - 例:
    - `DataManager.cs` → 「パラメータ仕様（DataManager）」セクション
    - `GachaSystem.cs` → 「ガチャシステム仕様（GachaSystem）」セクション
    - `ActivityManager.cs` → 「アクティビティ仕様（ActivityManager）」セクション
    - `UIManager.cs` / `SceneSetupTool.cs` → 「UI・シーン構成」セクション
- 新しい機能が既存クラスに追加された場合:
  - **既存の対応セクションを編集して追記すること**。新しい重複セクションを別に作らない。

### 2. コード変更検出と更新手順

1. **変更検出**:
   - 差分ツール（git diff等）またはエージェントのコード解析機能を用いて、
     - どのクラス / メソッド / フィールドが変更されたかを特定する。
2. **対応セクションを特定**:
   - 上記「実装レイヤ構成（クラス対応表）」を参照し、該当クラスの仕様セクションを探す。
3. **既存箇所を「上書き」して更新**:
   - 追加・変更されたフィールドやロジックに合わせて、
     - 既存の説明を**修正**または**項目の追記**を行う。
   - 同じフィールド・メソッドに関する説明を**別の場所に重複して書かない**。
4. **仕様とコードの整合性チェック**:
   - 仕様に書かれた計算式・条件分岐が、実際のコードと一致しているかを再確認する。

### 3. 新規機能・新規クラスの追加時

- 新しいクラスが追加された場合:
  - 「実装レイヤ構成（クラス対応表）」にクラス名と役割を追記する。
  - 必要に応じて**新しいセクション**をこの仕様書の末尾に追加し、
    - 概要
    - パラメータ
    - メソッドの挙動
    - UIとの関係（あれば）
    を整理する。
- 既存セクションに関係する場合:
  - 関係する既存セクションも**最小限の修正で整合性がとれるように更新**する。

### 4. 用語・定義の一貫性

- 「徳 (Karma)」「悪運 (LuckBias)」「欲求 (Desire)」「習熟度 (Proficiency)」「ガチャ当選確率」などの用語は、
  - 本仕様書で定義された名称を**単一の正準名称**として扱う。
- 別名・略称を使う場合は、初出時に
  - 例: 「悪運（LuckBias）」のように、**日本語＋コード上のプロパティ名**を併記する。

### 5. 本仕様書のファイル位置と名称

- ファイルパス（Unity プロジェクト内）:
  - `Assets/GameDesignSpec_GachaSavior.md`
- プロジェクトを利用するAIは、このファイルを**ゲーム仕様の第一参照元**として扱い、  
  仕様の不明点がある場合は、ここを起点にコードを参照すること。

---

## 変更履歴（概要）

- **v0.1**: 初版作成
  - 対象: 既存実装 (`DataManager`, `GachaSystem`, `ActivityManager`, `UIManager`, `SceneSetupTool`) に基づく仕様整理。
  - 目的: 他AI/エージェントがゲームのロジックと意図を正確に理解し、変更時に仕様とコードを同期しやすくするため。

