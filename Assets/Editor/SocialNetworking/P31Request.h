
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>


@protocol P31RequestDelegate;
@class OAToken;

@interface P31Request : NSObject
{
    id<P31RequestDelegate> _delegate;
    NSString *_pin;
    OAToken *_requestToken;
    NSMutableData *_payload;
    BOOL _isFetchingRequestToken;
}
@property (nonatomic, assign) id<P31RequestDelegate> delegate;
@property (nonatomic, retain) NSString *pin;
@property (nonatomic, retain) OAToken *requestToken;
@property (nonatomic, retain) NSMutableData *payload;


- (void)requestRequestToken;

- (void)requestAccessToken;

- (void)requestURL:(NSString*)url token:(OAToken*)token onSuccess:(SEL)success onFail:(SEL)fail;

@end



@protocol P31RequestDelegate <NSObject>
- (void)request:(P31Request*)request didGetRequestToken:(OAToken*)requestToken;
- (void)request:(P31Request*)request didGetAccessToken:(NSString*)data;
@end