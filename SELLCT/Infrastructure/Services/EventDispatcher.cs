using SELLCT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// イベントディスパッチャー実装クラス
    /// IEventDispatcherインターフェースの具体実装
    /// Clean ArchitectureのInfrastructure層に配置されたイベント管理サービス
    /// ドメインイベントの発行と購読を管理し、疎結合なコンポーネント間通信を実現
    /// インメモリでのイベントハンドラー管理とタイプセーフなイベント配信を提供
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        /// <summary>
        /// イベントハンドラー辞書
        /// イベント型をキーとし、そのイベントに対するハンドラーリストを値とする
        /// 型安全性を保つためobjectで格納し、実行時にキャストする
        /// </summary>
        private readonly Dictionary<Type, List<object>> _handlers = new Dictionary<Type, List<object>>();

        /// <summary>
        /// コンストラクタ
        /// イベントディスパッチャーインスタンスを初期化
        /// ハンドラー辞書を空の状態で初期化
        /// </summary>
        public EventDispatcher()
        {
            // ハンドラー辞書は既にフィールド初期化子で初期化済み
        }

        /// <summary>
        /// イベントを発行する
        /// 指定されたイベントをすべての登録済みハンドラーに配信
        /// 型安全性を保ちつつ、同期的にすべてのハンドラーを実行
        /// ハンドラー実行中の例外は呼び出し元に伝播する
        /// </summary>
        /// <typeparam name="TEvent">発行するイベントの型</typeparam>
        /// <param name="event">発行するイベントインスタンス</param>
        public void Dispatch<TEvent>(TEvent @event)
        {
            // 指定されたイベント型に対するハンドラーが存在するかチェック
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                // ハンドラーリストをコピーして反復中の変更に対応
                foreach (var handler in handlers.Cast<Action<TEvent>>().ToList())
                {
                    // 各ハンドラーを実行（例外は呼び出し元に伝播）
                    handler(@event);
                }
            }
            // デバッグ用にイベント発行をコンソールに出力
            Console.WriteLine($"Event Dispatched: {@event.GetType().Name}");
        }

        /// <summary>
        /// イベントハンドラーを購読登録する
        /// 指定された型のイベントが発行された際に実行されるハンドラーを登録
        /// 同じイベント型に対して複数のハンドラーを登録可能
        /// ハンドラーの実行順序は登録順
        /// </summary>
        /// <typeparam name="TEvent">購読するイベントの型</typeparam>
        /// <param name="handler">イベント発生時に実行されるアクション</param>
        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            
            // 該当イベント型のハンドラーリストが存在しない場合は新規作成
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<object>();
            }
            
            // ハンドラーをリストに追加
            _handlers[eventType].Add(handler);
        }
    }
}