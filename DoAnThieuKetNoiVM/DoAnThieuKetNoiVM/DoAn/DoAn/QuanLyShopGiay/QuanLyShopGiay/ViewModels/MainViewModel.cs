using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _tenNhanVien;
        private string _quyen;

        // ===== CALLBACK DELEGATES =====
        public Action<string> Navigate { get; set; }
        public Action MoLoginView { get; set; }

        // ===== PROPERTIES =====
        public string TenNhanVien
        {
            get => _tenNhanVien ?? SessionManager.HoTenNV; 
            set
            {
                _tenNhanVien = value;
                OnPropertyChanged();
            }
        }

        public string Quyen
        {
            get => _quyen ?? SessionManager.TenVT;  // ← Sửa
            set
            {
                _quyen = value;
                OnPropertyChanged();
            }
        }

        // ===== COMMANDS =====
        public ICommand NavigateToDashboardCommand { get; set; }
        public ICommand NavigateToSanPhamCommand { get; set; }
        public ICommand NavigateToKhachHangCommand { get; set; }
        public ICommand NavigateToHoaDonCommand { get; set; }
        public ICommand NavigateToNhanVienCommand { get; set; }
        public ICommand NavigateToTaiKhoanCommand { get; set; }
        public ICommand DangXuatCommand { get; set; }

        public MainViewModel()
        {
            InitCommands();
        }

        private void InitCommands()
        {
            NavigateToDashboardCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("Dashboard");
            });

            NavigateToSanPhamCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("SanPham");
            });

            NavigateToKhachHangCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("KhachHang");
            });

            NavigateToHoaDonCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("HoaDon");
            });

            NavigateToNhanVienCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("NhanVien");
            });

            NavigateToTaiKhoanCommand = new RelayCommand(o =>
            {
                Navigate?.Invoke("TaiKhoan");
            });

            DangXuatCommand = new RelayCommand(o =>
            {
                MoLoginView?.Invoke();
            });
        }
    }
}