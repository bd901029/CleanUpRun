//
//  AddAccountController.m
//  Tweets
//
//  Created by Mike on 11/25/09.
//  Copyright 2009 Prime31 Studios. All rights reserved.
//

#import "AddAccountController.h"
#import "P31MutableOauthRequest.h"
#import "TwitterManager.h"
#import "OAToken.h"
#import "OARequestParameter.h"


void UnitySendMessage( const char * className, const char * methodName, const char * param );


@implementation AddAccountController

@synthesize webView = _webView, request = _request, loadingView = _loadingView;

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

- (id)initWithNibName:(NSString*)nibNameOrNil bundle:(NSBundle*)bundle
{
    if( ( self = [super initWithNibName:nibNameOrNil bundle:bundle] ) )
	{
        UIBarButtonItem *cxl = [[[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemCancel
																				  target:self
                                                                              action:@selector(onTouchCancel)] autorelease];
		self.navigationItem.rightBarButtonItem = cxl;
		
		// grab a request token
        _request = [[P31Request alloc] init];
        _request.delegate = self;
        [_request requestRequestToken];

		self.title = @"Login to Twitter";
    }
    return self;
}


- (void)dealloc
{
	_webView.delegate = nil;
	[_webView release];
	_request.delegate = nil;
	[_request release];
    [_loadingView release];
    
    [super dealloc];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private

- (void)showLoading:(BOOL)show
{
    if( show )
        [_loadingView startAnimating];
    else
        [_loadingView stopAnimating];
}


- (void)onTouchCancel
{
	[[TwitterManager sharedManager] unpauseUnity];
	
	// check for the old 3.3 path first
	if( [[TwitterManager sharedManager] respondsToSelector:@selector(dismissWrappedViewController)] )
		[[TwitterManager sharedManager] performSelector:@selector(dismissWrappedViewController)];
	else
		[self dismissModalViewControllerAnimated:YES];
	
	UnitySendMessage( "TwitterManager", "twitterLoginDidFail", "cancelled" );
}


- (void)gotPin:(NSString*)pin
{
    NSLog( @"got verifier after successful login" );
    _request.pin = pin;
    [_request requestAccessToken];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIViewController

- (void)loadView
{
	[super loadView];
    
    _webView = [[UIWebView alloc] initWithFrame:self.view.bounds];
    _webView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    _webView.delegate = self;
    [self.view addSubview:_webView];
    
    _loadingView = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleGray];
    _loadingView.center = self.view.center;
    _loadingView.autoresizingMask = UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleBottomMargin | UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleTopMargin;
	_loadingView.hidesWhenStopped = YES;
    [self showLoading:YES];
    [self.view addSubview:_loadingView];
}


- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)toInterfaceOrientation
{
    return YES;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark P31RequestDelegate

- (void)request:(P31Request*)request didGetRequestToken:(OAToken*)requestToken
{
	// This generates a URL request that can be passed to a UIWebView. It will open a page in which the user must enter their Twitter creds to validate	
    P31MutableOauthRequest *req = [[[P31MutableOauthRequest alloc] initWithUrl:@"https://twitter.com/oauth/authorize"
                                                                           key:nil
                                                                        secret:nil
                                                                         token:requestToken] autorelease];
	[req setParameters:[NSArray arrayWithObject:[[[OARequestParameter alloc] initWithName:@"oauth_token" value:requestToken.key] autorelease]]];
	[_webView loadRequest:req];
}


- (void)request:(P31Request*)request didGetAccessToken:(NSString*)data
{
	[[TwitterManager sharedManager] completeLoginWithResponseData:data];
	[[TwitterManager sharedManager] unpauseUnity];
	[self dismissModalViewControllerAnimated:YES];
}



///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIWebViewDelegate

- (BOOL)webView:(UIWebView*)webView shouldStartLoadWithRequest:(NSURLRequest*)request navigationType:(UIWebViewNavigationType)navigationType
{
    [self showLoading:YES];
	
	NSLog( @"%@", [request URL] );
	
	
	// we are going to look for a non-twitter URL here.  If we are loading a non-twitter url we should have a verifier
	if( [[[request URL] absoluteString] rangeOfString:@"twitter"].location == NSNotFound )
	{
		// check for denied (touched the return to app link)
		if( [[[request URL] absoluteString] rangeOfString:@"callback?denied="].location != NSNotFound )
		{
			[self onTouchCancel];
			return NO;
		}
		
		// extract the oauth_verifier
		NSArray *parameters = [[[request URL] query] componentsSeparatedByCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"=&"]];
		
		for( int i = 0; i < parameters.count; i = i + 2 )
		{
			if( [[parameters objectAtIndex:i] isEqualToString:@"oauth_verifier"] )
			{
				NSString *verifier = [parameters objectAtIndex:i + 1];
				[self gotPin:verifier];
				return NO;
			}
		}
	}
	
	return YES;
}


- (void)webViewDidFinishLoad:(UIWebView*)webView
{
    [self showLoading:NO];
}


- (void)webViewDidStartLoad:(UIWebView*)webView
{
    [self showLoading:YES];
}

@end
