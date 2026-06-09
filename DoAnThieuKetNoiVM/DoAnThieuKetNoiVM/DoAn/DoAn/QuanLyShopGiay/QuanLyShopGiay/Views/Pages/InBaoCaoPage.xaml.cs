using Microsoft.Reporting.WinForms;
using QuanLyShopGiay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyShopGiay.Views.Pages
{
    /// <summary>
    /// Interaction logic for InBaoCaoPage.xaml
    /// </summary>
    public partial class InBaoCaoPage : Page
    {
        private ReportViewer _reportViewer;

        public InBaoCaoPage(InBaoCaoViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // ĐÃ SỬA: Đợi trang nạp xong giao diện hoàn toàn rồi mới bỏ ReportViewer vào Host
            this.Loaded += (s, e) =>
            {
                if (vm != null && vm.ReportViewerInstance != null)
                {
                    ReportHost.Child = vm.ReportViewerInstance;
                }
            };
        }
    }
}
