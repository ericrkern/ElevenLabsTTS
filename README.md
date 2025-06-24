# ElevenLabsTTS
A Windows desktop application built with C# that enables text-to-speech conversion using Eleven Labs voices.

**Current Version: 1.0.3**

## Description
This program allows users to:
- Configure their Eleven Labs API key in the settings
- Select their preferred voice from a dropdown menu
- Convert typed text to speech using their selected voice
- Customize voice parameters and output settings

## Features
- Easy-to-use interface for text-to-speech conversion
- Voice selection through dropdown menu
- Secure API key configuration
- Real-time text-to-speech processing
- Advanced voice configuration options:
  - Voice model selection (including latest ElevenLabs 3.0 models)
  - Stability adjustment
  - Speed control
  - Output format settings
- Voice list refresh functionality

## Supported Voice Models
The application supports the following ElevenLabs voice models:
- `eleven_multilingual_v2` (default)
- `eleven_flash_v2_5`
- `eleven_turbo_v2_5`
- `eleven_v3` (ElevenLabs 3.0 model)
- `eleven_ttv_v3` (ElevenLabs 3.0 model)

## Configuration
The configuration screen provides:
1. API Key management
2. Voice selection dropdown
3. Voice model options (including new 3.0 models)
4. Voice parameter adjustments:
   - Stability settings
   - Speed control
5. Output format selection
6. Load/Refresh voices button

## Getting Started
1. Obtain an API key from Eleven Labs
2. Launch the application and click the Configuration button
3. Enter your API key and click "Load Voices" to populate the voice list
4. Select your preferred voice from the dropdown menu
5. Choose your preferred voice model (including the new 3.0 models)
6. Adjust voice parameters as needed
7. Return to the main screen to start converting text to speech

## Technical Details
- Built with C# and Windows Forms/WPF
- Integrates with Eleven Labs API
- Supports various audio output formats
- Local configuration storage
- .NET 8.0 framework
