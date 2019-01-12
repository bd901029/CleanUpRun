using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prime31;


#if UNITY_IPHONE
public enum FacebookSessionDefaultAudience
{
	None = 0,
	OnlyMe = 10,
	Friends = 20,
	Everyone = 30
}

public enum FacebookSessionLoginBehavior
{
	WithFallbackToWebView,
	WithNoFallbackToWebView,
	ForcingWebView,
	UseSystemAccountIfPresent
}


public class FacebookBinding
{
	static FacebookBinding()
	{
		// on login, set the access token
		FacebookManager.preLoginSucceededEvent += () =>
		{
			Facebook.instance.accessToken = getAccessToken();
		};
	}


    [DllImport("__Internal")]
    private static extern void _facebookInit();

	// Initializes the Facebook plugin for your application
    public static void init()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookInit();
		
		// grab the access token in case it is saved
		Facebook.instance.accessToken = getAccessToken();
    }
	
	
	[DllImport("__Internal")]
	private static extern string _facebookGetAppLaunchUrl();

	// Gets the url used to launch the application. If no url was used returns string.Empty
	public static string getAppLaunchUrl()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _facebookGetAppLaunchUrl();

		return string.Empty;
	}
	
	
	[DllImport("__Internal")]
	private static extern void _facebookSetSessionLoginBehavior( int behavior );
	
	// Sets the login behavior. Must be called before any login calls! Understand what the login behaviors are and how they work before using this!
    public static void setSessionLoginBehavior( FacebookSessionLoginBehavior loginBehavior )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookSetSessionLoginBehavior( (int)loginBehavior );
    }
	

    [DllImport("__Internal")]
    private static extern void _facebookRenewCredentialsForAllFacebookAccounts();
 
	// iOS 6 only. Renews the credentials that iOS stores for any native Facebook accounts. You can safely call this at app launch or when logging a user out.
    public static void renewCredentialsForAllFacebookAccounts()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookRenewCredentialsForAllFacebookAccounts();
    }	

	
    [DllImport("__Internal")]
    private static extern bool _facebookIsLoggedIn();
 
	// Checks to see if the current session is valid
    public static bool isSessionValid()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _facebookIsLoggedIn();
		return false;
    }
    
    
	[DllImport("__Internal")]
	private static extern string _facebookGetFacebookAccessToken();
	
	// Gets the current access token
	public static string getAccessToken()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _facebookGetFacebookAccessToken();

		return string.Empty;
	}
	
	
	[DllImport("__Internal")]
	private static extern string _facebookGetSessionPermissions();
	
	// Gets the permissions granted to the current access token
	public static List<object> getSessionPermissions()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			var permissions = _facebookGetSessionPermissions();
			return permissions.listFromJson();
		}

		return new List<object>();
	}

	
    [DllImport("__Internal")]
    private static extern void _facebookLoginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( string perms, string urlSchemeSuffix );
	
	// Shows the native authorization dialog (iOS 6), opens the Facebook single sign on login in Safari or the official Facebook app with the requested read (not publish!) permissions
	[System.Obsolete( "Note that this auth flow has been deprecated by Facebook and could be removed at any time at Facebook's discretion" )]
	public static void loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( string[] permissions )
	{
#pragma warning disable 0618
		loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( permissions, null );
#pragma warning restore 0618
	}
	
	[System.Obsolete( "Note that this auth flow has been deprecated by Facebook and could be removed at any time at Facebook's discretion" )]
    public static void loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( string[] permissions, string urlSchemeSuffix )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			var permissionsString = string.Join( ",", permissions );
			_facebookLoginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( permissionsString, urlSchemeSuffix );
		}
    }

	
	// Opens the Facebook single sign on login in Safari, the official Facebook app or uses iOS 6 Accounts if available
    public static void login()
    {
        loginWithReadPermissions( new string[] {} );
    }

    public static void loginWithReadPermissions( string[] permissions )
    {
        loginWithReadPermissions( permissions, null );
    }


    [DllImport("__Internal")]
    private static extern void _facebookLoginWithRequestedPermissions( string perms, string urlSchemeSuffix );
	
	// Shows the native authorization dialog (iOS 6), opens the Facebook single sign on login in Safari or the official Facebook app with the requested read (not publish!) permissions
    public static void loginWithReadPermissions( string[] permissions, string urlSchemeSuffix )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			string permissionsString = null;
			if( permissions == null || permissions.Length == 0 )
				permissionsString = string.Empty;
			else
				permissionsString = string.Join( ",", permissions );
			
			_facebookLoginWithRequestedPermissions( permissionsString, urlSchemeSuffix );
		}
    }

	
    [DllImport("__Internal")]
    private static extern void _facebookReauthorizeWithReadPermissions( string perms );
	
	// Reauthorizes with the requested read permissions
    public static void reauthorizeWithReadPermissions( string[] permissions )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			var permissionsString = string.Join( ",", permissions );
			_facebookReauthorizeWithReadPermissions( permissionsString );
		}
    }

	
	[DllImport("__Internal")]
    private static extern void _facebookReauthorizeWithPublishPermissions( string perms, int defaultAudience );
	
	// Reauthorizes with the requested publish permissions and audience
    public static void reauthorizeWithPublishPermissions( string[] permissions, FacebookSessionDefaultAudience defaultAudience )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			var permissionsString = string.Join( ",", permissions );
			_facebookReauthorizeWithPublishPermissions( permissionsString, (int)defaultAudience );
		}
    }

	
    [DllImport("__Internal")]
    private static extern void _facebookLogout();
 
	// Logs the user out and invalidates the token
    public static void logout()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookLogout();
		
		Facebook.instance.accessToken = string.Empty;
    }
	
	
    [DllImport("__Internal")]
    private static extern void _facebookShowDialog( string dialogType, string json );
 
	// Full access to any existing or new Facebook dialogs that get added.  See Facebooks documentation for parameters and dialog types
    public static void showDialog( string dialogType, Dictionary<string,string> options )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookShowDialog( dialogType, options.toJson() );
    }

	
    [DllImport("__Internal")]
    private static extern void _facebookRestRequest( string restMethod, string httpMethod, string jsonDict );
 
	// Allows you to use any available Facebook REST API method
    public static void restRequest( string restMethod, string httpMethod, Hashtable keyValueHash )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			// convert the Hashtable to JSON
			string jsonDict = keyValueHash.toJson();
			if( jsonDict != null )
				_facebookRestRequest( restMethod, httpMethod, jsonDict );
		}
    }
	
	
    [DllImport("__Internal")]
    private static extern void _facebookGraphRequest( string graphPath, string httpMethod, string jsonDict );
 
	// Allows you to use any available Facebook Graph API method
    public static void graphRequest( string graphPath, string httpMethod, Hashtable keyValueHash )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			// convert the Hashtable to JSON
			string jsonDict = keyValueHash.toJson();
			if( jsonDict != null )
				_facebookGraphRequest( graphPath, httpMethod, jsonDict );
		}
    }
	

	#region iOS6 Facebook composer
	
	[DllImport("__Internal")]
	private static extern bool _facebookIsFacebookComposerSupported();

	// Facebook Composer methods
	public static bool isFacebookComposerSupported()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _facebookIsFacebookComposerSupported();

		return false;
	}


	[DllImport("__Internal")]
	private static extern bool _facebookCanUserUseFacebookComposer();

	// Checks to see if the user is using a version of iOS that supports the Facebook composer and if they have an account setup
	public static bool canUserUseFacebookComposer()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _facebookCanUserUseFacebookComposer();

		return false;
	}


	[DllImport("__Internal")]
	private static extern void _facebookShowFacebookComposer( string message, string imagePath, string link );
	
	public static void showFacebookComposer( string message )
	{
		showFacebookComposer( message, null, null );
	}
	
	
	// Shows the Facebook composer with optional image path and link
	public static void showFacebookComposer( string message, string imagePath, string link )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_facebookShowFacebookComposer( message, imagePath, link );
	}
	
	#endregion

}
#endif