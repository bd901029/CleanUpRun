using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Prime31;


#if UNITY_ANDROID
public enum FacebookSessionDefaultAudience
{
	NONE,
	ONLY_ME,
	FRIENDS,
	EVERYONE
}

public class FacebookAndroid
{
	private static AndroidJavaObject _facebookPlugin;
	
	
	static FacebookAndroid()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		// find the plugin instance
		using( var pluginClass = new AndroidJavaClass( "com.prime31.FacebookPlugin" ) )
			_facebookPlugin = pluginClass.CallStatic<AndroidJavaObject>( "instance" );
		
		// on login, set the access token
		FacebookManager.preLoginSucceededEvent += () =>
		{
			Facebook.instance.accessToken = getAccessToken();
		};
	}

	
	// Initializes the Facebook plugin for your application
	public static void init()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_facebookPlugin.Call( "init" );
	}
	
	
	// Checks to see if the current session is valid
	public static bool isSessionValid()
	{
		if( Application.platform != RuntimePlatform.Android )
			return false;
		
		return _facebookPlugin.Call<bool>( "isSessionValid" );
	}


	// Gets the current access token
	public static string getAccessToken()
	{
		if( Application.platform != RuntimePlatform.Android )
			return string.Empty;
			
		return _facebookPlugin.Call<string>( "getAccessToken" );
	}
	
	
	// Gets the permissions granted to the current access token
	public static List<object> getSessionPermissions()
	{
		if( Application.platform == RuntimePlatform.Android )
		{
			var permissions = _facebookPlugin.Call<string>( "getSessionPermissions" );
			return permissions.listFromJson();
		}
		
		return new List<object>();
	}


	public static void login()
	{
		loginWithReadPermissions( new string[] {} );
	}
	

	// Authenticates the user requesting the passed in read permissions
	public static void loginWithReadPermissions( string[] permissions )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_facebookPlugin.Call( "loginWithReadPermissions", new object[] { permissions } );
	}


	// Authenticates the user requesting the passed in publish permissions
	public static void loginWithPublishPermissions( string[] permissions )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_facebookPlugin.Call( "loginWithPublishPermissions", new object[] { permissions } );
	}

	
	// Reauthorizes with the requested read permissions
	public static void reauthorizeWithReadPermissions( string[] permissions )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_facebookPlugin.Call( "reauthorizeWithReadPermissions", permissions.toJson() );
	}
	
	
	// Reauthorizes with the requested publish permissions and audience
	public static void reauthorizeWithPublishPermissions( string[] permissions, FacebookSessionDefaultAudience defaultAudience )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_facebookPlugin.Call( "reauthorizeWithPublishPermissions", permissions.toJson(), defaultAudience.ToString() );
	}
	

	// Logs the user out and invalidates the token
	public static void logout()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
			
		_facebookPlugin.Call( "logout" );
		Facebook.instance.accessToken = string.Empty;
	}
	

	// Full access to any existing or new Facebook dialogs that get added.  See Facebooks documentation for parameters and dialog types
	public static void showDialog( string dialogType, Dictionary<string,string> parameters )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		// load up the Bundle
		using( var bundle = new AndroidJavaObject( "android.os.Bundle" ) )
		{
			var putStringMethod = AndroidJNI.GetMethodID( bundle.GetRawClass(), "putString", "(Ljava/lang/String;Ljava/lang/String;)V" );
			var args = new object[2];
			
			// add all our dictionary elements into the Bundle
			if( parameters != null )
			{
				foreach( var kv in parameters  )
				{
					args[0] = new AndroidJavaObject( "java.lang.String", kv.Key );
					args[1] = new AndroidJavaObject( "java.lang.String", kv.Value );
					AndroidJNI.CallVoidMethod( bundle.GetRawObject(), putStringMethod, AndroidJNIHelper.CreateJNIArgArray( args ) );
				}
			}
			
			// call off to java land
			_facebookPlugin.Call( "showDialog", dialogType, bundle );
		}
	}

	
	// Calls a custom Graph API method with the key/value pairs in the Dictionary.  Pass in a null dictionary if no parameters are needed.
	public static void graphRequest( string graphPath, string httpMethod, Dictionary<string,string> parameters )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		// load up the Bundle
		using( var bundle = new AndroidJavaObject( "android.os.Bundle" ) )
		{
			var putStringMethod = AndroidJNI.GetMethodID( bundle.GetRawClass(), "putString", "(Ljava/lang/String;Ljava/lang/String;)V" );
			var args = new object[2];
			
			// add all our dictionary elements into the Bundle
			if( parameters != null )
			{
				foreach( var kv in parameters  )
				{
					args[0] = new AndroidJavaObject( "java.lang.String", kv.Key );
					args[1] = new AndroidJavaObject( "java.lang.String", kv.Value );
					AndroidJNI.CallObjectMethod( bundle.GetRawObject(), putStringMethod, AndroidJNIHelper.CreateJNIArgArray( args ) );
				}
			}
			
			// call off to java land
			_facebookPlugin.Call( "graphRequest", graphPath, httpMethod, bundle );
		}
	}
	
}
#endif
