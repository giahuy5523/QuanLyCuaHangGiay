using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Views;
using QuanLyShopGiay.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyShopGiay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Bước 1: Khởi tạo test password (chỉ chạy lần đầu, sau đó comment lại)
            try
            {
                InitializePasswordHelper.SetTestPasswords();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo password: {ex.Message}", "Cảnh báo");
            }

            // Bước 2: Hiển thị LoginView
            Login loginWindow = new Login();
            bool? loginResult = loginWindow.ShowDialog();

            // Bước 3: Kiểm tra đăng nhập thành công
            if (loginResult == true && UserSession.IsLoggedIn)
            {
                // Đăng nhập thành công, khởi tạo MainWindow
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            else
            {
                // Hủy đăng nhập hoặc đóng LoginView
                MessageBox.Show("Đăng nhập bị hủy. Chương trình sẽ đóng.", "Thông báo");
                this.Shutdown();
            }
        }
    }
}