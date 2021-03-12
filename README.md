# SoRaVAC

## Description

Some Random Video Adapter get an audio/video stream from a capture adapter (UBS or Internal device that is detected by Windows as a Video Capture device) to play on your computer.

## History behind the project

I used to plug my Nintendo Switch on my second PC monitor. I was quickly anoyed by the crappy sound so I bought an USB video capture adapter on a well know chinese web marketplace for less than 10€ (you can also buy them for 15-25€ on the well known US web marketplace). I looked after a small software that could play the audio/video flux and found nothing appart from OBS.
I enjoyed playing through OBS but using a tactical nuke to kill fly is not the best idea... So I asked myself if I could develop something lighter.
Here is the result.
Enjoy!

## Technical description

SoRaVAC is a .Net UWP project for the [MediaCapture](https://docs.microsoft.com/en-gb/uwp/api/Windows.Media.Capture.MediaCapture?view=winrt-18362) class which is the project's angular stone.
> "The MediaCapture class exposes a robust set of APIs that provide low-level control over the capture pipeline and enable advanced capture scenarios [...]." - [Microsoft documentation](https://docs.microsoft.com/en-gb/windows/uwp/audio-video-camera/basic-photo-video-and-audio-capture-with-mediacapture)

SoRaVAC requires Windows 10 version 1903 (build 18362) or newer to work.

The first released version of SoRaVAC was a .net WPF with UWP host dependency software. I rewrote the project in full .net UWP to have more up to date UI (and a more simple project).

## Installation

  1. Download SoRaVAC_Installer_&lt;version&gt;.zip
  2. Unzip de file
  3. Right-click on "install.ps1" and select "Execute with PowerShell". Windows will warn you and maybe open a windows asking you to switch to developper mode, this should not be necessary, juste acknoledge the questions in the shell window.   

## Usage

Launch SoRaVAC shortcut in Windows Menu.

In Settings, register the video source (the video capture adapter), the audio source  (the video capture adapter virtual internace) and the audio renderer (the basic output for audio), the in the Capture panel press the play button.

Windows will ask you to authorize *Webcam* and *microphone* usage. You should do it if you want to play...

## Known supported an unsupported devices

The following devices are supported:

- High End
  - Avermedia Live Gamer Ultra GC553 (Note: Audio is reported to be a little bit out of sync)
- Cheap adapters (USB 3)
  - Hagibis UHC05
- Cheap adapters (USB 2)
  - AMANKA CJQ-ES (Warning: image quality is not very good)
  - ATopoler S1788016410

The following devices are **not** supported:

- Elgato Game Capture HD

## Support

Please use the GitHub's **Issues** functionality.

## Roadmap

- [ ] Proper packaging
  - [x] test autosigned packaging
  - [ ] move to Sicrosft Store
- [x] Full screen mode
- [x] Possibility to capture without using the prefered devices
- [x] Display prefered devices status on the capture page
- [ ] UI/UX review
- [ ] Add built-in help

## Contributing

I'm open to contributions and advices. Feel free to contact me.

## Author

Me, Aleron Wen (that's a pseudo if anyone's asking).

## License

![GitHub](https://img.shields.io/github/license/AleronWen/SoRaVAC?style=plastic)
