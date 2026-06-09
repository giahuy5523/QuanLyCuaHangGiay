using Microsoft.Reporting.WinForms;
using QuanLyShopGiay.Command; // Đảm bảo đã thiết lập namespace chứa class RelayCommand của bạn
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class InBaoCaoViewModel : BaseViewModel
    {
        private readonly ReportService _reportService;

        public string MaHD { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string LoaiBaoCao { get; set; }

        // ViewModel chỉ expose ReportViewer — không xử lý UI
        private ReportViewer _reportViewer;
        public ReportViewer ReportViewerInstance
        {
            get => _reportViewer;
            set { _reportViewer = value; OnPropertyChanged(); }
        }

        public InBaoCaoViewModel()
        {
            _reportService = new ReportService();
        }

        // Gọi method này sau khi set các thuộc tính
        public void KhoiTaoBaoCao()
        {
            ReportViewerInstance = _reportService.TaoVaHienThiBaoCao(
                loaiBaoCao: LoaiBaoCao,
                maHD: MaHD,
                tuNgay: TuNgay,
                denNgay: DenNgay
            );
        }
    }
}