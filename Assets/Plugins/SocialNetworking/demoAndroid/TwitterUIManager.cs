using UnityEngine;
using System.Collections.Generic;
using Prime31;


public class TwitterUIManager : MonoBehaviourGUI
{
#if UNITY_ANDROID
	void OnGUI()
	{
		beginColumn();


		if( GUILayout.Button( "Initialize Twitter" ) )
		{
			// Replace these with your own credentials!!!
			TwitterAndroid.init( "INSERT_YOUR_INFO_HERE", "INSERT_YOUR_INFO_HERE" );
		}


		if( GUILayout.Button( "Login" ) )
		{
			TwitterAndroid.showLoginDialog();
		}


		if( GUILayout.Button( "Is Logged In?" ) )
		{
			var isLoggedIn = TwitterAndroid.isLoggedIn();
			Debug.Log( "Is logged in?: " + isLoggedIn );
		}


		if( GUILayout.Button( "Post Update with Image" ) )
		{
			var pathToImage = Application.persistentDataPath + "/" + FacebookUIManager.screenshotFilename;
			var bytes = System.IO.File.ReadAllBytes( pathToImage );

			TwitterAndroid.postUpdateWithImage( "test update from Unity!", bytes );
		}


		endColumn( true );


		if( GUILayout.Button( "Logout" ) )
		{
			TwitterAndroid.logout();
		}


		if( GUILayout.Button( "Post Update" ) )
		{
			TwitterAndroid.postUpdate( "im an update from the Twitter Android Plugin" );
		}


		if( GUILayout.Button( "Get Home Timeline" ) )
		{
			TwitterAndroid.getHomeTimeline();
		}


		if( GUILayout.Button( "Get Followers" ) )
		{
			TwitterAndroid.getFollowers();
		}


		if( GUILayout.Button( "Custom Request" ) )
		{
			var dict = new Dictionary<string, string>();
			dict.Add( "screen_name", "prime_31" );
			dict.Add( "test", "paramters" );
			dict.Add( "test2", "asdf" );
			dict.Add( "test3", "wer" );
			dict.Add( "test4", "vbn" );

			TwitterAndroid.performRequest( "get", "/1/users/show.json", dict );
		}

		endColumn();




		if( bottomLeftButton( "Facebook Scene" ) )
		{
			Application.LoadLevel( "FacebookTestScene" );
		}

	}
#endif
}
