﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TwainDirect.App {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class lang_en_US {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal lang_en_US() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TwainDirect.App.lang_en_US", typeof(lang_en_US).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to busy.
        /// </summary>
        public static string errBusy {
            get {
                return ResourceManager.GetString("errBusy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner is busy working on another command.  Try again in a little bit..
        /// </summary>
        public static string errBusyFix {
            get {
                return ResourceManager.GetString("errBusyFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create settings folder..
        /// </summary>
        public static string errCantCreateSettingsFolder {
            get {
                return ResourceManager.GetString("errCantCreateSettingsFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We were unable to delete the following file.  It may be open in another program.  Please close any applications that are using these files, and then close and reopen the session before scanning..
        /// </summary>
        public static string errCantDeleteFile {
            get {
                return ResourceManager.GetString("errCantDeleteFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to closed session.
        /// </summary>
        public static string errClosedSession {
            get {
                return ResourceManager.GetString("errClosedSession", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This session has been closed, and will not accept attempts to scan.  Drain all images to free the scanner..
        /// </summary>
        public static string errClosedSessionFix {
            get {
                return ResourceManager.GetString("errClosedSessionFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cover open.
        /// </summary>
        public static string errCoverOpen {
            get {
                return ResourceManager.GetString("errCoverOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A cover or an interlock on the scanner is not properly secured.  Please check the scanner, and try again..
        /// </summary>
        public static string errCoverOpenFix {
            get {
                return ResourceManager.GetString("errCoverOpenFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The dotted notation, indicated above, points to the value that triggered the failure condition..
        /// </summary>
        public static string errDottedNotation {
            get {
                return ResourceManager.GetString("errDottedNotation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to double feed.
        /// </summary>
        public static string errDoubleFeed {
            get {
                return ResourceManager.GetString("errDoubleFeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner was requested or defaulted to check for instances where two or more sheets pass through the automatic document feeder.  Check the paper to see if any are stuck together, and try again..
        /// </summary>
        public static string errDoubleFeedFix {
            get {
                return ResourceManager.GetString("errDoubleFeedFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error starting.  Try uninstalling and reinstalling this software..
        /// </summary>
        public static string errErrorStarting {
            get {
                return ResourceManager.GetString("errErrorStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to folded corner.
        /// </summary>
        public static string errFoldedCorner {
            get {
                return ResourceManager.GetString("errFoldedCorner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner was asked to detect problems with the paper being scanned, such as folded corners.  Please check the paper, and try again..
        /// </summary>
        public static string errFoldedCornerFix {
            get {
                return ResourceManager.GetString("errFoldedCornerFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to invalid capturing options.
        /// </summary>
        public static string errInvalidCapturingOptions {
            get {
                return ResourceManager.GetString("errInvalidCapturingOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner has rejected this task.  This only happens when the task requires specific capabilities from the scanner.  Either select a task that this scanner will accept, or change to a scanner that supports the features required by the current task..
        /// </summary>
        public static string errInvalidCapturingOptionsFix {
            get {
                return ResourceManager.GetString("errInvalidCapturingOptionsFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to invalid image block number.
        /// </summary>
        public static string errInvalidImageBlockNumber {
            get {
                return ResourceManager.GetString("errInvalidImageBlockNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner was asked for image data that it can&apos;t provide.  Close this session and start a new one, if you want to continue..
        /// </summary>
        public static string errInvalidImageBlockNumberFix {
            get {
                return ResourceManager.GetString("errInvalidImageBlockNumberFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to invalid session id.
        /// </summary>
        public static string errInvalidSessionId {
            get {
                return ResourceManager.GetString("errInvalidSessionId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Communication was lost or compromised.  Make sure this session is closed, and try to create a new session..
        /// </summary>
        public static string errInvalidSessionIdFix {
            get {
                return ResourceManager.GetString("errInvalidSessionIdFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to json error, index=.
        /// </summary>
        public static string errJsonErrorIndex {
            get {
                return ResourceManager.GetString("errJsonErrorIndex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We have a response from the scanner, but it can&apos;t be parsed due to an error in its construction.  The index shows where we ran into trouble.  What follows is the raw data we received..
        /// </summary>
        public static string errJsonErrorIndexFix {
            get {
                return ResourceManager.GetString("errJsonErrorIndexFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to misfeed.
        /// </summary>
        public static string errMisfeed {
            get {
                return ResourceManager.GetString("errMisfeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A misfeed has occurred with the scanner.  This is a somewhat generic error, and could include communication errors.  Please resolve the problem, and try again..
        /// </summary>
        public static string errMisfeedFix {
            get {
                return ResourceManager.GetString("errMisfeedFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to missing error code.
        /// </summary>
        public static string errMissingErrorCode {
            get {
                return ResourceManager.GetString("errMissingErrorCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When an error occurs we expect to receive a code.  The response does not have this code, so we can&apos;t tell what error happened What follows is the raw data we received..
        /// </summary>
        public static string errMissingErrorCodeFix {
            get {
                return ResourceManager.GetString("errMissingErrorCodeFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to new session not allowed.
        /// </summary>
        public static string errNewSessionNotAllowed {
            get {
                return ResourceManager.GetString("errNewSessionNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner may be in use by another user.  Or it may be turned off or disconnected.  Check to to see which it is, and then retry.  If the problem persists, try turning the scanner off, wait a bit, and turn it back on. If you&apos;re using the TWAIN Bridge try exiting from it and restarting it..
        /// </summary>
        public static string errNewSessionNotAllowedFix {
            get {
                return ResourceManager.GetString("errNewSessionNotAllowedFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No device selected..
        /// </summary>
        public static string errNoDeviceSelected {
            get {
                return ResourceManager.GetString("errNoDeviceSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to no media.
        /// </summary>
        public static string errNoMedia {
            get {
                return ResourceManager.GetString("errNoMedia", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was no paper for the scanner to capture.  Please check that the paper is properly loaded, and try again..
        /// </summary>
        public static string errNoMediaFix {
            get {
                return ResourceManager.GetString("errNoMediaFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to not capturing.
        /// </summary>
        public static string errNotCapturing {
            get {
                return ResourceManager.GetString("errNotCapturing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This session is not capturing, and will not accept attempts to transfer images.  Close this session and start a new one, if you want to continue..
        /// </summary>
        public static string errNotCapturingFix {
            get {
                return ResourceManager.GetString("errNotCapturingFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to not ready.
        /// </summary>
        public static string errNotReady {
            get {
                return ResourceManager.GetString("errNotReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This session is not ready, and will not accept attempts to negotiate tasks.  Close this session and start a new one, if you want to continue..
        /// </summary>
        public static string errNotReadyFix {
            get {
                return ResourceManager.GetString("errNotReadyFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are no TWAIN drivers installed on this system..
        /// </summary>
        public static string errNoTwainDriversInstalled {
            get {
                return ResourceManager.GetString("errNoTwainDriversInstalled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are no TWAIN Direct scanners available at this time..
        /// </summary>
        public static string errNoTwainScanners {
            get {
                return ResourceManager.GetString("errNoTwainScanners", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to paper jam.
        /// </summary>
        public static string errPaperJam {
            get {
                return ResourceManager.GetString("errPaperJam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A paper jam has been reported.  Please clear the jam and try again..
        /// </summary>
        public static string errPaperJamFix {
            get {
                return ResourceManager.GetString("errPaperJamFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your scanner session has aborted.  Check your scanner to make sure it&apos;s on and connected to the network..
        /// </summary>
        public static string errSessionAborted {
            get {
                return ResourceManager.GetString("errSessionAborted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your scanner session has timed out due to lack of activity..
        /// </summary>
        public static string errSessionTimeout {
            get {
                return ResourceManager.GetString("errSessionTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This error is the result of the task specifying an exception of &apos;fail&apos; somewhere within it.  The scanner triggered this when it was unable to set a required value.  There are two possible solutions: find a scanner that supports the feature required by the task, or use a different task..
        /// </summary>
        public static string errTaskFailed {
            get {
                return ResourceManager.GetString("errTaskFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to timeout.
        /// </summary>
        public static string errTimeout {
            get {
                return ResourceManager.GetString("errTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner did not respond in a timely fashion.  Check that the scanner is still on, and that there is a good connection to the network..
        /// </summary>
        public static string errTimeoutFix {
            get {
                return ResourceManager.GetString("errTimeoutFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The scanner did not respond in a timely fashion. Check that the scanner is still on, and that there is a good connection to the network.  Since this happened for &apos;createSession&apos; it&apos;s possible the scanner was in a sleep state, and took too long to wake up.  Trying a second time may be successful..
        /// </summary>
        public static string errTimeoutWakeupFix {
            get {
                return ResourceManager.GetString("errTimeoutWakeupFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown error, please check the logs for more information..
        /// </summary>
        public static string errUnknownCheckLogs {
            get {
                return ResourceManager.GetString("errUnknownCheckLogs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to unrecognized error code.
        /// </summary>
        public static string errUnrecognizedErrorCode {
            get {
                return ResourceManager.GetString("errUnrecognizedErrorCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We don&apos;t have any advice to offer for this error, it may be custom to the scanner, or this application may not be recent enough to know about it.  What follows is the raw data we received..
        /// </summary>
        public static string errUnrecognizedErrorCodeFix {
            get {
                return ResourceManager.GetString("errUnrecognizedErrorCodeFix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Close.
        /// </summary>
        public static string strButtonClose {
            get {
                return ResourceManager.GetString("strButtonClose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cloud....
        /// </summary>
        public static string strButtonCloudEllipsis {
            get {
                return ResourceManager.GetString("strButtonCloudEllipsis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open.
        /// </summary>
        public static string strButtonOpen {
            get {
                return ResourceManager.GetString("strButtonOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Register....
        /// </summary>
        public static string strButtonRegisterEllipsis {
            get {
                return ResourceManager.GetString("strButtonRegisterEllipsis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scan.
        /// </summary>
        public static string strButtonScan {
            get {
                return ResourceManager.GetString("strButtonScan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select....
        /// </summary>
        public static string strButtonSelectEllipsis {
            get {
                return ResourceManager.GetString("strButtonSelectEllipsis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Setup....
        /// </summary>
        public static string strButtonSetupEllipsis {
            get {
                return ResourceManager.GetString("strButtonSetupEllipsis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stop.
        /// </summary>
        public static string strButtonStop {
            get {
                return ResourceManager.GetString("strButtonStop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unregister....
        /// </summary>
        public static string strButtonUnregisterEllipsis {
            get {
                return ResourceManager.GetString("strButtonUnregisterEllipsis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TWAIN Direct: Application.
        /// </summary>
        public static string strFormScanTitle {
            get {
                return ResourceManager.GetString("strFormScanTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open Scanner.
        /// </summary>
        public static string strFormSelectTitle {
            get {
                return ResourceManager.GetString("strFormSelectTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Setup Scan Session.
        /// </summary>
        public static string strFormSetupTitle {
            get {
                return ResourceManager.GetString("strFormSetupTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select image destination.
        /// </summary>
        public static string strGroupboxSelectImageDestination {
            get {
                return ResourceManager.GetString("strGroupboxSelectImageDestination", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select image destination folder:.
        /// </summary>
        public static string strLabelSelectImageDestination {
            get {
                return ResourceManager.GetString("strLabelSelectImageDestination", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select scanner:.
        /// </summary>
        public static string strLabelSelectScanner {
            get {
                return ResourceManager.GetString("strLabelSelectScanner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string titleError {
            get {
                return ResourceManager.GetString("titleError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Notification.
        /// </summary>
        public static string titleNotification {
            get {
                return ResourceManager.GetString("titleNotification", resourceCulture);
            }
        }
    }
}
