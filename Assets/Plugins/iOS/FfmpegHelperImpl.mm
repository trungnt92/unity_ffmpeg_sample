#include <ffmpegkit/FFmpegKit.h>

extern "C" void _Excute (const char* command)
{
    FFmpegSession *session = [FFmpegKit execute:[NSString stringWithUTF8String:command]];
    ReturnCode *returnCode = [session getReturnCode];
    if ([ReturnCode isSuccess:returnCode]) {

        // SUCCESS

    } else if ([ReturnCode isCancel:returnCode]) {

        // CANCEL

    } else {

        // FAILURE
//        NSLog(@"Command failed with state %@ and rc %@.%@", [FFmpegKitConfig sessionStateToString:[session getState]], returnCode, [session getFailStackTrace]);

    }
}
