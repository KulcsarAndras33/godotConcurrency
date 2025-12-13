using System;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

public class ValidatingNodeDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _nodeDeserializer;

    public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer)
    {
        _nodeDeserializer = nodeDeserializer;
    }

    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value, ObjectDeserializer rootDeserializer)
    {
        if (_nodeDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer))
        {
            var context = new ValidationContext(value, null, null);
            Validator.ValidateObject(value, context, true);
            return true;
        }
        return false;
    }
}