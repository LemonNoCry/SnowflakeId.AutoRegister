namespace SnowflakeId.AutoRegister.Db.Extensions;

public static class ExceptionTypeHelper
{
    private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
    private static readonly Type StackOverflowType = typeof(StackOverflowException);
    private static readonly Type ThreadAbortType = typeof(ThreadAbortException);
    private static readonly Type AccessViolationType = typeof(AccessViolationException);
    private static readonly Type SecurityType = typeof(SecurityException);

    public static bool IsCatchableExceptionType(this Exception e)
    {
        var type = e.GetType();
        return type != OutOfMemoryType && type != StackOverflowType && type != ThreadAbortType && type != AccessViolationType &&
            !SecurityType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
    }
}