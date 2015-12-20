using UnityEngine;
using System.Collections;
using RestSharp;
using System.Text.RegularExpressions;
using System;
using RestSharp.Deserializers;

public class RestSharpTester : MonoBehaviour {
    private readonly string[] testUrls = { "http://httpbin.org/gzip", "http://httpbin.org/deflate", "https://httpbin.org/gzip", "https://httpbin.org/deflate" };
    private JsonDeserializer deserializer = new JsonDeserializer();

    private void Start () {
        Invoke("StartTests", 1f);
    }

    private void StartTests() {
        foreach(string testUrl in testUrls) {
            DoRequest(testUrl);
        }
    }

    private void DoRequest(string requestUrl) {
        RestClient client = new RestClient(requestUrl);
        RestRequest request = new RestRequest("", Method.GET);
        request.AddHeader("test", "test");

        Debug.Log("Testing url: " + requestUrl);
        IRestResponse response = client.Get(request);
        ValidateResponse(requestUrl, response, false);
        
        client.GetAsync(request, OnResponse);
        
    }

    private void OnResponse(IRestResponse response, RestRequestAsyncHandle handle) {
        string requestUrl = response.ResponseUri != null ? response.ResponseUri.ToString() : "Could not get response URI";
        ValidateResponse(requestUrl, response, true);
    }

    private bool ValidateResponse(string requestUrl, IRestResponse response, bool isAsyncRequest) {
        string requestType = (isAsyncRequest ? "async" : "sync");
        if (response.ErrorException != null) {
            Debug.LogError("Request("+ requestType + ") failed for url: " + requestUrl);
            return false;
        }

        var deserializedResponse = deserializer.Deserialize<Response>(response);
        if (deserializedResponse == null || !deserializedResponse.Headers.Test.Equals("test")) {
            Debug.LogError("Could not parse ("+ requestType + ")response for url: " + requestUrl);
            return false;
        }

        Debug.Log("Successfully executed " + requestType + " request for url: " + requestUrl);
        return true;
    }
}


public class Response {
    public class ResponseHeaders {
        public string Accept { get; set; }
        public string ContentLength { get; set; }
        public string Host { get; set; }
        public string UserAgent { get; set; }
        public string Test { get; set; }
    }

    public bool Deflated { get; set; }
    public bool Gzipped { get; set; }

    public ResponseHeaders Headers { get; set; }

    public string Method { get; set; }
    public string Origin { get; set; }
}