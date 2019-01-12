//
//  FacebookManager.h
//  Facebook
//
//  Created by Mike on 9/13/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Facebook.h"
//#import <FacebookSDK/FacebookSDK.h>



extern NSString *const kFacebookAppIdKey;


@interface FacebookManager : NSObject <FBDialogDelegate>
@property (nonatomic, copy) NSString *urlSchemeSuffix;
@property (nonatomic, copy) NSString *appLaunchUrl;
@property (nonatomic, retain) Facebook *facebook;
@property (nonatomic) FBSessionLoginBehavior loginBehavior;


+ (FacebookManager*)sharedManager;

// Facebook composer methods
+ (BOOL)isFacebookComposerSupported;

+ (BOOL)userCanUseFacebookComposer;


- (void)renewCredentialsForAllFacebookAccounts;

- (void)startSessionQuietly;

- (void)showFacebookComposerWithMessage:(NSString*)message image:(UIImage*)image link:(NSString*)link;

- (BOOL)isLoggedIn;

- (NSString*)accessToken;

- (NSArray*)sessionPermissions;

- (void)loginUsingDeprecatedAuthorizationFlowWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)urlSchemeSuffix;

- (void)loginWithRequestedPermissions:(NSMutableArray*)permissions urlSchemeSuffix:(NSString*)urlSchemeSuffix;

- (void)reauthorizeWithReadPermissions:(NSArray*)permissions;

- (void)reauthorizeWithPublishPermissions:(NSArray*)permissions defaultAudience:(FBSessionDefaultAudience)audience;

- (void)logout;

- (void)showDialog:(NSString*)dialogType withParms:(NSMutableDictionary*)dict;

- (void)requestWithGraphPath:(NSString*)path httpMethod:(NSString*)method params:(NSMutableDictionary*)params;

- (void)requestWithRestMethod:(NSString*)restMethod httpMethod:(NSString*)method params:(NSMutableDictionary*)params;

@end
