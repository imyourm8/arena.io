using System;
using System.Collections;
using System.Collections.Generic;

using proto_common;

public class RequestsManager 
{
    private Dictionary<int, Action<Response>> requests_ = new Dictionary<int, Action<Response>>();

    public void AddRequest(Request request, Action<Response> handler)
    {
        requests_[request.id] = handler;
    }

    public void AddRequest(int requestID, Action<Response> handler)
    {
        requests_[requestID] = handler;
    }

    public void Update(Response response)
    {
        Action<Response> handler = null;
        if (requests_.TryGetValue(response.id, out handler))
        {
            handler(response);
            requests_.Remove(response.id);
        }
    }
}
