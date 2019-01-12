//
//  P31Twitter.m
//  SocialNetworking
//
//  Created by Mike on 9/11/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "TwitterManager.h"
#import "P31MutableOauthRequest.h"
#import "OARequestParameter.h"
#import "JSONKit.h"
#import "AddAccountController.h"
#include <Twitter/Twitter.h>


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool shouldPause );

#if USE_UNITY_3_5
UIViewController *UnityGetGLViewController();
#endif

NSString *const kLoggedInUser = @"kLoggedInUser";


@implementation TwitterManager

@synthesize consumerKey = _consumerKey, consumerSecret = _consumerSecret, payload = _payload;


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (TwitterManager*)sharedManager
{
	static TwitterManager *sharedSingleton;
	
	if( !sharedSingleton )
		sharedSingleton = [[TwitterManager alloc] init];
	
	return sharedSingleton;
}


+ (BOOL)isTweetSheetSupported
{
	return NSClassFromString( @"TWTweetComposeViewController" ) != nil;
}


+ (BOOL)userCanTweet
{
	Class twComposer = NSClassFromString( @"TWTweetComposeViewController" );
	if( twComposer && [twComposer performSelector:@selector(canSendTweet)] )
		return YES;
	return NO;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private (not anymore)

- (NSString*)extractUsernameFromHTTPBody:(NSString*)body
{
	if( !body )
		return nil;
	
	NSArray	*tuples = [body componentsSeparatedByString: @"&"];
	if( tuples.count < 1 )
		return nil;
	
	for( NSString *tuple in tuples )
	{
		NSArray *keyValueArray = [tuple componentsSeparatedByString: @"="];
		
		if( keyValueArray.count == 2 )
		{
			NSString *key = [keyValueArray objectAtIndex: 0];
			NSString *value = [keyValueArray objectAtIndex: 1];
			
			if( [key isEqualToString:@"screen_name"] )
				return value;
		}
	}
	
	return nil;
}


- (void)completeLoginWithResponseData:(NSString*)data
{
	NSString *username = [self extractUsernameFromHTTPBody:data];
	if( !username )
	{
		UnitySendMessage( "TwitterManager", "twitterLoginDidFail", [data UTF8String] );
	}
	else
	{
		// save the token for posting
		[[NSUserDefaults standardUserDefaults] setObject:data forKey:kLoggedInUser];
		[[NSUserDefaults standardUserDefaults] synchronize];
		
		// send success message back to Unity
		UnitySendMessage( "TwitterManager", "twitterLoginSucceeded", "" );
	}
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private

- (UIViewController*)getViewControllerForModalPresentation:(BOOL)destroyIfExists
{
#if USE_UNITY_3_5
	return UnityGetGLViewController();
#else
	
	if( destroyIfExists && _viewControllerWrapper )
	{
		[_viewControllerWrapper dismissModalViewControllerAnimated:NO];
		[_viewControllerWrapper.view removeFromSuperview];
		[_viewControllerWrapper release];
		_viewControllerWrapper = nil;
	}
	else if( !_viewControllerWrapper )
	{
		// Create a wrapper controller to house the picker.  If this is iPad, use a rotating view controller
		if( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
			_viewControllerWrapper = [[UIViewController alloc] initWithNibName:nil bundle:nil];
		else
			_viewControllerWrapper = [[UIViewController alloc] initWithNibName:nil bundle:nil];
		
		// add the wrapper to the window
		[[UIApplication sharedApplication].keyWindow addSubview:_viewControllerWrapper.view];
	}
	
	// zero the frame so it is hidden
	_viewControllerWrapper.view.frame = CGRectZero;
	
	return _viewControllerWrapper;
#endif
}


#ifndef USE_UNITY_3_5
- (void)dismissWrappedViewController
{
	// No view controller? Get out of here.
	if( !_viewControllerWrapper )
		return;

	// cancel the prvious delayed call to dismiss the view controller if it exists
	[NSObject cancelPreviousPerformRequestsWithTarget:self];

	// dismiss the vc
	[_viewControllerWrapper dismissModalViewControllerAnimated:YES];

	// make sure it doesn't eat touches
	_viewControllerWrapper.view.frame = CGRectZero;
	
	// remove the wrapper view controller
	[self performSelector:@selector(removeAndReleaseViewControllerWrapper) withObject:nil afterDelay:1.0];
}


- (void)removeAndReleaseViewControllerWrapper
{
	[_viewControllerWrapper.view removeFromSuperview];
	[_viewControllerWrapper release];
	_viewControllerWrapper = nil;
}
#endif


- (void)showViewControllerModallyInWrapper:(UIViewController*)viewController
{
	// pause the game
	UnityPause( true );
	
	UIViewController *vc = [self getViewControllerForModalPresentation:YES];
	
	// show the mail composer on iPad in a form sheet
	if( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
		viewController.modalPresentationStyle = UIModalPresentationFormSheet;
	
	// show the controller
	[vc presentModalViewController:viewController animated:YES];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (void)unpauseUnity
{
	UnityPause( false );
}


- (BOOL)isLoggedIn
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( tokenString )
		return YES;
	return NO;
}


- (NSString*)loggedInUsername
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
		return @"";
	return [self extractUsernameFromHTTPBody:tokenString];
}


- (void)xAuthLoginWithUsername:(NSString*)username password:(NSString*)password
{
	_requestType = TwitterRequestLogin;
	P31MutableOauthRequest *request = [[P31MutableOauthRequest alloc] initWithUrl:@"https://api.twitter.com/oauth/access_token"
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:nil];
	
	[request setHTTPMethod:@"POST"];


	[request setParameters:[NSArray arrayWithObjects:
							[OARequestParameter requestParameter:@"x_auth_mode" value:@"client_auth"],
							[OARequestParameter requestParameter:@"x_auth_username" value:username],
							[OARequestParameter requestParameter:@"x_auth_password" value:password],
							nil]];

	[request prepareRequest];
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];	
	[request release];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
}


- (void)showOauthLoginDialog
{
    AddAccountController *con = [[AddAccountController alloc] initWithNibName:nil bundle:nil];
    UINavigationController *navCon = [[UINavigationController alloc] initWithRootViewController:con];
    navCon.navigationBar.barStyle = UIBarStyleBlack;
    
    [self showViewControllerModallyInWrapper:navCon];
    [con release];
    [navCon release];
}


- (void)logout
{
	[[NSUserDefaults standardUserDefaults] setObject:nil forKey:kLoggedInUser];
	[[NSUserDefaults standardUserDefaults] synchronize];
}


- (void)postStatusUpdate:(NSString*)status
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "twitterPostDidFail", "User is not logged in" );
		return;
	}
	
	OAToken *accessToken = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	[self postStatusUpdate:status withToken:accessToken];
}


- (void)postStatusUpdate:(NSString*)status withToken:(OAToken*)token
{
	_requestType = TwitterRequestUpdateStatus;
	P31MutableOauthRequest *request = [[P31MutableOauthRequest alloc] initWithUrl:@"https://api.twitter.com/1/statuses/update.json"
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token];
	
	NSString *body = [NSString stringWithFormat:@"status=%@", [status encodedURLString]];
	[request setHTTPMethod:@"POST"];
	[request setHTTPBody:[body dataUsingEncoding:NSUTF8StringEncoding]];
	
	[request prepareRequest];
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];	
	[request release];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
}


- (void)postStatusUpdate:(NSString*)status withImageAtPath:(NSString*)path
{
	// token check
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "twitterPostDidFail", "User is not logged in" );
		return;
	}
	OAToken *token = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	
	// setup the request
	_requestType = TwitterRequestUpdateStatus;
	P31MutableOauthRequest *request = [[P31MutableOauthRequest alloc] initWithUrl:@"https://upload.twitter.com/1/statuses/update_with_media.json"
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token];
	[request setHTTPMethod:@"POST"];	
	
	NSString *boundary = @"---------------------------14737809831466499882746641449";
	NSString *contentType = [NSString stringWithFormat:@"multipart/form-data; boundary=%@", boundary];																		
	[request addValue:contentType forHTTPHeaderField:@"Content-Type"];


	NSMutableData *body = [NSMutableData data];

	// file
	UIImage *image = [UIImage imageWithContentsOfFile:path];
	[body appendData:[[NSString stringWithFormat:@"--%@\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"Content-Disposition: attachment; name=\"media[]\"; filename=\"screenshot.png\"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"Content-Type: application/octet-stream\r\n\r\n" dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:UIImagePNGRepresentation( image )];
	[body appendData:[@"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];

	// text parameter
	[body appendData:[[NSString stringWithFormat:@"--%@\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[[NSString stringWithFormat:@"Content-Disposition: form-data; name=\"status\"\r\n\r\n"] dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[status dataUsingEncoding:NSUTF8StringEncoding]];
	[body appendData:[@"\r\n" dataUsingEncoding:NSUTF8StringEncoding]];

	// close form
	[body appendData:[[NSString stringWithFormat:@"--%@--\r\n", boundary] dataUsingEncoding:NSUTF8StringEncoding]];

	// set request body
	[request setHTTPBody:body];




	[request prepareRequest];
	
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];	
	[request release];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
}


- (void)getHomeTimeline
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "twitterPostDidFail", "User is not logged in" );
		return;
	}
	
	OAToken *token = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	
	_requestType = TwitterRequestHomeTimeline;
	P31MutableOauthRequest *request = [[P31MutableOauthRequest alloc] initWithUrl:@"https://api.twitter.com/1/statuses/home_timeline.json"
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token];
	
	[request setHTTPMethod:@"GET"];
	[request prepareRequest];
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];	
	[request release];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
}


- (void)performRequest:(NSString*)methodType path:(NSString*)path params:(NSDictionary*)params
{
	NSString *tokenString = [[NSUserDefaults standardUserDefaults] objectForKey:kLoggedInUser];
	if( !tokenString )
	{
		UnitySendMessage( "TwitterManager", "twitterPostDidFail", "User is not logged in" );
		return;
	}
	
	OAToken *token = [[OAToken alloc] initWithHTTPResponseBody:tokenString];
	
	NSString *url = [NSString stringWithFormat:@"https://api.twitter.com%@", path];
	_requestType = TwitterRequestCustom;
	P31MutableOauthRequest *request = [[P31MutableOauthRequest alloc] initWithUrl:url
																			  key:_consumerKey
																		   secret:_consumerSecret
																			token:token];
	
	[request setHTTPMethod:[methodType uppercaseString]];
	
	// add the parameters (OARequestParameter)
	if( params )
	{
		NSArray *allKeys = [params allKeys];
		NSMutableArray *oaParameters = [NSMutableArray arrayWithCapacity:allKeys.count];
		
		for( NSString *key in allKeys )
		{
			OARequestParameter *p = [[OARequestParameter alloc] initWithName:key value:[params objectForKey:key]];
			[oaParameters addObject:p];
			[p release];
		}
		[request setParameters:oaParameters];
	}
	
	[request prepareRequest];
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];	
	[request release];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
}


- (void)showTweetComposerWithMessage:(NSString*)message image:(UIImage*)image link:(NSString*)link
{
	// early out if we cant tweet
	if( ![TwitterManager isTweetSheetSupported] )
		return;
	
	// Create the tweet sheet
	TWTweetComposeViewController *tweetSheet = [[TWTweetComposeViewController alloc] init];
	
	if( !tweetSheet )
		return;
	
	// Add a tweet message
	[tweetSheet performSelector:@selector(setInitialText:) withObject:message];
	
	// add an image
	if( image )
		[tweetSheet performSelector:@selector(addImage:) withObject:image];
	
	// add a link
	if( link )
		[tweetSheet addURL:[NSURL URLWithString:link]];

	// set a blocking handler for the tweet sheet
	tweetSheet.completionHandler = ^( TWTweetComposeViewControllerResult result )
	{
		UnityPause( false );
		[UnityGetGLViewController() dismissModalViewControllerAnimated:YES];
		
		if( result == TWTweetComposeViewControllerResultDone )
			UnitySendMessage( "TwitterManager", "tweetSheetCompleted", "1" );
		else if( result == TWTweetComposeViewControllerResultCancelled )
			UnitySendMessage( "TwitterManager", "tweetSheetCompleted", "0" );
	};
	
	// Show the tweet sheet
	UnityPause( true );
	UIViewController *vc = [self getViewControllerForModalPresentation:YES];
	[vc presentModalViewController:tweetSheet animated:YES];
	
	[tweetSheet release];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSURLConnection Delegates

- (void)connection:(NSURLConnection*)conn didReceiveResponse:(NSURLResponse*)response
{
	[_payload setLength:0];
}


- (void)connection:(NSURLConnection*)conn didReceiveData:(NSData*)data
{
	[_payload appendData:data];
}


- (void)connectionDidFinishLoading:(NSURLConnection*)conn
{
	NSString *data = [[[NSString alloc] initWithData:_payload encoding:NSUTF8StringEncoding] autorelease];

	switch( _requestType )
	{
		case TwitterRequestLogin:
		{
			[self completeLoginWithResponseData:data];

			break;
		}
		case TwitterRequestUpdateStatus:
		{
			// was this successful or not?
			if( [data rangeOfString:@"\"error\""].location != NSNotFound )
			{
				// try to extract a useful error message
				id dict = [data objectFromJSONString];
				if( [dict isKindOfClass:[NSDictionary class]] && [[dict allKeys] containsObject:@"error"] )
				{
					NSString *error = [dict objectForKey:@"error"];
					UnitySendMessage( "TwitterManager", "twitterPostDidFail", [error UTF8String] );
				}
				else
				{
					UnitySendMessage( "TwitterManager", "twitterPostDidFail", [data UTF8String] );
				}
			}
			else
			{
				UnitySendMessage( "TwitterManager", "twitterPostSucceeded", "" );
			}

			break;
		}
		case TwitterRequestHomeTimeline:
		{
			// Return statuses to Unity
			UnitySendMessage( "TwitterManager", "twitterHomeTimelineDidFinish", [data UTF8String] );
			break;
		}
		case TwitterRequestCustom:
		{
			// Return statuses to Unity
			UnitySendMessage( "TwitterManager", "twitterRequestDidFinish", [data UTF8String] );
			break;
		}
	}
	
	// clean up
	self.payload = nil;
	[conn release];
}


- (void)connection:(NSURLConnection*)conn didFailWithError:(NSError*)error
{
	if( _requestType == TwitterRequestLogin )
		UnitySendMessage( "TwitterManager", "twitterLoginDidFail", [[error localizedDescription] UTF8String] );
	else if( _requestType == TwitterRequestUpdateStatus )
		UnitySendMessage( "TwitterManager", "twitterPostDidFail", [[error localizedDescription] UTF8String] );
	else if( _requestType == TwitterRequestCustom )
		UnitySendMessage( "TwitterManager", "twitterRequestDidFail", [[error localizedDescription] UTF8String] );
	else
		UnitySendMessage( "TwitterManager", "twitterHomeTimelineDidFail", [[error localizedDescription] UTF8String] );

	
	// clean up
	self.payload = nil;
}


@end
