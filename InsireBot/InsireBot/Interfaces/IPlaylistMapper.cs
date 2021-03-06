﻿namespace Maple
{
    /// <summary>
    ///
    /// </summary>
    public interface IPlaylistMapper : IBaseMapper<Playlist, Core.Playlist, Data.Playlist>
    {
        /// <summary>
        /// Gets the new playlist.
        /// </summary>
        /// <returns></returns>
        /// <autogeneratedoc />
        Playlist GetNewPlaylist(int sequence);
    }
}
