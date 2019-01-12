using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Prime31;


public class TwitterGUIManager : Prime31.MonoBehaviourGUI
{
#if UNITY_IPHONE
	public bool canUseTweetSheet = false; // requires iOS 5 and a Twitter account setup in Settings.app
	

	void Start()
	{
		canUseTweetSheet = TwitterBinding.isTweetSheetSupported() && TwitterBinding.canUserTweet();
		Application.CaptureScreenshot( FacebookGUIManager.screenshotFilename );
	}
	
	
	// common event handler used for all graph requests that logs the data to the console
	void completionHandler( string error, object result )
	{
		if( error != null )
			Debug.LogError( error );
		else
			Prime31.Utils.logObject( result );
	}
	

	void OnGUI()
	{
		beginColumn();
		
		if( GUILayout.Button( "Initialize Twitter" ) )
		{
			TwitterBinding.init( "INSERT_YOUR_INFO_HERE", "INSERT_YOUR_INFO_HERE" );
		}
		
		
		if( GUILayout.Button( "Is Logged In?" ) )
		{
			bool isLoggedIn = TwitterBinding.isLoggedIn();
			Debug.Log( "Twitter is logged in: " + isLoggedIn );
		}
		
		
		if( GUILayout.Button( "Logged in Username" ) )
		{
			string username = TwitterBinding.loggedInUsername();
			Debug.Log( "Twitter username: " + username );
		}


		if( GUILayout.Button( "Login with Oauth" ) )
		{
			TwitterBinding.showOauthLoginDialog();
		}

		
		if( GUILayout.Button( "Logout" ) )
		{
			TwitterBinding.logout();
		}
		
		
		endColumn( true );
		
		
		if( GUILayout.Button( "Post Status Update" ) )
		{
			TwitterBinding.postStatusUpdate( "im posting this from Unity: " + Time.deltaTime );
		}


		if( GUILayout.Button( "Post Status Update + Image" ) )
		{
			var pathToImage = Application.persistentDataPath + "/" + FacebookGUIManager.screenshotFilename;
			TwitterBinding.postStatusUpdate( "I'm posting this from Unity with a fancy image: " + Time.deltaTime, pathToImage );
		}
	
		
		// if we are on iOS 5+ with a Twitter account setup we can use the tweet sheet
		if( canUseTweetSheet )
		{
			if( GUILayout.Button( "Show Tweet Sheet" ) )
			{
				var pathToImage = Application.persistentDataPath + "/" + FacebookGUIManager.screenshotFilename;
				TwitterBinding.showTweetComposer( "I'm posting this from Unity with a fancy image: " + Time.deltaTime, pathToImage );
			}
		}
		
		
		if( GUILayout.Button( "Custom Request" ) )
		{
			var dict = new Dictionary<string,string>();
			dict.Add( "status", "word up with a boogie boogie update" );
			TwitterBinding.performRequest( "POST", "/statuses/update.json", dict );
		}
		
		endColumn( false );
		
		
		if( bottomRightButton( "Sharing..." ) )
		{
			Application.LoadLevel( "SharingTestScene" );
		}
	}
#endif
}
