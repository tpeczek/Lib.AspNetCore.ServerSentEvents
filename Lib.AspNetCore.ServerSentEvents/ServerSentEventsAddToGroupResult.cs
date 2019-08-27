namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The add to group results.
    /// </summary>
    public enum ServerSentEventsAddToGroupResult
    {
        /// <summary>
        /// The client has been added to an existing group.
        /// </summary>
        AddedToExistingGroup = 1,
        /// <summary>
        /// The client has been added to a new group.
        /// </summary>
        AddedToNewGroup = 2
    }
}
