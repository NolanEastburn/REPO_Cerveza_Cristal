using System;


namespace Cerveza_Cristal;


[Serializable]
public class RepoStaticInstanceNullException : Exception
{
    private Type _instanceType { get; set; }
    private const string MESSAGE_FORMAT = "The REPO class {0} has a null static instance!";
    public RepoStaticInstanceNullException(Type instanceType) : base(string.Format(MESSAGE_FORMAT, instanceType))
    {
        _instanceType = instanceType;
    }
    public RepoStaticInstanceNullException(Type instanceType, Exception inner) : base(string.Format(MESSAGE_FORMAT, instanceType), inner)
    {
        _instanceType = instanceType;
    }
}