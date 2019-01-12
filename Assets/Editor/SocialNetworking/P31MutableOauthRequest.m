//
//  P31OauthRequest.m
//  SocialNetworking
//
//  Created by Mike on 9/11/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import "P31MutableOauthRequest.h"
#import "NSString+URLEncoding.h"
#import "OARequestParameter.h"



@interface P31MutableOauthRequest(Private)
- (void)generateNonce;
@end



@implementation P31MutableOauthRequest

@synthesize key = _key, secret = _secret, realm = _realm, signature = _signature,
			token = _token, nonce = _nonce, timestamp = _timestamp, url = _url;

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

- (id)initWithUrl:(NSString*)url key:(NSString*)key secret:(NSString*)secret token:(OAToken*)token
{
	if( ( self = [super initWithURL:[NSURL URLWithString:url]] ) )
	{
		self.url = url;
		self.key = key;
		self.secret = secret;
		self.token = token;
		
		[self generateNonce];
		self.timestamp = [NSString stringWithFormat:@"%ld", time( NULL )];
	}
	return self;
}


- (void)dealloc
{
	[_realm release];
	[_signature release];
	[_nonce release];
	[_timestamp release];
	
	[_secret release];
	[_key release];
	[_token release];
	
	[super dealloc];
}


- (void)generateNonce
{
    CFUUIDRef theUUID = CFUUIDCreate( NULL );
    CFStringRef string = CFUUIDCreateString( NULL, theUUID );
    self.nonce = (NSString*)string;
}


- (NSString*)signatureBaseString
{
    // OAuth Spec, Section 9.1.1 "Normalize Request Parameters"
    // build a sorted array of both request parameters and OAuth header parameters
	NSDictionary *tokenParameters = [_token parameters];
	
	// 5 being the number of OAuth params in the Signature Base String
	NSMutableArray *parameterPairs = [[NSMutableArray alloc] initWithCapacity:(5 + [[self parameters] count] + [tokenParameters count])];
    
    [parameterPairs addObject:[NSString stringWithFormat:@"oauth_consumer_key=%@", [_key encodedURLParameterString]]];
    [parameterPairs addObject:@"oauth_signature_method=HMAC-SHA1"];
    [parameterPairs addObject:[NSString stringWithFormat:@"oauth_timestamp=%@", [_timestamp encodedURLParameterString]]];
    [parameterPairs addObject:[NSString stringWithFormat:@"oauth_nonce=%@", [_nonce encodedURLParameterString]]];
    [parameterPairs addObject:@"oauth_version=1.0"];
	
	
	// add the token paramters
	for( NSString *k in tokenParameters )
		[parameterPairs addObject:[[OARequestParameter requestParameter:k value:[tokenParameters objectForKey:k]] URLEncodedNameValuePair]];
    
	
	if( ![[self valueForHTTPHeaderField:@"Content-Type"] hasPrefix:@"multipart/form-data"] )
	{
		for( OARequestParameter *param in [self parameters] )
			[parameterPairs addObject:[param URLEncodedNameValuePair]];
	}
    
    NSArray *sortedPairs = [parameterPairs sortedArrayUsingSelector:@selector(compare:)];
    NSString *normalizedRequestParameters = [sortedPairs componentsJoinedByString:@"&"];
    
	// grab the url without any query string
	NSArray *parts = [_url componentsSeparatedByString:@"?"];
	NSString *urlWithoutQuery = [parts objectAtIndex:0];
	
    // OAuth Spec, Section 9.1.2 "Concatenate Request Elements"
    return [NSString stringWithFormat:@"%@&%@&%@",
            [self HTTPMethod],
            [urlWithoutQuery encodedURLParameterString],
            [normalizedRequestParameters encodedURLString]];
}


- (void)prepareRequest
{
	NSString *secretText = [NSString stringWithFormat:@"%@&%@", _secret, _token.secret ? _token.secret : @""];
	self.signature = [[self signatureBaseString] signClearTextWithSecret:secretText];
    
    // set OAuth headers
	NSMutableArray *chunks = [[NSMutableArray alloc] init];
	[chunks addObject:@"realm=\"\""];
	[chunks addObject:[NSString stringWithFormat:@"oauth_consumer_key=\"%@\"", [_key encodedURLParameterString]]];
	
	NSDictionary *tokenParameters = [_token parameters];
	for( NSString *k in tokenParameters )
		[chunks addObject:[NSString stringWithFormat:@"%@=\"%@\"", k, [[tokenParameters objectForKey:k] encodedURLParameterString]]];
	
	[chunks addObject:[NSString stringWithFormat:@"oauth_signature_method=\"%@\"", [@"HMAC-SHA1" encodedURLParameterString]]];
	[chunks addObject:[NSString stringWithFormat:@"oauth_signature=\"%@\"", [_signature encodedURLParameterString]]];
	[chunks addObject:[NSString stringWithFormat:@"oauth_timestamp=\"%@\"", _timestamp]];
	[chunks addObject:[NSString stringWithFormat:@"oauth_nonce=\"%@\"", _nonce]];
	[chunks	addObject:@"oauth_version=\"1.0\""];
	
	NSString *oauthHeader = [NSString stringWithFormat:@"OAuth %@", [chunks componentsJoinedByString:@", "]];
	[chunks release];
	
    [self setValue:oauthHeader forHTTPHeaderField:@"Authorization"];
}


- (void)setHTTPBodyWithString:(NSString*)body
{
	NSData *bodyData = [body dataUsingEncoding:NSASCIIStringEncoding allowLossyConversion:YES];
	[self setValue:[NSString stringWithFormat:@"%d", [bodyData length]] forHTTPHeaderField:@"Content-Length"];
	[self setHTTPBody:bodyData];
}


- (BOOL)isMultipart
{
	return [[self valueForHTTPHeaderField:@"Content-Type"] hasPrefix:@"multipart/form-data"];
}


- (NSArray*)parameters
{
    NSString *encodedParameters = nil;
    
	if( ![self isMultipart] )
	{
		if( [[self HTTPMethod] isEqualToString:@"GET"] || [[self HTTPMethod] isEqualToString:@"DELETE"] )
			encodedParameters = [[self URL] query];
		else
			encodedParameters = [[[NSString alloc] initWithData:[self HTTPBody] encoding:NSASCIIStringEncoding] autorelease];
	}
    
    if( encodedParameters == nil || [encodedParameters isEqualToString:@""] )
        return nil;
	
    NSArray *encodedParameterPairs = [encodedParameters componentsSeparatedByString:@"&"];
    NSMutableArray *requestParameters = [NSMutableArray arrayWithCapacity:[encodedParameterPairs count]];
    
    for( NSString *encodedPair in encodedParameterPairs )
	{
        NSArray *encodedPairElements = [encodedPair componentsSeparatedByString:@"="];
        OARequestParameter *parameter = [[OARequestParameter alloc] initWithName:[[encodedPairElements objectAtIndex:0] stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding]
                                                                           value:[[encodedPairElements objectAtIndex:1] stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding]];
        [requestParameters addObject:parameter];
    }
    
    return requestParameters;
}


- (void)setParameters:(NSArray*)parameters
{
	NSMutableArray *pairs = [[[NSMutableArray alloc] initWithCapacity:parameters.count] autorelease];
	for( OARequestParameter *requestParameter in parameters )
		[pairs addObject:[requestParameter URLEncodedNameValuePair]];
	
	NSString *encodedParameterPairs = [pairs componentsJoinedByString:@"&"];
    
	if( [[self HTTPMethod] isEqualToString:@"GET"] || [[self HTTPMethod] isEqualToString:@"DELETE"] )
	{
		NSArray *parts = [[self.URL absoluteString] componentsSeparatedByString:@"?"];
		NSString *urlWithoutQuery = [parts objectAtIndex:0];
		[self setURL:[NSURL URLWithString:[NSString stringWithFormat:@"%@?%@", urlWithoutQuery, encodedParameterPairs]]];
	}
	else
	{
		// POST, PUT
		[self setHTTPBodyWithString:encodedParameterPairs];
		[self setValue:@"application/x-www-form-urlencoded" forHTTPHeaderField:@"Content-Type"];
	}
}


@end
