﻿using Maple.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple
{
    [DebuggerDisplay("{Title}, {Sequence}")]
    public class Playlist : BaseViewModel<Data.Playlist>, IIsSelected, ISequence, IIdentifier
    {
        private readonly DialogViewModel _dialogViewModel;
        public int ItemCount => Items?.Count ?? 0;

        public ICommand LoadFromFileCommand { get; private set; }
        public ICommand LoadFromFolderCommand { get; private set; }
        public ICommand LoadFromUrlCommand { get; private set; }

        /// <summary>
        /// contains indices of played <see cref="IMediaItem"/>
        /// </summary>
        public Stack<int> History { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public RangeObservableCollection<MediaItemViewModel> Items { get; private set; }

        private MediaItemViewModel _currentItem;
        /// <summary>
        /// is <see cref="SetActive"/> when a IMediaPlayer picks a <see cref="IMediaItem"/> from this
        /// </summary>
        public MediaItemViewModel CurrentItem
        {
            get { return _currentItem; }
            private set { SetValue(ref _currentItem, value); }
        }

        private int _sequence;
        /// <summary>
        /// the index of this item if its part of a collection
        /// </summary>
        public int Sequence
        {
            get { return _sequence; }
            set { SetValue(ref _sequence, value, Changed: () => Model.Sequence = value); }
        }

        private PrivacyStatus _privacyStatus;
        /// <summary>
        /// Youtube Property
        /// </summary>
        public PrivacyStatus PrivacyStatus
        {
            get { return _privacyStatus; }
            private set { SetValue(ref _privacyStatus, value, Changed: () => Model.PrivacyStatus = (int)value); }
        }

        private bool _isSelected;
        /// <summary>
        /// if this list is part of a ui bound collection and selected this should be true
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        private bool _isShuffeling;
        /// <summary>
        /// indicates whether the next item is selected randomly from the list of items on a call of Next()
        /// </summary>
        public bool IsShuffeling
        {
            get { return _isShuffeling; }
            set { SetValue(ref _isShuffeling, value, Changed: () => Model.IsShuffeling = value); }
        }

        private string _title;
        /// <summary>
        /// the title/name of this playlist (human readable identifier)
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetValue(ref _title, value, Changed: () => Model.Title = value); }
        }

        private string _description;
        /// <summary>
        /// the description of this playlist
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetValue(ref _description, value, Changed: () => Model.Description = value); }
        }

        private int _id;
        public int Id
        {
            get { return _id; }
            private set { SetValue(ref _id, value, Changed: () => Model.Id = value); }
        }

        private ObservableCollection<RepeatMode> _repeatModes;
        public ObservableCollection<RepeatMode> RepeatModes
        {
            get { return _repeatModes; }
            private set { SetValue(ref _repeatModes, value); }
        }

        private RepeatMode _repeatMode;
        /// <summary>
        /// defines what happens when the last <see cref="IMediaItem"/> of <see cref="Items"/> is <see cref="SetActive"/> and the <see cref="Next"/> is requested
        /// </summary>
        public RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set { SetValue(ref _repeatMode, value, Changed: () => Model.RepeatMode = (int)value); }
        }

        public Playlist(DialogViewModel dialogViewModel, Data.Playlist model)
            : base(model)
        {
            using (_busyStack.GetToken())
            {
                _dialogViewModel = dialogViewModel;

                Items = new RangeObservableCollection<MediaItemViewModel>();
                RepeatModes = new ObservableCollection<RepeatMode>(Enum.GetValues(typeof(RepeatMode)).Cast<RepeatMode>().ToList());
                History = new Stack<int>();

                Title = model.Title;
                Description = model.Description;
                Id = model.Id;
                RepeatMode = (RepeatMode)model.RepeatMode;
                IsShuffeling = model.IsShuffeling;

                if (model.MediaItems == null)
                    throw new ArgumentException($"{model.MediaItems} cannot be null");

                model.MediaItems.ForEach(p => Items.Add(new MediaItemViewModel(p)));

                Items.CollectionChanged += (o, e) =>
                {
                    OnPropertyChanged(nameof(ItemCount));
                };

                InitializeCommands();
                IntializeValidation();
            }
        }

        private void InitializeCommands()
        {
            LoadFromFileCommand = new AsyncRelayCommand(LoadFromFile, () => CanLoadFromFile());
            LoadFromFolderCommand = new AsyncRelayCommand(LoadFromFolder, () => CanLoadFromFolder());
            LoadFromUrlCommand = new AsyncRelayCommand(LoadFromUrl, () => CanLoadFromUrl());
        }

        private void IntializeValidation()
        {
            AddRule(nameof(Title), Title, new NotNullOrEmptyRule());
            AddRule(nameof(Description), Description, new NotNullOrEmptyRule());
            AddRule(nameof(CurrentItem), CurrentItem, new NotNullRule());
            AddRule(nameof(RepeatModes), RepeatModes, new NotNullRule());
            AddRule(nameof(Items), Items, new NotNullRule());
        }

        private async Task LoadFromUrl()
        {
            using (_busyStack.GetToken())
            {
                var items = await _dialogViewModel.ShowUrlParseDialog();
                var result = items.Select(p => new MediaItemViewModel(p));
                Items.AddRange(result);
            }
        }

        private bool CanLoadFromUrl()
        {
            return !IsBusy;
        }

        private Task LoadFromFolder()
        {
            using (_busyStack.GetToken())
            {
                return _dialogViewModel.ShowFolderBrowserDialog();
            }
        }

        private bool CanLoadFromFolder()
        {
            return !IsBusy;
        }

        private Task LoadFromFile()
        {
            using (_busyStack.GetToken())
            {
                return _dialogViewModel.ShowFileBrowserDialog();
            }
        }

        private bool CanLoadFromFile()
        {
            return !IsBusy;
        }

        public virtual void Clear()
        {
            Items.Clear();
            History.Clear();
            CurrentItem = null;
        }

        public virtual void Add(MediaItemViewModel item)
        {
            using (_busyStack.GetToken())
            {
                if (Items?.Any() == true)
                    item.Sequence = Items.Select(p => p.Sequence).Max() + 1;
                else
                    item.Sequence = 0;

                if (Items.Any() != true)
                    History.Push(item.Sequence);

                item.PlaylistId = Id;
                Items.Add(item);

                if (CurrentItem == null)
                    CurrentItem = Items.First();
            }
        }

        public virtual void AddRange(IEnumerable<MediaItemViewModel> items)
        {
            using (_busyStack.GetToken())
            {
                var currentIndex = -1;
                if (Items.Any())
                {
                    var indices = Items.Select(p => p.Sequence);
                    currentIndex = (indices != null) ? indices.Max() : 0;
                }

                foreach (var item in items)
                {
                    currentIndex++;
                    item.Sequence = currentIndex;
                    Add(item);
                }

                if (CurrentItem == null)
                    CurrentItem = Items.First();
            }
        }

        public virtual void Remove(MediaItemViewModel item)
        {
            using (_busyStack.GetToken())
            {
                while (Items.Contains(item))
                    Items.Remove(item);
            }
        }

        public virtual bool CanRemove(MediaItemViewModel item)
        {
            if (Items == null || Items.Count == 0 || item as MediaItemViewModel == null)
                return false;

            return Items.Contains(item) && !IsBusy;
        }

        public virtual MediaItemViewModel Next()
        {
            using (_busyStack.GetToken())
            {
                if (Items != null && Items.Any())
                {
                    if (IsShuffeling)
                        return NextShuffle();
                    else
                    {
                        switch (RepeatMode)
                        {
                            case RepeatMode.All: return NextRepeatAll();

                            case RepeatMode.None: return NextRepeatNone();

                            case RepeatMode.Single: return NextRepeatSingle();

                            default:
                                throw new NotImplementedException(nameof(RepeatMode));
                        }
                    }
                }
                return null;
            }
        }

        private MediaItemViewModel NextRepeatNone()
        {
            var currentIndex = 0;
            if (CurrentItem?.Sequence != null)
                currentIndex = CurrentItem?.Sequence ?? 0;

            if (Items.Count > 1)
            {
                var nextPossibleItems = Items.Where(p => p.Sequence > currentIndex);

                if (nextPossibleItems?.Any() == true) // try to find items after the current one
                {
                    Items.ToList().ForEach(p => p.IsSelected = false);
                    var foundItem = nextPossibleItems.Where(q => q.Sequence == nextPossibleItems.Select(p => p.Sequence).Min()).First();
                    foundItem.IsSelected = true;
                    return foundItem;
                }

                return null;
                // we dont repeat, so there is nothing to do here
            }
            else
                return NextRepeatSingle();
        }

        private MediaItemViewModel NextRepeatSingle()
        {
            if (RepeatMode != RepeatMode.None)
                return CurrentItem;
            else
                return null;
        }

        private MediaItemViewModel NextRepeatAll()
        {
            var currentIndex = 0;
            if (CurrentItem?.Sequence != null)
                currentIndex = CurrentItem?.Sequence ?? 0;

            if (Items.Count > 1)
            {
                var nextPossibleItems = Items.Where(p => p.Sequence > currentIndex);

                if (nextPossibleItems.Any()) // try to find items after the current one
                {
                    Items.ToList().ForEach(p => p.IsSelected = false);
                    var foundItem = nextPossibleItems.Where(q => q.Sequence == nextPossibleItems.Select(p => p.Sequence).Min()).First();
                    foundItem.IsSelected = true;
                    return foundItem;
                }
                else // if there are none, use the first item in the list
                {
                    Items.ToList().ForEach(p => p.IsSelected = false);
                    var foundItem = Items.First();
                    foundItem.IsSelected = true;
                    return foundItem;
                }
            }
            else
                return NextRepeatSingle();
        }

        private MediaItemViewModel NextShuffle()
        {
            if (Items.Count > 1)
            {
                var nextItems = Items.Where(p => p.Sequence != CurrentItem?.Sequence); // get all items besides the current one
                Items.ToList().ForEach(p => p.IsSelected = false);
                var foundItem = nextItems.Random();
                foundItem.IsSelected = true;
                return foundItem;
            }
            else
                return NextRepeatSingle();
        }

        /// <summary>
        /// Removes the last <see cref="MediaItemViewModel"/> from <seealso cref="History"/> and returns it
        /// </summary>
        /// <returns>returns the last <see cref="MediaItemViewModel"/> from <seealso cref="History"/></returns>
        public virtual MediaItemViewModel Previous()
        {
            using (_busyStack.GetToken())
            {
                if (History?.Any() == true)
                {
                    Items.ToList().ForEach(p => p.IsSelected = false);      // deselect all items in the list
                    while (History.Any())
                    {
                        var previous = History.Pop();

                        if (previous == CurrentItem?.Sequence) // the most recent item in the history, is the just played item, so we wanna skip that
                            continue;

                        if (previous > -1)
                        {
                            var previousItems = Items.Where(p => p.Sequence == previous); // try to get the last played item
                            if (previousItems.Any())
                            {
                                var foundItem = previousItems.First();
                                foundItem.IsSelected = true;

                                Assert.IsTrue(previousItems.Count() > 1, "Warning SelectPrevious returned more than one value, when it should only return one");

                                return foundItem;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public bool CanNext()
        {
            return Items != null && Items.Any();
        }

        public bool CanPrevious()
        {
            return History != null && History.Any();
        }

        //public override IEnumerable<ValidationResult> Validate(ValidationContext context)
        //{
        //    if (string.IsNullOrWhiteSpace(Title))
        //        yield return new ValidationResult($"{nameof(Title)} {Resources.IsRequired}", new[] { nameof(Title) });
        //}
    }
}
