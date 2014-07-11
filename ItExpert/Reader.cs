using System.Drawing;
using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;

namespace ReaderLib {

	[BaseType (typeof (UIView))]
	public partial interface ReaderContentPage {

		[Export ("initWithURL:page:password:")]
		IntPtr Constructor (NSUrl fileURL, int page, string phrase);

		[Export ("processSingleTap:")]
		NSObject ProcessSingleTap (UITapGestureRecognizer recognizer);
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderDocumentLink : NSObject {

		[Export ("rect", ArgumentSemantic.Assign)]
		RectangleF Rect { get; }

		[Export ("dictionary", ArgumentSemantic.Assign)]
		CGPDFDictionary Dictionary { get; }

		[Static, Export ("newWithRect:dictionary:")]
		NSObject NewWithRect (RectangleF linkRect, CGPDFDictionary linkDictionary);

		[Export ("initWithRect:dictionary:")]
		IntPtr Constructor (RectangleF linkRect, CGPDFDictionary linkDictionary);
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderThumbView {

		[Export ("operation", ArgumentSemantic.Retain)]
		NSOperation Operation { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; set; }

		[Export ("showImage:")]
		void ShowImage (UIImage image);

		[Export ("showTouched:")]
		void ShowTouched (bool touched);

		[Export ("reuse")]
		void Reuse ();
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderContentViewDelegate {

		[Export ("contentView:touchesBegan:")]
		void TouchesBegan (ReaderContentView contentView, NSSet touches);
	}

	[BaseType (typeof (UIScrollView))]
	public partial interface ReaderContentView {

		[Export ("message", ArgumentSemantic.Assign)]
		ReaderContentViewDelegate Message { get; set; }

		[Export ("initWithFrame:fileURL:page:password:")]
		IntPtr Constructor (RectangleF frame, NSUrl fileURL, uint page, string phrase);

		[Export ("showPageThumb:page:password:guid:")]
		void ShowPageThumb (NSUrl fileURL, int page, string phrase, string guid);

		[Export ("processSingleTap:")]
		NSObject ProcessSingleTap (UITapGestureRecognizer recognizer);

		[Export ("zoomIncrement")]
		void ZoomIncrement ();

		[Export ("zoomDecrement")]
		void ZoomDecrement ();

		[Export ("zoomReset")]
		void ZoomReset ();
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderDocument : NSObject {

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("fileDate", ArgumentSemantic.Retain)]
		NSDate FileDate { get; }

		[Export ("lastOpen", ArgumentSemantic.Retain)]
		NSDate LastOpen { get; set; }

		[Export ("fileSize", ArgumentSemantic.Retain)]
		NSNumber FileSize { get; }

		[Export ("pageCount", ArgumentSemantic.Retain)]
		NSNumber PageCount { get; }

		[Export ("pageNumber", ArgumentSemantic.Retain)]
		NSNumber PageNumber { get; set; }

		[Export ("bookmarks", ArgumentSemantic.Retain)]
		NSMutableIndexSet Bookmarks { get; }

		[Export ("fileName", ArgumentSemantic.Retain)]
		string FileName { get; }

		[Export ("password", ArgumentSemantic.Retain)]
		string Password { get; }

		[Export ("fileURL", ArgumentSemantic.Retain)]
		NSUrl FileURL { get; }

		[Static, Export ("withDocumentFilePath:password:")]
		ReaderDocument WithDocumentFilePath (string filename, string phrase);

		[Static, Export ("unarchiveFromFileName:password:")]
		ReaderDocument UnarchiveFromFileName (string filename, string phrase);

		[Export ("initWithFilePath:password:")]
		IntPtr Constructor (string fullFilePath, string phrase);

		[Export ("saveReaderDocument")]
		void SaveReaderDocument ();

		[Export ("updateProperties")]
		void UpdateProperties ();
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderDocumentOutline : NSObject {

		[Static, Export ("outlineFromFileURL:password:"), Verify ("NSArray may be reliably typed, check the documentation", "/Users/developer/Downloads/Reader-master/Sources/ReaderDocumentOutline.h", Line = 30)]
		NSObject [] OutlineFromFileURL (NSUrl fileURL, string phrase);

		[Static, Export ("logDocumentOutlineArray:")]
		void LogDocumentOutlineArray ([Verify ("NSArray may be reliably typed, check the documentation", "/Users/developer/Downloads/Reader-master/Sources/ReaderDocumentOutline.h", Line = 32)] NSObject [] array);
	}

	[BaseType (typeof (NSObject))]
	public partial interface DocumentOutlineEntry : NSObject {

		[Static, Export ("newWithTitle:target:level:")]
		NSObject NewWithTitle (string title, NSObject target, int level);

		[Export ("initWithTitle:target:level:")]
		IntPtr Constructor (string title, NSObject target, int level);

		[Export ("level")]
		int Level { get; }

		[Export ("children", ArgumentSemantic.Retain)]
		NSMutableArray Children { get; set; }

		[Export ("title", ArgumentSemantic.Retain)]
		string Title { get; }

		[Export ("target", ArgumentSemantic.Retain)]
		NSObject Target { get; }
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderThumbView {


		[Export ("showImage:")]
		void ShowImage (UIImage image);

		[Export ("showTouched:")]
		void ShowTouched (bool touched);

		[Export ("reuse")]
		void Reuse ();
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderMainPagebarDelegate {

		[Export ("pagebar:gotoPage:")]
		void GotoPage (ReaderMainPagebar pagebar, int page);
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderMainPagebar {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ReaderMainPagebarDelegate Delegate { get; set; }

		[Export ("initWithFrame:document:")]
		IntPtr Constructor (RectangleF frame, ReaderDocument object);

		[Export ("updatePagebar")]
		void UpdatePagebar ();

		[Export ("hidePagebar")]
		void HidePagebar ();

		[Export ("showPagebar")]
		void ShowPagebar ();
	}

	[BaseType (typeof (UIControl))]
	public partial interface ReaderTrackControl {

		[Export ("value")]
		float Value { get; }
	}

	[BaseType (typeof (ReaderThumbView))]
	public partial interface ReaderPagebarThumb {

		[Export ("initWithFrame:small:")]
		IntPtr Constructor (RectangleF frame, bool small);
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderMainToolbarDelegate {

		[Export ("tappedInToolbar:doneButton:")]
		void DoneButton (ReaderMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:thumbsButton:")]
		void ThumbsButton (ReaderMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:printButton:")]
		void PrintButton (ReaderMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:emailButton:")]
		void EmailButton (ReaderMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:markButton:")]
		void MarkButton (ReaderMainToolbar toolbar, UIButton button);
	}

	[BaseType (typeof (UIXToolbarView))]
	public partial interface ReaderMainToolbar {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ReaderMainToolbarDelegate Delegate { get; set; }

		[Export ("initWithFrame:document:")]
		IntPtr Constructor (RectangleF frame, ReaderDocument object);

		[Export ("bookmarkState"), Verify ("ObjC method massaged into setter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderMainToolbar.h", Line = 51)]
		bool BookmarkState { set; }

		[Export ("hideToolbar")]
		void HideToolbar ();

		[Export ("showToolbar")]
		void ShowToolbar ();
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbRequest : NSObject {

		[Export ("fileURL", ArgumentSemantic.Retain)]
		NSUrl FileURL { get; }

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("password", ArgumentSemantic.Retain)]
		string Password { get; }

		[Export ("cacheKey", ArgumentSemantic.Retain)]
		string CacheKey { get; }

		[Export ("thumbName", ArgumentSemantic.Retain)]
		string ThumbName { get; }

		[Export ("thumbView", ArgumentSemantic.Retain)]
		ReaderThumbView ThumbView { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; }

		[Export ("thumbPage")]
		int ThumbPage { get; }

		[Export ("thumbSize", ArgumentSemantic.Assign)]
		SizeF ThumbSize { get; }

		[Export ("scale")]
		float Scale { get; }

		[Static, Export ("newForView:fileURL:password:guid:page:size:")]
		NSObject NewForView (ReaderThumbView view, NSUrl url, string phrase, string guid, int page, SizeF size);

		[Export ("initWithView:fileURL:password:guid:page:size:")]
		IntPtr Constructor (ReaderThumbView view, NSUrl url, string phrase, string guid, int page, SizeF size);
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbCache : NSObject {

		[Static, Export ("sharedInstance"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbCache.h", Line = 32)]
		ReaderThumbCache SharedInstance { get; }

		[Static, Export ("touchThumbCacheWithGUID:")]
		void TouchThumbCacheWithGUID (string guid);

		[Static, Export ("createThumbCacheWithGUID:")]
		void CreateThumbCacheWithGUID (string guid);

		[Static, Export ("removeThumbCacheWithGUID:")]
		void RemoveThumbCacheWithGUID (string guid);

		[Static, Export ("purgeThumbCachesOlderThan:")]
		void PurgeThumbCachesOlderThan (double age);

		[Static, Export ("thumbCachePathForGUID:")]
		string ThumbCachePathForGUID (string guid);

		[Export ("thumbRequest:priority:")]
		NSObject ThumbRequest (ReaderThumbRequest request, bool priority);

		[Export ("setObject:forKey:")]
		void SetObject (UIImage image, string key);

		[Export ("removeObjectForKey:")]
		void RemoveObjectForKey (string key);

		[Export ("removeNullForKey:")]
		void RemoveNullForKey (string key);

		[Export ("removeAllObjects")]
		void RemoveAllObjects ();
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbQueue : NSObject {

		[Static, Export ("sharedInstance"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbQueue.h", Line = 30)]
		ReaderThumbQueue SharedInstance { get; }

		[Export ("addLoadOperation:")]
		void AddLoadOperation (NSOperation operation);

		[Export ("addWorkOperation:")]
		void AddWorkOperation (NSOperation operation);

		[Export ("cancelOperationsWithGUID:")]
		void CancelOperationsWithGUID (string guid);

		[Export ("cancelAllOperations")]
		void CancelAllOperations ();
	}

	[BaseType (typeof (NSOperation))]
	public partial interface ReaderThumbOperation {

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("initWithGUID:")]
		IntPtr Constructor (string guid);
	}

	[BaseType (typeof (ReaderThumbOperation))]
	public partial interface ReaderThumbFetch {

		[Export ("initWithRequest:")]
		IntPtr Constructor (ReaderThumbRequest options);
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbQueue : NSObject {

		[Static, Export ("sharedInstance"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbQueue.h", Line = 30)]
		ReaderThumbQueue SharedInstance { get; }

		[Export ("addLoadOperation:")]
		void AddLoadOperation (NSOperation operation);

		[Export ("addWorkOperation:")]
		void AddWorkOperation (NSOperation operation);

		[Export ("cancelOperationsWithGUID:")]
		void CancelOperationsWithGUID (string guid);

		[Export ("cancelAllOperations")]
		void CancelAllOperations ();
	}

	[BaseType (typeof (NSOperation))]
	public partial interface ReaderThumbOperation {

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("initWithGUID:")]
		IntPtr Constructor (string guid);
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbQueue : NSObject {

		[Static, Export ("sharedInstance"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbQueue.h", Line = 30)]
		ReaderThumbQueue SharedInstance { get; }

		[Export ("addLoadOperation:")]
		void AddLoadOperation (NSOperation operation);

		[Export ("addWorkOperation:")]
		void AddWorkOperation (NSOperation operation);

		[Export ("cancelOperationsWithGUID:")]
		void CancelOperationsWithGUID (string guid);

		[Export ("cancelAllOperations")]
		void CancelAllOperations ();
	}

	[BaseType (typeof (NSOperation))]
	public partial interface ReaderThumbOperation {

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("initWithGUID:")]
		IntPtr Constructor (string guid);
	}

	[BaseType (typeof (ReaderThumbOperation))]
	public partial interface ReaderThumbRender {

		[Export ("initWithRequest:")]
		IntPtr Constructor (ReaderThumbRequest options);
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderThumbRequest : NSObject {

		[Export ("fileURL", ArgumentSemantic.Retain)]
		NSUrl FileURL { get; }

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("password", ArgumentSemantic.Retain)]
		string Password { get; }

		[Export ("cacheKey", ArgumentSemantic.Retain)]
		string CacheKey { get; }

		[Export ("thumbName", ArgumentSemantic.Retain)]
		string ThumbName { get; }

		[Export ("thumbView", ArgumentSemantic.Retain)]
		ReaderThumbView ThumbView { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; }

		[Export ("thumbPage")]
		int ThumbPage { get; }

		[Export ("thumbSize", ArgumentSemantic.Assign)]
		SizeF ThumbSize { get; }

		[Export ("scale")]
		float Scale { get; }

		[Static, Export ("newForView:fileURL:password:guid:page:size:")]
		NSObject NewForView (ReaderThumbView view, NSUrl url, string phrase, string guid, int page, SizeF size);

		[Export ("initWithView:fileURL:password:guid:page:size:")]
		IntPtr Constructor (ReaderThumbView view, NSUrl url, string phrase, string guid, int page, SizeF size);
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderThumbView {

		[Export ("operation", ArgumentSemantic.Retain)]
		NSOperation Operation { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; set; }

		[Export ("showImage:")]
		void ShowImage (UIImage image);

		[Export ("showTouched:")]
		void ShowTouched (bool touched);

		[Export ("reuse")]
		void Reuse ();
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderThumbView {

		[Export ("operation", ArgumentSemantic.Retain)]
		NSOperation Operation { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; set; }

		[Export ("showImage:")]
		void ShowImage (UIImage image);

		[Export ("showTouched:")]
		void ShowTouched (bool touched);

		[Export ("reuse")]
		void Reuse ();
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderThumbsViewDelegate : UIScrollViewDelegate {

		[Export ("numberOfThumbsInThumbsView:")]
		uint  (ReaderThumbsView thumbsView);

		[Export ("thumbsView:thumbCellWithFrame:")]
		NSObject ThumbCellWithFrame (ReaderThumbsView thumbsView, RectangleF frame);

		[Export ("thumbsView:updateThumbCell:forIndex:")]
		void UpdateThumbCell (ReaderThumbsView thumbsView, NSObject thumbCell, int index);

		[Export ("thumbsView:didSelectThumbWithIndex:")]
		void DidSelectThumbWithIndex (ReaderThumbsView thumbsView, int index);

		[Export ("thumbsView:refreshThumbCell:forIndex:")]
		void RefreshThumbCell (ReaderThumbsView thumbsView, NSObject thumbCell, int index);

		[Export ("thumbsView:didPressThumbWithIndex:")]
		void DidPressThumbWithIndex (ReaderThumbsView thumbsView, int index);
	}

	[BaseType (typeof (UIScrollView))]
	public partial interface ReaderThumbsView {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ReaderThumbsViewDelegate Delegate { get; set; }

		[Export ("thumbSize"), Verify ("ObjC method massaged into setter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbsView.h", Line = 56)]
		SizeF ThumbSize { set; }

		[Export ("reloadThumbsCenterOnIndex:")]
		void ReloadThumbsCenterOnIndex (int index);

		[Export ("reloadThumbsContentOffset:")]
		void ReloadThumbsContentOffset (PointF newContentOffset);

		[Export ("refreshThumbWithIndex:")]
		void RefreshThumbWithIndex (int index);

		[Export ("refreshVisibleThumbs")]
		void RefreshVisibleThumbs ();

		[Export ("insetContentOffset"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbsView.h", Line = 66)]
		PointF InsetContentOffset { get; }
	}

	[BaseType (typeof (NSObject))]
	public partial interface ReaderDocument : NSObject {

		[Export ("guid", ArgumentSemantic.Retain)]
		string Guid { get; }

		[Export ("fileDate", ArgumentSemantic.Retain)]
		NSDate FileDate { get; }

		[Export ("lastOpen", ArgumentSemantic.Retain)]
		NSDate LastOpen { get; set; }

		[Export ("fileSize", ArgumentSemantic.Retain)]
		NSNumber FileSize { get; }

		[Export ("pageCount", ArgumentSemantic.Retain)]
		NSNumber PageCount { get; }

		[Export ("pageNumber", ArgumentSemantic.Retain)]
		NSNumber PageNumber { get; set; }

		[Export ("bookmarks", ArgumentSemantic.Retain)]
		NSMutableIndexSet Bookmarks { get; }

		[Export ("fileName", ArgumentSemantic.Retain)]
		string FileName { get; }

		[Export ("password", ArgumentSemantic.Retain)]
		string Password { get; }

		[Export ("fileURL", ArgumentSemantic.Retain)]
		NSUrl FileURL { get; }

		[Static, Export ("withDocumentFilePath:password:")]
		ReaderDocument WithDocumentFilePath (string filename, string phrase);

		[Static, Export ("unarchiveFromFileName:password:")]
		ReaderDocument UnarchiveFromFileName (string filename, string phrase);

		[Export ("initWithFilePath:password:")]
		IntPtr Constructor (string fullFilePath, string phrase);

		[Export ("saveReaderDocument")]
		void SaveReaderDocument ();

		[Export ("updateProperties")]
		void UpdateProperties ();
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderViewControllerDelegate {

		[Export ("dismissReaderViewController:")]
		void  (ReaderViewController viewController);
	}

	[BaseType (typeof (UIViewController))]
	public partial interface ReaderViewController {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ReaderViewControllerDelegate Delegate { get; set; }

		[Export ("initWithReaderDocument:")]
		IntPtr Constructor (ReaderDocument object);
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ThumbsMainToolbarDelegate {

		[Export ("tappedInToolbar:doneButton:")]
		void DoneButton (ThumbsMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:showControl:")]
		void ShowControl (ThumbsMainToolbar toolbar, UISegmentedControl control);
	}

	[BaseType (typeof (UIXToolbarView))]
	public partial interface ThumbsMainToolbar {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ThumbsMainToolbarDelegate Delegate { get; set; }

		[Export ("initWithFrame:title:")]
		IntPtr Constructor (RectangleF frame, string title);
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ThumbsMainToolbarDelegate {

		[Export ("tappedInToolbar:doneButton:")]
		void DoneButton (ThumbsMainToolbar toolbar, UIButton button);

		[Export ("tappedInToolbar:showControl:")]
		void ShowControl (ThumbsMainToolbar toolbar, UISegmentedControl control);
	}

	[BaseType (typeof (UIXToolbarView))]
	public partial interface ThumbsMainToolbar {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ThumbsMainToolbarDelegate Delegate { get; set; }

		[Export ("initWithFrame:title:")]
		IntPtr Constructor (RectangleF frame, string title);
	}

	[BaseType (typeof (UIView))]
	public partial interface ReaderThumbView {

		[Export ("operation", ArgumentSemantic.Retain)]
		NSOperation Operation { get; set; }

		[Export ("targetTag")]
		uint TargetTag { get; set; }

		[Export ("showImage:")]
		void ShowImage (UIImage image);

		[Export ("showTouched:")]
		void ShowTouched (bool touched);

		[Export ("reuse")]
		void Reuse ();
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ReaderThumbsViewDelegate : UIScrollViewDelegate {

		[Export ("numberOfThumbsInThumbsView:")]
		uint  (ReaderThumbsView thumbsView);

		[Export ("thumbsView:thumbCellWithFrame:")]
		NSObject ThumbCellWithFrame (ReaderThumbsView thumbsView, RectangleF frame);

		[Export ("thumbsView:updateThumbCell:forIndex:")]
		void UpdateThumbCell (ReaderThumbsView thumbsView, NSObject thumbCell, int index);

		[Export ("thumbsView:didSelectThumbWithIndex:")]
		void DidSelectThumbWithIndex (ReaderThumbsView thumbsView, int index);

		[Export ("thumbsView:refreshThumbCell:forIndex:")]
		void RefreshThumbCell (ReaderThumbsView thumbsView, NSObject thumbCell, int index);

		[Export ("thumbsView:didPressThumbWithIndex:")]
		void DidPressThumbWithIndex (ReaderThumbsView thumbsView, int index);
	}

	[BaseType (typeof (UIScrollView))]
	public partial interface ReaderThumbsView {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ReaderThumbsViewDelegate Delegate { get; set; }

		[Export ("thumbSize"), Verify ("ObjC method massaged into setter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbsView.h", Line = 56)]
		SizeF ThumbSize { set; }

		[Export ("reloadThumbsCenterOnIndex:")]
		void ReloadThumbsCenterOnIndex (int index);

		[Export ("reloadThumbsContentOffset:")]
		void ReloadThumbsContentOffset (PointF newContentOffset);

		[Export ("refreshThumbWithIndex:")]
		void RefreshThumbWithIndex (int index);

		[Export ("refreshVisibleThumbs")]
		void RefreshVisibleThumbs ();

		[Export ("insetContentOffset"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ReaderThumbsView.h", Line = 66)]
		PointF InsetContentOffset { get; }
	}

	[Model, BaseType (typeof (NSObject))]
	public partial interface ThumbsViewControllerDelegate {

		[Export ("thumbsViewController:gotoPage:")]
		void GotoPage (ThumbsViewController viewController, int page);

		[Export ("dismissThumbsViewController:")]
		void  (ThumbsViewController viewController);
	}

	[BaseType (typeof (UIViewController))]
	public partial interface ThumbsViewController {

		[Export ("delegate", ArgumentSemantic.Assign)]
		ThumbsViewControllerDelegate Delegate { get; set; }

		[Export ("initWithReaderDocument:")]
		IntPtr Constructor (ReaderDocument object);
	}

	[BaseType (typeof (ReaderThumbView))]
	public partial interface ThumbsPageThumb {

		[Export ("maximumContentSize"), Verify ("ObjC method massaged into getter property", "/Users/developer/Downloads/Reader-master/Sources/ThumbsViewController.h", Line = 60)]
		SizeF MaximumContentSize { get; }

		[Export ("showText:")]
		void ShowText (string text);

		[Export ("showBookmark:")]
		void ShowBookmark (bool show);
	}
}
