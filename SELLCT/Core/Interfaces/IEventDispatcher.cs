using System;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// イベントディスパッチャーインターフェース
    /// ドメインイベントの発行と購読を管理するための抽象化
    /// Clean Architectureのイベント駆動アーキテクチャを実現するための契約
    /// ドメイン層がインフラストラクチャ層の実装に依存しないようにするための抽象化
    /// 疎結合なコンポーネント間通信を可能にし、横断的関心事を分離する
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// イベントを発行する
        /// 指定されたイベントをすべての購読者に配信する
        /// ジェネリック型パラメータによって型安全性を保証
        /// 同期的にすべてのハンドラーを実行する
        /// イベント処理中の例外は呼び出し元に伝播される可能性がある
        /// </summary>
        /// <typeparam name="TEvent">発行するイベントの型</typeparam>
        /// <param name="event">発行するイベントインスタンス</param>
        void Dispatch<TEvent>(TEvent @event);
        
        /// <summary>
        /// イベントハンドラーを購読登録する
        /// 指定された型のイベントが発行された際に実行されるハンドラーを登録
        /// ジェネリック型パラメータによって型安全性を保証
        /// 同じイベント型に対して複数のハンドラーを登録可能
        /// ハンドラーの実行順序は実装依存
        /// </summary>
        /// <typeparam name="TEvent">購読するイベントの型</typeparam>
        /// <param name="handler">イベント発生時に実行されるアクション</param>
        void Subscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// すべてのイベントハンドラーをクリアする
        /// アイドルリセット時にイベント購読の蓄積を防ぐために使用
        /// </summary>
        void ClearAll();
    }
}