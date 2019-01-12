using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;


public class FlurryManager : AbstractManager
{
	// Fired when an ad space dismisses the ad
	public static event Action<string> spaceDidDismissEvent;

	// Fired when an ad touch will leave the app
	public static event Action<string> spaceWillLeaveApplicationEvent;

	// Fired when an ad space fails to render
	public static event Action<string> spaceDidFailToRenderEvent;
	
	// Fired when an ad space fails to download an ad
	public static event Action<string> spaceDidFailToReceiveAdEvent;
	
	// Fired when an ad space receives an ad
	public static event Action<string> spaceDidReceiveAdEvent;


	static FlurryManager()
	{
		AbstractManager.initialize( typeof( FlurryManager ) );
	}


	public void spaceDidDismiss( string space )
	{
		if( spaceDidDismissEvent != null )
			spaceDidDismissEvent( space );
	}


	public void spaceWillLeaveApplication( string space )
	{
		if( spaceWillLeaveApplicationEvent != null )
			spaceWillLeaveApplicationEvent( space );
	}


	public void spaceDidFailToRender( string space )
	{
		if( spaceDidFailToRenderEvent != null )
			spaceDidFailToRenderEvent( space );
	}
	
	
	public void spaceDidFailToReceiveAd( string space )
	{
		if( spaceDidFailToReceiveAdEvent != null )
			spaceDidFailToReceiveAdEvent( space );
	}
	
	
	public void spaceDidReceiveAd( string space )
	{
		if( spaceDidReceiveAdEvent != null )
			spaceDidReceiveAdEvent( space );
	}
}

