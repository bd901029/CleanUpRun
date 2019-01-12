//
//  AppControllerPushAdditions.h
//  EtceteraTest
//
//  Created by Mike on 10/5/10.
//  Copyright 2010 Prime31 Studios. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "AppController.h"



@interface AppController(PushAdditions)

+ (void)registerForRemoteNotificationTypes:(NSNumber*)types;

+ (NSNumber*)enabledRemoteNotificationTypes;

@end
