using System;
using System.Windows;
using QuanLyShopGiay.Views;
using QuanLyShopGiay.Helpers;

namespace QuanLyShopGiay
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Mở Login trước
            Login loginWindow = new Login();
            if (loginWindow.ShowDialog() == true)
            {
                // Đăng nhập thành công
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            else
            {
                // Hủy đăng nhập
                this.Shutdown();
            }
        }
    }
}