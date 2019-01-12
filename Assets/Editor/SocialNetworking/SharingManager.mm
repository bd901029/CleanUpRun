//
//  SharingManager.mm
//  Unity-iPhone
//
//  Created by Mike Desaro on 1/28/13.
//
//

#import "SharingManager.h"


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool pause );
UIViewController *UnityGetGLViewController();


@implementation SharingManager

+ (SharingManager*)sharedManager
{
	static dispatch_once_t pred;
	static SharingManager *_sharedInstance = nil;
	
	dispatch_once( &pred, ^{ _sharedInstance = [[self alloc] init]; } );
	return _sharedInstance;
}


+ (NSObject*)objectFromJsonString:(NSString*)json
{
	NSError *error = nil;
	NSData *data = [NSData dataWithBytes:json.UTF8String length:json.length];
    NSObject *object = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingAllowFragments error:&error];
	
	if( error )
		NSLog( @"failed to deserialize JSON: %@ with error: %@", json, [error localizedDescription] );
    
    return object;
}


+ (void)shareItems:(NSArray*)items excludedActivityTypes:(NSArray*)excludedActivityTypes
{
	if( !NSClassFromString( @"UIActivityViewController" ) )
	{
		NSLog( @"Not running on iOS 6 or later so sharing disabled" );
		return;
	}
	
	UnityPause( true );
	
	UIActivityViewController *activityController = [[[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil] autorelease];
	activityController.excludedActivityTypes = excludedActivityTypes;
	activityController.completionHandler = ^( NSString *activityType, BOOL completed )
	{
		UnityPause( false );
		
		if( completed )
			UnitySendMessage( "SharingManager", "sharingFinishedWithActivityType", activityType.UTF8String );
		else
			UnitySendMessage( "SharingManager", "sharingCancelled", "" );
	};
	[UnityGetGLViewController() presentModalViewController:activityController animated:YES];
}

@end
