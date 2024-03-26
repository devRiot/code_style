using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace devRiot.Network
{
    public enum RequestType
    {
        Get,
        Post,
    }

    /// <summary>
    /// ApiRequest 클래스
    /// </summary>
    public abstract class ApiRequest
    {
        public string endpoint;
        public Action<string> callback;

        public RequestType requestType;
        public Dictionary<string,object> parameters;

        public int timeout = 500;
        public bool isWaitForResponce = true;    //api요청 시 응답을 기다릴지 여부

        private string resultText = null;
        public bool IsResult { get {  return resultText != null; } }

        public ApiRequest(Action<string> callback)
        {
            this.callback = callback;
            parameters = new Dictionary<string, object>();
        }

        public string ToJsonParameters()
        {
            return JsonConvert.SerializeObject(parameters);
        }

        // 응답 콜백 메서드
        public virtual void OnResponse(string responseData)
        {
            Debug.Log(responseData);

            resultText = responseData;
            callback?.Invoke(responseData);
        }

        // 응답 에러
        public virtual void OnResponseError(string error)
        {
            Debug.Log("OnResponseError : " + error);
        }
    }
}