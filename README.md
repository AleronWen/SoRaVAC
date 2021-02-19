# SoRaVAC

## Description

Some Random Video Adapter get an audio/video stream from a capture adapter (UBS or Internal) to play on your computer.

## History behind the project

I used to plug my Nintendo Switch on my second PC monitor. I was quickly anoyed by the crappy sound so I bought an USB video capture adapter on a well know chinese web marketplace for less than 10€ (you can also buy them for 15-25€ on the well known US web marketplace). I looked after a small software that could play the audio/video flux and found nothing appart from OBS.
I enjoyed playing through OBS but using a tactical nuke to kill fly is not the best idea... So I asked myself if I could develop something lighter.
Here is the result.
Enjoy!

## Technical description

SoRaVAC is a .Net Core 3.1 project with WPF UI and UWP dependency for the [MediaCapture](https://docs.microsoft.com/en-gb/uwp/api/Windows.Media.Capture.MediaCapture?view=winrt-18362) class which is the project's angular stone.
> "The MediaCapture class exposes a robust set of APIs that provide low-level control over the capture pipeline and enable advanced capture scenarios [...]." - [Microsoft documentation](https://docs.microsoft.com/en-gb/windows/uwp/audio-video-camera/basic-photo-video-and-audio-capture-with-mediacapture)

SoRaVAC requires Windows 10 version 1903 (build 18362) or newer to work because of the dependency to [WindowsXamlHost control host](https://docs.microsoft.com/en-gb/windows/communitytoolkit/controls/wpf-winforms/windowsxamlhost) that permits to use MediaCapture in a .Net Core 3.1 + WPF UI.

The very first version of SoRaVAC was a full UWP software but I was unhappy with packaging possibilies. I wanted to have more freedom in distribution so this .Net Core 3.1 + WPF UI + UWP dependency is a good compromise.

## Installation

Unzip the release package anywhere on your computer.

## Usage

Launch **SoRaVAC.exe**. In Settings, register the video source (the video capture adapter), the audio source  (the video capture adapter virtual internace) and the audio renderer (the basic output for audio), the in the Capture panel press the play button.

Windows may ask you to authorize *Webcam* and *microphone* usage. You should do it if you want to play...

## Support

Please use the GitHub's **Issues** functionality.

## Roadmap

- [ ] proper packaging
- [ ] full screen mode
- [ ] Possibility to capture without using the prefered devices
- [ ] Display prefered devices status on the capture page
- [ ] UI/UX review
- [ ] Add built-in help

## Contributing

I'm open to contributions and advices. Feel free to contact me.

## Author

Me, Aleron Wen (that's a pseudo if anyone's asking).

## License

![GitHub](https://img.shields.io/github/license/AleronWen/SoRaVAC?style=plastic)
