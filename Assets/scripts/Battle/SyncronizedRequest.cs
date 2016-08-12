using UnityEngine;

using System;
using System.Collections;

public class SyncronizedRequest 
{
    private bool sendRequest_ = true;
    private Func<object> request_ = null;
    private proto_common.Commands cmd_ = proto_common.Commands.UNKNOWN_CMD;
    private Action<proto_common.Response> responseHandler_;
    private proto_common.Response response_ = null;

    public void ExecuteAction(Func<object> requestCreator, proto_common.Commands cmd, Action<proto_common.Response> responseHandler)
    {
        sendRequest_ = true;
        response_ = null;
        request_ = requestCreator;
        cmd_ = cmd;
        responseHandler_ = responseHandler;
    }

    public bool IsFinished
    {
        get { return  !sendRequest_ && request_ == null && response_ != null; }
    }

    private void HandleResponse(proto_common.Response response)
    {
        response_ = response;
    }

    public void Update()
    {
        if (sendRequest_)
        {
            var id = GameApp.Instance.Client.Send(request_(), cmd_);
            GameApp.Instance.RequestsManager.AddRequest(id, HandleResponse);
            sendRequest_ = false;
        }
        else if (response_ != null)
        {
            if (responseHandler_ != null) 
                responseHandler_(response_);
            request_ = null;
        }
    }
}
