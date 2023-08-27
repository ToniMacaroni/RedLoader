namespace SonsSdk.Exceptions;

public class NotInWorldException : Exception
{
    public NotInWorldException() : base("Player is not in world.")
    { }
}