using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class TwitterAndroidEventListener : MonoBehaviour
{
#if UNITY_ANDROID
	void OnEnable()
	{
		// Listen to all events for illustration purposes
		TwitterAndroidManager.loginDidSucceedEvent += loginDidSucceedEvent;
		TwitterAndroidManager.loginDidFailEvent += loginDidFailEvent;
		TwitterAndroidManager.requestSucceededEvent += requestSucceededEvent;
		TwitterAndroidManager.requestFailedEvent += requestFailedEvent;
		TwitterAndroidManager.twitterInitializedEvent += twitterInitializedEvent;
	}


	void OnDisable()
	{
		// Remove all event handlers
		TwitterAndroidManager.loginDidSucceedEvent -= loginDidSucceedEvent;
		TwitterAndroidManager.loginDidFailEvent -= loginDidFailEvent;
		TwitterAndroidManager.requestSucceededEvent -= requestSucceededEvent;
		TwitterAndroidManager.requestFailedEvent -= requestFailedEvent;
		TwitterAndroidManager.twitterInitializedEvent -= twitterInitializedEvent;
	}



	void loginDidSucceedEvent( string username )
	{
		Debug.Log( "loginDidSucceedEvent.  username: " + username );
	}


	void loginDidFailEvent( string error )
	{
		Debug.Log( "loginDidFailEvent. error: " + error );
	}


	void requestSucceededEvent( object response )
	{
		Debug.Log( "requestSucceededEvent" );
		Prime31.Utils.logObject( response );
	}


	void requestFailedEvent( string error )
	{
		Debug.Log( "requestFailedEvent. error: " + error );
	}


	void twitterInitializedEvent()
	{
		Debug.Log( "twitterInitializedEvent" );
	}
#endif
}


