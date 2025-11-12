using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// ゲーム構成要素タイプ
    /// ファイルシステム上で作成されるコンポーネントの種類を定義
    /// 各タイプは異なる色とアイコンで表示される
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// ユーザーインターフェース要素（青色 #3498DB）
        /// ボタンやウィンドウなどのUI部品を表す
        /// </summary>
        UI,
        
        /// <summary>
        /// テキスト要素（緑色 #2ECC71）
        /// テキストファイルやメッセージなどの文字情報を表す
        /// </summary>
        Text,
        
        /// <summary>
        /// ビジュアル要素（紫色 #9B59B6）
        /// 画像や図形などの視覚的要素を表す
        /// </summary>
        Visual,
        
        /// <summary>
        /// システム要素（赤色 #E74C3C）
        /// システム制御やメタゲーム機能を表す
        /// 通常は隠しフォルダに配置され、重要な機能を持つ
        /// </summary>
        System
    }

    /// <summary>
    /// ゲーム構成要素を表すモデルクラス
    /// ファイルシステム上の各ファイルに対応し、ゲーム内のオブジェクトとして機能する
    /// INotifyPropertyChangedを実装してUIバインディングをサポート
    /// </summary>
    public class GameComponent : INotifyPropertyChanged
    {
        // プライベートフィールド - データバインディング用のバッキングストア
        private string _name;           // コンポーネント名（ファイル名から拡張子を除いたもの）
        private ComponentType _type;    // コンポーネントの種類
        private bool _isVisible;        // UI上での可視性
        private string _filePath;       // 実際のファイルパス
        private DateTime _lastModified; // ファイル最終更新日時
        private string _content;        // ファイルの内容

        /// <summary>
        /// 構成要素名
        /// ファイル名から拡張子を除いた部分がコンポーネント名になる
        /// 例: "Button.txt" → "Button"
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                // プロパティ変更をUIに通知（WPFデータバインディング用）
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 構成要素タイプ
        /// コンポーネントがどのフォルダに配置されるかによって決まる
        /// タイプ変更時は表示名と色も自動更新される
        /// </summary>
        public ComponentType Type
        {
            get => _type;
            set
            {
                _type = value;
                // 基本プロパティの変更通知
                OnPropertyChanged();
                // 派生プロパティも更新通知（表示名と色が自動更新される）
                OnPropertyChanged(nameof(TypeDisplayName));
                OnPropertyChanged(nameof(TypeColor));
            }
        }

        /// <summary>
        /// 可視性
        /// UIでこのコンポーネントが表示されるかどうかを制御
        /// falseの場合、コンポーネントリストに表示されない
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                // 可視性変更をUIに通知
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ファイルパス
        /// このコンポーネントに対応する実際のファイルシステム上のパス
        /// 例: "C:\Users\Desktop\components\UI\Button.txt"
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                // ファイルパス変更をUIに通知
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 最終更新日時
        /// ファイルが最後に変更された日時
        /// ファイルシステムの更新日時と同期される
        /// </summary>
        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                _lastModified = value;
                // 更新日時変更をUIに通知
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ファイル内容
        /// ファイルのテキスト内容を保持
        /// パズルやメッセージで使用される場合がある
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                // コンテンツ変更をUIに通知
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// プロパティ辞書
        /// コンポーネントに付加するカスタムプロパティを保持
        /// パズルシステムで特定の情報を記録するために使用
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// タイプ表示名
        /// ComponentType列挙値を日本語の表示名に変換
        /// UI上でユーザーに表示されるフレンドリーな名前
        /// </summary>
        public string TypeDisplayName
        {
            get
            {
                // switch式で列挙値を日本語にマッピング
                return Type switch
                {
                    ComponentType.UI => "UI要素",        // ユーザーインターフェース要素
                    ComponentType.Text => "テキスト要素",  // テキスト情報要素
                    ComponentType.Visual => "ビジュアル要素", // 視覚的要素
                    ComponentType.System => "システム要素", // システム制御要素
                    _ => Type.ToString()                   // 未定義の場合は列挙値をそのまま使用
                };
            }
        }

        /// <summary>
        /// タイプ色
        /// コンポーネントタイプに対応するHEXカラーコード
        /// UIでの表示時にコンポーネントを種類別に色分けするために使用
        /// </summary>
        public string TypeColor
        {
            get
            {
                // 各タイプに対応する色を返す
                return Type switch
                {
                    ComponentType.UI => "#3498DB",      // 青色 - UI要素
                    ComponentType.Text => "#2ECC71",    // 緑色 - テキスト要素
                    ComponentType.Visual => "#9B59B6",  // 紫色 - ビジュアル要素
                    ComponentType.System => "#E74C3C",  // 赤色 - システム要素
                    _ => "#95A5A6"                      // グレー - デフォルト
                };
            }
        }

        /// <summary>
        /// 構成要素変更イベント
        /// コンポーネントのプロパティが変更されたときに発火
        /// パズルシステムやゲームロジックが購読して反応する
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs> ComponentChanged;

        /// <summary>
        /// プロパティ変更通知イベント
        /// WPFのデータバインディングのINotifyPropertyChangedインターフェースの実装
        /// UIがコンポーネントの変更を自動的に検知して更新される
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// コンストラクタ
        /// 新しいGameComponentインスタンスを初期化
        /// デフォルトでは可視状態で、現在時刻を更新日時として設定
        /// </summary>
        public GameComponent()
        {
            // デフォルトではコンポーネントを表示状態に設定
            _isVisible = true;
            // 作成時刻を現在時刻で初期化
            _lastModified = DateTime.Now;
        }

        /// <summary>
        /// 静的ファクトリメソッド
        /// 新しいGameComponentインスタンスを作成するための便利メソッド
        /// コンストラクタとプロパティ設定を一度に行える
        /// </summary>
        /// <param name="name">コンポーネント名</param>
        /// <param name="type">コンポーネントタイプ</param>
        /// <param name="content">ファイル内容（オプション）</param>
        /// <returns>初期化されたGameComponentインスタンス</returns>
        public static GameComponent Create(string name, ComponentType type, string content = "")
        {
            // オブジェクト初期化子を使用して新しいインスタンスを作成
            return new GameComponent
            {
                Name = name,                    // コンポーネント名を設定
                Type = type,                    // タイプを設定
                Content = content,              // コンテンツを設定
                IsVisible = true,               // デフォルトで表示状態
                LastModified = DateTime.Now     // 作成時刻を設定
            };
        }

        /// <summary>
        /// プロパティ取得
        /// 指定されたキーのカスタムプロパティ値を取得
        /// キーが存在しない場合はデフォルト値を返す
        /// </summary>
        /// <param name="key">取得したいプロパティのキー</param>
        /// <param name="defaultValue">キーが存在しない場合のデフォルト値</param>
        /// <returns>プロパティ値またはデフォルト値</returns>
        public string GetProperty(string key, string defaultValue = "")
        {
            // TryGetValueで安全に値を取得、存在しない場合はデフォルト値を返す
            return Properties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// プロパティ設定
        /// 指定されたキーにカスタムプロパティ値を設定
        /// 値が変更された場合、ComponentChangedイベントを発火
        /// </summary>
        /// <param name="key">設定したいプロパティのキー</param>
        /// <param name="value">設定する値</param>
        public void SetProperty(string key, string value)
        {
            // 変更前の値を取得（イベント通知用）
            var oldValue = GetProperty(key);
            // 新しい値を設定
            Properties[key] = value;
            // プロパティ変更イベントを発火（パズルシステムが反応する）
            ComponentChanged?.Invoke(this, new ComponentChangedEventArgs(key, oldValue, value));
        }

        /// <summary>
        /// 削除可能かどうか
        /// コンポーネントが安全に削除できるかどうかを判定
        /// 重要なシステムコンポーネントは削除を禁止
        /// </summary>
        /// <returns>true: 削除可能, false: 削除不可</returns>
        public bool CanBeDeleted()
        {
            // Explorerシステムコンポーネントは重要なため削除禁止
            // （ゲームの安全性と復旧機能を保護）
            if (Type == ComponentType.System && Name == "Explorer")
            {
                return false;
            }

            // その他のコンポーネントは削除可能
            return true;
        }

        /// <summary>
        /// 構成要素削除
        /// コンポーネントを削除し、関連するイベントを発火
        /// 削除不可能なコンポーネントの場合は例外をスロー
        /// </summary>
        /// <exception cref="InvalidOperationException">削除不可能なコンポーネントの場合</exception>
        public void Delete()
        {
            // 削除可能性をチェック
            if (!CanBeDeleted())
                throw new InvalidOperationException($"Component {Name} cannot be deleted");

            // 削除イベントを発火（パズルシステムが反応する）
            ComponentChanged?.Invoke(this, new ComponentChangedEventArgs("Deleted", false, true));
        }

        /// <summary>
        /// プロパティ変更通知
        /// INotifyPropertyChangedインターフェースの実装メソッド
        /// WPFのデータバインディング機能にUI更新を通知
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名（CallerMemberName属性で自動取得）</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // PropertyChangedイベントを発火（UIが購読して表示を更新）
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 文字列表現
        /// コンポーネントの情報を人が読める形式で返す
        /// デバッグやログ出力で使用される
        /// </summary>
        /// <returns>"コンポーネント名 (タイプ表示名) - 表示状態"形式の文字列</returns>
        public override string ToString()
        {
            // コンポーネントの基本情報をフォーマットして返す
            return $"{Name} ({TypeDisplayName}) - {(IsVisible ? "表示" : "非表示")}";
        }
    }

    /// <summary>
    /// 構成要素変更イベント引数
    /// GameComponentの変更を通知するためのイベント引数クラス
    /// 変更されたプロパティ名と変更前後の値を保持
    /// </summary>
    public class ComponentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 変更されたプロパティ名
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// 変更前の値
        /// </summary>
        public object OldValue { get; }
        
        /// <summary>
        /// 変更後の値
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名</param>
        /// <param name="oldValue">変更前の値</param>
        /// <param name="newValue">変更後の値</param>
        public ComponentChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}