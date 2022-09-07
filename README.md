# unity_ffmpeg_sample

Sample demo project calling ffmpeg command from within unity. Project use [FfmpegKit]( https://github.com/tanersener/ffmpeg-kit)

##Instalation

### Android
Do not need to do anything, but if you want to update library look for Assets/Plugins/Android and replace file with the version you want.

### iOS
1. Copy the Podfile to your iOS build project folder
2. Update the pod file with the version ffmpegkit you want to use
3. Install cocoapod if need and run pod install to generate the workspace

## Usage

FfmpegHelper is just a wrapping to call the excute functions in both android and iOS platform. You can define other wrapper in it, remember to use the same name with function include from iOS in both FfmpegHelper.cs and FfmpegHelperImpl.mm.
