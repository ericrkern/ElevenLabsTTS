# ElevenLabsTTS
A Windows desktop application built with C# that enables text-to-speech conversion using Eleven Labs voices.

## Description
This program allows users to:
- Configure their Eleven Labs API key in the settings
- Select their preferred voice from a dropdown menu
- Convert typed text to speech using their selected voice
- Customize voice parameters and output settings
- Choose from multiple languages for text-to-speech conversion

## Features
- Easy-to-use interface for text-to-speech conversion
- Voice selection through dropdown menu
- Secure API key configuration
- Real-time text-to-speech processing
- Multi-language support (English, French, Spanish, Italian, German, Dutch, Chinese)
- Advanced voice configuration options:
  - Voice model selection
  - Stability adjustment
  - Speed control
  - Output format settings
- Voice list refresh functionality

## Configuration
The configuration screen provides:
1. API Key management
2. Voice selection dropdown
3. Voice model options
4. Language selection
5. Voice parameter adjustments:
   - Stability settings
   - Speed control
6. Output format selection
7. Load/Refresh voices button

## Getting Started
1. Obtain an API key from Eleven Labs
2. Launch the application and click the Configuration button
3. Enter your API key and click "Load Voices" to populate the voice list
4. Select your preferred voice from the dropdown menu
5. Choose your preferred language
6. Adjust voice parameters as needed
7. Return to the main screen to start converting text to speech

## Technical Details
- Built with C# and Windows Forms/WPF
- Integrates with Eleven Labs API
- Supports various audio output formats
- Local configuration storage

## Build for Windows
The latest Windows build is available for download:
- [ElevenLabsTTS_Build008.zip](https://github.com/ericrkern/ElevenLabsTTS/raw/master/ElevenLabsTTS/ElevenLabsTTS_Build008.zip)

### Installation
1. Download the zip file above
2. Extract all contents to a folder of your choice
3. Run the ElevenLabsTTS.exe file
4. No installation required - the application is self-contained

### System Requirements
- Windows 10 or later
- .NET runtime is included in the package (self-contained application)
