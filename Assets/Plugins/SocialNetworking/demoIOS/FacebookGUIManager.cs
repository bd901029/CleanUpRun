using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Prime31;

// trick when using both the iOS and Android version of the plugin in the same project. Add this block to the
// top of the file you are calling the Facebook methods from so they can share code. Note that it will only work
// when calling methods that are common to both platforms!
/*
#if UNITY_ANDROID
using FacebookAccess = FacebookAndroid;
#elif UNITY_IPHONE
using FacebookAccess = FacebookBinding;
#endif
*/


public class FacebookGUIManager : Prime31.MonoBehaviourGUI
{
	private string userId;
	private bool canUserUseFacebookComposer = false;
	public static string screenshotFilename = "someScreenshot.png";
	
#if UNITY_IPHONE
	// common event handler used for all graph requests that logs the data to the console
	void completionHandler( string error, object result )
	{
		if( error != null )
			Debug.LogError( error );
		else
			Prime31.Utils.logObject( result );
	}
	
	
	void Start()
	{
		// Dump custom data to log after a request completes
		FacebookManager.graphRequestCompletedEvent += result =>
		{
			Prime31.Utils.logObject( result );
		};
		
		// grab a screenshot for later use
		Application.CaptureScreenshot( screenshotFilename );
		
		// this is iOS 6 only!
		canUserUseFacebookComposer = FacebookBinding.canUserUseFacebookComposer();
		
		// optionally enable logging of all requests that go through the Facebook class
		//Facebook.instance.debugRequests = true;
	}

		
	void OnGUI()
	{
		beginColumn();
		
		if( GUILayout.Button( "Initialize Facebook" ) )
		{
			FacebookBinding.init();
		}
		
		
		if( GUILayout.Button( "Login" ) )
		{
			// Note: requesting publish permissions here will result in a crash. Only read permissions are permitted.
			var permissions = new string[] { "user_games_activity" };
			FacebookBinding.loginWithReadPermissions( permissions );
		}

		
		if( GUILayout.Button( "Reauth with Publish Permissions" ) )
		{
			var permissions = new string[] { "publish_actions", "publish_stream" };
			FacebookBinding.reauthorizeWithPublishPermissions( permissions, FacebookSessionDefaultAudience.OnlyMe );
		}

		
		if( GUILayout.Button( "Logout" ) )
		{
			FacebookBinding.logout();
		}
		
		
		if( GUILayout.Button( "Is Session Valid?" ) )
		{
			bool isLoggedIn = FacebookBinding.isSessionValid();
			Debug.Log( "Facebook is session valid: " + isLoggedIn );
		}
		
		
		if( GUILayout.Button( "Get Access Token" ) )
		{
			var token = FacebookBinding.getAccessToken();
			Debug.Log( "access token: " + token );
		}
		
		
		if( GUILayout.Button( "Get Granted Permissions" ) )
		{
			var permissions = FacebookBinding.getSessionPermissions();
			foreach( var perm in permissions )
				Debug.Log( perm );
		}
		

		endColumn( true );
		
		
		// toggle to show two different sets of buttons
		if( toggleButtonState( "Toggle Alternate Buttons" ) )
			secondColumnButtonsGUI();
		else
			secondColumnAdditionalButtonsGUI();
		toggleButton( "Toggle Alternate Buttons", "Toggle Buttons" );
		
		endColumn( false );
		
		
		if( bottomRightButton( "Twitter..." ) )
		{
			Application.LoadLevel( "TwitterTestScene" );
		}
	}
	
	
	private void secondColumnButtonsGUI()
	{
		if( GUILayout.Button( "Post Image" ) )
		{
			var pathToImage = Application.persistentDataPath + "/" + FacebookGUIManager.screenshotFilename;
			if( !System.IO.File.Exists( pathToImage ) )
			{
				Debug.LogError( "there is no screenshot avaialable at path: " + pathToImage );
				return;
			}
				
			var bytes = System.IO.File.ReadAllBytes( pathToImage );
			Facebook.instance.postImage( bytes, "im an image posted from iOS", completionHandler );
		}
		
		
		if( GUILayout.Button( "Graph Request (me)" ) )
		{
			Facebook.instance.graphRequest( "me", HTTPVerb.GET, ( error, obj ) =>
			{
				// if we have an error we dont proceed any further
				if( error != null )
					return;
				
				if( obj == null )
					return;
				
				// grab the userId and persist it for later use
				var ht = obj as Hashtable;
				userId = ht["id"].ToString();
				
				Debug.Log( "me Graph Request finished: " );
				Prime31.Utils.logObject( ht );
			});
		}
		
		
		if( GUILayout.Button( "Post Message" ) )
		{
			Facebook.instance.postMessage( "im posting this from Unity: " + Time.deltaTime, completionHandler );
		}
		
		
		if( GUILayout.Button( "Post Message & Extras" ) )
		{
			Facebook.instance.postMessageWithLinkAndLinkToImage( "link post from Unity: " + Time.deltaTime, "http://prime31.com", "Prime31 Studios", "http://prime31.com/assets/images/prime31logo.png", "Prime31 Logo", completionHandler );
		}
		
		
		if( GUILayout.Button( "Show Post Message Dialog" ) )
		{
			// parameters are optional. See Facebook's documentation for all the dialogs and paramters that they support
			var parameters = new Dictionary<string,string>
			{
				{ "link", "http://prime31.com" },
				{ "name", "link name goes here" },
				{ "picture", "http://prime31.com/assets/images/prime31logo.png" },
				{ "caption", "the caption for the image is here" }
			};
			FacebookBinding.showDialog( "stream.publish", parameters );
		}
		
		
		if( GUILayout.Button( "Get Friends" ) )
		{
			Facebook.instance.getFriends( completionHandler );
		}
		
		
		if( canUserUseFacebookComposer )
		{
			if( GUILayout.Button( "Show Facebook Composer" ) )
			{
				// ensure the image exists before attempting to add it!
				var pathToImage = Application.persistentDataPath + "/" + FacebookGUIManager.screenshotFilename;
				if( !System.IO.File.Exists( pathToImage ) )
					pathToImage = null;
				
				FacebookBinding.showFacebookComposer( "I'm posting this from Unity with a fancy image: " + Time.deltaTime, pathToImage, "http://prime31.com" );
			}
		}
	}
	
	
	private void secondColumnAdditionalButtonsGUI()
	{
		if( GUILayout.Button( "Custom Graph Request" ) )
		{
			Facebook.instance.graphRequest( "platform/posts", HTTPVerb.GET, completionHandler );
		}
		

		if( GUILayout.Button( "Custom REST Request" ) )
		{
			var hash = new Hashtable();
			hash.Add( "query", "SELECT uid,name FROM user WHERE uid=4" );
			FacebookBinding.restRequest( "fql.query", "POST", hash );
		}

		
		/* Note: it is not recommended to include your app secret in your app binary. Getting an app access token
		 * should occur on your own web server to remain secure. The commented out code below will show you how you can get
		 * an app access token though Facebook highly recommends only doing a call like this on your own server.
		if( GUILayout.Button( "Get App Access Token" ) )
		{
			Facebook.instance.getAppAccessToken( "YOUR_APP_ID", "YOUR_APP_SECRET", token =>
			{
				Debug.Log( "app access token retrieved: " + token );
			});
		}
		*/
		
		
		if( GUILayout.Button( "Post Score" ) )
		{
			if( userId == null )
			{
				Debug.Log( "First call the 'me' graph request to gather the user's userId" );
				return;
			}

			Facebook.instance.postScore( userId, 250, didSucceed =>
			{
				Debug.Log( "score post suceeded? " + didSucceed );
			});
		}
		
		
		if( GUILayout.Button( "Get Scores" ) )
		{
			if( userId == null )
			{
				Debug.Log( "First call the 'me' graph request to gather the user's userId" );
				return;
			}
			
			Facebook.instance.getScores( userId, completionHandler );
		}
		
		
		if( GUILayout.Button( "Get App Launch Url" ) )
		{
			Debug.Log( "app launch url: " + FacebookBinding.getAppLaunchUrl() );
		}
		
		
		if( GUILayout.Button( "Renew Credentials for Facebook Accounts" ) )
		{
			FacebookBinding.renewCredentialsForAllFacebookAccounts();
		}
	
	}
#endif
}
