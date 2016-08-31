using UnityEngine;
using System;
using System.Collections;

public class ServerTimeSync 
{
    private proto_game.Ping.Request pingRequest_ = new proto_game.Ping.Request();
    private long serverTimeDiff_ = 0;
    private long timestampSent_ = 0;
    private long nextUpdate_ = -1;
    private long ping_ = 0;
    private long latency_ = 0;

    public ServerTimeSync()
    {
    }

    public void Start()
    {
        nextUpdate_ = GetClientTime ();
        GameApp.Instance.Client.OnServerResponse += HandleResponse;
    }

    public long Ping 
    {
        get { return ping_; }
    }

    public long Latency 
    {
        get { return latency_; }
    }

    public long ServerTimeDifference
    {
        get { return serverTimeDiff_; }
    }

    private long GetClientTime()
    {
        return GameApp.Instance.ClientTimeMs();
    }

    public void Update()
    {
        if (nextUpdate_ < 0 || nextUpdate_ > GetClientTime())
        {
            return;
        }

        pingRequest_.timestamp = GetClientTime();
        pingRequest_.current_ping = (int)latency_;
        timestampSent_ = pingRequest_.timestamp;

        proto_common.Request req = new proto_common.Request ();
        req.type = proto_common.Commands.PING;
        
        ProtoBuf.Extensible.AppendValue (req, (int)proto_common.Commands.PING, pingRequest_);
        GameApp.Instance.Client.Send(req);

        nextUpdate_ = -1;
    }

    public void Dispose()
    {
        GameApp.Instance.Client.OnServerResponse -= HandleResponse;
    }

    void HandleResponse (proto_common.Response response)
    {
        if (response.type != proto_common.Commands.PING)
        {
            return;
        }

        var pong = ProtoBuf.Extensible.GetValue<proto_game.Ping.Response> (response, (int)proto_common.Commands.PING);

        var lat = (GetClientTime () - timestampSent_);
        if (lat < 0) 
            lat = 0;

        if (lat > latency_)
        {
            latency_ = (lat + latency_) / 2;
        } 
        else 
        {
            latency_ = (latency_ * 7 + lat) / 8;
        }

        ping_ = latency_ / 2;
        serverTimeDiff_ = (pong.timestamp + latency_ / 2) - GetClientTime();

        nextUpdate_ = GetClientTime() + 1000;
    }
}