using System;

namespace Cerveza_Cristal;

[Serializable]
public class RepoInvalidActionException : Exception
{
    private string _action { get; set; }
    private string _reason { get; set; }
    private const string MESSAGE_FORMAT = "Could not {0} because {1}";
    public RepoInvalidActionException(string action, string reason) : base(string.Format(MESSAGE_FORMAT, action, reason))
    {
        _action = action;
        _reason = reason;
    }
    public RepoInvalidActionException(string action, string reason, Exception inner) : base(string.Format(MESSAGE_FORMAT, action, reason), inner)
    {
        _action = action;
        _reason = reason;
    }
}