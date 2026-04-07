using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace WPF_EXAM
{
    internal class ItemsData : PropertyChangedHandler
    {
        private ObservableCollection<ItemBase> _items = new ObservableCollection<ItemBase>(), _likedItems = new ObservableCollection<ItemBase>();

        public ObservableCollection<ItemBase> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }
        public ObservableCollection<ItemBase> LikedItems
        {
            get { return _likedItems; }
            set
            {
                if (_likedItems != value)
                {
                    _likedItems = value;
                    OnPropertyChanged(nameof(LikedItems));
                }
            }
        }

    }

    internal enum FileType
    {
        Unknown,
        Audio
    }

    internal class ItemBase : PropertyChangedHandler
    {
        private string _index, _title, _artist, _album, _genre, _year, _duration;
        private byte[] _pictureBytes;
        private bool _isLiked, _isPlaying;

        public FileType FileType { get; set; } = FileType.Unknown;
        public string Path { get; set; } = string.Empty;
        public string UniqueId { get; set; } = Guid.NewGuid().ToString();
        public string Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }
        public byte[] PictureBytes
        {
            get { return _pictureBytes; }
            set
            {
                if (_pictureBytes != value)
                {
                    _pictureBytes = value;
                    OnPropertyChanged(nameof(PictureBytes));
                }
            }
        }
        public BitmapImage Picture => ByteArrayToBitmapImage(PictureBytes);
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged(nameof(Artist));
                }
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                if (_album != value)
                {
                    _album = value;
                    OnPropertyChanged(nameof(Album));
                }
            }
        }
        public string Genre
        {
            get { return _genre; }
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    OnPropertyChanged(nameof(Genre));
                }
            }
        }
        public string Year
        {
            get { return _year; }
            set
            {
                if (_year != value)
                {
                    _year = value;
                    OnPropertyChanged(nameof(Year));
                }
            }
        }
        public string Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }
        public bool IsLiked
        {
            get { return _isLiked; }
            set
            {
                if (_isLiked != value)
                {
                    _isLiked = value;
                    OnPropertyChanged(nameof(IsLiked));
                    OnPropertyChanged(nameof(Index));
                }
            }
        }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }


        public ItemBase()
        {
            FileType = FileType.Audio;
        }


        public ItemBase Clone()
        {
            return new ItemBase
            {
                FileType = this.FileType,
                Path = this.Path,
                UniqueId = this.UniqueId,
                Index = this.Index,
                Title = this.Title,
                Artist = this.Artist,
                Album = this.Album,
                Year = this.Year,
                Genre = this.Genre,
                Duration = this.Duration,
                PictureBytes = this.PictureBytes,
                IsLiked = this.IsLiked
            };
        }

        private BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                var defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.UriSource = new Uri("pack://application:,,,/Images/unknownImage.png");
                defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                defaultImage.EndInit();
                return defaultImage;
            }

            try
            {
                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream(byteArray))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }
                return bitmapImage;
            }
            catch
            {
                var defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.UriSource = new Uri("pack://application:,,,/Images/unknownImage.png");
                defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                defaultImage.EndInit();
                return defaultImage;
            }
        }
    }
}
