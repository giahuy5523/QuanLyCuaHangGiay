using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Models; // Namespace chứa các thực thể sinh ra từ DB của bạn
using QuanLyShopGiay.Helpers; // Namespace chứa class RelayCommand của bạn

namespace QuanLyShopGiay.ViewModels
{
    public class KhoHangViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities _db;

        // ════════════════════════════════════════════════════════════════
        // TAB 1: TỒN KHO - CHỈ SỬ DỤNG CLASS THỰC THỂ TỪ SQL
        // ════════════════════════════════════════════════════════════════
        public ObservableCollection<TON_KHO> ListTonKho { get; set; } = new ObservableCollection<TON_KHO>();
        public List<KHO> ListFilterKho { get; set; }
        public List<NHA_SAN_XUAT> ListFilterNSX { get; set; }

        private string _searchTextKho;
        public string SearchTextKho { get => _searchTextKho; set { if (SetProperty(ref _searchTextKho, value)) LoadTonKho(); } }

        private string _selectedMaKho;
        public string SelectedMaKho { get => _selectedMaKho; set { if (SetProperty(ref _selectedMaKho, value)) LoadTonKho(); } }

        private string _selectedMaNSX;
        public string SelectedMaNSX { get => _selectedMaNSX; set { if (SetProperty(ref _selectedMaNSX, value)) LoadTonKho(); } }


        // ════════════════════════════════════════════════════════════════
        // TAB 2: PHIẾU NHẬP - CHỈ SỬ DỤNG CLASS THỰC THỂ TỪ SQL
        // ════════════════════════════════════════════════════════════════
        public ObservableCollection<PHIEU_NHAP> ListPhieuNhap { get; set; } = new ObservableCollection<PHIEU_NHAP>();
        public ObservableCollection<CT_PHIEU_NHAP> ListCtPhieuNhapTam { get; set; } = new ObservableCollection<CT_PHIEU_NHAP>();

        public List<KHO> ListKhoNhap { get; set; }
        public List<NHA_SAN_XUAT> ListNSXNhap { get; set; }
        public List<BIEN_THE_SP> ListBienTheCombo { get; set; }

        private string _maPN;
        public string MaPN { get => _maPN; set => SetProperty(ref _maPN, value); }

        private string _cboSelectedKhoNhap;
        public string CboSelectedKhoNhap { get => _cboSelectedKhoNhap; set => SetProperty(ref _cboSelectedKhoNhap, value); }

        private string _cboSelectedNSXNhap;
        public string CboSelectedNSXNhap { get => _cboSelectedNSXNhap; set => SetProperty(ref _cboSelectedNSXNhap, value); }

        private int? _cboSelectedBienThe;
        public int? CboSelectedBienThe { get => _cboSelectedBienThe; set => SetProperty(ref _cboSelectedBienThe, value); }

        private string _txtSoLuongNhap;
        public string TxtSoLuongNhap { get => _txtSoLuongNhap; set => SetProperty(ref _txtSoLuongNhap, value); }

        private string _txtGiaNhap;
        public string TxtGiaNhap { get => _txtGiaNhap; set => SetProperty(ref _txtGiaNhap, value); }


        // ════════════════════════════════════════════════════════════════
        // COMMANDS (DẠNG NON-GENERIC ICOMMAND)
        // ════════════════════════════════════════════════════════════════
        public ICommand AddToGridCommand { get; set; }
        public ICommand SavePhieuNhapCommand { get; set; }
        public ICommand ClearFormCommand { get; set; }


        // ════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ════════════════════════════════════════════════════════════════
        public KhoHangViewModel()
        {
            _db = new QLShopGiayEntities();

            LoadFiltersAndCombos();
            LoadTonKho();
            LoadLichSuPhieuNhap();
            TaoMaPhieuNhapMoi();

            // ════════════════════════════════════════════════════════════════
            // SỬA ĐỔI LOGIC THÊM VÀO LƯỚI TẠM ĐỂ HIỂN THỊ CHUẨN XÁC GIÁ TRỊ
            // ════════════════════════════════════════════════════════════════

            // 1. Lệnh thêm biến thể vào lưới tạm
            AddToGridCommand = new RelayCommand(
                p => {
                    if (CboSelectedBienThe == null) return;
                    if (!int.TryParse(TxtSoLuongNhap, out int sl) || sl <= 0) return;
                    if (!decimal.TryParse(TxtGiaNhap, out decimal gia) || gia < 0) return;

                    var trungItem = ListCtPhieuNhapTam.FirstOrDefault(x => x.MaBienThe == CboSelectedBienThe.Value);
                    if (trungItem != null)
                    {
                        // Tạo một đối tượng chi tiết hoàn toàn mới để ép WPF nhận biết sự thay đổi địa chỉ vùng nhớ và tự động render lại số lượng, giá
                        var newCt = new CT_PHIEU_NHAP
                        {
                            MaPN = trungItem.MaPN,
                            MaBienThe = trungItem.MaBienThe,
                            SoLuong = (trungItem.SoLuong ?? 0) + sl, // Cộng dồn số lượng cũ và mới
                            GiaNhap = gia,                           // Cập nhật giá nhập mới nhất
                            BIEN_THE_SP = trungItem.BIEN_THE_SP
                        };

                        // Tìm vị trí dòng cũ và gán đè dòng mới vào để ObservableCollection tự kích hoạt sự kiện cập nhật giao diện hiển thị
                        int index = ListCtPhieuNhapTam.IndexOf(trungItem);
                        ListCtPhieuNhapTam[index] = newCt;
                    }
                    else
                    {
                        // LẤY TỪ ListBienTheCombo ĐÃ ĐƯỢC INCLUDE ĐẦY ĐỦ TÊN SẢN PHẨM, SIZE, MÀU để hiển thị chuẩn xác, không bị trống tên
                        var bienTheGoc = ListBienTheCombo.FirstOrDefault(x => x.MaBienThe == CboSelectedBienThe.Value);

                        ListCtPhieuNhapTam.Add(new CT_PHIEU_NHAP
                        {
                            MaBienThe = CboSelectedBienThe.Value,
                            SoLuong = sl,
                            GiaNhap = gia,
                            BIEN_THE_SP = bienTheGoc
                        });
                    }

                    // Xóa trống thông tin nhập liệu để chuẩn bị nhập sản phẩm tiếp theo
                    TxtSoLuongNhap = string.Empty;
                    TxtGiaNhap = string.Empty;
                },
                p => CboSelectedBienThe != null
            );

            // 2. Lệnh lưu toàn bộ phiếu nhập vào Database
            SavePhieuNhapCommand = new RelayCommand(
                p => {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(CboSelectedKhoNhap) || string.IsNullOrEmpty(CboSelectedNSXNhap) || ListCtPhieuNhapTam.Count == 0)
                            {
                                MessageBox.Show("Vui lòng nhập đầy đủ thông tin bắt buộc và thêm ít nhất một sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            var phieuNhap = new PHIEU_NHAP
                            {
                                MaPN = MaPN,
                                NgayNhap = DateTime.Now,
                                MaKho = CboSelectedKhoNhap,
                                MaNSX = CboSelectedNSXNhap
                            };
                            _db.PHIEU_NHAP.Add(phieuNhap);

                            foreach (var item in ListCtPhieuNhapTam)
                            {
                                var ct = new CT_PHIEU_NHAP
                                {
                                    MaPN = MaPN,
                                    MaBienThe = item.MaBienThe,
                                    SoLuong = item.SoLuong,
                                    GiaNhap = item.GiaNhap
                                };
                                _db.CT_PHIEU_NHAP.Add(ct);
                            }

                            _db.SaveChanges();
                            transaction.Commit();

                            MessageBox.Show($"Lưu thành công phiếu nhập {MaPN}!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            ClearAllInputs();
                            LoadTonKho();
                            LoadLichSuPhieuNhap();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                },
                p => ListCtPhieuNhapTam.Count > 0
            );

            // 3. Lệnh làm mới form
            ClearFormCommand = new RelayCommand(
                p => ClearAllInputs(),
                p => true
            );
        }

        // ════════════════════════════════════════════════════════════════
        // CÁC PHƯƠNG THỨC TRUY VẤN DỮ LIỆU TỪ SQL
        // ════════════════════════════════════════════════════════════════
        private void LoadFiltersAndCombos()
        {
            try
            {
                var khoGoc = _db.KHO.ToList();
                var nsxGoc = _db.NHA_SAN_XUAT.ToList();

                var filterKhos = new List<KHO>(khoGoc);
                filterKhos.Insert(0, new KHO { MaKho = "ALL", TenKho = "── Tất cả kho ──" });
                ListFilterKho = filterKhos; SelectedMaKho = "ALL";

                var filterNsxs = new List<NHA_SAN_XUAT>(nsxGoc);
                filterNsxs.Insert(0, new NHA_SAN_XUAT { MaNSX = "ALL", TenNSX = "── Tất cả NSX ──" });
                ListFilterNSX = filterNsxs; SelectedMaNSX = "ALL";

                ListKhoNhap = khoGoc;
                ListNSXNhap = nsxGoc;

                ListBienTheCombo = _db.BIEN_THE_SP
                                      .Include("SAN_PHAM")
                                      .Include("SIZE_GIAY")
                                      .Include("MAU_SAC")
                                      .Where(x => x.IsDeleted == false && x.SAN_PHAM.IsDeleted == false)
                                      .ToList();
            }
            catch { }
        }

        public void LoadTonKho()
        {
            try
            {
                var query = _db.TON_KHO
                               .Include("KHO")
                               .Include("BIEN_THE_SP.SAN_PHAM.NHA_SAN_XUAT")
                               .Include("BIEN_THE_SP.SIZE_GIAY")
                               .Include("BIEN_THE_SP.MAU_SAC")
                               .AsQueryable();

                if (!string.IsNullOrEmpty(SelectedMaKho) && SelectedMaKho != "ALL") query = query.Where(x => x.MaKho == SelectedMaKho);
                if (!string.IsNullOrEmpty(SelectedMaNSX) && SelectedMaNSX != "ALL") query = query.Where(x => x.BIEN_THE_SP.SAN_PHAM.MaNSX == SelectedMaNSX);
                if (!string.IsNullOrWhiteSpace(SearchTextKho))
                {
                    string kw = SearchTextKho.Trim().ToLower();
                    query = query.Where(x => x.BIEN_THE_SP.SAN_PHAM.TenSP.ToLower().Contains(kw) || x.BIEN_THE_SP.SKU.ToLower().Contains(kw));
                }

                ListTonKho = new ObservableCollection<TON_KHO>(query.OrderBy(x => x.KHO.TenKho).ThenBy(x => x.BIEN_THE_SP.SAN_PHAM.TenSP).ToList());
                OnPropertyChanged(nameof(ListTonKho));
            }
            catch { }
        }

        public void LoadLichSuPhieuNhap()
        {
            try
            {
                // BỔ SUNG .Include("CT_PHIEU_NHAP") để tính được Tổng Số Lượng và Tổng Tiền của mỗi phiếu nhập lịch sử
                var query = _db.PHIEU_NHAP
                               .Include("KHO")
                               .Include("NHA_SAN_XUAT")
                               .Include("CT_PHIEU_NHAP");

                ListPhieuNhap = new ObservableCollection<PHIEU_NHAP>(query.OrderByDescending(x => x.NgayNhap).ToList());
                OnPropertyChanged(nameof(ListPhieuNhap));
            }
            catch { }
        }

        private void TaoMaPhieuNhapMoi()
        {
            string ngayChon = DateTime.Now.ToString("yyyyMMdd");
            var maxPNTrongNgay = _db.PHIEU_NHAP
                                    .Where(p => p.MaPN.StartsWith("PN" + ngayChon))
                                    .Select(p => p.MaPN)
                                    .ToList()
                                    .OrderByDescending(x => x)
                                    .FirstOrDefault();

            if (maxPNTrongNgay == null)
            {
                MaPN = $"PN{ngayChon}001";
            }
            else
            {
                int sttCu = int.Parse(maxPNTrongNgay.Substring(10, 3));
                MaPN = $"PN{ngayChon}{(sttCu + 1).ToString("D3")}";
            }
        }

        private void ClearAllInputs()
        {
            ListCtPhieuNhapTam.Clear();
            CboSelectedKhoNhap = null;
            CboSelectedNSXNhap = null;
            CboSelectedBienThe = null;
            TxtSoLuongNhap = string.Empty;
            TxtGiaNhap = string.Empty;
            TaoMaPhieuNhapMoi();
        }
    }
}