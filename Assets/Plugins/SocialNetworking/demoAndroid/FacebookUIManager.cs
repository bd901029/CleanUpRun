using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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


public class FacebookUIManager : MonoBehaviourGUI
{
#if UNITY_ANDROID
	public static string screenshotFilename = "someScreenshot.png";
	
	
	// common event handler used for all Facebook graph requests that logs the data to the console
	void completionHandler( string error, object result )
	{
		if( error != null )
			Debug.LogError( error );
		else
			Prime31.Utils.logObject( result );
	}
	
	
	void Start()
	{
		// grab a screenshot for later use
		Application.CaptureScreenshot( screenshotFilename );
		
		// optionally enable logging of all requests that go through the Facebook class
		//Facebook.instance.debugRequests = true;
	}
	
	
	void OnGUI()
	{
		beginColumn();


		if( GUILayout.Button( "Initialize Facebook" ) )
		{
			FacebookAndroid.init();
		}
	
	
		if( GUILayout.Button( "Login" ) )
		{
			FacebookAndroid.loginWithReadPermissions( new string[] { "email", "user_birthday" } );
		}
		
		
		if( GUILayout.Button( "Reauthorize with Publish Permissions" ) )
		{
			FacebookAndroid.reauthorizeWithPublishPermissions( new string[] { "publish_actions", "manage_friendlists" }, FacebookSessionDefaultAudience.EVERYONE );
		}

		
		if( GUILayout.Button( "Logout" ) )
		{
			FacebookAndroid.logout();
		}
	
	
		if( GUILayout.Button( "Is Session Valid?" ) )
		{
			var isSessionValid = FacebookAndroid.isSessionValid();
			Debug.Log( "Is session valid?: " + isSessionValid );
		}
		
	
		if( GUILayout.Button( "Get Session Token" ) )
		{
			var token = FacebookAndroid.getAccessToken();
			Debug.Log( "session token: " + token );
		}

		
		if( GUILayout.Button( "Get Granted Permissions" ) )
		{
			var permissions = FacebookAndroid.getSessionPermissions();
			Debug.Log( "granted permissions: " + permissions.Count );
			Prime31.Utils.logObject( permissions );
		}

	
		endColumn( true );
		

		if( GUILayout.Button( "Post Image" ) )
		{
			var pathToImage = Application.persistentDataPath + "/" + screenshotFilename;
			var bytes = System.IO.File.ReadAllBytes( pathToImage );
			
			Facebook.instance.postImage( bytes, "im an image posted from Android", completionHandler );
		}


		if( GUILayout.Button( "Graph Request (me)" ) )
		{
			Facebook.instance.graphRequest( "me", completionHandler );
		}


		if( GUILayout.Button( "Post Message" ) )
		{
			Facebook.instance.postMessage( "im posting this from Unity: " + Time.deltaTime, completionHandler );
		}
		
		
		if( GUILayout.Button( "Post Message & Extras" ) )
		{
			Facebook.instance.postMessageWithLinkAndLinkToImage( "link post from Unity: " + Time.deltaTime, "http://prime31.com", "Prime31 Studios", "http://prime31.com/assets/images/prime31logo.png", "Prime31 Logo", completionHandler );
		}


		if( GUILayout.Button( "Show Post Dialog" ) )
		{
			// parameters are optional. See Facebook's documentation for all the dialogs and paramters that they support
			var parameters = new Dictionary<string,string>
			{
				{ "link", "http://prime31.com" },
				{ "name", "link name goes here" },
				{ "picture", "http://prime31.com/assets/images/prime31logo.png" },
				{ "caption", "the caption for the image is here" }
			};
			FacebookAndroid.showDialog( "stream.publish", parameters );
		}


		if( GUILayout.Button( "Get Friends" ) )
		{
			Facebook.instance.getFriends( completionHandler );
		}

		
		endColumn();
		
		
		if( bottomLeftButton( "Twitter Scene" ) )
		{
			Application.LoadLevel( "TwitterTestScene" );
		}
	}

#endif
}
