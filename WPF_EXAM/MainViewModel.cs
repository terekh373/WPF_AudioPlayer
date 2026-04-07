using Id3;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WPF_EXAM
{
    internal class MainViewModel : PropertyChangedHandler
    {
        
        private string _searchText;
        private ObservableCollection<ItemBase> _defaultItems = new ObservableCollection<ItemBase>();


        #region mediaElement interaction

        private MediaElement _mediaElement;
        private ItemBase _actualSelectedItem = null, _playingItem;
        private int _actualSelectedIndex = -1, _currentShuffleIndex;
        private bool _isShuffled, _isReplay;
        private Random _rnd = new Random();
        private List<int> _shuffledIndices;
        private DispatcherTimer _positionTimer;
        private TimeSpan _currentPosition;

        public ItemBase ActualSelectedItem
        {
            get { return _actualSelectedItem; }
            set
            {
                if (_actualSelectedItem != value)
                {
                    _actualSelectedItem = value;
                    OnPropertyChanged(nameof(ActualSelectedItem));
                }
            }
        }
        public ItemBase PlayingItem
        {
            get { return _playingItem; }
            set
            {
                if (_playingItem != value)
                {
                    _playingItem = value;
                    OnPropertyChanged(nameof(PlayingItem));
                }
            }
        }
        public int ActualSelectedIndex
        {
            get { return _actualSelectedIndex; }
            set
            {
                if (_actualSelectedIndex != value)
                {
                    _actualSelectedIndex = value;
                    OnPropertyChanged(nameof(ActualSelectedIndex));
                }
            }
        }
        public bool IsShuffled
        {
            get { return _isShuffled; }
            set
            {
                if (_isShuffled != value)
                {
                    _isShuffled = value;
                    OnPropertyChanged(nameof(IsShuffled));
                }
            }
        }
        public bool IsReplay
        {
            get { return _isReplay; }
            set
            {
                if (_isReplay != value)
                {
                    _isReplay = value;
                    OnPropertyChanged(nameof(IsReplay));
                }
            }
        }
        public TimeSpan CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    OnPropertyChanged();
                }
            }
        }


        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand ShuffleCommand { get; }
        public ICommand ReplayCommand { get; }


        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            Next();
        }

        private void Play(ItemBase actualSelectedItem)
        {
            if (PlayingItem != null)
            {
                PlayingItem.IsPlaying = false;
                _mediaElement.Stop();
            }

            ItemBase itemToPlay = ItemsData.Items.FirstOrDefault(i => i.UniqueId == actualSelectedItem.UniqueId) ?? ItemsData.LikedItems.FirstOrDefault(i => i.UniqueId == actualSelectedItem.UniqueId);

            PlayingItem = itemToPlay;
            PlayingItem.IsPlaying = true;
            if (IsShuffled && _shuffledIndices != null) _currentShuffleIndex = _shuffledIndices.IndexOf(ActualSelectedIndex);

            _mediaElement.Source = new Uri(itemToPlay.Path);
            _mediaElement.Play();
            _positionTimer.Start();
        }
        
        private void Stop()
        {
            PlayingItem.IsPlaying = false;
            PlayingItem = null;
            ActualSelectedIndex = -1;
            ActualSelectedItem = null;
            _mediaElement.Stop();
            _positionTimer.Stop();
        }

        private void Next()
        {
            if (PlayingItem == null) return;

            if (IsReplay)
            {
                _mediaElement.Position = TimeSpan.Zero;
                _mediaElement.Play();
                return;
            }

            ItemBase itemToPlay = null;

            if (ActualSelectedItem.IsLiked && !ItemsData.Items.Contains(ActualSelectedItem))
            {
                if (ItemsData.LikedItems.Count == 1) return;

                ActualSelectedIndex = (ActualSelectedIndex + 1) % ItemsData.LikedItems.Count;
                itemToPlay = ItemsData.LikedItems[ActualSelectedIndex];
            }
            else
            {
                if (ItemsData.Items.Count == 1) return;

                if (IsShuffled && _shuffledIndices != null && _shuffledIndices.Count > 0)
                {
                    _currentShuffleIndex = (_currentShuffleIndex + 1) % _shuffledIndices.Count;
                    int actualIndex = _shuffledIndices[_currentShuffleIndex];
                    itemToPlay = ItemsData.Items[actualIndex];
                    ActualSelectedIndex = actualIndex;
                }
                else
                {
                    ActualSelectedIndex = (ActualSelectedIndex + 1) % ItemsData.Items.Count;
                    itemToPlay = ItemsData.Items[ActualSelectedIndex];
                }
            }

            PlayingItem.IsPlaying = false;
            PlayingItem = itemToPlay;
            PlayingItem.IsPlaying = true;
            ActualSelectedItem = PlayingItem;
            if (IsShuffled && _shuffledIndices != null) _currentShuffleIndex = _shuffledIndices.IndexOf(ActualSelectedIndex);

            _mediaElement.Source = new Uri(itemToPlay.Path);
            _mediaElement.Play();
            _positionTimer.Start();
        }

        private void Previous()
        {
            if (PlayingItem == null) return;

            ItemBase itemToPlay = null;

            if (ActualSelectedItem.IsLiked && !ItemsData.Items.Contains(ActualSelectedItem))
            {
                if (ItemsData.LikedItems.Count == 1) return;
                if (ActualSelectedIndex > 0) ActualSelectedIndex -= 1;
                else if (ActualSelectedIndex == 0) ActualSelectedIndex = ItemsData.LikedItems.Count - 1;
                itemToPlay = ItemsData.LikedItems[ActualSelectedIndex];
            }
            else
            {
                if (ItemsData.Items.Count == 1) return;
                if (ActualSelectedIndex > 0) ActualSelectedIndex -= 1;
                else if (ActualSelectedIndex == 0) ActualSelectedIndex = ItemsData.Items.Count - 1;
                itemToPlay = ItemsData.Items[ActualSelectedIndex];
            }

            PlayingItem.IsPlaying = false;
            PlayingItem = null;

            PlayingItem = itemToPlay;
            ActualSelectedItem = PlayingItem;
            PlayingItem.IsPlaying = true;
            if (IsShuffled && _shuffledIndices != null) _currentShuffleIndex = _shuffledIndices.IndexOf(ActualSelectedIndex);
            IsReplay = false;

            _mediaElement.Source = new Uri(itemToPlay.Path);
            _mediaElement.Play();
            _positionTimer.Start();
        }

        private void Shuffle()
        {
            if (IsReplay) IsReplay = false;

            IsShuffled = !IsShuffled;

            if (IsShuffled)
            {
                var indices = Enumerable.Range(0, ItemsData.Items.Count).ToList();
                _shuffledIndices = indices.OrderBy(x => _rnd.Next()).ToList();

                int currentIndex = ItemsData.Items.IndexOf(ActualSelectedItem);
                _currentShuffleIndex = _shuffledIndices.IndexOf(currentIndex);

                if (_currentShuffleIndex == -1) _currentShuffleIndex = 0;
            }
            else _shuffledIndices = null;
        }

        private void Replay()
        {
            if (IsShuffled) IsShuffled = false;

            IsReplay = !IsReplay;
        }

        private void UpdateMediaPosition()
        {
            if (_mediaElement.NaturalDuration.HasTimeSpan) CurrentPosition = _mediaElement.Position;
        }

        #endregion


        public ItemsData ItemsData { get; set; } = new ItemsData();

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));

                    if (ItemsData != null)
                    {
                        if (_searchText != "")
                        {
                            if (_defaultItems.Count == 0) _defaultItems = ItemsData.Items;
                            ItemsData.Items = new ObservableCollection<ItemBase>();

                            for (int i = 0; i < _defaultItems.Count; i++) if (_defaultItems[i].Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0) ItemsData.Items.Add(_defaultItems[i]);
                        }
                        else
                        {
                            ItemsData.Items = _defaultItems;
                            _defaultItems = new ObservableCollection<ItemBase>();
                        }
                    }
                }
            }
        }


        public ICommand OpenCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand LikeCommand { get; }
        public ICommand UnlikeCommand { get; }
        public ICommand DeleteCommand { get; }


        public MainViewModel(MediaElement mediaElement)
        {
            OpenCommand = new RelayCommand(Open);
            AddCommand = new RelayCommand(Add);
            LikeCommand = new RelayCommand<string>(Like);
            UnlikeCommand = new RelayCommand<string>(Unlike);
            DeleteCommand = new RelayCommand<string>(Delete);


            #region mediaElement interaction

            _mediaElement = mediaElement;

            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _mediaElement.MediaOpened += (s, e) =>
            {
                CurrentPosition = TimeSpan.Zero;
            };

            PlayCommand = new RelayCommand<ItemBase>(Play);
            StopCommand = new RelayCommand(Stop);
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);
            ShuffleCommand = new RelayCommand(Shuffle);
            ReplayCommand = new RelayCommand(Replay);

            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _positionTimer.Tick += (s, e) => UpdateMediaPosition();

            #endregion
        }


        private void SerializeObject()
        {
            string json = JsonConvert.SerializeObject(ItemsData, Newtonsoft.Json.Formatting.Indented);
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "items.txt");
            File.WriteAllText(filePath, json);
        }

        private void UpdateCollectionIndexes(ObservableCollection<ItemBase> items)
        {
            for (int i = 0; i < items.Count; i++) items[i].Index = (i + 1).ToString();
        }

        public void Load()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "items.txt");

            if (!File.Exists(filePath))
            {
                var defaultData = new ItemsData();
                File.WriteAllText(filePath, JsonConvert.SerializeObject(defaultData));
            }

            string json = File.ReadAllText(filePath);

            ItemsData = JsonConvert.DeserializeObject<ItemsData>(json) ?? new ItemsData();
        }

        private void Open()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Music files (.mp3)|*.mp3",
                Multiselect = true
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                ItemsData.Items = new ObservableCollection<ItemBase>();

                List<string> filenames = dialog.FileNames.ToList();
                int idx = -1;

                foreach (string musicFile in filenames)
                {
                    idx++;
                    using (var mp3 = new Mp3(musicFile))
                    {
                        try
                        {
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                            FileType fileType = dialog.DefaultExt == "mp3" ? FileType.Audio : FileType.Unknown;
                            string path = Path.Combine(Directory.GetCurrentDirectory(), filenames[idx]);
                            string index = (ItemsData.Items.Count + 1).ToString();
                            byte[] pictureBytes = tag.Pictures.FirstOrDefault()?.PictureData ?? new byte[0];
                            string title = tag.Title ?? "Unknown Title";
                            string artist = tag.Band ?? "Unknown Artist";
                            string album = tag.Album ?? "Unknown Album";
                            string year = tag.Year ?? "Unknown Year";
                            string genre = tag.Genre ?? "Unknown Genre";
                            bool isLiked = false;

                            TimeSpan duration = new TimeSpan();
                            using (var audioFile = new AudioFileReader(musicFile)) duration = audioFile.TotalTime;
                            string formattedDuration = duration.Hours > 0 ? duration.ToString(@"hh\:mm\:ss") : duration.ToString(@"mm\:ss");

                            ItemBase itemBase = new ItemBase()
                            {
                                FileType = fileType,
                                Path = path,
                                UniqueId = Guid.NewGuid().ToString(),
                                Index = index,
                                PictureBytes = pictureBytes,
                                Title = title,
                                Artist = artist,
                                Album = album,
                                Year = year,
                                Genre = genre,
                                Duration = formattedDuration,
                                IsLiked = isLiked
                            };

                            ItemsData.Items.Add(itemBase);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}");
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));

            SerializeObject();
        }

        private void Add()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Music files (.mp3)|*.mp3",
                Multiselect = true
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                List<string> filenames = dialog.FileNames.ToList();
                int idx = -1;

                foreach (string musicFile in filenames)
                {
                    idx++;
                    using (var mp3 = new Mp3(musicFile))
                    {
                        try
                        {
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                            FileType fileType = dialog.DefaultExt == "mp3" ? FileType.Audio : FileType.Unknown;
                            string path = Path.Combine(Directory.GetCurrentDirectory(), filenames[idx]);
                            string index = (ItemsData.Items.Count + 1).ToString();
                            byte[] pictureBytes = tag.Pictures.FirstOrDefault()?.PictureData ?? new byte[0];
                            string title = tag.Title ?? "Unknown Title";
                            string artist = tag.Band ?? "Unknown Artist";
                            string album = tag.Album ?? "Unknown Album";
                            string year = tag.Year ?? "Unknown Year";
                            string genre = tag.Genre ?? "Unknown Genre";
                            bool isLiked = false;

                            TimeSpan duration = new TimeSpan();
                            using (var audioFile = new AudioFileReader(musicFile)) duration = audioFile.TotalTime;
                            string formattedDuration = duration.Hours > 0 ? duration.ToString(@"hh\:mm\:ss") : duration.ToString(@"mm\:ss");

                            ItemBase itemBase = new ItemBase()
                            {
                                FileType = fileType,
                                Path = path,
                                UniqueId = Guid.NewGuid().ToString(),
                                Index = index,
                                PictureBytes = pictureBytes,
                                Title = title,
                                Artist = artist,
                                Album = album,
                                Year = year,
                                Genre = genre,
                                Duration = formattedDuration,
                                IsLiked = isLiked
                            };

                            ItemsData.Items.Add(itemBase);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}");
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));

            SerializeObject();
        }

        private void Like(string uniqueId)
        {
            var originalItem = ItemsData.Items.FirstOrDefault(i => i.UniqueId == uniqueId);
            if (originalItem == null) return;

            originalItem.IsLiked = true;

            var existingInLiked = ItemsData.LikedItems.FirstOrDefault(i => i.UniqueId == uniqueId);
            if (existingInLiked == null)
            {
                var newItem = originalItem.Clone();
                newItem.Index = (ItemsData.LikedItems.Count + 1).ToString();
                ItemsData.LikedItems.Add(newItem);
            }

            OnPropertyChanged(nameof(originalItem.IsLiked));
            SerializeObject();
        }

        private void Unlike(string uniqueId)
        {
            var originalItem = ItemsData.Items.FirstOrDefault(i => i.UniqueId == uniqueId);
            if (originalItem != null)
            {
                originalItem.IsLiked = false;
                OnPropertyChanged(nameof(originalItem.IsLiked));
            }

            var likedItem = ItemsData.LikedItems.FirstOrDefault(i => i.UniqueId == uniqueId);
            if (likedItem != null)
            {
                ItemsData.LikedItems.Remove(likedItem);
                UpdateCollectionIndexes(ItemsData.LikedItems);
            }

            SerializeObject();
        }

        private void Delete(string uniqueId)
        {
            var itemToDelete = ItemsData.Items.FirstOrDefault(i => i.UniqueId == uniqueId);
            if (itemToDelete == null) return;

            ItemsData.Items.Remove(itemToDelete);
            UpdateCollectionIndexes(ItemsData.Items);

            SerializeObject();
        }
    }
}
