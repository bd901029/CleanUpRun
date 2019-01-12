using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prime31;



#if UNITY_IPHONE
public class TwitterBinding
{
    [DllImport("__Internal")]
    private static extern void _twitterInit( string consumerKey, string consumerSecret );
 
	// Initializes the Twitter plugin and sets up the required oAuth information
    public static void init( string consumerKey, string consumerSecret )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterInit( consumerKey, consumerSecret );
    }
	
	
    [DllImport("__Internal")]
    private static extern bool _twitterIsLoggedIn();
 
	// Checks to see if there is a currently logged in user
    public static bool isLoggedIn()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _twitterIsLoggedIn();
		return false;
    }
	
	
    [DllImport("__Internal")]
    private static extern string _twitterLoggedInUsername();
 
	// Retuns the currently logged in user's username
    public static string loggedInUsername()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _twitterLoggedInUsername();
		return string.Empty;
    }
	

    [DllImport("__Internal")]
    private static extern void _twitterLogin( string username, string password );
 
	// Logs in the user using xAuth
    public static void login( string username, string password )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterLogin( username, password );
    }
	

    [DllImport("__Internal")]
    private static extern void _twitterShowOauthLoginDialog();
 
	// Logs in the user using oAuth which request displaying the login prompt via an in app browser
    public static void showOauthLoginDialog()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterShowOauthLoginDialog();
    }

	
    [DllImport("__Internal")]
    private static extern void _twitterLogout();
 
	// Logs out the current user
    public static void logout()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterLogout();
    }


    [DllImport("__Internal")]
    private static extern void _twitterPostStatusUpdate( string status );
 
	// Posts the status text.  Be sure status text is less than 140 characters!
    public static void postStatusUpdate( string status )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterPostStatusUpdate( status );
    }


    [DllImport("__Internal")]
    private static extern void _twitterPostStatusUpdateWithImage( string status, string imagePath );
 
	// Posts the status text and an image.  Note that the url will be appended onto the tweet so you don't have the full 140 characters
    public static void postStatusUpdate( string status, string pathToImage )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterPostStatusUpdateWithImage( status, pathToImage );
    }
    
    
    [DllImport("__Internal")]
    private static extern void _twitterGetHomeTimeline();
 
	// Receives tweets from the users home timeline
    public static void getHomeTimeline()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterGetHomeTimeline();
    }
	
	
	[DllImport("__Internal")]
    private static extern void _twitterPerformRequest( string methodType, string path, string parameters );
	
	// Performs a request for any available Twitter API methods.  methodType must be either "get" or "post".  path is the
	// url fragment from the API docs (excluding https://api.twitter.com) and parameters is a dictionary of key/value pairs
	// for the given method.  Path must reqeust .json!  See Twitter's API docs for all available methods.
	public static void performRequest( string methodType, string path, Dictionary<string,string> parameters )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterPerformRequest( methodType, path, parameters != null ? parameters.toJson() : null );
	}
	
	
	#region iOS 5 Tweet Sheet methods
	
    [DllImport("__Internal")]
    private static extern bool _twitterIsTweetSheetSupported();
 
	// Checks to see if the current iOS version supports the tweet sheet
    public static bool isTweetSheetSupported()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _twitterIsTweetSheetSupported();
		return false;
    }
	
	
    [DllImport("__Internal")]
    private static extern bool _twitterCanUserTweet();
 
	// Checks to see if a user can tweet (are they logged in with a Twitter account)?
    public static bool canUserTweet()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _twitterCanUserTweet();
		return false;
    }
	
	
    [DllImport("__Internal")]
    private static extern void _twitterShowTweetComposer( string status, string imagePath, string url );
 
	public static void showTweetComposer( string status )
	{
		showTweetComposer( status, null, null );
	}
	
	public static void showTweetComposer( string status, string pathToImage )
	{
		showTweetComposer( status, pathToImage, null );
	}
	
	// Shows the tweet composer with the status message and optional image and link
    public static void showTweetComposer( string status, string pathToImage, string link )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_twitterShowTweetComposer( status, pathToImage, link );
    }
	
	#endregion
	
}
#endif