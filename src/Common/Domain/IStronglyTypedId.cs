namespace PAS.Domain;

public interface IStronglyTypedId<TValue> where TValue : notnull {
    TValue Value { get; }
}
