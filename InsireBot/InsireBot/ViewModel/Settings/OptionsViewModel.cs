﻿using Maple.Core;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Maple.Core.ObservableObject" />
    /// <seealso cref="Maple.Core.ILoadableViewModel" />
    /// <seealso cref="Maple.Core.ISaveableViewModel" />
    public class OptionsViewModel : ObservableObject, ILoadableViewModel, ISaveableViewModel
    {
        private readonly ITranslationService _manager;

        private RangeObservableCollection<CultureInfo> _items;
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public RangeObservableCollection<CultureInfo> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value); }
        }

        private CultureInfo _selectedCulture;
        /// <summary>
        /// Gets or sets the selected culture.
        /// </summary>
        /// <value>
        /// The selected culture.
        /// </value>
        public CultureInfo SelectedCulture
        {
            get { return _selectedCulture; }
            set { SetValue(ref _selectedCulture, value, OnChanged: SyncCulture); }
        }

        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public ICommand RefreshCommand => new RelayCommand(Load);
        /// <summary>
        /// Gets the load command.
        /// </summary>
        /// <value>
        /// The load command.
        /// </value>
        public ICommand LoadCommand => new RelayCommand(Load, () => !IsLoaded);
        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public ICommand SaveCommand => new RelayCommand(Save);

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsViewModel"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public OptionsViewModel(ITranslationService manager)
        {
            _manager = manager;
            Items = new RangeObservableCollection<CultureInfo>(_manager.Languages);
        }

        private void SyncCulture()
        {
            _manager.CurrentLanguage = SelectedCulture;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            _manager.Save();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            _manager.Load();
            SelectedCulture = Properties.Settings.Default.StartUpCulture;
            IsLoaded = true;
        }

        public async Task SaveAsync()
        {
            await _manager.SaveAsync();
        }

        public async Task LoadAsync()
        {
            await _manager.LoadAsync();
            SelectedCulture = Properties.Settings.Default.StartUpCulture;
            IsLoaded = true;
        }
    }
}
