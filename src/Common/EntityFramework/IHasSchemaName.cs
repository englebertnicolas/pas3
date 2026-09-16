namespace PAS.EntityFramework;

public interface IHasSchemaName {
    static abstract string SchemaName { get; }
}
