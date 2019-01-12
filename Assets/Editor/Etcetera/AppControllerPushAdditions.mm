//
//  AppControllerPushAdditions.m
//  EtceteraTest
//
//  Created by Mike on 10/5/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "AppControllerPushAdditions.h"
#import "EtceteraManager.h"
#import "JSONKit.h"


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnitySendDeviceToken( NSData* deviceToken );
void UnitySendRemoteNotification( NSDictionary* notification );
void UnitySendRemoteNotificationError( NSError* error );


@implementation AppController(PushAdditions)


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - Class methods

+ (void)load
{
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidFinishLaunchingNotification:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}


+ (void)applicationDidFinishLaunchingNotification:(NSNotification*)note
{
	if( note.userInfo )
	{
		NSDictionary *remoteNotificationDictionary = [note.userInfo objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];
		if( remoteNotificationDictionary )
		{
			NSLog( @"launched with remote notification: %@", remoteNotificationDictionary );
			AppController *appCon = (AppController*)[UIApplication sharedApplication].delegate;
			[appCon performSelector:@selector(handleNotification:) withObject:remoteNotificationDictionary afterDelay:5];
		}

		NSDictionary *localNotificationDict = [note.userInfo objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
		if( localNotificationDict )
		{
			NSLog( @"launched with local notification: %@", localNotificationDict );
			//AppController *appCon = (AppController*)[UIApplication sharedApplication].delegate;
			//[appCon performSelector:@selector(handleNotification:) withObject:remoteNotificationDictionary afterDelay:5];
		}
	}
}


+ (void)registerForRemoteNotificationTypes:(NSNumber*)types
{
	[[UIApplication sharedApplication] registerForRemoteNotificationTypes:[types intValue]];
}


+ (NSNumber*)enabledRemoteNotificationTypes
{
	int val = [[UIApplication sharedApplication] enabledRemoteNotificationTypes];
	return [NSNumber numberWithInt:val];
}


// From: http://www.cocoadev.com/index.pl?BaseSixtyFour
- (NSString*)base64forData:(NSData*)theData
{
    const uint8_t *input = (const uint8_t*)[theData bytes];
    NSInteger length = [theData length];

    static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

    NSMutableData *data = [NSMutableData dataWithLength:((length + 2) / 3) * 4];
    uint8_t *output = (uint8_t*)data.mutableBytes;

    NSInteger i;
    for( i = 0; i < length; i += 3 )
	{
        NSInteger value = 0;
        NSInteger j;
        for( j = i; j < (i + 3); j++ )
		{
            value <<= 8;

            if( j < length )
                value |= (0xFF & input[j]);
        }

        NSInteger theIndex = (i / 3) * 4;
        output[theIndex + 0] =                    table[(value >> 18) & 0x3F];
        output[theIndex + 1] =                    table[(value >> 12) & 0x3F];
        output[theIndex + 2] = (i + 1) < length ? table[(value >> 6)  & 0x3F] : '=';
        output[theIndex + 3] = (i + 2) < length ? table[(value >> 0)  & 0x3F] : '=';
    }

    return [[[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding] autorelease];
}


- (void)handleNotification:(NSDictionary*)dict
{
	[self handleNotification:dict isLaunchNotification:NO];
}


- (void)handleNotification:(NSDictionary*)dict isLaunchNotification:(BOOL)isLaunchNotification
{
	NSDictionary *aps = [dict objectForKey:@"aps"];
	if( !aps )
		return;

	NSString *json = [aps JSONString];

	const char * managerMethod = isLaunchNotification ? "remoteNotificationWasReceivedAtLaunch" : "remoteNotificationWasReceived";

	if( json )
		UnitySendMessage( "EtceteraManager", managerMethod, json.UTF8String );
	else
		UnitySendMessage( "EtceteraManager", managerMethod, "" );
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIApplicationDelegate

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
	UnitySendDeviceToken( deviceToken );

	NSString *deviceTokenString = [[[[deviceToken description]
									 stringByReplacingOccurrencesOfString:@"<" withString:@""]
									stringByReplacingOccurrencesOfString:@">" withString:@""]
								   stringByReplacingOccurrencesOfString:@" " withString:@""];

	UnitySendMessage( "EtceteraManager", "remoteRegistrationDidSucceed", [deviceTokenString UTF8String] );

	// If this is a user deregistering for notifications, dont proceed past this point
	if( [[UIApplication sharedApplication] enabledRemoteNotificationTypes] == UIRemoteNotificationTypeNone )
	{
		NSLog( @"Notifications are disabled for this application. Not registering with Urban Airship" );
		return;
	}

	// Grab the Urban Airship info from the info.plist file
	NSString *appKey = [EtceteraManager sharedManager].urbanAirshipAppKey;
	NSString *appSecret = [EtceteraManager sharedManager].urbanAirshipAppSecret;
	NSString *alias = [EtceteraManager sharedManager].urbanAirshipAlias;

	if( !appKey || !appSecret )
		return;

    // Register the deviceToken with Urban Airship
    NSString *UAServer = @"https://go.urbanairship.com";
    NSString *urlString = [NSString stringWithFormat:@"%@%@%@/", UAServer, @"/api/device_tokens/", deviceTokenString];
    NSURL *url = [NSURL URLWithString:urlString];

    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:url];
    [request setHTTPMethod:@"PUT"];

	// handle the alias if we are sending one
	if( alias )
	{
		[request setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
		NSDictionary *dict = [NSDictionary dictionaryWithObject:alias forKey:@"alias"];
		[request setHTTPBody:[dict JSONData]];
	}

    // Authenticate to the server
    [request addValue:[NSString stringWithFormat:@"Basic %@",
                       [self base64forData:[[NSString stringWithFormat:@"%@:%@",
											 appKey,
											 appSecret] dataUsingEncoding: NSUTF8StringEncoding]]] forHTTPHeaderField:@"Authorization"];

    [[NSURLConnection connectionWithRequest:request delegate:self] start];
	[request release];
}


- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
	UnitySendRemoteNotificationError( error );

	UnitySendMessage( "EtceteraManager", "remoteRegistrationDidFail", [[error localizedDescription] UTF8String] );
	NSLog( @"remoteRegistrationDidFail: %@", error );
}


- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
	UnitySendRemoteNotification( userInfo );

	if( [UIApplication sharedApplication].applicationState == UIApplicationStateInactive )
    {
        [self handleNotification:userInfo isLaunchNotification:YES];
    }
    else
    {
        [self handleNotification:userInfo isLaunchNotification:NO];
    }
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSURLConnection

- (void)connection:(NSURLConnection*)theConnection didReceiveResponse:(NSURLResponse*)response
{
	UnitySendMessage( "EtceteraManager", "urbanAirshipRegistrationDidSucceed", "" );

    NSLog( @"registered with UA: %@, %d",
		  [(NSHTTPURLResponse*)response allHeaderFields],
          [(NSHTTPURLResponse*)response statusCode] );
}


- (void)connection:(NSURLConnection*)theConnection didFailWithError:(NSError*)error
{
	UnitySendMessage( "EtceteraManager", "urbanAirshipRegistrationDidFail", [[error localizedDescription] UTF8String] );
	NSLog( @"Failed to register with UA: %@", error );
}


@end





@implementation UIImage(OrientationAdditions)

- (UIImage*)imageWithImageDataMatchingOrientation
{
    // no-op if the orientation is already correct
    if( self.imageOrientation == UIImageOrientationUp )
		return self;

    // We need to calculate the proper transformation to make the image upright.
    // We do it in 2 steps: Rotate if Left/Right/Down, and then flip if Mirrored.
    CGAffineTransform transform = CGAffineTransformIdentity;

    switch( self.imageOrientation )
	{
        case UIImageOrientationDown:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate( transform, self.size.width, self.size.height );
            transform = CGAffineTransformRotate( transform, M_PI );
            break;

        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
            transform = CGAffineTransformTranslate( transform, self.size.width, 0 );
            transform = CGAffineTransformRotate( transform, M_PI_2 );
            break;

        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate( transform, 0, self.size.height );
            transform = CGAffineTransformRotate( transform, -M_PI_2 );
            break;
    }

    switch( self.imageOrientation )
	{
        case UIImageOrientationUpMirrored:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate( transform, self.size.width, 0 );
            transform = CGAffineTransformScale( transform, -1, 1 );
            break;

        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate( transform, self.size.height, 0 );
            transform = CGAffineTransformScale( transform, -1, 1 );
            break;
    }

    // Now we draw the underlying CGImage into a new context, applying the transform calculated above.
    CGContextRef ctx = CGBitmapContextCreate( NULL, self.size.width, self.size.height,
                                             CGImageGetBitsPerComponent( self.CGImage ), 0,
                                             CGImageGetColorSpace( self.CGImage ),
                                             CGImageGetBitmapInfo( self.CGImage ) );
    CGContextConcatCTM( ctx, transform );
    switch( self.imageOrientation )
	{
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            CGContextDrawImage( ctx, CGRectMake( 0, 0, self.size.height, self.size.width ), self.CGImage );
            break;

        default:
            CGContextDrawImage( ctx, CGRectMake( 0, 0, self.size.width, self.size.height ), self.CGImage );
            break;
    }

    // And now we just create a new UIImage from the drawing context
    CGImageRef cgimg = CGBitmapContextCreateImage(ctx);
    UIImage *img = [UIImage imageWithCGImage:cgimg];
    CGContextRelease(ctx);
    CGImageRelease(cgimg);

    return img;
}

@end


