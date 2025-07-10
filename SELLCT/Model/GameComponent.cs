using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SELLCT.Models
{
    /// <summary>
    /// ゲーム構成要素タイプ
    /// </summary>
    public enum ComponentType
    {
        UI,
        Text,
        Visual,
        System
    }

    /// <summary>
    /// ゲーム構成要素を表すモデルクラス
    /// </summary>
    public class GameComponent : INotifyPropertyChanged
    {
        private string _name;
        private ComponentType _type;
        private bool _isVisible;
        private string _filePath;
        private DateTime _lastModified;
        private string _content;

        /// <summary>
        /// 構成要素名
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 構成要素タイプ
        /// </summary>
        public ComponentType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TypeDisplayName));
                OnPropertyChanged(nameof(TypeColor));
            }
        }

        /// <summary>
        /// 可視性
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                _lastModified = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ファイル内容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// プロパティ辞書
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// タイプ表示名
        /// </summary>
        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    ComponentType.UI => "UI要素",
                    ComponentType.Text => "テキスト要素",
                    ComponentType.Visual => "ビジュアル要素",
                    ComponentType.System => "システム要素",
                    _ => Type.ToString()
                };
            }
        }

        /// <summary>
        /// タイプ色
        /// </summary>
        public string TypeColor
        {
            get
            {
                return Type switch
                {
                    ComponentType.UI => "#3498DB",
                    ComponentType.Text => "#2ECC71",
                    ComponentType.Visual => "#9B59B6",
                    ComponentType.System => "#E74C3C",
                    _ => "#95A5A6"
                };
            }
        }

        /// <summary>
        /// 構成要素変更イベント
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs> ComponentChanged;

        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameComponent()
        {
            _isVisible = true;
            _lastModified = DateTime.Now;
        }

        /// <summary>
        /// 静的ファクトリメソッド
        /// </summary>
        public static GameComponent Create(string name, ComponentType type, string content = "")
        {
            return new GameComponent
            {
                Name = name,
                Type = type,
                Content = content,
                IsVisible = true,
                LastModified = DateTime.Now
            };
        }

        /// <summary>
        /// プロパティ取得
        /// </summary>
        public string GetProperty(string key, string defaultValue = "")
        {
            return Properties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// プロパティ設定
        /// </summary>
        public void SetProperty(string key, string value)
        {
            var oldValue = GetProperty(key);
            Properties[key] = value;
            ComponentChanged?.Invoke(this, new ComponentChangedEventArgs(key, oldValue, value));
        }

        /// <summary>
        /// 削除可能かどうか
        /// </summary>
        public bool CanBeDeleted()
        {
            // 重要なシステム要素は削除不可
            if (Type == ComponentType.System && Name == "Explorer")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 構成要素削除
        /// </summary>
        public void Delete()
        {
            if (!CanBeDeleted())
                throw new InvalidOperationException($"Component {Name} cannot be deleted");

            ComponentChanged?.Invoke(this, new ComponentChangedEventArgs("Deleted", false, true));
        }

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({TypeDisplayName}) - {(IsVisible ? "表示" : "非表示")}";
        }
    }

    /// <summary>
    /// 構成要素変更イベント引数
    /// </summary>
    public class ComponentChangedEventArgs : EventArgs
    {
        public string PropertyName { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public ComponentChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}