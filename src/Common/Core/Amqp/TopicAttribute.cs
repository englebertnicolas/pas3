namespace PAS.Core.Amqp;

/// <summary>
/// Specifies the custom AMQP routing key (topic name) associated with an integration event.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TopicAttribute(string name) : Attribute {
    public string Name { get; } = name;
}