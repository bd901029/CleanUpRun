//
//  FacebookBinding.m
//  Facebook
//
//  Created by Mike on 9/13/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "FacebookManager.h"
#import "JSONKit.h"
#import "FBSession.h"


// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil


void _facebookInit()
{
	// check for a URL scheme
	NSDictionary *dict = [[NSBundle mainBundle] infoDictionary];
	if( ![[dict allKeys] containsObject:@"CFBundleURLTypes"] )
		NSLog( @"ERROR: You have not setup a URL scheme. Authentication via Safari or the Facebook.app will not work" );
	
	[[FacebookManager sharedManager] startSessionQuietly];
}


const char * _facebookGetAppLaunchUrl()
{
	NSString *url = [FacebookManager sharedManager].appLaunchUrl;
	if( url )
		return MakeStringCopy( url );
	return MakeStringCopy( @"" );
}


void _facebookSetSessionLoginBehavior( int behavior )
{
	FBSessionLoginBehavior loginBehavior = (FBSessionLoginBehavior)behavior;
	[FacebookManager sharedManager].loginBehavior = loginBehavior;
}


void _facebookRenewCredentialsForAllFacebookAccounts()
{
	[[FacebookManager sharedManager] renewCredentialsForAllFacebookAccounts];
}


bool _facebookIsLoggedIn()
{
	return [[FacebookManager sharedManager] isLoggedIn];
}


const char * _facebookGetFacebookAccessToken()
{
	return MakeStringCopy( [[FacebookManager sharedManager] accessToken] );
}


const char * _facebookGetSessionPermissions()
{
	NSString *permissionsString = [[FacebookManager sharedManager] sessionPermissions].JSONString;
	return MakeStringCopy( permissionsString );
}


void _facebookLoginUsingDeprecatedAuthorizationFlowWithRequestedPermissions( const char * perms, const char * urlSchemeSuffix )
{
	NSString *permsString = GetStringParam( perms );
	NSMutableArray *permissions = nil;
	
	if( permsString.length == 0 )
		permissions = [NSArray array];
	else
		permissions = [[[permsString componentsSeparatedByString:@","] mutableCopy] autorelease];
	
	[[FacebookManager sharedManager] loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions:permissions
												   urlSchemeSuffix:GetStringParam( urlSchemeSuffix )];
}


void _facebookLoginWithRequestedPermissions( const char * perms, const char * urlSchemeSuffix )
{
	NSString *permsString = GetStringParam( perms );
	NSMutableArray *permissions = nil;
	
	if( permsString.length == 0 )
		permissions = [NSArray array];
	else
		permissions = [[[permsString componentsSeparatedByString:@","] mutableCopy] autorelease];
	
	[[FacebookManager sharedManager] loginWithRequestedPermissions:permissions
												   urlSchemeSuffix:GetStringParam( urlSchemeSuffix )];
}


void _facebookReauthorizeWithReadPermissions( const char * perms )
{
	NSString *permsString = GetStringParam( perms );
	NSMutableArray *permissions = [[[permsString componentsSeparatedByString:@","] mutableCopy] autorelease];
	
	[[FacebookManager sharedManager] reauthorizeWithReadPermissions:permissions];
}


void _facebookReauthorizeWithPublishPermissions( const char * perms, int defaultAudience )
{
	NSString *permsString = GetStringParam( perms );
	NSMutableArray *permissions = [[[permsString componentsSeparatedByString:@","] mutableCopy] autorelease];
	
	[[FacebookManager sharedManager] reauthorizeWithPublishPermissions:permissions defaultAudience:(FBSessionDefaultAudience)defaultAudience];
}


void _facebookLogout()
{
	[[FacebookManager sharedManager] logout];
}


void _facebookShowDialog( const char * dialogType, const char * json )
{
	// make sure we have a legit dictionary
	NSString *jsonString = GetStringParamOrNil( json );
	NSMutableDictionary *dict = nil;
	
	if( jsonString && [jsonString isKindOfClass:[NSString class]] && jsonString.length )
		dict = [[jsonString objectFromJSONString] mutableCopy];
	
	[[FacebookManager sharedManager] showDialog:GetStringParam( dialogType ) withParms:dict];
}


void _facebookRestRequest( const char * restMethod, const char * httpMethod, const char * jsonDict )
{
	// make sure we have a legit dictionary
	NSString *jsonString = GetStringParam ( jsonDict );
	NSMutableDictionary *dict = [jsonString mutableObjectFromJSONString];
	
	if( ![dict isKindOfClass:[NSMutableDictionary class]] )
		return;
	
	[[FacebookManager sharedManager] requestWithRestMethod:GetStringParam( restMethod )
												httpMethod:GetStringParam( httpMethod )
													params:dict];
}


void _facebookGraphRequest( const char * graphPath, const char * httpMethod, const char * jsonDict )
{
	// make sure we have a legit dictionary
	NSString *jsonString = GetStringParam ( jsonDict );
	NSMutableDictionary *dict = [jsonString objectFromJSONString];
	
	if( ![dict isKindOfClass:[NSMutableDictionary class]] )
		return;
	
	[[FacebookManager sharedManager] requestWithGraphPath:GetStringParam( graphPath )
											   httpMethod:GetStringParam( httpMethod )
												   params:dict];
}



// Facebook Composer methods
bool _facebookIsFacebookComposerSupported()
{
	return [FacebookManager isFacebookComposerSupported];
}


bool _facebookCanUserUseFacebookComposer()
{
	return [FacebookManager userCanUseFacebookComposer];
}


void _facebookShowFacebookComposer( const char * message, const char * imagePath, const char * link )
{
	NSString *path = GetStringParamOrNil( imagePath );
	UIImage *image = nil;
	if( [[NSFileManager defaultManager] fileExistsAtPath:path] )
		image = [UIImage imageWithContentsOfFile:path];
	
	[[FacebookManager sharedManager] showFacebookComposerWithMessage:GetStringParam( message ) image:image link:GetStringParamOrNil( link )];
}


