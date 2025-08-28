using System.Reflection;
using FluentResults;

namespace Streetcode.BLL.MediatR;

public static class ResultFactory
{
    public static T? CreateFailure<T>(IEnumerable<string> messages) where T : IResultBase
    {
        var errors = (messages ?? Array.Empty<string>())
            .Where(static m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.Ordinal)
            .Select(static m => (IError)new Error(m))
            .ToList();
        var type = typeof(T);

        // Case 1: Generic Result<TValue>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = type.GetGenericArguments()[0];
            // Find Result.Fail<TValue>(IEnumerable<IError>)
            var method = typeof(Result)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == nameof(Result.Fail)
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 1
                            && typeof(IEnumerable<IError>).IsAssignableFrom(m.GetParameters()[0].ParameterType));

            var genericMethod = method.MakeGenericMethod(valueType);
            return (T)genericMethod.Invoke(null, new object[] { errors })!;
        }

        // Case 2: Non-generic Result
        if (type == typeof(Result))
        {
            // Call Result.Fail(IEnumerable<IError>)
            return (T)(IResultBase)Result.Fail(errors);
        }

        throw new InvalidOperationException($"Unsupported result type: {type}");
    }
}