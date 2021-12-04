using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Geospatial;
using Microsoft.Maps.Unity;

public class UnityHttpListener : MonoBehaviour
{
    public GameObject PinObject;
    public GameObject Map;
	private HttpListener listener;
	private Thread listenerThread;

	void Start()
	{
		listener = new HttpListener();
		listener.Prefixes.Add("http://192.168.159.50:4444/");
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		listener.Start();

		listenerThread = new Thread(startListener);
		listenerThread.Start();
		Debug.Log("Server Started");
	}

	void Update()
	{
	}

	private void startListener()
	{
		while (true)
		{
			var result = listener.BeginGetContext(ListenerCallback, listener);
			result.AsyncWaitHandle.WaitOne();
		}
	}

	private void ListenerCallback(IAsyncResult result)
	{
		var context = listener.EndGetContext(result);

		Debug.Log("Method: " + context.Request.HttpMethod);
		Debug.Log("LocalUrl: " + context.Request.Url.LocalPath);

		if (context.Request.QueryString.AllKeys.Length > 0)
			foreach (var key in context.Request.QueryString.AllKeys)
			{
				Debug.Log("Key: " + key + ", Value: " + context.Request.QueryString.GetValues(key)[0]);
			}





		if(context.Request.Url.LocalPath=="/send")
        {
            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            foreach (var key in context.Request.QueryString.AllKeys)
            {
                dict.Add(key,decimal.Parse(
                    (context.Request.QueryString.GetValues(key)[0]).Replace('.', ',')));
            }

            decimal lat = dict["lat"];
            var lon = dict["lon"];
            var elev = dict["elev"];

			UnityMainThreadDispatcher.Instance().Enqueue(UnityCreator(lat, lon, elev));

        }
		if (context.Request.HttpMethod == "POST")
		{
			Thread.Sleep(1000);
			var data_text = new StreamReader(context.Request.InputStream,
								context.Request.ContentEncoding).ReadToEnd();
			Debug.Log(data_text);
		}

		context.Response.Close();
	}

    public IEnumerator UnityCreator(decimal lat, decimal lon, decimal alt)
    {
        
        print("PIN CREATED");
		var gobj = Map;
        var obj = Instantiate(PinObject, gobj.transform);
        ;
        var mapPin = obj.AddComponent<MapPin>();
        mapPin.AltitudeReference = AltitudeReference.Ellipsoid;
        mapPin.UseRealWorldScale = true;
        mapPin.Altitude = (double)alt + 116;
        mapPin.Location = new LatLon((double)lat, (double)lon);
        
        //mapPin.UseRealWorldScale = true;
        yield break;

    }
}
