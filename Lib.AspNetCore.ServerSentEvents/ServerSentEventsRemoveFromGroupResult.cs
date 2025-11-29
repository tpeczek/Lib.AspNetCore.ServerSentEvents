namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The remove from group results.
    /// </summary>
    public enum ServerSentEventsRemoveFromGroupResult
    {
        /// <summary>
        /// The specified group could not be found.
        /// </summary>
        NotFoundGroup = 0,

        /// <summary>
        /// The client has been removed from an existing group.
        /// </summary>
        RemovedFromExistingGroup = 1,

        /// <summary>
        /// The client was not part of the group, so no removal occurred.
        /// </summary>
        NotInGroup = 2,
    }
}
