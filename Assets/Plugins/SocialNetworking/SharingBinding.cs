using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prime31;


#if UNITY_IPHONE
public class SharingBinding
{
    [DllImport("__Internal")]
    private static extern void _sharingShareItems( string items, string excludedActivityTypes );

	// Shows the share sheet with the given items. Items can be text, urls or full and proper paths to sharable files
    public static void shareItems( string[] items )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_sharingShareItems( Json.jsonEncode( items ), null );
    }
	
	// Shows the share sheet with the given items with a list of excludedActivityTypes. See Apple's docs for more information on excludedActivityTypes.
    public static void shareItems( string[] items, string[] excludedActivityTypes )
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
			_sharingShareItems( Json.jsonEncode( items ), Json.jsonEncode( excludedActivityTypes ) );
    }
}
#endif