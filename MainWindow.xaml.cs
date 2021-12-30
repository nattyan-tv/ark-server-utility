using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ark_server_utility
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<arg_data> args = new List<arg_data>();
        public MainWindow()
        {
            InitializeComponent();
            var dict = new Dictionary<string, string>();

            // 起動オプションの引数をぉおお！！！ここにぃいいい！！！つっこむうぅうう！！！ぜんぶうぅうううう！！（地獄）

            dict.Add("allowansel", "シングルプレーヤーでNVIDIA Anselサポートをアクティブにします。 サーバーに接続すると、電源が入っていて点灯していても、ほとんどのライト（ランプ）が機能しなくなります。\nパッチ246.0で導入");
            dict.Add("AllowAnyoneBabyImprintCuddle", "誰もが刻印されている人だけでなく、ベビーディノ（抱擁など）を「ケア」できるようにする場合に使用します。");
            dict.Add("AllowCrateSpawnsOnTopOfStructures", "サーバーは、構造物によって防止されるのではなく、空からの供給クレートを構造物の上に表示できるようになりました。");
            dict.Add("AllowFlyerCarryPvE", "PvEの飛行生物は野生の恐竜を運ぶことができます。");
            dict.Add("AllowFlyingStaminaRecovery", "プレーヤーが乗っている代わりに立っているときに飛行生物がスタミナを回復する必要がある場合は、trueに設定します。");
            dict.Add("AllowMultipleAttachedC4", "恐竜ごとに複数のC4を接続できるようにするにはtrueに設定します。");
            dict.Add("AutoDestroyDecayedDinos", "要求可能な恐竜をロード時に自動破壊し、要求可能なままにしておきます。\nパッチ255.0で導入");
            dict.Add("automanagedmods", "MODの自動ダウンロード/インストール/更新。\nパッチ244.3で導入");
            dict.Add("AppendItemSets", "に設定すると、すべてのアイテムをオーバーライドするのではなく、クレートを提供するアイテムセットを追加します。ConfigOverrideSupplyCrateItemsを参照してください\nパッチ273.7で導入");
            dict.Add("AppendPreventIncreasingMinMaxItemSets", "trueに設定すると、ドロップされるアイテムの量が動的に増加します。ConfigOverrideSupplyCrateItemsを参照してください\nパッチ273.7で導入");
            dict.Add("ClampItemSpoilingTimes", "すべての腐敗時間をアイテムの最大腐敗時間に固定します。腐敗時間を変更するModで問題を引き起こす可能性があります。\nパッチ254.944で導入");
            dict.Add("ClampItemStats", "アイテムの統計クランプを有効/無効にします。 詳細については、ItemStatClampsを参照してください。\nパッチ255.0で導入");
            dict.Add("ClearOldItems", "公式のPvPサーバーは、アイテム複製のバグが悪用された後の公平性を確保するために、すべての古い装備されていないアイテム（設計図、食べられるもの、メモ、クエストアイテムを除く）の1回限りのクリアランスを提供します。 サーバー管理者は、このコマンド引数を使用して実行する場合、これを1回強制できます（更新前の保存ゲームでのみ機能します）。\nパッチ178.0で導入");
            dict.Add("ClearOldItems", "このコマンドラインを使用して、言語を直接オーバーライドできます。 現在サポートされている言語コードのリスト: ca");
            dict.Add("d3d10", "ゲームはsm4によってDX11の代わりにDX10を使用することを強制されます。 これにより、グラフィックエンジンがより小さなバージョンに削減され、一部のグラフィックは削減されますが、フレームレートは上がります。");
            dict.Add("DestroyUnconnectedWaterPipes", "2日間のリアルタイムの後、パイプは非パイプ（直接または間接）に接続されておらず、近くに味方プレイヤーがいない場合、パイプは自動的に破壊されます。");
            dict.Add("DisableImprintDinoBuff", "気に入らない場合は、これを使用してDino Imprinting-Player Stat Bonusを無効にします（Dinoに特に刻印され、Imprinting Qualityに上げると、追加のダメージ/耐性バフが得られます）");
            dict.Add("EnableIdlePlayerKick", "KickIdlePlayersPeriod内で移動または対話していないキャラクターを蹴ります。");
            dict.Add("EnableExtraStructurePreventionVolumes", "これを使用して、特定のリソースが豊富なエリア、特に主要な山々の周りのTheIslandでのセットアップを完全に無効にします。");
            dict.Add("ExtinctionEventTimeInterval", "これを使用して、30日間の絶滅を有効にします。 数値は秒単位の時間です。");
            dict.Add("FastDecayUnsnappedCoreStructures", "スナップされていない基礎/支柱を5倍の速度で崩壊させます。\nパッチ245.987で導入");
            dict.Add("ForceAllowCaveFlyers", "飛行恐竜を強制的に洞窟に入れます（飛行恐竜はカスタムマップでデフォルトで洞窟に入ることができます）");
            dict.Add("ForceFlyerExplosives", "飛行（QuetzalとWyvernは、C4が取り付けられた状態では飛べません。このパラメーターをtrueに設定すると、飛ぶことができます。\nパッチ252.83で導入");
            dict.Add("ForceRespawnDinos", "起動時にすべての野生生物を破壊するには、このコマンドで起動します。 （これは、現在飼い慣らされていない野生のクリーチャーのみを破壊します）\nパッチ216.0で導入");
            dict.Add("gameplaylogging", "Fittestサーバーのサバイバルは、このコマンドラインオプションを使用して起動し、日付付きログファイルをSavedフォルダーに出力します。このフォルダーには、タイムスタンプ付きのキルと勝者のログが含まれます。");
            dict.Add("insecure", "バルブアンチチート（VAC）システムを無効にします。");
            dict.Add("lowmemory", "グラフィックとオーディオ効果を減らして約800 MBのRAMを節約する起動オプション。4GBのRAMプレーヤーが無限のロード画面を通過できる可能性が高い");
            dict.Add("MapModID=#########", "専用サーバーは、この構文を使用してマップ名を直接指定するのではなく、ModIDを介してカスタムマップをオプションでロードできるようになりました（MapModIDはカスタムマップのSteam Workshop FileIDで、GameModIdsはスタックされたmodのIDです） 順番に使用したい）\nパッチ193.0で導入");
            dict.Add("MaxPersonalTamedDinos", "部族ごとの恐竜のテイム制限を有効にします。 （公式サーバーでは500）\nパッチ255.0で導入");
            dict.Add("MinimumDinoReuploadInterval", "許可された恐竜再アップロード間のクールダウンの秒数（デフォルトは0、オフィシャルサーバーでは43200に設定され、12時間です）。");
            dict.Add("noantispeedhack", "アンチスピードハック検出がデフォルトで有効になりました—無効にするには、このサーバーのコマンドラインを使用します\nパッチ218.5で導入");
            dict.Add("NoBattlEye", "Battleyeなしでサーバーを実行する");
            dict.Add("NoBiomeWalls", "v241.5のバージョン管理されていない追加で導入された、今後のバイオーム変更エリアウォール効果を排除します。\nパッチ242.7で導入");
            dict.Add("nocombineclientmoves", "サーバーplayer-move-physics最適化はデフォルトで有効になりました（perfが改善されます）—無効にするには、このサーバーコマンドラインを使用します\nパッチ218.5で導入");
            dict.Add("nofishloot", "Fishing Rodを使用する際に、肉以外のFish Lootを無効にします\nパッチ245.9で導入");
            dict.Add("noninlinesaveload", "大きなセーブゲームでのセーブゲーム破損のケースを修正しました。これは実験的なものです。ロードしないセーブがある場合は、このコマンドを試してください。 副作用がないことを100％確信した後、このローダーの変更を次のパッチで正式に展開します。");
            dict.Add("nomansky", "雲や星空など、詳細な空の機能の多くは無効になっています。 これはそれらのすべてを減らしますが、あなたはまだそれらを持つことができます。 あなたはまだ星、太陽、月を手に入れています。 これは、夜に視力を失わせる輝く星を取り除くのに役立ちます");
            dict.Add("nomemorybias", "クライアントゲームのメモリ使用量を約600 MBシステムと600 MB GPU RAMで削減しました！ （すべてのメッシュがLODを動的にストリーミングするようになりました）。 これにより、実行時のパフォーマンスにわずかな影響が生じる可能性があるため、古い方法を使用するには（RAMの使用量を増やすが、潜在的なパフォーマンスの低下はありません）、-nomemorybiasを指定して起動します");
            dict.Add("NonPermanentDiseases", "これにより病気は永続的ではなくなります（リスポーンすると失われます）。");
            dict.Add("NotifyAdminCommandsInChat", "");
            dict.Add("oldsaveformat", "デフォルトでは、「新しい保存形式」を使用します。これは、約4倍高速で、50％小さくなります。 古い保存形式を使用する場合は、-oldsaveformatを使用して起動します。");
            dict.Add("OnlyAutoDestroyCoreStructures", "このオプションで自動破壊対応サーバーを起動して、非コア/非基礎構造が自動破壊されないようにすることができます（もちろん、それらが存在するフロアが自動破壊された場合でも自動破壊されます）。 公式PvEサーバーはこのオプションを使用します。\nパッチ245.989で導入");
            dict.Add("OnlyDecayUnsnappedCoreStructures", "設定すると、スナップされていないコア構造のみが減衰します。 PvPサーバー上の孤立した柱/基盤スパムを排除するのに役立ちます。\nパッチ245.986で導入");
            dict.Add("OverrideOfficialDifficulty", "デフォルトのサーバー難易度レベル4を5で上書きして、新しい公式サーバー難易度レベルに一致させることができます。\nパッチ247.95で導入");
            dict.Add("OverrideOfficialDifficulty", "platform saddlesでturretまたはspike構造を構築可能にし、機能させるには、このオーバーライドを使用します。\nパッチ242.0で導入");
            dict.Add("OxygenSwimSpeedStatMultiplier", "これを使用して、泳ぐ速度に酸素の消費レベルを掛ける方法を設定します。 256.0で値が80％減少しました。\nパッチ256.3で導入");
            dict.Add("PreventDiseases", "これにより、サーバー上の病気が完全に無効になります(これまでのところ「スワンプフィーバー」)。");
            dict.Add("PreventHibernation", "シングルプレイヤーサーバーと非専用サーバーの両方で、非アクティブゾーンにいるクリーチャーは、静止状態ではなく休止状態にあります。 このオプションを使用して、パフォーマンスとメモリ使用量を犠牲にしてハイバネーションを防ぎます。\nパッチ259.0で導入");
            dict.Add("PreventOfflinePvP", "これを使用して、オフラインの襲撃防止オプションを有効にします。");
            dict.Add("PreventOfflinePvPInterval", "それは、部族/プレイヤーの恐竜/構造がログオフした後に不死身/非アクティブになる前に、15分間待機します。 （部族の場合、すべての部族メンバーがログオフする必要があります！）");
            dict.Add("PreventSpawnAnimations", "trueに設定すると、ウェイクアップアニメーションなしでプレイヤーキャラクターを（再）スポーンできます。\nパッチ261.0で導入");
            dict.Add("PvEAllowStructuresAtSupplyDrops", "PvEモードで供給ドロップポイントの近くに構築できるようにするには、trueに設定します。\nパッチ247.999で導入");
            dict.Add("PvEDinoDecayPeriodMultiplier", "Dino PvE Auto-Claim time multiplier\nパッチ241.4で導入");
            dict.Add("PvPDinoDecay", "trueに設定すると、オフラインレイディング防止がアクティブなときに恐竜が腐敗するのを防ぎます。");
            dict.Add("PvPStructureDecay", "trueに設定すると、Offline Raiding Preventionがアクティブなときに構造が崩壊するのを防ぎます。\nパッチ206.0で導入");
            dict.Add("PvPStructureDecay", "Enables RCON");
            dict.Add("RCONPort", "RCOnの動作にRCONの接続ポートが必要であることを指定します。指定されたポートを手動で有効にする必要があります。使用されていないポートは使用できます\nパッチ185.0で導入");
            dict.Add("ShowFloatingDamageText", "これを使用して、RPGスタイルのポップアップテキスト統計モードを有効にします。");
            dict.Add("server", "?");
            dict.Add("ServerAdminPassword", "指定されている場合、プレイヤーはこのパスワードを（ゲーム内コンソールを介して）提供して、サーバー上の管理者コマンドにアクセスする必要があります。 RCON経由のログインにも使用されます。");
            dict.Add("servergamelog", "サーバー管理ログを有効にする（RCONサポートを含む）RCONコマンド「getgamelog」を使用して一度に100エントリを印刷し、「Logs」内の日付ファイルに出力し、コマンドラインでRCONバッファーの最大長を調整する: “?RCONServerGameLogBuffer=600”\nパッチ224.0で導入");
            dict.Add("servergamelogincludetribelogs", "");
            dict.Add("ServerRCONOutputTribeLogs", "グローバルチャットに加えて、部族チャットをrconで表示できます。");
            dict.Add("sm4", "ゲームでは、通常どおり、Shader Model 5の代わりにShader Model 4を使用する必要があります。 さて、大きな違いはありません。ゲームは同じように見えますが、パフォーマンスは向上します（Windowsのみ？）（フレームを増やすと報告されていますが、ゲームの照明は変わります）");
            dict.Add("StasisKeepControllers", "AIコントローラーがStasisで再び破棄されるため、メモリのオーバーヘッドが大きすぎて大きなマップに配置できません。 多くのRAMを備えたサーバーの場合、AIをメモリ内に保持するためにこれを実行することにより、オプションでパフォーマンスを向上できるようになりました。");
            dict.Add("StructureDestructionTag=DestroySwampSnowStructures", "スワンプゾーンとスノーゾーンでの1回限りの自動構造破壊：これを行うには、v216に更新した後に一度だけ実行できます。このコマンドラインでサーバーまたはゲームを実行します");
            dict.Add("TheMaxStructuresInRange", "サーバー上の最大許容構造の新しい値。 NewMaxStructuresInRangeを置き換えました\nパッチ252.1で導入");
            dict.Add("TribeLogDestroyedEnemyStructures", "デフォルトでは、（被害者の部族の）敵の構造破壊は部族のログに表示されません。これを有効にするにはtrueに設定します。\nパッチ247.93で導入");
            dict.Add("USEALLAVAILABLECORES", "すべてのCPUコアが使用されます。この起動オプションを使用した後、すべてのCPUコアが使用されていない場合は、無効にしてください。");
            dict.Add("usecache", "〜70％高速な読み込み速度オプション。 「Experiment Fast Load Cache」起動オプションを選択します（手動で起動コマンドラインに「-usecache」を追加します）。 ゲームを開始する1回目と2回目以降はまだ遅くなりますが、3回目以降は速くなります。 これは、デフォルトでPrimalDataに追加された188.2の時点で非推奨になる可能性があります。 また、すべてのマップに追加されるはずでしたが、これが発生したかどうかについては明確にされていません。");
            dict.Add("UseOptimizedHarvestingHealth", "HarvestAmountMultiplierが高い「最適化された」サーバー（ただし、まれではないアイテム）。");
            dict.Add("bRawSockets", "サーバーネットワークのパフォーマンスを大幅に向上させるため、Steam P2Pではなく直接UDPソケット接続。 サーバーが接続を受け入れるためには、ポート7777および7778を手動で開く必要があります。\nパッチ213.0で導入");
            dict.Add("nonetthreading", "ネットワークに単一スレッドのみを使用するbRawSocketsサーバーのオプション（特にLinuxでCPUコアよりも多くのサーバーを搭載したマシンのパフォーマンスを向上させるのに役立ちます）\nパッチ271.15で導入 パッチ271.17で非推奨のようです");
            dict.Add("forcenetthreading", "デフォルトの専用サーバー?bRawSocketsモードは、スレッドネットワーキングを使用しないように設定されていましたが、分析では一般に正味のパフォーマンスの低下と思われました。 これを使用して、強制的に有効にします。");
            dict.Add("vday", "バレンタインデーのイベントを有効にします（2倍のメイトブースト範囲、3倍の交配速度/交配回復、3倍の赤ちゃん/卵成熟速度、1/3のベビーフード消費）。\nおそらく非推奨です。 代わりに-ActiveEvent=vdayを使用してください。");
            dict.Add("webalarm", "Tripwireアラームが発生したり、赤ちゃんが生まれたりするなど、部族に重要なことが発生したときにWebアラームをアクティブにします。 詳細については、Web Notificationsを参照してください。\nパッチ243.0で導入");
            dict.Add("exclusivejoin", "許可リストに追加された場合にのみプレーヤーの参加を許可するサーバーでホワイトリストのみモードをアクティブにします。 「AllowedCheaterSteamIDs.txt」を使用してプレイヤーにチートを許可する「Admin Whitelisting」と混同しないでください。 このオプションでは、「PlayersJoinNoCheckList.txt」を使用して、ユーザーは参加できるがチートは許可されません。 ファイルがLinux / Win64 Binariesフォルダーに存在しない場合は、ファイルを作成し、サーバーへの参加を許可するプレーヤーのSteam64 IDを追加します。 パス:'/ShooterGame/Binaries/Win64/' '/ShooterGame/Binaries/Linux/'\nファイルの実行中にファイルに新しい行を追加する場合は、サーバーを再起動する必要があります。 ただし、ゲームで「Cheat AllowPlayerToJoinNoCheck 12345678901234567」を使用して、新しいプレーヤーをホワイトリストに追加できます。 このメソッドはファイルにも保存するため、サーバーを再起動する必要はありません。");
            dict.Add("ActiveEvent=<eventname>", "指定されたイベントを有効にします\n一度に1つだけ指定してアクティブにできます");
            dict.Add("GameModIds", "順序とロードするmodを指定します。ModIDはコンマで区切る必要があります\nパッチ190.0で導入");
            dict.Add("MaxTamedDinos", "サーバー上の飼いならされたDinoの最大数を設定します。これはグローバルキャップです。\nパッチ191.0で導入");
            dict.Add("SpectatorPassword", "管理者以外の観客を使用するには、サーバーは観客のパスワードを指定する必要があります。 その後、すべてのクライアントがこれらのコンソールコマンドを使用できます: requestspectator <password>およびstopspectating。詳細およびホットキーについては、パッチ191.0を参照してください。");
            dict.Add("AllowCaveBuildingPVE", "Trueに設定すると、PvEモードも有効になっているときに洞窟内での構築が可能になります。\nパッチ194.0で導入");
            dict.Add("AdminLogging", "すべての管理コマンドをゲーム内チャットに記録します\nパッチ206.0で導入");
            dict.Add("ForceAllStructureLocking", "これを有効にすると、デフォルトですべての構造がロックされます\nパッチ222.0で導入");
            dict.Add("AutoDestroyOldStructuresMultiplier", "十分な「近くの部族がいない」時間が経過した後、自動破壊構造を許可するサーバーオプション（許可請求期間の乗数として定義）。 サーバーが必要に応じて、時間の経過とともに自動的に放棄された構造をクリアするのに役立ちます\nパッチ222.0で導入");
            dict.Add("RCONServerGameLogBuffer", "RCONを介して送信されるゲームログの行数を決定します\nパッチ224.0で導入");
            dict.Add("PreventTribeAlliances", "これを有効にすると、部族は同盟を作成できなくなります\nパッチ243.0で導入");
            dict.Add("AllowRaidDinoFeeding", "サーバーのTitanosaursを恒久的に飼い慣らす（つまり、餌を与える）\nただし、現在、TheIslandは3つのティタノサウルスしかスポーンしていないことに注意してください。\nパッチ243.0で導入");
            dict.Add("AllowHitMarkers", "これを使用して、範囲攻撃のオプションのヒットマーカーを無効にします\nパッチ245.0で導入");
            dict.Add("ServerCrosshair", "これを使用して、サーバーのクロスヘアを無効にします\nパッチ245.0で導入");
            dict.Add("PreventMateBoost", "Dino Mate Boostを無効にするオプション\nパッチ247.0で導入");
            dict.Add("ServerAutoForceRespawnWildDinosInterval", "サーバーの再起動時にWild Dinoを強制的に再スポーンします。 公式サーバーでデフォルトで有効になっているため、すべてのサーバーで毎週ディノが強制的に再スポーンされ、特定のディノタイプ（BasiloやSpinoなど）が長時間実行されるサーバーで過密になるのを防ぎます。\n注：場合によっては、これは複数回機能しない場合があります\nパッチ265.0で導入");
            dict.Add("PersonalTamedDinosSaddleStructureCost", "プラットフォームサドルが部族の恐竜の制限に向けて使用するディノスロットの量を決定します\nパッチ265.0で導入");
            dict.Add("structurememopts", "構造メモリの最適化を有効にします\n注：これらのmod構造のスナップポイントが破損する可能性があるため、構造関連のmodを実行するとき（更新されるまで）は使用しないでください。\nパッチ295.108で導入");
            dict.Add("nodinos", "野生の恐竜が生成されない。\n注：既に稼働しているサーバー上にこのオプションを追加した場合、全ての恐竜を消去(チートコマンド：destroywilddinos)を実行する必要があります。");
            dict.Add("noundermeshchecking", "アンチメッシングシステムをオフにします。\nパッチ304.445で導入");
            dict.Add("noundermeshkilling", "アンチメッシングシステムによるプレイヤーキルをオフにします。（但し、テレポートを許可します）\nパッチ304.445で導入");
            dict.Add("AutoDestroyStructures", "期限切れ構造物の自動破壊を有効にします。\n期限は以下のオプションで調整できます。AutoDestroyOldStructuresMultiplier");
            dict.Add("crossplay", "専用サーバーでのクロスプレイ（SteamとEPIC間）を可能にします。\nパッチ311.74で導入");
            dict.Add("epiconly", "Epic Game Storeのプレイヤーのみが専用サーバーに接続できるようになります。\nパッチ311.74で導入");
            dict.Add("UseVivox", "Steamサーバーでのみ、Vivoxを有効化します。\nEpicサーバーのデフォルトはSteamサーバーで有効にすることができます。\nパッチ311.74で導入");
            dict.Add("PublicIPForEpic=<IPアドレス>", "このコマンドラインがなく、-Multihomeが指定されている場合、EGSクライアントはMultihomeで指定されたIPに接続しようとします。\n注：Multihomeを使用していて、パブリックでないIPアドレスを指定すると、プレイヤーはEGSを使用してサーバーに接続できなくなります。必ずパブリックIPアドレスを指定してください。（WANまたは外部など）");

            foreach (KeyValuePair<string, string> kvp in dict)
            {
                args.Add(new arg_data { arg = kvp.Key, detail = kvp.Value });
            }

            if (!File.Exists(@"settings.json"))
            {
                string myPythonApp = "python/settings.py";
                var myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = myPythonApp + " first",
                        CreateNoWindow = true
                    }
                };
                myProcess.Start();
                myProcess.WaitForExit();
                myProcess.Close();
                // string[,] settings_data = new string[99, 3];
            }
            string read_set = "python/settings.py";
            var read_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = read_set + " read 1",
                    CreateNoWindow = true
                }
            };
            read_pro.Start();
            StreamReader read_str = read_pro.StandardOutput;
            string data_1 = read_str.ReadLine();
            read_pro.WaitForExit();
            read_pro.Close();
            string[] arr = data_1.Split(',');
            label_name.Content = "サーバー名：" + arr[0];
            label_map.Content = "マップ名：" + arr[1];
            label_dir.Content = "ディレクトリ：" + arr[2];
            var value_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py value",
                    CreateNoWindow = true
                }
            };
            value_pro.Start();
            StreamReader val_read = value_pro.StandardOutput;
            string value = val_read.ReadLine();
            value_pro.WaitForExit();
            value_pro.Close();
            if (value == "1")
            {
                del_list.IsEnabled = false;
            }
            if (!File.Exists(@arr[2] + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe"))
            {
                // サーバーデータがインストールされていない場合の処理
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                var version_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = "python/version.py version 0",
                        CreateNoWindow = true
                    }
                };
                version_pro.Start();
                StreamReader ver_read = version_pro.StandardOutput;
                string version = ver_read.ReadLine();
                version_pro.WaitForExit();
                version_pro.Close();
                latest_version.Content = "配信されている最新バージョン：" + version;
            }
            else
            {
                // サーバーデータがインストールされている場合の処理
                start_server.IsEnabled = true;
                install_server.Content = "アンインストール";
                var version_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = "python/webapi.py version 1",
                        CreateNoWindow = true
                    }
                };
                version_pro.Start();
                StreamReader ver_read = version_pro.StandardOutput;
                string version = ver_read.ReadLine();
                Console.WriteLine(version);
                version_pro.WaitForExit();
                version_pro.Close();
                string[] vers = version.Split(',');
                latest_version.Content = "配信されている最新バージョン：" + vers[0];
                current_version.Content = "インストールされているバージョン：" + vers[1];
                update_bt.IsEnabled = true;

                if(float.Parse(vers[0]) > float.Parse(vers[1]))
                {
                    update_bt.Content = "アップデート";
                }
                else
                {
                    update_bt.Content = "ファイルのチェック";
                }
                server_pass_bool.IsEnabled = true;
                admin_pass.IsEnabled = true;
                game_port.IsEnabled = true;
                query_port.IsEnabled = true;
                arg_setting_box.IsEnabled = true;
            }
            server_name.Text = arr[0];
            map.Text = arr[1];
            server_dir.Text = arr[2];
            server_list.Items.Add(arr[0]);
            server_list.Text = arr[0];
            main_pbar.Value = 100;
            main_ptext.Content = "ARK: Server Utility";
        }
        private void start_debug(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@server_dir.Text + @"\\ShooterGame\\Binaries\\Win64\\ShooterGameServer.exe", map.Text + "?listen?SessionName=" + server_name.Text + "?ServerPassword=" + join_pass.Password + "?ServerAdminPassword=" + admin_pass.Password + "?Port=7777?QueryPort=27015?MaxPlayers=3");
            Process.Start(processStartInfo);
        }
        private void exit_app(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void install_steamCMD(object sender, RoutedEventArgs e)
        {
            DialogResult dr = System.Windows.Forms.MessageBox.Show("SteamCMDのインストールには時間がかかります。\n実行してもよろしいですか？", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (Directory.Exists(@"SteamCMD") == false)
            {
                Directory.CreateDirectory(@"SteamCMD");
                Console.WriteLine("ディレクトリ「SteamCMD」がありませんでした。");
            }
            else if (Directory.Exists(@"SteamCMD") == true)
            {
                Console.WriteLine("ディレクトリ「SteamCMD」がありました。");
                System.Windows.Forms.MessageBox.Show("既にSteamCMDがインストールされています。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            WebClient mywebClient = new WebClient();
            mywebClient.DownloadFile("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", @"SteamCMD\\steamcmd.zip");
            try
            {
                ZipFile.ExtractToDirectory(@"SteamCMD\\steamcmd.zip", @"SteamCMD");
            }
            catch (IOException)
            {
                ;
            }
            await Task.Delay(1000);
            File.Delete(@"SteamCMD\\steamcmd.zip");
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+quit");
            Process steamcmd_installer = Process.Start(processStartInfo);
            steamcmd_installer.WaitForExit();
            int exitCode = steamcmd_installer.ExitCode;
            steamcmd_installer.Close();
            Console.WriteLine("SteamCMDのインストールステータス：" + exitCode);
            if (exitCode == 7)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのインストールが完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのインストール作業が中断されたか、正常にインストールできませんでした。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void launch_steamCMD(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(@"SteamCMD") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (File.Exists(@"SteamCMD\\steamcmd.exe") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe");
            Process.Start(processStartInfo);
        }
        private void uninstall_steamCMD(object sender, RoutedEventArgs e)
        {
            DialogResult dr = System.Windows.Forms.MessageBox.Show("SteamCMDをアンインストールしてもよろしいですか？", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            if (Directory.Exists(@"SteamCMD") == false)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDはインストールされていません。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            System.IO.DirectoryInfo steamcmd_del = new System.IO.DirectoryInfo(@"SteamCMD");
            try
            {
                steamcmd_del.Delete(true);
            }
            catch (IOException err)
            {
                System.Windows.Forms.MessageBox.Show("SteamCMDのアンインストール中にエラーが発生しました。再度やり直してください。\n" + err, "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            System.Windows.Forms.MessageBox.Show("SteamCMDのアンインストールが完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(join_pass.IsEnabled == true)
            {
                join_pass.IsEnabled = false;
            }
            else
            {
                join_pass.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            label_name.Content = "サーバー名：" + server_name.Text;
            label_map.Content = "マップ名：" + map.Text;
            label_dir.Content = "ディレクトリ：" + server_dir.Text;
            string edit_set = "python/settings.py";
            var edit_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = edit_set + " edit 1 " + server_name.Text + " " + map.Text + " " + server_dir.Text,
                    CreateNoWindow = true
                }
            };
            edit_pro.Start();
            edit_pro.WaitForExit();
            edit_pro.Close();
            System.Windows.Forms.MessageBox.Show("保存しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void install_server_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@server_dir.Text + @"/ShooterGameServer.exe"))
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("ARKのサーバーデータをアンインストールしてもよろしいですか？\n再度インストールするまでサーバーは起動できません。\nアンインストールには時間がかかります。", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                /// サーバーが起動している場合は終了する的な処理を。
                string read_set = "python/settings.py";
                var read_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = read_set + " read 1",
                        CreateNoWindow = true
                    }
                };
                read_pro.Start();
                StreamReader read_str = read_pro.StandardOutput;
                string data = read_str.ReadLine();
                read_pro.WaitForExit();
                read_pro.Close();
                string[] arr = data.Split(',');
                main_pbar.Value = 0;
                main_ptext.Content = "データアンインストール処理中...";
                try
                {
                    Directory.Delete(@arr[2]);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("データの削除中にエラーが発生しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    main_pbar.Value = 100;
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
                main_pbar.Value = 100;
                main_ptext.Content = "データの削除が完了しました。";
                System.Windows.Forms.MessageBox.Show("データの削除が完了しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                start_server.IsEnabled = false;
                install_server.Content = "インストール";
                main_ptext.Content = "ARK: Server Utility";
                return;
            }
            else
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("ARKのサーバーデータをインストールしてもよろしいですか？\nインストールには時間がかかります。", "ARK Server Utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                main_pbar.Value = 0;
                main_ptext.Content = "SteamCMDチェック中...";
                if (Directory.Exists(@"SteamCMD") == false)
                {
                    System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (File.Exists(@"SteamCMD\\steamcmd.exe") == false)
                {
                    System.Windows.Forms.MessageBox.Show("SteamCMDがインストールされていません。\n「SteamCMD」より「SteamCMDのインストール」を選択してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                
                main_pbar.Value = 25;
                main_ptext.Content = "SteamCMDチェック完了...";
                /// 設定読み込み
                
                main_pbar.Value = 50;
                main_ptext.Content = "インストール処理中...";
                string read_set = "python/settings.py";
                var read_pro = new Process
                {
                    StartInfo = new ProcessStartInfo("python")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = read_set + " read 1",
                        CreateNoWindow = true
                    }
                };
                read_pro.Start();
                StreamReader read_str = read_pro.StandardOutput;
                string data = read_str.ReadLine();
                read_pro.WaitForExit();
                read_pro.Close();
                string[] arr = data.Split(',');

                main_pbar.Value = 75;
                main_ptext.Content = "インストール処理中...";
                /// SteamCMDよりARKをダウンロード
                ProcessStartInfo processStartInfo = new ProcessStartInfo(@"SteamCMD\\steamcmd.exe", "+login anonymous +force_install_dir " + @arr[2] + " +app_update 376030");
                Process steamcmd_installer = Process.Start(processStartInfo);

                int exitCode = steamcmd_installer.ExitCode;
                Console.WriteLine(exitCode);
                if (exitCode == 1)
                {
                    main_pbar.Value = 100;
                    main_ptext.Content = "インストールに成功しました。";
                    System.Windows.Forms.MessageBox.Show("インストールに成功しました。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    start_server.IsEnabled = true;
                    install_server.Content = "アンインストール";
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
                else
                {
                    main_pbar.Value = 100;
                    main_ptext.Content = "インストールに失敗しました。";
                    System.Windows.Forms.MessageBox.Show("正常にインストールが終了しませんでした。\n再度やり直してください。", "ARK Server Utility", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    main_ptext.Content = "ARK: Server Utility";
                    return;
                }
            }
        }
        private void install_dir_bt_Click(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "データを保存する場所を選択してください。",
                InitialDirectory = @"C:",
                // フォルダ選択モードにする
                RestoreDirectory = true,
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                server_dir.Text = cofd.FileName;
            }
        }

        private void add_list_bt(object sender, RoutedEventArgs e)
        {
            var value_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py value",
                    CreateNoWindow = true
                }
            };
            value_pro.Start();
            StreamReader val_read = value_pro.StandardOutput;
            string value = val_read.ReadLine();
            value_pro.WaitForExit();
            value_pro.Close();
            int new_value = int.Parse(value);
            new_value++;
            server_list.Items.Add("server" + new_value);
            server_list.Text = "server" + new_value;
            server_name.Text = "server" + new_value;
            label_name.Content = "サーバー名：server" + new_value;
            map.Text = "TheIsland";
            label_map.Content = "マップ名：TheIsland";
            server_dir.Text = "C:\\";
            label_dir.Content = "ディレクトリ：C:\\";
            game_port.IsEnabled = false;
            query_port.IsEnabled = false;
            server_pass_bool.IsEnabled = false;
            admin_pass.IsEnabled = false;
            var list_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py write server" + new_value + " TheIsland C:\\",
                    CreateNoWindow = true
                }
            };
            list_pro.Start();
            list_pro.WaitForExit();
            list_pro.Close();
        }

        private void list_changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            string server = server_list.Text;
            int index = server_list.Items.IndexOf(server);
            var list_pro = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "python/settings.py read " + index,
                    CreateNoWindow = true
                }
            };
            list_pro.Start();
            StreamReader list_read = list_pro.StandardOutput;
            string list_string = list_read.ReadLine();
            Console.WriteLine(list_string);
            list_pro.WaitForExit();
            list_pro.Close();
            string[] arr = list_string.Split(',');
            server_name.Text = arr[0];
            label_name.Content = "サーバー名：" + arr[0];
            map.Text = arr[1];
            label_map.Content = "マップ名：" + arr[1];
            server_dir.Text = arr[2];
            label_dir.Content = "ディレクトリ：" + arr[2];
        }

        private void arg_setting_change(object sender, RoutedEventArgs e)
        {
            ;
        }
        private void arg_list_changed(object sender, RoutedEventArgs e)
        {
            if (arg_setting.SelectedItem == null) return;
            arg_data item = (arg_data)arg_setting.SelectedItem;
            arg_arg.Text = item.arg;
            arg_detail.Text = item.detail;
        }
    }

}
