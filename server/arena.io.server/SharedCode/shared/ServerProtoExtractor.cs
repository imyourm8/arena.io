using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


static class ServerProtoExtractor
{
    public static T Extract<T>(this proto_server.Request msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_server.Request msg, proto_server.Commands field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }

    public static T Extract<T>(this proto_server.Response msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_server.Response msg, proto_server.Commands field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }

    public static T Extract<T>(this proto_server.Event msg, int field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, field);
    }

    public static T Extract<T>(this proto_server.Event msg, proto_server.Events field)
    {
        return ProtoBuf.Extensible.GetValue<T>(msg, (int)field);
    }
}

