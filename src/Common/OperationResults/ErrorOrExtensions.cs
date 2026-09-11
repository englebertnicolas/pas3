namespace PAS.OperationResults;

public static class ErrorOrExtensions {

    /// <summary>
    /// Chain with another operation that returns an <c>ErrorOr&lt;&gt;</c> itself (avoid nested types).
    /// </summary>
    public static ErrorOr<TOut> Then<TIn, TOut>(this ErrorOr<TIn> errorOr, Func<TIn, ErrorOr<TOut>> func) {
        if (errorOr.IsFailure)
            return ErrorOr<TOut>.Failure([.. errorOr.Errors]);

        return func(errorOr.Value);
    }

    /// <summary>
    /// Chain with another operation that returns an <c>Task&lt;ErrorOr&lt;&gt;&gt;</c> itself (avoid nested types).
    /// </summary>
    public static async Task<ErrorOr<TOut>> ThenAsync<TIn, TOut>(this ErrorOr<TIn> errorOr, Func<TIn, Task<ErrorOr<TOut>>> func) {
        if (errorOr.IsFailure)
            return ErrorOr<TOut>.Failure([.. errorOr.Errors]);

        return await func(errorOr.Value);
    }

    /// <summary>
    /// Chain with another operation that returns an <c>ErrorOr&lt;&gt;</c> itself (avoid nested types).
    /// </summary>
    public static async Task<ErrorOr<TOut>> ThenAsync<TIn, TOut>(this Task<ErrorOr<TIn>> errorOr, Func<TIn, ErrorOr<TOut>> func) {
        return await errorOr.ThenAsync(func);
    }

    /// <summary>
    /// Chain with another operation that returns an <c>Task&lt;ErrorOr&lt;&gt;&gt;</c> itself (avoid nested types).
    /// </summary>
    public static async Task<ErrorOr<TOut>> ThenAsync<TIn, TOut>(this Task<ErrorOr<TIn>> errorOr, Func<TIn, Task<ErrorOr<TOut>>> func) {
        return await (await errorOr).ThenAsync(func);
    }

    /// <summary>
    /// Transform the value of a success. If it's a failure, propagate the error without executing the function.
    /// </summary>
    public static ErrorOr<TOut> Map<TIn, TOut>(this ErrorOr<TIn> errorOr, Func<TIn, TOut> func) {
        if (errorOr.IsFailure)
            return ErrorOr<TOut>.Failure([.. errorOr.Errors]);

        var v = func(errorOr.Value);
        return ErrorOr<TOut>.Success(v);
    }

    /// <summary>
    /// Transform the value of a success. If it's a failure, propagate the error without executing the function.
    /// </summary>
    public static async Task<ErrorOr<TOut>> MapAsync<TIn, TOut>(this ErrorOr<TIn> errorOr, Func<TIn, TOut> func) {
        if (errorOr.IsFailure)
            return ErrorOr<TOut>.Failure([.. errorOr.Errors]);

        var v = func(errorOr.Value);
        return ErrorOr<TOut>.Success(v);
    }

    /// <summary>
    /// Transform the value of a success. If it's a failure, propagate the error without executing the function.
    /// </summary>
    public static async Task<ErrorOr<TOut>> MapAsync<TIn, TOut>(this Task<ErrorOr<TIn>> errorOr, Func<TIn, TOut> func) {
        return await (await errorOr).MapAsync(func);
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static ErrorOr<T> Tap<T>(this ErrorOr<T> errorOr, Action<T> action) {
        if (errorOr.IsSuccess) {
            action(errorOr.Value);
        }
        return errorOr;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this Task<ErrorOr<T>> errorOr, Action<T> action) {
        await errorOr;
        if (errorOr.Result.IsSuccess) {
            action(errorOr.Result.Value);
        }
        return errorOr.Result;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this ErrorOr<T> errorOr, Func<T, Task> action) {
        if (errorOr.IsSuccess) {
            await action(errorOr.Value);
        }
        return errorOr;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this Task<ErrorOr<T>> errorOr, Func<T, Task> action) {
        return await (await errorOr).TapAsync(action);
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static ErrorOr<T> Tap<T>(this ErrorOr<T> errorOr, Func<T, ErrorOr<Success>> action) {
        if (errorOr.IsSuccess) {
            var eoAction = action(errorOr.Value);
            if (eoAction.IsFailure) return eoAction.Errors;
        }
        return errorOr;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this Task<ErrorOr<T>> errorOr, Func<T, ErrorOr<Success>> action) {
        await errorOr;
        if (errorOr.Result.IsSuccess) {
            var eoAction = action(errorOr.Result.Value);
            if (eoAction.IsFailure) return eoAction.Errors;
        }
        return errorOr.Result;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this ErrorOr<T> errorOr, Func<T, Task<ErrorOr<Success>>> action) {
        if (errorOr.IsSuccess) {
            var eoAction = await action(errorOr.Value);
            if (eoAction.IsFailure) return eoAction.Errors;
        }
        return errorOr;
    }

    /// <summary>
    /// Execute an action if the result is a success, then return the initial result unchanged.
    /// </summary>
    public static async Task<ErrorOr<T>> TapAsync<T>(this Task<ErrorOr<T>> errorOr, Func<T, Task<ErrorOr<Success>>> action) {
        return await (await errorOr).TapAsync(action);
    }

    /*
    /// <summary>
    /// Combines a sequence of ErrorOr instances into a single ErrorOr containing a list of values.
    /// Aggregates all errors if any failure occurs.
    /// </summary>
    public static ErrorOr<IReadOnlyList<T>> Combine<T>(this IEnumerable<ErrorOr<T>> source) {
        var items = source.ToList();
        var errors = items.Where(x => x.IsFailure)
            .SelectMany(x => x.Errors.ToArray())
            .ToList();

        if (errors.Count > 0)
            return ErrorOr<IReadOnlyList<T>>.Failure(errors);

        return items.Select(x => x.Value).ToList().AsReadOnly();
    }*/
}
