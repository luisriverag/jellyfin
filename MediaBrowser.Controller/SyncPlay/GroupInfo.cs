using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Session;

namespace MediaBrowser.Controller.SyncPlay
{
    /// <summary>
    /// Class GroupInfo.
    /// </summary>
    /// <remarks>
    /// Class is not thread-safe, external locking is required when accessing methods.
    /// </remarks>
    public class GroupInfo
    {
        /// <summary>
        /// The default ping value used for sessions.
        /// </summary>
        public const long DefaultPing = 500;

        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        /// <value>The group identifier.</value>
        public Guid GroupId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the playing item.
        /// </summary>
        /// <value>The playing item.</value>
        public BaseItem PlayingItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether playback is paused.
        /// </summary>
        /// <value>Playback is paused.</value>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are position ticks.
        /// </summary>
        /// <value>The position ticks.</value>
        public long PositionTicks { get; set; }

        /// <summary>
        /// Gets or sets the last activity.
        /// </summary>
        /// <value>The last activity.</value>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Gets the participants.
        /// </summary>
        /// <value>The participants, or members of the group.</value>
        public Dictionary<string, GroupMember> Participants { get; } =
            new Dictionary<string, GroupMember>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if a session is in this group.
        /// </summary>
        /// <param name="sessionId">The session id to check.</param>
        /// <returns><c>true</c> if the session is in this group; <c>false</c> otherwise.</returns>
        public bool ContainsSession(string sessionId)
        {
            return Participants.ContainsKey(sessionId);
        }

        /// <summary>
        /// Adds the session to the group.
        /// </summary>
        /// <param name="session">The session.</param>
        public void AddSession(SessionInfo session)
        {
            Participants.TryAdd(
                session.Id,
                new GroupMember
                {
                    Session = session,
                    Ping = DefaultPing,
                    IsBuffering = false
                });
        }

        /// <summary>
        /// Removes the session from the group.
        /// </summary>
        /// <param name="session">The session.</param>
        public void RemoveSession(SessionInfo session)
        {
            Participants.Remove(session.Id);
        }

        /// <summary>
        /// Updates the ping of a session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ping">The ping.</param>
        public void UpdatePing(SessionInfo session, long ping)
        {
            if (Participants.TryGetValue(session.Id, out GroupMember value))
            {
                value.Ping = ping;
            }
        }

        /// <summary>
        /// Gets the highest ping in the group.
        /// </summary>
        /// <returns>The highest ping in the group.</returns>
        public long GetHighestPing()
        {
            long max = long.MinValue;
            foreach (var session in Participants.Values)
            {
                max = Math.Max(max, session.Ping);
            }

            return max;
        }

        /// <summary>
        /// Sets the session's buffering state.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="isBuffering">The state.</param>
        public void SetBuffering(SessionInfo session, bool isBuffering)
        {
            if (Participants.TryGetValue(session.Id, out GroupMember value))
            {
                value.IsBuffering = isBuffering;
            }
        }

        /// <summary>
        /// Gets the group buffering state.
        /// </summary>
        /// <returns><c>true</c> if there is a session buffering in the group; <c>false</c> otherwise.</returns>
        public bool IsBuffering()
        {
            foreach (var session in Participants.Values)
            {
                if (session.IsBuffering)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the group is empty.
        /// </summary>
        /// <returns><c>true</c> if the group is empty; <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return Participants.Count == 0;
        }
    }
}