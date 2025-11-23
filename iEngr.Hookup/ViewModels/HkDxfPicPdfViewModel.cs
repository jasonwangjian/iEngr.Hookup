using iEngr.Hookup.Models;
using iEngr.Hookup.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace iEngr.Hookup.ViewModels
{
    public enum FileStatus
    {
        NotAssigned,
        NotDxfExtension,
        ValidedDxf,
        InValidedDxf
    }
    public enum AttachTo
    {
        ComosAssigned,
        ComosAvailable,
        LibAssigned,
        LibAvailable
    }
    public class HkDxfPicPdfViewModel : INotifyPropertyChanged
    {
        public DxfRenderer DxfRenderer;
        private DxfRenderer _renderer;
        private double _scale = 1.0;


        public HkDxfPicPdfViewModel(Canvas dxfCanvas)
        {
            PdfPages = new ObservableCollection<BitmapImage>();
            PreviousPageCommand = new RelayCommand<object>(
                _ => CurrentPageIndex--,
                _ => IsPdfFile && CurrentPageIndex > 0);

            NextPageCommand = new RelayCommand<object>(
                _ => CurrentPageIndex++,
                _ => IsPdfFile && CurrentPageIndex < TotalPages - 1);

            _emptyPicturePath = "pack://application:,,,/iEngr.Hookup;component/Resources/EmptyPicture.png";
            _unfoundPicturePath = "pack://application:,,,/iEngr.Hookup;component/Resources/UnfoundPicture.Png";
            SetImageSource(_emptyPicturePath);
            _renderer = new DxfRenderer(dxfCanvas);
            DxfRenderer = _renderer;
        }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        string _emptyPicturePath;
        string _unfoundPicturePath;

        private AttachTo _attachTo;
        public AttachTo AttachTo
        {
            get => _attachTo;
            set
            {
                if (SetField(ref _attachTo, value))
                    OnPropertyChanged(nameof(AllowDrop));
            }
        }

        public bool AllowDrop
        {
            get
            {
                if (AttachTo == AttachTo.ComosAssigned || AttachTo == AttachTo.ComosAvailable)
                {
                    return true;
                }
                return false;
            }
        }

        private bool _isPdfFile;
        public bool IsPdfFile
        {
            get => _isPdfFile;
            set
            {
                _isPdfFile = value;
                OnPropertyChanged();
            }
        }
        private bool _isDxfFile;
        public bool IsDxfFile
        {
            get => _isDxfFile;
            set
            {
                _isDxfFile = value;
                OnPropertyChanged();
            }
        }
        private string _picturePath;
        public string PicturePath
        {
            get => _picturePath;
            set
            {
                if (SetField(ref _picturePath, value))
                {
                    if (string.IsNullOrEmpty(PicturePath))
                    {
                        SetImageSource(_emptyPicturePath);
                    }
                    else
                        OpenFile(PicturePath);
                }
            }
        }
        private bool IsValidFile
        {
            get
            {
                return FileStatus == FileStatus.ValidedDxf;
            }
        }
        private FileStatus _fileStatus;
        public FileStatus FileStatus
        {
            get => _fileStatus;
            set
            {
                if (SetField(ref _fileStatus, value))
                    OnPropertyChanged(nameof(IsValidFile));
            }
        }
        //private async void OpenFile(string filePath)
        //{
        //    string fileExtension = Path.GetExtension(filePath).ToLower();

        //    if (fileExtension == ".pdf")
        //    {
        //        await OpenPdfFileAsync(filePath);
        //    }
        //    else
        //    {
        //        OpenImageFile(filePath);
        //    }
        //}
        private async void OpenFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".pdf")
            {
                await OpenPdfFileAsync(filePath);
                IsDxfFile = false;
            }
            else if (fileExtension == ".dxf")
            {
                FileStatus = _renderer.RenderDxf(filePath, _scale);
                if (FileStatus == FileStatus.InValidedDxf)
                {
                    SetImageSource(_unfoundPicturePath);
                    IsDxfFile = false;
                }
                else
                {
                    IsDxfFile = true;
                }
            }
            else
            {
                OpenImageFile(filePath);
                IsDxfFile = false;
            }
            //else
            //{
            //    FileStatus = FileStatus.NotDxfExtension; ;
            //}
        }
        private async Task OpenPdfFileAsync(string filePath)
        {
            //ShowLoading(true);

            try
            {
                // 使用ViewModel加载PDF
                await LoadPdfFileAsync(filePath);
            }
            catch (Exception ex)
            {
                SetImageSource(_unfoundPicturePath);
                Debug.WriteLine($"___HkPictureViewModel.OpenImageFile(string filePath), Error: 无法加载PDF文件: {ex.Message}");
            }
            finally
            {
                //ShowLoading(false);
            }
        }

        private void OpenImageFile(string filePath)
        {
            // 清除PDF状态
            ClearPdfPages();

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ImageSource = bitmap;

                // 居中处理在ContentImage_Loaded中完成
            }
            catch (Exception ex)
            {
                SetImageSource(_unfoundPicturePath);
                Debug.WriteLine($"___HkPictureViewModel.OpenImageFile(string filePath), Error: 无法加载图像: {ex.Message}");
            }
        }

        private BitmapImage _imageSource;

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set => SetField(ref _imageSource, value);
        }
        private ObservableCollection<BitmapImage> _pdfPages;
        public ObservableCollection<BitmapImage> PdfPages
        {
            get => _pdfPages;
            set => SetField(ref _pdfPages, value);
        }
        private int _currentPageIndex;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                _currentPageIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPageDisplay));
                UpdateCurrentImage();
            }
        }
        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPageDisplay));
            }
        }
        public string CurrentPageDisplay => IsPdfFile ? $"第 {CurrentPageIndex + 1} 页 / 共 {TotalPages} 页" : "";

        private void UpdateCurrentImage()
        {
            if (IsPdfFile && PdfPages != null && CurrentPageIndex >= 0 && CurrentPageIndex < PdfPages.Count)
            {
                ImageSource = PdfPages[CurrentPageIndex];
            }
        }
        // 加载PDF文件的方法
        public async Task LoadPdfFileAsync(string filePath)
        {
            IsPdfFile = true;
            //CurrentFilePath = filePath;

            try
            {
                // 获取PDF页数
                TotalPages = PDFWrapper.GetPageCount(filePath);
                PdfPages?.Clear();

                // 逐页加载PDF
                for (int i = 0; i < TotalPages; i++)
                {

                    var bitmap = await Task.Run(() =>
                        PDFWrapper.GetImage(filePath, i, 300));

                    var bitmapSource = ConvertBitmapToBitmapSource(bitmap);
                    PdfPages.Add(bitmapSource);
                }

                CurrentPageIndex = 0;
            }
            catch (Exception ex)
            {
                // 处理异常
                ClearPdfPages();
                throw;
            }
        }
        private BitmapImage ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        public void ClearPdfPages()
        {
            PdfPages?.Clear();
            TotalPages = 0;
            CurrentPageIndex = 0;
            IsPdfFile = false;
        }
        public void SetImageSource(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            if (filePath == null) return;
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 确保立即加载
            bitmap.EndInit();

            ImageSource = bitmap;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
