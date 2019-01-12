//
//  FacebookManager.m
//  Facebook
//
//  Created by Mike on 9/13/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "FacebookManager.h"
#import <objc/runtime.h>
#import "JSONKit.h"
#import "TwitterManager.h" // to get the USE_UNITY_3_5 define

#import <Accounts/Accounts.h>


NSString* const kFacebookUrlSchemeSuffixKey = @"kFacebookUrlSchemeKey";


void UnitySendMessage( const char * className, const char * methodName, const char * param );

void UnityPause( bool pause );

#if USE_UNITY_3_5
UIViewController *UnityGetGLViewController();
#endif


@implementation FacebookManager

@synthesize facebook, urlSchemeSuffix, appLaunchUrl, loginBehavior;

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (void)load
{
	[[NSNotificationCenter defaultCenter] addObserver:[self sharedManager]
											 selector:@selector(applicationDidFinishLaunching:)
												 name:UIApplicationDidFinishLaunchingNotification
											   object:nil];
	
	[[NSNotificationCenter defaultCenter] addObserver:[self sharedManager]
											 selector:@selector(applicationDidBecomeActive:)
												 name:UIApplicationDidBecomeActiveNotification
											   object:nil];
}


+ (FacebookManager*)sharedManager
{
	static dispatch_once_t pred;
	static FacebookManager *_sharedInstance = nil;
	
	dispatch_once( &pred, ^{ _sharedInstance = [[self alloc] init]; } );
	return _sharedInstance;
}


+ (BOOL)isFacebookComposerSupported
{
	return NSClassFromString( @"SLComposeViewController" ) != nil;
}


+ (BOOL)userCanUseFacebookComposer
{
	Class slComposer = NSClassFromString( @"SLComposeViewController" );
	if( slComposer && [slComposer performSelector:@selector(isAvailableForServiceType:) withObject:@"com.apple.social.facebook"] )
		return YES;
	return NO;
}


- (id)init
{
	if( ( self = [super init] ) )
	{
		self.loginBehavior = FBSessionLoginBehaviorUseSystemAccountIfPresent;
		
		// if we have an appId or urlSchemeSuffix tucked away, set it now
		if( [[NSUserDefaults standardUserDefaults] objectForKey:kFacebookUrlSchemeSuffixKey] )
			self.urlSchemeSuffix = [[NSUserDefaults standardUserDefaults] objectForKey:kFacebookUrlSchemeSuffixKey];
		
		NSDictionary *dict = [[NSBundle mainBundle] infoDictionary];
		if( ![[dict allKeys] containsObject:@"FacebookAppID"] )
		{
			NSLog( @"ERROR: You have not setup your Facebook app ID in the Info.plist file. Not having it in the Info.plist will cause your application to crash." );
			return nil;
		}
	}
	return self;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - NSNotifications

- (void)applicationDidFinishLaunching:(NSNotification*)note
{
	// did we get launched with a userInfo dict?
	if( note.userInfo )
	{
		NSURL *url = [note.userInfo objectForKey:UIApplicationLaunchOptionsURLKey];
		if( url )
		{
			NSLog( @"recovered URL from jettisoned app. going to attempt login" );
			[self handleOpenURL:url];
		}
	}
}


- (void)applicationDidBecomeActive:(NSNotification*)note
{
	[FBSession.activeSession handleDidBecomeActive];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private/Internal

- (BOOL)handleOpenURL:(NSURL*)url
{
	NSLog( @"url used to open app: %@", url );
	self.appLaunchUrl = url.absoluteString;
	BOOL res = [FBSession.activeSession handleOpenURL:url];
	
	return res;
}


- (NSString*)getAppId
{
	return [[[NSBundle mainBundle] infoDictionary] objectForKey:@"FacebookAppID"];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - SLComposer

- (void)showFacebookComposerWithMessage:(NSString*)message image:(UIImage*)image link:(NSString*)link
{
#if USE_UNITY_3_5
	if( ![FacebookManager userCanUseFacebookComposer] )
		return;
	
	Class slComposerClass = NSClassFromString( @"SLComposeViewController" );
	UIViewController *slComposer = [slComposerClass performSelector:@selector(composeViewControllerForServiceType:) withObject:@"com.apple.social.facebook"];
	
	if( !slComposer )
		return;
	
	// Add a message
	[slComposer performSelector:@selector(setInitialText:) withObject:message];
	
	// add an image
	if( image )
		[slComposer performSelector:@selector(addImage:) withObject:image];
	
	// add a link
	if( link )
		[slComposer performSelector:@selector(addURL:) withObject:[NSURL URLWithString:link]];
	
	// set a blocking handler for the tweet sheet
	[slComposer performSelector:@selector(setCompletionHandler:) withObject:^( NSInteger result )
	{
		UnityPause( false );
		[UnityGetGLViewController() dismissModalViewControllerAnimated:YES];
		
		if( result == 1 )
			UnitySendMessage( "FacebookManager", "facebookComposerCompleted", "1" );
		else if( result == 0 )
			UnitySendMessage( "FacebookManager", "facebookComposerCompleted", "0" );
	}];
	
	// Show the tweet sheet
	UnityPause( true );
	[UnityGetGLViewController() presentModalViewController:slComposer animated:YES];
#endif
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (void)renewCredentialsForAllFacebookAccounts
{
	if( !NSClassFromString( @"ACAccountStore" ) )
		return;
	
	if( &ACAccountTypeIdentifierFacebook == NULL )
		return;
	
	ACAccountStore *accountStore = [[ACAccountStore alloc] init];
	
	if( !accountStore )
		return;
	
    ACAccountType *accountTypeFB = [accountStore accountTypeWithAccountTypeIdentifier:ACAccountTypeIdentifierFacebook];
	NSArray *facebookAccounts = [accountStore accountsWithAccountType:accountTypeFB];
	
	if( facebookAccounts.count == 0 )
		return;
	
	for( ACAccount *fbAccount in facebookAccounts )
	{
		[accountStore renewCredentialsForAccount:fbAccount completion:^( ACAccountCredentialRenewResult renewResult, NSError *error )
		{
			if( error )
				NSLog( @"account %@ failed to renew: %@", fbAccount, error );
			else
				NSLog( @"account %@ renewed successfully", fbAccount );
		}];
	}
}


- (void)startSessionQuietly
{
	[FBSettings publishInstall:[self getAppId]];
	[FBSession setDefaultAppID:[self getAppId]];
	
	// create a session manually in case we have a url scheme suffix
	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:nil urlSchemeSuffix:self.urlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];
	
	if( [FBSession openActiveSessionWithAllowLoginUI:NO] )
	{
		self.facebook = [[[Facebook alloc] initWithAppId:FBSession.activeSession.appID andDelegate:nil] autorelease];
		self.facebook.accessToken = FBSession.activeSession.accessToken;
		self.facebook.expirationDate = FBSession.activeSession.expirationDate;
		
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessToken.UTF8String );
	}
}


- (BOOL)isLoggedIn
{
	if( !FBSession.activeSession )
		return NO;
	
	return FBSession.activeSession.isOpen;
}


- (NSString*)accessToken
{
    return FBSession.activeSession.accessToken;
}


- (NSArray*)sessionPermissions
{
	return FBSession.activeSession.permissions;
}


- (void)loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)aUrlSchemeSuffix
{
	// store the url scheme suffix for later use
	self.urlSchemeSuffix = aUrlSchemeSuffix;
	[[NSUserDefaults standardUserDefaults] setObject:self.urlSchemeSuffix forKey:kFacebookUrlSchemeSuffixKey];
	
	if( [self isLoggedIn] )
	{
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessToken.UTF8String );
		return;
	}
	
	// we must have email, user_birthday, or user_location to authorize!
	if( ![permissions containsObject:@"email"] && ![permissions containsObject:@"user_birthday"] && ![permissions containsObject:@"user_location"] )
		[permissions addObject:@"email"];
	
	
	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:permissions urlSchemeSuffix:aUrlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];
	[FBSession openActiveSessionWithPermissions:permissions allowLoginUI:YES completionHandler:^( FBSession *sess, FBSessionState status, NSError *error )
	 {
		 if( FB_ISSESSIONOPENWITHSTATE( status ) )
		 {
			 // setup the old, deprecated Facebook object
			 self.facebook = [[[Facebook alloc] initWithAppId:FBSession.activeSession.appID andDelegate:nil] autorelease];
			 self.facebook.accessToken = FBSession.activeSession.accessToken;
			 self.facebook.expirationDate = FBSession.activeSession.expirationDate;
			 
			 UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessToken.UTF8String );
		 }
		 else
		 {
			 if( status == FBSessionStateClosed )
			 {
				 NSLog( @"session closed" );
				 //UnitySendMessage( "FacebookManager", "facebookDidLogout", "" );
			 }
			 else
			 {
				 NSString *errorString = error.localizedDescription != nil ? error.localizedDescription : @"unknown error";
				 UnitySendMessage( "FacebookManager", "loginFailed", errorString.UTF8String );
			 }
		 }
	 }];
}


- (void)loginWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)aUrlSchemeSuffix
{
	// store the url scheme suffix for later use
	self.urlSchemeSuffix = aUrlSchemeSuffix;
	[[NSUserDefaults standardUserDefaults] setObject:self.urlSchemeSuffix forKey:kFacebookUrlSchemeSuffixKey];
	
	if( [self isLoggedIn] )
	{
		UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessToken.UTF8String );
		return;
	}
	
	
	FBSession *facebookSession = [[FBSession alloc] initWithAppID:[self getAppId] permissions:permissions urlSchemeSuffix:self.urlSchemeSuffix tokenCacheStrategy:nil];
	[FBSession setActiveSession:facebookSession];
	
	// change the behavior here to force different types. the available tyeps are: FBSessionLoginBehaviorUseSystemAccountIfPresent, FBSessionLoginBehaviorWithFallbackToWebView,
	// FBSessionLoginBehaviorWithNoFallbackToWebView, FBSessionLoginBehaviorForcingWebView
	[facebookSession openWithBehavior:self.loginBehavior completionHandler:^( FBSession *sess, FBSessionState status, NSError *error )
	{
		 if( FB_ISSESSIONOPENWITHSTATE( status ) )
		 {
			 // setup the old, deprecated Facebook object
			 self.facebook = [[[Facebook alloc] initWithAppId:FBSession.activeSession.appID andDelegate:nil] autorelease];
			 self.facebook.accessToken = FBSession.activeSession.accessToken;
			 self.facebook.expirationDate = FBSession.activeSession.expirationDate;
			 
			 UnitySendMessage( "FacebookManager", "sessionOpened", FBSession.activeSession.accessToken.UTF8String );
		 }
		 else
		 {
			 if( status == FBSessionStateClosed )
			 {
				 NSLog( @"session closed" );
				 //UnitySendMessage( "FacebookManager", "facebookDidLogout", "" );
			 }
			 else if( error )
			 {
				 NSString *errorString = error.localizedDescription != nil ? error.localizedDescription : @"unknown error";
				 UnitySendMessage( "FacebookManager", "loginFailed", errorString.UTF8String );
			 }
		 }
	 }];
}


- (void)reauthorizeWithReadPermissions:(NSArray*)permissions
{
	[[FBSession activeSession] reauthorizeWithReadPermissions:permissions completionHandler:^( FBSession *session, NSError *error )
	{
		if( error )
			UnitySendMessage( "FacebookManager", "reauthorizationFailed", error.localizedDescription.UTF8String );
		else
			UnitySendMessage( "FacebookManager", "reauthorizationSucceeded", "" );
	}];
}


- (void)reauthorizeWithPublishPermissions:(NSArray*)permissions defaultAudience:(FBSessionDefaultAudience)audience
{
	[[FBSession activeSession] reauthorizeWithPublishPermissions:permissions defaultAudience:audience completionHandler:^( FBSession *session, NSError *error )
	{
		if( error )
			UnitySendMessage( "FacebookManager", "reauthorizationFailed", error.localizedDescription.UTF8String );
		else
			UnitySendMessage( "FacebookManager", "reauthorizationSucceeded", "" );
	}];
}


- (void)logout
{
	[FBSession.activeSession closeAndClearTokenInformation];
	//[FBSession.activeSession close];
}


- (void)showDialog:(NSString*)dialogType withParms:(NSMutableDictionary*)dict
{
	// add the apiKey
	if( !dict )
		dict = [NSMutableDictionary dictionary];
	
	[dict setObject:[self getAppId] forKey:@"api_key"];
	[self.facebook dialog:dialogType andParams:dict andDelegate:self];
}


- (void)requestWithGraphPath:(NSString*)path httpMethod:(NSString*)method params:(NSMutableDictionary*)params
{
	[FBRequestConnection startWithGraphPath:path parameters:params HTTPMethod:method completionHandler:^( FBRequestConnection *conn, id result, NSError *error )
	{
		if( error )
		{
			UnitySendMessage( "FacebookManager", "graphRequestFailed", [[error localizedDescription] UTF8String] );
		}
		else
		{
			NSString *json = nil;
			if( [result isKindOfClass:[NSDictionary class]] )
			{
				NSDictionary *dictionary = (NSDictionary*)result;
				json = [dictionary JSONString];
			}
			else if( [result isKindOfClass:[NSArray class]] )
			{
				NSArray *arr = (NSArray*)result;
				json = [arr JSONString];
			}
			UnitySendMessage( "FacebookManager", "graphRequestCompleted", json.UTF8String );
		}
	}];
}


- (void)requestWithRestMethod:(NSString*)restMethod httpMethod:(NSString*)method params:(NSMutableDictionary*)params
{
	FBRequest *req = [[[FBRequest alloc] initWithSession:FBSession.activeSession restMethod:restMethod parameters:params HTTPMethod:method] autorelease];
	FBRequestConnection *connection = [[FBRequestConnection alloc] init];
	[connection addRequest:req completionHandler:^( FBRequestConnection *conn, id result, NSError *error )
	{
		if( error )
		{
			UnitySendMessage( "FacebookManager", "restRequestFailed", [[error localizedDescription] UTF8String] );
		}
		else
		{
			NSString *json = nil;
			if( [result isKindOfClass:[NSDictionary class]] )
			{
				NSDictionary *dictionary = (NSDictionary*)result;
				json = [dictionary JSONString];
			}
			else if( [result isKindOfClass:[NSArray class]] )
			{
				NSArray *arr = (NSArray*)result;
				json = [arr JSONString];
			}
			UnitySendMessage( "FacebookManager", "restRequestCompleted", json.UTF8String );
		}
		[conn autorelease];
	}];
	[connection start];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark FBDialogDelegate

- (void)dialogDidComplete:(FBDialog*)dialog
{
	UnitySendMessage( "FacebookManager", "dialogCompleted", "" );
}


- (void)dialogCompleteWithUrl:(NSURL*)url
{
	UnitySendMessage( "FacebookManager", "dialogCompletedWithUrl", url.absoluteString.UTF8String );
}


- (void)dialogDidNotComplete:(FBDialog*)dialog
{
	UnitySendMessage( "FacebookManager", "dialogDidNotComplete", "" );
}


- (void)dialog:(FBDialog*)dialog didFailWithError:(NSError*)error
{
	NSLog( @"error description: %@", [error description] );
	NSLog( @"error userInfo: %@", [error userInfo] );
	
	UnitySendMessage( "FacebookManager", "dialogFailedWithError", [[error localizedDescription] UTF8String] );
}


@end





#import "AppController.h"


@implementation AppController(FacebookURLHandler)

- (BOOL)application:(UIApplication*)application handleOpenURL:(NSURL*)url
{
	BOOL res = [[FacebookManager sharedManager] handleOpenURL:url];
	return res;
}


// For iOS 4.2+ support
- (BOOL)application:(UIApplication*)application
			openURL:(NSURL*)url
  sourceApplication:(NSString*)sourceApplication
		 annotation:(id)annotation
{
    BOOL res = [[FacebookManager sharedManager] handleOpenURL:url];
    return res;
}

@end


