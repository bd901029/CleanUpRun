//
//  SharingBinding.m
//  Unity-iPhone
//
//  Created by Mike Desaro on 1/28/13.
//
//

#import "SharingManager.h"

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]



NSArray * _sharingSharingArrayFromItems( NSArray *items )
{
	NSMutableArray *fixedItems = [NSMutableArray array];
	
	for( NSString *item in items )
	{
		// check for valid file paths
		if( [[item substringWithRange:NSMakeRange( 0, 1 ) ] isEqualToString:@"/"] && [[NSFileManager defaultManager] fileExistsAtPath:item] )
		{
			// see if we have an image
			UIImage *image = [UIImage imageWithContentsOfFile:item];
			if( image )
			{
				[fixedItems addObject:image];
			}
			else
			{
				NSURL *url = [NSURL fileURLWithPath:item];
				[fixedItems addObject:url];
			}
		}
		else if( [[item substringWithRange:NSMakeRange( 0, 4 ) ] isEqualToString:@"http"] )
		{
			[fixedItems addObject:[NSURL URLWithString:item]];
		}
		else
		{
			[fixedItems addObject:item];
		}
	}
	
	return fixedItems;
}


void _sharingShareItems( const char * items, const char * excludedActivityTypes )
{
	NSArray *itemsArray = (NSArray*)[SharingManager objectFromJsonString:GetStringParam( items )];
	
	// excludedActivityTypes could very well be nil
	NSArray *excludedActivityTypesArray = nil;
	if( excludedActivityTypes != NULL )
		excludedActivityTypesArray = (NSArray*)[SharingManager objectFromJsonString:GetStringParam( excludedActivityTypes )];

	[SharingManager shareItems:_sharingSharingArrayFromItems( itemsArray ) excludedActivityTypes:excludedActivityTypesArray];
}
