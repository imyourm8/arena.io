using ProtoBuf;

public static class ProtoExtractor
{
    public static T Extract<T>(this proto_common.Request msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_common.Request msg, proto_common.Commands field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }

    public static T Extract<T>(this proto_common.Response msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_common.Response msg, proto_common.Commands field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }

    public static T Extract<T>(this proto_common.Event msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_common.Event msg, proto_common.Events field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }
}
