//
//  P31OauthRequest.h
//  SocialNetworking
//
//  Created by Mike on 9/11/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "OAToken.h"


@interface P31MutableOauthRequest : NSMutableURLRequest
{
@protected
	NSString *_key;
	NSString *_secret;
	NSString *_url;
	OAToken *_token;
	
    NSString *_realm;
    NSString *_signature;
    NSString *_nonce;
    NSString *_timestamp;
}
@property (nonatomic, retain) NSString *key;
@property (nonatomic, retain) NSString *secret;
@property (nonatomic, retain) NSString *url;
@property (nonatomic, retain) OAToken *token;

@property (nonatomic, retain) NSString *realm;
@property (nonatomic, retain) NSString *signature;
@property (nonatomic, retain) NSString *nonce;
@property (nonatomic, retain) NSString *timestamp;

@property (nonatomic, retain) NSArray *parameters;


- (id)initWithUrl:(NSString*)url key:(NSString*)key secret:(NSString*)secret token:(OAToken*)token;

- (void)prepareRequest;

- (void)setHTTPBodyWithString:(NSString*)body;

@end
