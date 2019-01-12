//
//  AddAccountController.h
//  Tweets
//
//  Created by Mike on 11/25/09.
//  Copyright 2009 Prime31 Studios. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "P31Request.h"


@interface AddAccountController : UIViewController <UIWebViewDelegate, P31RequestDelegate>
{
	UIWebView *_webView;
    P31Request *_request;
    UIActivityIndicatorView *_loadingView;
}
@property (nonatomic, retain) IBOutlet UIWebView *webView;
@property (nonatomic, retain) P31Request *request;
@property (nonatomic, retain) UIActivityIndicatorView *loadingView;


- (void)gotPin:(NSString*)pin;


@end
