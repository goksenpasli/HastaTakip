using FastReport.Data;
using FastReport.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace HastaTakip
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            RegisteredObjects.AddConnection(typeof(SqlCeDataConnection));
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.KeyDownEvent, new KeyEventHandler(KeyDown));
            EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.KeyDownEvent, new KeyEventHandler(KeyDown));
        }
     
        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var t = args.LoadedAssembly.GetLoadedModules();

            if (t[0].Name.Substring(t[0].Name.Length - 3, 3) == "exe")
                Shutdown();
        }

        private void CmbResimKaydet_Click(object sender, RoutedEventArgs e) => Resim.ComboBoxResimKaydet(e);

        #region Keypress

        private static void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) MoveToNextUiElement(e);
        }

        private static void MoveToNextUiElement(RoutedEventArgs e)
        {
            const FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
            var request = new TraversalRequest(focusDirection);
            if (!(Keyboard.FocusedElement is UIElement elementWithFocus)) return;
            if (elementWithFocus.MoveFocus(request)) e.Handled = true;
        }

        #endregion Keypress
 
    }
}