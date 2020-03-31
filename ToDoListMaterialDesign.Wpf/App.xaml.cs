using Prism.Ioc;
using System.Windows;
using ToDoListMaterialDesign.Views;

namespace ToDoListMaterialDesign
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>DIコンテナへ型を登録します。</summary>
        /// <param name="containerRegistry">DIコンテナへの型を登録するIContainerRegistry</param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<EditView>();
            containerRegistry.RegisterForNavigation<MainView>();

            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<DialogWindowEditView, ViewModels.DialogWindowEditViewModel>();
        }

        /// <summary>
        /// 二重起動防止
        /// </summary>
        private static System.Threading.Mutex mutex;

        private void PrismApplication_Startup(object sender, StartupEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            mutex = new System.Threading.Mutex(false, System.IO.Path.GetFileNameWithoutExtension(assembly.GetName().CodeBase));

            if (!mutex.WaitOne(0, false))
            {
                mutex.Close();
                mutex = null;

                this.Shutdown();
            }
        }

        private void PrismApplication_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }
    }
}
