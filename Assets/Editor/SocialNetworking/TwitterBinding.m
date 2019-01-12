//
//  TwitterBinding.m
//  SocialNetworking
//
//  Created by Mike on 9/18/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "TwitterManager.h"
#import "JSONKit.h"


// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil


void _twitterInit( const char * consumerKey, const char * consumerSecret )
{
	[TwitterManager sharedManager].consumerKey = GetStringParam( consumerKey );
	[TwitterManager sharedManager].consumerSecret = GetStringParam( consumerSecret );
}


bool _twitterIsLoggedIn()
{
	return [[TwitterManager sharedManager] isLoggedIn];
}


const char * _twitterLoggedInUsername()
{
	NSString *username = [[TwitterManager sharedManager] loggedInUsername];
	return MakeStringCopy( username );
}


void _twitterLogin( const char * username, const char * password )
{
	[[TwitterManager sharedManager] xAuthLoginWithUsername:GetStringParam( username ) password:GetStringParam( password )];
}


void _twitterShowOauthLoginDialog()
{
	[[TwitterManager sharedManager] showOauthLoginDialog];
}


void _twitterLogout()
{
	[[TwitterManager sharedManager] logout];
}


void _twitterPostStatusUpdate( const char * status )
{
	[[TwitterManager sharedManager] postStatusUpdate:GetStringParam( status )];
}


void _twitterPostStatusUpdateWithImage( const char * status, const char * imagePath )
{
	NSString *path = GetStringParam( imagePath );
	if( ![[NSFileManager defaultManager] fileExistsAtPath:path] )
	{
		NSLog( @"image file does not exist at path: %@", path );
		return;
	}
	
	[[TwitterManager sharedManager] postStatusUpdate:GetStringParam( status ) withImageAtPath:path];
}


void _twitterGetHomeTimeline()
{
	[[TwitterManager sharedManager] getHomeTimeline];
}


void _twitterPerformRequest( const char * methodType, const char * path, const char * parameters )
{
	NSDictionary *dict = nil;
	NSString *params = GetStringParamOrNil( parameters );
	if( params )
		dict = [params objectFromJSONString];
	
	[[TwitterManager sharedManager] performRequest:GetStringParam( methodType ) path:GetStringParam( path ) params:dict];
}


// iOS 5 Tweet sheet

bool _twitterIsTweetSheetSupported()
{
	return [TwitterManager isTweetSheetSupported];
}


bool _twitterCanUserTweet()
{
	return [TwitterManager userCanTweet];
}


void _twitterShowTweetComposer( const char * status, const char * imagePath, const char * link )
{
	NSString *path = GetStringParamOrNil( imagePath );
	UIImage *image = nil;
	if( [[NSFileManager defaultManager] fileExistsAtPath:path] )
		image = [UIImage imageWithContentsOfFile:path];
	
	[[TwitterManager sharedManager] showTweetComposerWithMessage:GetStringParam( status ) image:image link:GetStringParamOrNil( link )];
}



