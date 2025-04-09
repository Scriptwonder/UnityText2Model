using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenAI;
using OpenAI.Models;
using OpenAI.Images;
using UnityEngine.Assertions;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using System.IO.Pipes;

[InitializeOnLoad]
public class Text2ModelEditor : EditorWindow
{
    // User input fields for the prompt
    [SerializeField] private string prompt = "Enter your prompt here...";
    
    // List of generated images and selection
    private List<Texture2D> generatedImages = new List<Texture2D>();

    private List<string> generatedImagePaths = new List<string>();


    // Settings
    [Header("Image Generation Settings")]
    [SerializeField, Tooltip("Temporary directory to store generated images")]
    private string tempDir = "Assets/Temp/";

    [Header("Image Generation Settings")]
    [SerializeField, Range(1, 10), Tooltip("Number of images to generate")]
    private int imageCount = 3;

    [SerializeField, Tooltip("Size of generated images (e.g. 1024x1024)")]
    private string imageSize = "1024x1024";
    
    // OpenAI API settings
    [Header("OpenAI Settings")]
    [SerializeField, Tooltip("Your OpenAI API Key")]
    private string openaiApiKey = "";
    [SerializeField, Tooltip("Your OpenAI Organization ID")]
    private string orgID="";

    [SerializeField, Tooltip("Your OpenAI Project ID")]
    private string projectID="";
    
    

    //processInfo
     [Header("Python Settings")]
    [SerializeField, Tooltip("Full path to Python executable")]
    private string pythonPath = "D:/Conda/python.exe";

    [SerializeField, Tooltip("Full path to Hunyuan3D script")]
    private string pythonScriptPath = "f:/Hunyuan3D-2/examples/hunyuanDemo.py";

    [SerializeField, Tooltip("Resources folder to save generated models")]
    private string resourcesFolder = "Assets/Temp/Models/";

    [SerializeField, Tooltip("Directory to store generated 3D models")]
    private string outputObjName = "Default";


    private OpenAIClient api;
    private Vector2 scrollPosition;
    private Vector2 settingsScrollPosition;
    private bool showSettings = false;
    private int selectedImageIndex = -1;

    // UI layout variables
    private int imagesPerRow = 3;
    private int imageSize_UI = 200;

    void Awake()
    {
        //Editor wont go through Awake!!
        //api = new OpenAIClient(new OpenAIAuthentication(openaiApiKey));
    }

    async Task<IReadOnlyList<ImageResult>> CreateImageFromPrompt(string prompt) {
        api = new OpenAIClient(new OpenAIAuthentication(openaiApiKey, orgID, projectID));
        //ImageGenerationRequest request = new ImageGenerationRequest(prompt, Model.GPT4o);
        var request = new ImageGenerationRequest(prompt, Model.DallE_3, 1, null, ImageResponseFormat.B64_Json, imageSize);
        var imageResult = await api.ImagesEndPoint.GenerateImageAsync(request);
        UnityEngine.Debug.Log($"Image generation completed. Number of images: {imageResult.Count}");
        foreach (var result in imageResult) {
            UnityEngine.Debug.Log(result.ToString());
            Assert.IsNotNull(result.Texture);
        }
        return imageResult;
    }

    async Task<IReadOnlyList<ImageResult>> CreateImagesFromPrompt(string prompt, int count) {
        List<ImageResult> imageResults = new List<ImageResult>();
        for (int i = 0; i < count; i++) {
            var result = await CreateImageFromPrompt(prompt);
            imageResults.AddRange(result);
        }
        return imageResults;
    }

    [MenuItem("Tools/Text2Model Generator")]
    public static void ShowWindow()
    {
        GetWindow<Text2ModelEditor>("Text2Model");
    }

    private void OnGUI()
    {
        GUILayout.Label("Text to 3D Model Generator", EditorStyles.boldLabel);

        // Settings foldout
        showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);
        if (showSettings)
        {
            settingsScrollPosition = EditorGUILayout.BeginScrollView(settingsScrollPosition, GUILayout.Height(200));
            
            // OpenAI API Settings
            EditorGUILayout.LabelField("OpenAI API Settings", EditorStyles.boldLabel);
            openaiApiKey = EditorGUILayout.PasswordField(new GUIContent("API Key", "Your OpenAI API Key"), openaiApiKey);
            orgID = EditorGUILayout.TextField(new GUIContent("Organization ID", "Your OpenAI Organization ID"), orgID);
            projectID = EditorGUILayout.TextField(new GUIContent("Project ID", "Your OpenAI Project ID"), projectID);
            
            EditorGUILayout.Space(10);
            
            // Image Generation Settings
            EditorGUILayout.LabelField("Image Generation Settings", EditorStyles.boldLabel);
            imageCount = EditorGUILayout.IntSlider(new GUIContent("Image Count", "Number of images to generate"), imageCount, 1, 10);
            imageSize = EditorGUILayout.TextField(new GUIContent("Image Size", "Size of generated images (e.g. 1024x1024)"), imageSize);
            tempDir = EditorGUILayout.TextField(new GUIContent("Temp Directory", "Directory to store temporary files"), tempDir);
            
            EditorGUILayout.Space(10);
            
            // Python Settings
            EditorGUILayout.LabelField("Python Settings", EditorStyles.boldLabel);
            pythonPath = EditorGUILayout.TextField(new GUIContent("Python Path", "Full path to Python executable"), pythonPath);
            pythonScriptPath = EditorGUILayout.TextField(new GUIContent("Python Script", "Full path to Hunyuan3D script"), pythonScriptPath);
            resourcesFolder = EditorGUILayout.TextField(new GUIContent("Models Folder", "Directory to store generated 3D models"), resourcesFolder);
            outputObjName = EditorGUILayout.TextField(new GUIContent("Output OBJ Name", "Name for the output OBJ file, default if no given"), outputObjName);
            
            // UI Settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
            imagesPerRow = EditorGUILayout.IntSlider(new GUIContent("Images Per Row", "Number of images to display per row"), imagesPerRow, 1, 5);
            imageSize_UI = EditorGUILayout.IntSlider(new GUIContent("UI Image Size", "Size of images in the UI"), imageSize_UI, 100, 500);
            
            EditorGUILayout.EndScrollView();

            // Save settings button
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Save Settings"))
            {
                EditorPrefs.SetString("OpenAI_ApiKey", openaiApiKey);
                EditorPrefs.SetString("OpenAI_OrgID", orgID);
                EditorPrefs.SetString("OpenAI_ProjectID", projectID);
                EditorPrefs.SetString("PythonPath", pythonPath);
                EditorPrefs.SetString("ModelGenerationScriptPath", pythonScriptPath);
                EditorPrefs.SetString("ResourceFolder", resourcesFolder);
                EditorPrefs.SetString("OutputObjName", outputObjName);
                EditorUtility.DisplayDialog("Settings Saved", "Your settings have been saved for future sessions.", "OK");
            }
            
            // Load settings button
            if (GUILayout.Button("Load Saved Settings"))
            {
                LoadSavedSettings();
            }
            
            EditorGUILayout.Space(10);
        }
        
        // Step 1: Generate images from text prompt
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Step 1: Generate Images from Text", EditorStyles.boldLabel);
        
        prompt = EditorGUILayout.TextField("Prompt", prompt);
        
        if (GUILayout.Button("Generate Images"))
        {
            GenerateImagesFromPrompt();
        }
        
        EditorGUILayout.Space(10);
        
        // Step 2: Display generated images for selection
        if (generatedImages.Count > 0)
        {
            EditorGUILayout.LabelField("Step 2: Select an Image and Generate 3D Model", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < generatedImages.Count; i++)
            {
                if (i > 0 && i % 3 == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                
                EditorGUILayout.BeginVertical(GUILayout.Width(120));
                
                GUI.backgroundColor = (selectedImageIndex == i) ? Color.cyan : Color.white;
                if (GUILayout.Button(new GUIContent(generatedImages[i]), GUILayout.Width(300), GUILayout.Height(300)))
                {
                    selectedImageIndex = i;
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(selectedImageIndex < 0);
            if (GUILayout.Button("Generate 3D Model from Selected Image"))
            {
                GenerateMeshFromImage(selectedImageIndex);
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    private void LoadSavedSettings()
    {
        openaiApiKey = EditorPrefs.GetString("OpenAI_ApiKey", openaiApiKey);
        orgID = EditorPrefs.GetString("OpenAI_OrgID", orgID);
        projectID = EditorPrefs.GetString("OpenAI_ProjectID", projectID);
        pythonPath = EditorPrefs.GetString("PythonPath", pythonPath);
        pythonScriptPath = EditorPrefs.GetString("ModelGenerationScriptPath", pythonScriptPath);
        resourcesFolder = EditorPrefs.GetString("ResourceFolder", resourcesFolder);
        outputObjName = EditorPrefs.GetString("OutputObjName", outputObjName);
    }

    private void OnEnable()
    {
        // Load settings when the window is opened
        LoadSavedSettings();
    }

    private async void GenerateImagesFromPrompt()
    {
        // Clear previous images
        generatedImages.Clear();
        selectedImageIndex = -1;
        
        EditorUtility.DisplayProgressBar("Generating Images", "Sending request to OpenAI...", 0.3f);
        
        try
        {
            // Use the CreateImageFromPrompt method to generate images
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var imageResults = await CreateImagesFromPrompt(prompt, imageCount);
            
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Image generation completed in {stopwatch.ElapsedMilliseconds}ms");
            
            UnityEngine.Debug.Log("Image generation completed. Number of images: " + imageResults.Count);
            // Ensure temp directory exists
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            
            EditorUtility.DisplayProgressBar("Generating Images", "Processing images...", 0.6f);
            
            int count = 0;
            foreach (var result in imageResults)
            {
                // Save the texture to disk
                string imagePath = Path.Combine(tempDir, $"generated_{count}.png");
                File.WriteAllBytes(imagePath, result.Texture.EncodeToPNG());
                
                // Add the texture to our list
                generatedImages.Add(result.Texture);
                generatedImagePaths.Add(imagePath);
                count++;
            }
            
            AssetDatabase.Refresh();
            Repaint();
            EditorUtility.DisplayDialog("Success", $"Generated {imageResults.Count} images from your prompt.", "OK");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Image generation error: " + e);
            EditorUtility.DisplayDialog("Error", "Failed to generate images: " + e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void GenerateMeshFromImage(int inputImageIndex)
    {
        UnityEngine.Debug.Log("Generating mesh from image at index: " + inputImageIndex);
        var inputImage = generatedImages[inputImageIndex];
        var imagePath = generatedImagePaths[inputImageIndex];
        if (inputImage == null)
        {
            EditorUtility.DisplayDialog("Error", "No image selected.", "OK");
            return;
        }

        EditorUtility.DisplayProgressBar("Generating 3D Model", "Processing image...", 0.2f);

        try
        {
            // Save the selected image to a temporary location
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

                
            imagePath = imagePath=="" ? Path.Combine(tempDir, "selected_image.png") : imagePath;
            File.WriteAllBytes(imagePath, inputImage.EncodeToPNG());
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayProgressBar("Generating 3D Model", "Running Python script...", 0.4f);

            // Set up the process to call the Python script
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = @pythonPath;
            processInfo.Arguments = $"\"{pythonScriptPath}\" \"{imagePath}\" \"{outputObjName}\" \"{resourcesFolder}\"";
            UnityEngine.Debug.Log($"Python script arguments: {processInfo.Arguments}");
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError("Python script error: " + error);
                EditorUtility.DisplayDialog("Error", "Failed to generate 3D model. See console for details.", "OK");
                return;
            }
            
            EditorUtility.DisplayProgressBar("Generating 3D Model", "Importing model...", 0.8f);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error generating mesh: " + e);
            EditorUtility.DisplayDialog("Error", "An error occurred: " + e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    //A one time function call to generate model from prompt. Intended to use for MCP function calls
    //Directly retrieve image from prompt and generate model from it
    public async void GenerateModelFromPrompt(string prompt)
    {
        // Clear previous images
        generatedImages.Clear();
        selectedImageIndex = -1;
        
        EditorUtility.DisplayProgressBar("Generating Images", "Sending request to OpenAI...", 0.3f);
        
        try
        {
            // Use the CreateImageFromPrompt method to generate images
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            //var imageResults = await CreateImagesFromPrompt(prompt, imageCount);
            var imageResults = await CreateImageFromPrompt(prompt);

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Image generation completed in {stopwatch.ElapsedMilliseconds}ms");

            // Ensure temp directory exists
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            
            EditorUtility.DisplayProgressBar("Generating Images", "Processing images...", 0.6f);
            
            int count = 0;
            // Save the texture to disk
            string imagePath = Path.Combine(tempDir, $"generated_{count}.png");
            File.WriteAllBytes(imagePath, imageResults[0].Texture.EncodeToPNG());
            
            // Add the texture to our list
            generatedImages.Add(imageResults[0].Texture);
            generatedImagePaths.Add(imagePath);
            //Generate Mesh
            GenerateMeshFromImage(0);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Generated {imageResults.Count} images from your prompt.", "OK");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Image generation error: " + e);
            EditorUtility.DisplayDialog("Error", "Failed to generate images: " + e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}