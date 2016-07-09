using Microsoft.Xaml.Interactivity;
using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace App46
{
    public class AncestorBindingBehavior : DependencyObject, IBehavior
    {
        /// <summary>
        /// Binding対象のオブジェクト
        /// </summary>
        public DependencyObject AssociatedObject { get; set; }

        /// <summary>
        /// Binding
        /// </summary>
        public Binding Binding { get; set; }

        /// <summary>
        /// 親方向に遡って探したい型の名前
        /// </summary>
        public string AncestorType { get; set; }

        /// <summary>
        /// Binding対象のプロパティ名
        /// </summary>
        public string TargetPropertyName { get; set; }

        public void Attach(DependencyObject associatedObject)
        {
            this.AssociatedObject = associatedObject;
            // プロパティの設定が未完全の場合は何もしない
            if (this.Binding == null || string.IsNullOrWhiteSpace(this.TargetPropertyName) || string.IsNullOrWhiteSpace(this.AncestorType)) { return; }
            // VisualTree構築後に1度だけ実行する
            ((FrameworkElement)this.AssociatedObject).Loaded += this.AncestorBindingBehavior_Loaded;
        }

        private void AncestorBindingBehavior_Loaded(object sender, RoutedEventArgs e)
        {
            // イベント解除
            ((FrameworkElement)this.AssociatedObject).Loaded -= this.AncestorBindingBehavior_Loaded;

            // 親を遡ってソースとなるオブジェクトを探す
            var source = FindAncestorType(this.AssociatedObject, this.AncestorType);
            // バインドする添付プロパティを探す
            var targetProperty = GetDependencyProperty(this.AssociatedObject.GetType(), this.TargetPropertyName);
            // 失敗したら何もしない
            if (source == null || targetProperty == null) { return; }
            // Bindingのソースをセットしてプロパティにバインドする
            this.Binding.Source = source;
            ((FrameworkElement)this.AssociatedObject).SetBinding(targetProperty,
                this.Binding);
        }

        public void Detach()
        {
        }

        /// <summary>
        /// VisualTreeを親へ辿っていって指定した型名のものがあったら返す
        /// </summary>
        /// <param name="element"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static DependencyObject FindAncestorType(DependencyObject element, string type)
        {
            if (element.GetType().Name == type) { return element; }

            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null) { return null; }
            return FindAncestorType(parent, type);
        }

        /// <summary>
        /// 継承関係を親へ辿っていきながら指定した添付プロパティが定義されてたらそれを返す
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            var field = type
                .GetTypeInfo()
                .GetDeclaredField($"{propertyName}Property");
            if (field != null)
            {
                return (DependencyProperty)field.GetValue(null);
            }

            var property = type
                .GetTypeInfo()
                .GetDeclaredProperty($"{propertyName}Property");
            if (property != null)
            {
                return (DependencyProperty)property.GetValue(null);
            }

            var baseType = type.GetTypeInfo().BaseType;
            if (baseType == typeof(object)) { return null; }

            return GetDependencyProperty(baseType, propertyName);
        }
    }
}

