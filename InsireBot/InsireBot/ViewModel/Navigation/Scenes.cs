﻿using Maple.Core;
using Maple.Localization.Properties;
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
        private IMapleLog _log;

        public ICommand OpenColorOptionsCommand { get; private set; }
        public ICommand OpenMediaPlayerCommand { get; private set; }
        public ICommand OpenGithubPageCommand { get; private set; }

        public Scenes(ITranslationManager manager, IMapleLog log)
        {
            _manager = manager;
            _log = log;

            _log.Info(_manager.Translate(nameof(Resources.NavigationLoad)));

            var content = new[]
            {
                new Scene(_manager)
                {
                    Content = new MediaPlayerPage(_manager),
                    Key = nameof(Resources.Playback),
                    IsSelected = true,
                    Sequence = 100,
                },

                new Scene(_manager)
                {
                    Content = new PlaylistsPage(_manager),
                    Key = nameof(Resources.Playlists),
                    IsSelected = false,
                    Sequence = 300,
                },

                new Scene(_manager)
                {
                    Content = new ColorOptionsPage(_manager),
                    Key = nameof(Resources.Themes),
                    IsSelected = false,
                    Sequence = 500,
                },

                new Scene(_manager)
                {
                    Content = new OptionsPage(_manager),
                    Key = nameof(Resources.Options),
                    IsSelected = false,
                    Sequence = 600,
                },

                new Scene(_manager)
                {
                    Content = new MediaPlayersPage(_manager),
                    Key = nameof(Resources.Director),
                    IsSelected = false,
                    Sequence = 150,
                },
            };

            using (BusyStack.GetToken())
            {
                AddRange(content);

                SelectedItem = Items[0];

                using (View.DeferRefresh())
                {
                    View.SortDescriptions.Add(new SortDescription(nameof(Scene.Sequence), ListSortDirection.Ascending));
                }
            }

            InitializeCommands();

            _log.Info(manager.Translate(nameof(Resources.NavigationLoaded)));
        }

        private void InitializeCommands()
        {
            OpenColorOptionsCommand = new RelayCommand(OpenColorOptionsView, CanOpenColorOptionsView);
            OpenMediaPlayerCommand = new RelayCommand(OpenMediaPlayerView, CanOpenMediaPlayerView);
            OpenGithubPageCommand = new RelayCommand(OpenGithubPage);
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
