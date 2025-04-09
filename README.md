# Text2Model - Unity Plugin

## Overview
Text2Model is a Unity Editor plugin that makes AI Model Generation easy. The plugin leverages OpenAI's DALL-E model to create images based on text descriptions, then processes these images through Tencent's Hunyuan3D2 to generate 3D models.

## Features
- Generate one or multiple image variations from a single prompt using DALLE3
- Select your preferred image from the generated options
- Convert selected images to 3D models using Hunyuan3D2
- Save generated models and images directly in your Unity project
- Settings persistence between editor sessions

## Future Plan
- Prompt Tuning - Avoid the issues of AI-Generated Images not identifiable by Hunyuan Model in some cases
- MCP Integration - Have LLM directly call the model generation via MCP
- Runtime Generation - Bypass Scene Reload
- More Models Integration - A unified pipeline

## Prerequisites
- Unity 2020.3 or newer
- OpenAI API key with Organization and Project ID
- Python 3.11+ with Hunyuan3D-2 installed

## Setup Instructions

### 1. API and Python Setup
- Obtain an OpenAI API key from [OpenAI Platform](https://platform.openai.com/)
- Install Python and the Hunyuan3D-2 library on your system(https://github.com/Tencent/Hunyuan3D-2)
- Install OpenAI-Unity(https://github.com/RageAgainstThePixel/com.openai.unity)
- Ensure the Hunyuan3D-2 and OpenAI-Unity are properly configured
- Put Text2Model.cs in Unity Project folder anywhere
- Put hunyuanDemo.py under the Hunyuan3D-2 folder
- The Text2Model process can be run through Editor or Function call

### 2. Plugin Configuration
1. Open the Text2Model window from the Unity menu bar: **Tools > Text2Model Generator**
2. Expand the **Settings** section
3. Enter your OpenAI API Key, Organization ID, and Project ID (if applicable)
4. Set the Python Path to your Python executable (e.g., "D:/Conda/python.exe")
5. Set the Python Script Path to your Hunyuan3D script location(e.g., "F:/Hunyuan3D-2/examples/hunyuanDemo.py)
6. Specify the output folder for models and temporary files
7. Click "Save Settings" to persist your configuration

## Usage

### Generate a 3D Model from Text
1. Enter a descriptive prompt in the text field (e.g., "A detailed castle on a hill")
2. Click "Generate Images" to create images based on your prompt
3. Select your preferred image from the displayed options
4. Click "Generate 3D Model from Selected Image" to create a 3D model
5. The model will be saved to your specified Resources folder

### Advanced Configuration
- **Image Count**: Control the number of images generated from each prompt
- **Image Size**: Set the resolution of generated images (e.g., "1024x1024")
- **UI Settings**: Adjust how images are displayed in the editor window

### Scripting Integration(For MCP Integration Later)
You can also generate models programmatically using the `GenerateModelFromPrompt` method:

```csharp
// Get a reference to the editor window
var window = EditorWindow.GetWindow<Text2ModelEditor>();

// Generate a model from prompt (will use the first generated image)
window.GenerateModelFromPrompt("A detailed castle on a hill");
```

## Troubleshooting

### Common Issues
- Model Generation Issue: The model currently used(Hunyuan3D-2 mini) is incredible fast, but also cannot recognize certain AIGC images. Will need to prompt it further to have a proper generation

## License
This plugin is provided under the MIT License.

## Credits
- Text2Model uses OpenAI's DALL-E for image generation
- 3D model generation is powered by Hunyuan3D
- OpenAI Unity through com.openai.unity(https://github.com/RageAgainstThePixel/com.openai.unity)