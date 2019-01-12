
#import "P31Request.h"
#import "OAToken.h"
#import "P31MutableOauthRequest.h"
#import "TwitterManager.h"


@implementation P31Request

@synthesize delegate = _delegate, pin = _pin, requestToken = _requestToken, payload = _payload;


- (void)dealloc
{
    [_payload release];
    [_requestToken release];
    [_pin release];
    
    [super dealloc];
}


//A request token is used to eventually generate an access token
- (void)requestRequestToken
{
    _isFetchingRequestToken = YES;
	[self requestURL:@"https://twitter.com/oauth/request_token"
			   token:nil
		   onSuccess:@selector(setRequestToken:withData:)
			  onFail:@selector(outhTicketFailed:data:)];
}


//this is what we eventually want
- (void)requestAccessToken
{
    _isFetchingRequestToken = NO;
	[self requestURL:@"https://twitter.com/oauth/access_token"
			   token:_requestToken
		   onSuccess:@selector(setAccessToken:withData:)
			  onFail:@selector(outhTicketFailed:data:)];
}


- (void)requestURL:(NSString*)url token:(OAToken*)token onSuccess:(SEL)success onFail:(SEL)fail
{
    P31MutableOauthRequest *request = [[[P31MutableOauthRequest alloc] initWithUrl:url
                                                                               key:[TwitterManager sharedManager].consumerKey
                                                                            secret:[TwitterManager sharedManager].consumerSecret
                                                                             token:token] autorelease];
    
	if( !request )
		return;
	
	if( self.pin.length )
		token.pin = self.pin;
    [request setHTTPMethod:@"POST"];
	
	[request prepareRequest];
	
	NSURLConnection *connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];
	
    if( connection )
        _payload = [[NSMutableData alloc] init];
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

    // requestRequestToken call
    if( _isFetchingRequestToken )
    {
        NSLog( @"got reqeust token" );
        self.requestToken = nil;
        _requestToken = [[OAToken alloc] initWithHTTPResponseBody:data];
        
        if( self.pin.length )
            _requestToken.pin = self.pin;
        
        [_delegate request:self didGetRequestToken:_requestToken];
    }
    else
    {
		if( self.pin.length && [data rangeOfString: @"oauth_verifier"].location == NSNotFound )
			data = [data stringByAppendingFormat:@"&oauth_verifier=%@", self.pin];
		
        [_delegate request:self didGetAccessToken:data];
    }
    
	// clean up
	self.payload = nil;
	[conn release];
}


- (void)connection:(NSURLConnection*)conn didFailWithError:(NSError*)error
{
	// clean up
	[conn release];
	self.payload = nil;
}




@end
