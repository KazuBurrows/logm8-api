namespace LogMate.Application.Exceptions;

public class ConflictException : Exception
{
    public List<int> ConflictingIds { get; }

    public ConflictException(string message, List<int> conflictingIds) : base(message)
    {
        ConflictingIds = conflictingIds;
    }
}
