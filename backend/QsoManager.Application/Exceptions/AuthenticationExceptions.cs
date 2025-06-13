namespace QsoManager.Application.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class UserAlreadyExistsException : AuthenticationException
{
    public UserAlreadyExistsException(string message) : base(message)
    {
    }
}

public class UserNotFoundException : AuthenticationException
{
    public UserNotFoundException(string message) : base(message)
    {
    }
}

public class UserRegistrationFailedException : AuthenticationException
{
    public UserRegistrationFailedException(string message) : base(message)
    {
    }
}

public class PasswordResetFailedException : AuthenticationException
{
    public PasswordResetFailedException(string message) : base(message)
    {
    }
}

public class UnauthorizedException : AuthenticationException
{
    public UnauthorizedException() : base("Unauthorized")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}
