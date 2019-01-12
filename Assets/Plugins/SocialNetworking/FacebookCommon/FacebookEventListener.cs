using UnityEngine;
using System.Collections;


public class FacebookEventListener : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID
	// Listens to all the events.  All event listeners MUST be removed before this object is disposed!
	void OnEnable()
	{
		FacebookManager.sessionOpenedEvent += sessionOpenedEvent;
		FacebookManager.loginFailedEvent += loginFailedEvent;
		
		FacebookManager.dialogCompletedWithUrlEvent += dialogCompletedEvent;
		FacebookManager.dialogFailedEvent += dialogFailedEvent;
		FacebookManager.dialogCompletedEvent += facebokDialogCompleted;
		FacebookManager.dialogDidNotCompleteEvent += dialogDidNotCompleteEvent;
		
		FacebookManager.graphRequestCompletedEvent += graphRequestCompletedEvent;
		FacebookManager.graphRequestFailedEvent += facebookCustomRequestFailed;
		FacebookManager.restRequestCompletedEvent += restRequestCompletedEvent;
		FacebookManager.restRequestFailedEvent += restRequestFailedEvent;
		FacebookManager.facebookComposerCompletedEvent += facebookComposerCompletedEvent;
		
		FacebookManager.reauthorizationFailedEvent += reauthorizationFailedEvent;
		FacebookManager.reauthorizationSucceededEvent += reauthorizationSucceededEvent;
	}

	
	void OnDisable()
	{
		// Remove all the event handlers when disabled
		FacebookManager.sessionOpenedEvent -= sessionOpenedEvent;
		FacebookManager.loginFailedEvent -= loginFailedEvent;

		FacebookManager.dialogCompletedEvent -= facebokDialogCompleted;
		FacebookManager.dialogCompletedWithUrlEvent -= dialogCompletedEvent;
		FacebookManager.dialogDidNotCompleteEvent -= dialogDidNotCompleteEvent;
		FacebookManager.dialogFailedEvent -= dialogFailedEvent;
		
		FacebookManager.graphRequestCompletedEvent -= graphRequestCompletedEvent;
		FacebookManager.graphRequestFailedEvent -= facebookCustomRequestFailed;
		FacebookManager.restRequestCompletedEvent -= restRequestCompletedEvent;
		FacebookManager.restRequestFailedEvent -= restRequestFailedEvent;
		FacebookManager.facebookComposerCompletedEvent -= facebookComposerCompletedEvent;
		
		FacebookManager.reauthorizationFailedEvent -= reauthorizationFailedEvent;
		FacebookManager.reauthorizationSucceededEvent -= reauthorizationSucceededEvent;
	}

	

	void sessionOpenedEvent()
	{
		Debug.Log( "Successfully logged in to Facebook" );
	}
	
	
	void loginFailedEvent( string error )
	{
		Debug.Log( "Facebook login failed: " + error );
	}


	void dialogCompletedEvent( string url )
	{
		Debug.Log( "dialogCompletedEvent: " + url );
	}
	
	
	void dialogFailedEvent( string error )
	{
		Debug.Log( "dialogFailedEvent: " + error );
	}
	
	
	void facebokDialogCompleted()
	{
		Debug.Log( "facebokDialogCompleted" );
	}

	
	void dialogDidNotCompleteEvent()
	{
		Debug.Log( "facebookDialogDidntComplete" );
	}

	
	void graphRequestCompletedEvent( object obj )
	{
		Debug.Log( "graphRequestCompletedEvent" );
		Prime31.Utils.logObject( obj );
	}
	
	
	void facebookCustomRequestFailed( string error )
	{
		Debug.Log( "facebookCustomRequestFailed failed: " + error );
	}
	
	
	void restRequestCompletedEvent( object obj )
	{
		Debug.Log( "restRequestCompletedEvent" );
		Prime31.Utils.logObject( obj );
	}
	
	
	void restRequestFailedEvent( string error )
	{
		Debug.Log( "restRequestFailedEvent failed: " + error );
	}
	
	
	void facebookComposerCompletedEvent( bool didSucceed )
	{
		Debug.Log( "facebookComposerCompletedEvent did succeed: " + didSucceed );
	}


	void reauthorizationSucceededEvent()
	{
		Debug.Log( "reauthorizationSucceededEvent" );
	}
	
	
	void reauthorizationFailedEvent( string error )
	{
		Debug.Log( "reauthorizationFailedEvent: " + error );
	}
	
#endif
}
