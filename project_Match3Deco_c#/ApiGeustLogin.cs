using devRiot.GameLogic;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace devRiot.Network
{
    public class ApiGeustLogin : ApiRequest
    {
        public ApiGeustLogin(Action<string> callback) : base(callback)
        {
            endpoint = "/users/login";
            requestType = RequestType.Post;
            isWaitForResponce = true;

            parameters.Add("uid", PlayerInfo.Instance.uid);
            parameters.Add("login_type", "guest");
        }

        public override void OnResponse(string responseData)
        {
            base.OnResponse(responseData);

            Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
            if (result != null)
            {
                PlayerInfo.Instance.uid = result["uid"];
                PlayerInfo.Instance.username = result["user_name"];

                Debug.Log("login ok");
            }
        }
    }
}