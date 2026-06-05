using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command;

namespace QuanLyShopGiay.ViewModels
{
    public class KhachHangDisplayModel
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string DienThoai { get; set; }
        public int Diem { get; set; }
        public decimal TongChiTieu { get; set; }
        public string HangThanhVien { get; set; }
    }

    public class KhachHangViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities3 _db;

        private ObservableCollection<KhachHangDisplayModel> _listKhachHang;
        public ObservableCollection<KhachHangDisplayModel> ListKhachHang
        {
            get => _listKhachHang;
            set => SetProperty(ref _listKhachHang, value);
        }

        private KhachHangDisplayModel _selectedItem;
        public KhachHangDisplayModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    MaKhachHang = value.MaKhachHang;
                    TenKhachHang = value.TenKhachHang;
                    DienThoai = value.DienThoai;
                    Diem = value.Diem;
                    TongMuaText = string.Format("{0:N0} ₫", value.TongChiTieu);
                }
            }
        }

        private string _maKhachHang;
        public string MaKhachHang { get => _maKhachHang; set => SetProperty(ref _maKhachHang, value); }

        private string _tenKhachHang;
        public string TenKhachHang { get => _tenKhachHang; set => SetProperty(ref _tenKhachHang, value); }

        private string _dienThoai;
        public string DienThoai { get => _dienThoai; set => SetProperty(ref _dienThoai, value); }

        private int _diem;
        public int Diem { get => _diem; set => SetProperty(ref _diem, value); }

        private string _tongMuaText = "0 ₫";
        public string TongMuaText { get => _tongMuaText; set => SetProperty(ref _tongMuaText, value); }

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public KhachHangViewModel()
        {
            try
            {
                _db = new QLShopGiayEntities3();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối cơ sở dữ liệu: " + ex.Message);
            }

            SearchCommand = new RelayCommand<object>(_ => true, _ => LoadData());
            AddCommand = new RelayCommand<object>(_ => CanExecuteSave(), _ => ExecuteAdd());
            EditCommand = new RelayCommand<object>(_ => SelectedItem != null && CanExecuteSave(), _ => ExecuteEdit());
            DeleteCommand = new RelayCommand<object>(_ => SelectedItem != null, _ => ExecuteDelete());
            ClearCommand = new RelayCommand<object>(_ => true, _ => ClearInputs());

            LoadData();
        }

        private void LoadData()
        {
            if (_db == null) return;
            try
            {
                var query = _db.KhachHang.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(kh => kh.TenKhachHang.ToLower().Contains(keyword) || 
                                              kh.MaKhachHang.ToLower().Contains(keyword) || 
                                              (kh.DienThoai != null && kh.DienThoai.Contains(keyword)));
                }

                ListKhachHang = new ObservableCollection<KhachHangDisplayModel>(query.ToList().Select(kh => {
                    decimal tongChiTieu = _db.HoaDon.Where(hd => hd.MaKhachHang == kh.MaKhachHang && hd.TrangThai == "Đã thanh toán").Sum(hd => (decimal?)hd.TongTien) ?? 0;
                    return new KhachHangDisplayModel
                    {
                        MaKhachHang = kh.MaKhachHang,
                        TenKhachHang = kh.TenKhachHang,
                        DienThoai = kh.DienThoai,
                        Diem = kh.Diem ?? 0,
                        TongChiTieu = tongChiTieu,
                        HangThanhVien = GetRankName(tongChiTieu)
                    };
                }).OrderBy(x => x.MaKhachHang).ToList());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message); }
        }

        private static string GetRankName(decimal totalAmount)
        {
            if (totalAmount >= 30000000) return "Kim cương 💎";
            if (totalAmount >= 15000000) return "Vàng 🥇";
            if (totalAmount >= 5000000) return "Bạc 🥈";
            return "Thành viên mới 🌱";
        }

        private void ClearInputs()
        {
            _selectedItem = null; 
            OnPropertyChanged(nameof(SelectedItem));
            MaKhachHang = TenKhachHang = DienThoai = string.Empty;
            Diem = 0;
            TongMuaText = "0 ₫";
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(TenKhachHang) && !string.IsNullOrWhiteSpace(DienThoai);
        }

        private void ExecuteAdd()
        {
            try
            {
                string newId = GenerateNextId();
                var kh = new KhachHang
                {
                    MaKhachHang = newId,
                    TenKhachHang = TenKhachHang.Trim(),
                    DienThoai = DienThoai.Trim(),
                    Diem = Diem
                };

                _db.KhachHang.Add(kh);
                _db.SaveChanges();
                LoadData();
                ClearInputs();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm mới: " + ex.Message); }
        }

        private void ExecuteEdit()
        {
            try
            {
                var kh = _db.KhachHang.FirstOrDefault(x => x.MaKhachHang == SelectedItem.MaKhachHang);
                if (kh != null)
                {
                    kh.TenKhachHang = TenKhachHang.Trim();
                    kh.DienThoai = DienThoai.Trim();
                    kh.Diem = Diem;

                    _db.SaveChanges();
                    LoadData();
                    ClearInputs();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi cập nhật: " + ex.Message); }
        }

        private void ExecuteDelete()
        {
            if (MessageBox.Show("Xóa khách hàng này và toàn bộ dữ liệu liên quan?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var kh = _db.KhachHang.FirstOrDefault(x => x.MaKhachHang == SelectedItem.MaKhachHang);
                    if (kh != null)
                    {
                        var hds = _db.HoaDon.Where(h => h.MaKhachHang == kh.MaKhachHang).ToList();
                        foreach (var hd in hds)
                        {
                            var cthds = _db.ChiTietHoaDon.Where(c => c.MaHD == hd.MaHD);
                            _db.ChiTietHoaDon.RemoveRange(cthds);
                        }
                        _db.HoaDon.RemoveRange(hds);

                        _db.KhachHang.Remove(kh);
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi khi xóa: " + ex.Message); }
            }
        }

        private string GenerateNextId()
        {
            var maxId = _db.KhachHang
                .Select(x => x.MaKhachHang)
                .ToList()
                .Where(x => x.StartsWith("KH") && x.Length > 2)
                .Select(x => int.TryParse(x.Substring(2), out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();

            return string.Format("KH{0:D2}", maxId + 1);
        }
    }
}
