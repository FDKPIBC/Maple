﻿using AutoMapper;
using Maple.Core;
using Maple.Localization.Properties;
using System;

namespace Maple
{
    /// <summary>
    /// Provides logic to map between different domain objects of the Playlisttype
    /// </summary>
    /// <seealso cref="Maple.IPlaylistMapper" />
    public class PlaylistMapper : IPlaylistMapper
    {
        private readonly IMapper _mapper;
        private readonly ITranslationService _translator;
        private readonly IMediaItemMapper _mediaItemMapper;
        private readonly ISequenceProvider _sequenceProvider;

        private DialogViewModel _dialogViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistMapper"/> class.
        /// </summary>
        /// <param name="dialogViewModel">The dialog view model.</param>
        public PlaylistMapper(ITranslationService translator, IMediaItemMapper mediaItemMapper, DialogViewModel dialogViewModel, ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider ?? throw new ArgumentNullException(nameof(sequenceProvider));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _dialogViewModel = dialogViewModel ?? throw new ArgumentNullException(nameof(dialogViewModel));
            _mediaItemMapper = mediaItemMapper ?? throw new ArgumentNullException(nameof(mediaItemMapper));

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Data.Playlist, Core.Playlist>();
                cfg.CreateMap<Core.Playlist, Data.Playlist>()
                    .Ignore(p => p.IsDeleted)
                    .Ignore(p => p.IsNew);
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        public Playlist GetNewPlaylist(int sequence)
        {
            return new Playlist(_translator, _mediaItemMapper, _sequenceProvider, _dialogViewModel, new Data.Playlist
            {
                Title = _translator.Translate(nameof(Resources.New)),
                Description = string.Empty,
                Location = string.Empty,
                RepeatMode = 0,
                IsShuffeling = false,
                Sequence = sequence,
            });
        }

        /// <summary>
        /// Gets the specified mediaitem.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Playlist Get(Core.Playlist mediaitem)
        {
            return new Playlist(_translator, _mediaItemMapper, _sequenceProvider, _dialogViewModel, GetData(mediaitem));
        }

        /// <summary>
        /// Gets the specified mediaitem.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Playlist Get(Data.Playlist mediaitem)
        {
            return new Playlist(_translator, _mediaItemMapper, _sequenceProvider, _dialogViewModel, mediaitem);
        }

        /// <summary>
        /// Gets the core.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Core.Playlist GetCore(Data.Playlist mediaitem)
        {
            return _mapper.Map<Data.Playlist, Core.Playlist>(mediaitem);
        }

        /// <summary>
        /// Gets the core.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Core.Playlist GetCore(Playlist mediaitem)
        {
            return GetCore(mediaitem.Model);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Data.Playlist GetData(Playlist mediaitem)
        {
            return mediaitem.Model;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="mediaitem">The mediaitem.</param>
        /// <returns></returns>
        public Data.Playlist GetData(Core.Playlist mediaitem)
        {
            return _mapper.Map<Core.Playlist, Data.Playlist>(mediaitem);
        }
    }
}
