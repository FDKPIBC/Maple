﻿using Maple.Core;
using Maple.Localization.Properties;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Maple.Core.BaseDataListViewModel{Maple.Playlist, Maple.Data.Playlist}" />
    /// <seealso cref="Maple.Core.ILoadableViewModel" />
    /// <seealso cref="Maple.Core.ISaveableViewModel" />
    public class Playlists : BaseDataListViewModel<Playlist, Data.Playlist>, ILoadableViewModel, ISaveableViewModel, IPlaylistsViewModel
    {
        private readonly ITranslationService _translator;
        private readonly IMapleLog _log;
        private readonly Func<IMediaRepository> _repositoryFactory;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly IPlaylistMapper _playlistMapper;

        /// <summary>
        /// Gets the play command.
        /// </summary>
        /// <value>
        /// The play command.
        /// </value>
        public ICommand PlayCommand { get; private set; }
        /// <summary>
        /// Gets the load command.
        /// </summary>
        /// <value>
        /// The load command.
        /// </value>
        public ICommand LoadCommand => new RelayCommand(Load, () => !IsLoaded);
        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public ICommand RefreshCommand => new RelayCommand(Load);
        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public ICommand SaveCommand => new RelayCommand(Save);

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Playlists"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="repo">The repo.</param>
        /// <param name="dialogViewModel">The dialog view model.</param>
        public Playlists(ITranslationService translator, IMapleLog log, IPlaylistMapper playlistMapper, Func<IMediaRepository> repo, ISequenceProvider sequenceProvider)
            : base()
        {
            _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _repositoryFactory = repo ?? throw new ArgumentNullException(nameof(repo));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _playlistMapper = playlistMapper ?? throw new ArgumentNullException(nameof(playlistMapper));

            AddCommand = new RelayCommand(Add, CanAdd);
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            _log.Info($"{_translator.Translate(nameof(Resources.Loading))} {_translator.Translate(nameof(Resources.Playlists))}");
            Clear();

            using (var context = _repositoryFactory())
                AddRange(context.GetAllPlaylists());

            SelectedItem = Items.FirstOrDefault();
            IsLoaded = true;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            _log.Info($"{_translator.Translate(Resources.Saving)} {_translator.Translate(Resources.Playlists)}");
            using (var context = _repositoryFactory())
            {
                context.Save(this);
            }
        }

        // TODO order changing + sync, Commands, UserInteraction, async load

        /// <summary>
        /// Adds this instance.
        /// </summary>
        public void Add()
        {
            var sequence = _sequenceProvider.Get(Items.Select(p => (ISequence)p).ToList());
            Add(_playlistMapper.GetNewPlaylist(sequence));
        }

        public Task SaveAsync()
        {
            return Task.Run(() => Save());
        }

        public async Task LoadAsync()
        {
            _log.Info($"{_translator.Translate(Resources.Loading)} {_translator.Translate(Resources.Playlists)}");
            Clear();

            using (var context = _repositoryFactory())
            {
                var result = await context.GetAllPlaylistsAsync();
                AddRange(result);
            }

            SelectedItem = Items.FirstOrDefault();
            IsLoaded = true;
        }
    }
}
