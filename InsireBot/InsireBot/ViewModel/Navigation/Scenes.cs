﻿using Maple.Core;
using Maple.Data;
using Maple.Localization.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Maple
{
    /// <summary>
    /// ViewModel that stores and controls which UserControl(Page/View) whatever is displayed in the mainwindow of this app)
    /// </summary>
    public class Scenes : BaseListViewModel<Scene>
    {
        private ITranslationManager _manager;
        private PlaylistContext _context;
        private IBotLog _log;
        public ICommand OpenColorOptionsCommand { get; private set; }
        public ICommand OpenMediaPlayerCommand { get; private set; }
        public ICommand OpenGithubPageCommand { get; private set; }

        public Scenes(ITranslationManager manager,
                        PlaylistContext context,
                        IBotLog log,
                        DirectorViewModel directorViewModel,
                        MediaPlayers mediaPlayersViewModel,
                        Playlists playlistsViewModel,
                        UIColorsViewModel uIColorsViewModel,
                        OptionsViewModel optionsViewModel)
        {
            _manager = manager;
            _log = log;
            _context = context;

            _log.Info(_manager.Translate(nameof(Resources.NavigationLoad)));

            var content = new[]
            {
                new Scene(_manager,context)
                {
                    Content = new MediaPlayerPage(_manager),
                    Key = nameof(Resources.Playback),
                    GetDataContext = () => mediaPlayersViewModel.Items.First(p => p is MainMediaPlayer),
                    IsSelected = true,
                    Sequence = 100,
                },

                new Scene(_manager,context)
                {
                    Content = new PlaylistsPage(_manager),
                    Key = nameof(Resources.Playlists),
                    GetDataContext = () => playlistsViewModel,
                    IsSelected = false,
                    Sequence = 300,
                },

                new Scene(_manager,context)
                {
                    Content = new ColorOptionsPage(_manager),
                    Key = nameof(Resources.Themes),
                    GetDataContext = () => uIColorsViewModel,
                    IsSelected = false,
                    Sequence = 500,
                },

                new Scene(_manager,context)
                {
                    Content = new OptionsPage(_manager),
                    Key = nameof(Resources.Options),
                    GetDataContext = () => optionsViewModel,
                    IsSelected = false,
                    Sequence = 600,
                },

                new Scene(_manager,context)
                {
                    Content = new MediaPlayersPage(_manager),
                    Key = nameof(Resources.Director),
                    GetDataContext = () => directorViewModel,
                    IsSelected = false,
                    Sequence = 150,
                },
            };

            using (BusyStack.GetToken())
            {
                AddRange(content);

                using (View.DeferRefresh())
                {
                    View.SortDescriptions.Add(new SortDescription(nameof(Scene.Sequence), ListSortDirection.Ascending));
                }
            }

            InitializeCommands();

            SelectionChanging += SelectedSceneChanging;

            _log.Info(manager.Translate(nameof(Resources.NavigationLoaded)));
        }

        private void InitializeCommands()
        {
            OpenColorOptionsCommand = new RelayCommand(OpenColorOptionsView, CanOpenColorOptionsView);
            OpenMediaPlayerCommand = new RelayCommand(OpenMediaPlayerView, CanOpenMediaPlayerView);
            OpenGithubPageCommand = new RelayCommand(OpenGithubPage);
        }

        private async void SelectedSceneChanging(object sender, EventArgs e)
        {
            await _context.SaveChangesAsync();
        }

        private void OpenColorOptionsView()
        {
            SelectedItem = Items.First(p => p.Content.GetType() == typeof(ColorOptionsPage));
        }

        private bool CanOpenColorOptionsView()
        {
            return Items?.Any(p => p.Content.GetType() == typeof(ColorOptionsPage)) == true;
        }

        private void OpenMediaPlayerView()
        {
            SelectedItem = Items.First(p => p.Content.GetType() == typeof(MediaPlayerPage));
        }

        private bool CanOpenMediaPlayerView()
        {
            return Items?.Any(p => p.Content.GetType() == typeof(ColorOptionsPage)) == true;
        }

        private void OpenGithubPage()
        {
            Process.Start(_manager.Translate(nameof(Resources.GithubProjectLink)));
        }
    }
}
