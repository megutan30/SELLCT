using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// ゲームコンテキストインターフェース
    /// ゲーム実行時に必要な主要サービスと状態情報への統一アクセスを提供
    /// Clean Architectureのサービスロケーターパターンの実装
    /// 複数のサービスにまたがる処理で使用される共通コンテキスト
    /// 依存性注入の簡素化とテスタビリティの向上を目的とした抽象化
    /// </summary>
    public interface IGameContext
    {
        /// <summary>
        /// ゲーム状態管理オブジェクト
        /// ゲーム全体の状態、アクション履歴、変数等を管理
        /// プレイヤーの行動記録やゲーム進行状況の追跡に使用
        /// すべてのゲームロジックで参照される中央状態ストア
        /// 読み取り専用プロパティ
        /// </summary>
        GameState GameState { get; }
        
        /// <summary>
        /// コンポーネント管理サービス
        /// ファイルシステム監視、コンポーネント生成、ライフサイクル管理を担当
        /// ファイル作成・削除・名前変更の検出とイベント発行を行う
        /// ゲームの基盤となるファイルシステム連携の中核サービス
        /// 読み取り専用プロパティ
        /// </summary>
        ComponentManager ComponentManager { get; }
        
        /// <summary>
        /// パズルアクションハンドラー
        /// パズルシステムで発生するアクションの実行を担当
        /// UI操作、システム制御、メッセージ表示等の具体的な処理を実行
        /// Clean Architectureの依存性逆転原則に従った抽象化
        /// 読み取り専用プロパティ
        /// </summary>
        IPuzzleActionHandler ActionHandler { get; }
        
        /// <summary>
        /// イベントディスパッチャーサービス
        /// ドメインイベントの発行と購読を管理
        /// コンポーネント間の疎結合な通信を実現
        /// イベント駆動アーキテクチャの中核サービス
        /// 読み取り専用プロパティ
        /// </summary>
        IEventDispatcher EventDispatcher { get; }
        
        /// <summary>
        /// メタゲームコントローラー
        /// フェーズ2のシステム制御デモンストレーションを管理
        /// エクスプローラー操作、入力制御、コマンドプロンプト戦闘等を担当
        /// 社会工学教育ゲームの高度な制御機能を提供
        /// 読み取り専用プロパティ
        /// </summary>
        MetaGameController MetaGameController { get; }
    }
}