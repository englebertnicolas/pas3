using System.Collections.Frozen;
using System.Reflection;
using Rebus.Handlers;
using Rebus.Serialization;
using Rebus.Topic;

namespace PAS.AspNetCore.Rebus;

internal class MessageTypeNameConvention : IMessageTypeNameConvention, ITopicNameConvention {
    private readonly FrozenDictionary<string, Type> topicToType;

    public MessageTypeNameConvention(params Assembly[] assemblies) {
        // Looking for IHandleMessages<TMessage> types
        var handledEventTypes = assemblies.SelectMany(x => x.GetTypes())
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct();

        var dictionarySource = new Dictionary<string, Type>();
        foreach (var eventType in handledEventTypes) {
            var attribute = eventType.GetCustomAttribute<TopicAttribute>();
            var topicName = attribute?.Name ?? eventType.FullName!;

            dictionarySource[topicName] = eventType;
        }

        topicToType = dictionarySource.ToFrozenDictionary();
    }

    // Publisher (Type -> String)
    public string GetTypeName(Type type) {
        var pair = topicToType.FirstOrDefault(x => x.Value == type);
        if (pair.Key is not null) return pair.Key;

        // Fallback: pure publisher (no local handler)
        var attribute = type.GetCustomAttribute<TopicAttribute>();
        return attribute?.Name ?? type.FullName!;
    }

    // Consumer (String -> Type)
    public Type GetType(string name) {
        if (topicToType.TryGetValue(name, out var type)) return type;
        throw new ArgumentException($"The RabbitMQ topic '{name}' is not associated with a local handler.");
    }

    string ITopicNameConvention.GetTopic(Type eventType) => GetTypeName(eventType);
}
