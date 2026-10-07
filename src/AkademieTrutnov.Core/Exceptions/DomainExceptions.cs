namespace AkademieTrutnov.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class CapacityExceededException : DomainException
{
    public CapacityExceededException(string message) : base(message) { }
}

public class DuplicateEnrollmentException : DomainException
{
    public DuplicateEnrollmentException(string message) : base(message) { }
}

public class InvalidAttendanceException : DomainException
{
    public InvalidAttendanceException(string message) : base(message) { }
}

public class RegistrationLateException : DomainException
{
    public RegistrationLateException(string message) : base(message) { }
}

public class WaitlistViolationException : DomainException
{
    public WaitlistViolationException(string message) : base(message) { }
}

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key) 
        : base($"Položka '{entityName}' s identifikátorem '{key}' nebyla nalezena.") { }
}
