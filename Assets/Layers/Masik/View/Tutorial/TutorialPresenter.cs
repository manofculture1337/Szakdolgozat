using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using VContainer;

public class TutorialPresenter : MonoBehaviour
{
    private TutorialUseCase tutorialUseCase;
    private DetectUseCase detectUseCase;

    [Inject]
    private IStepHandler stepHandler;

    private List<Texture2D> images = new List<Texture2D>();
    private int currentImageIndex = 0;

    private string audioPath;

    void Start()
    {
        tutorialUseCase = new TutorialUseCase(stepHandler);
    }

    public void NextStep()
    {
        tutorialUseCase.NextStep();
        loadImages();
    }

    public void PrevStep()
    {
        tutorialUseCase.PrevStep();
        loadImages();
    }

    public void loadImages()
    {
        List<PicData> stepImageDatas = tutorialUseCase.GetCurrentStepImages();
        List<Texture2D> loadedImages = new List<Texture2D>();
        currentImageIndex = 0;
        foreach (PicData stepImage in stepImageDatas)
        {
            
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(stepImage.data);
            loadedImages.Add(texture);
        }
        images = loadedImages;
    }

    public Step LoadTutorial(string zipPath)
    {
        tutorialUseCase.LoadTutorial(zipPath);
        loadImages();

        return new Step()
        {
            StepNumber = stepHandler.getCurrentStep().StepNumber,
            Images = tutorialUseCase.GetCurrentStepImages(),
            Text = tutorialUseCase.GetTutorialText(),
            Video = tutorialUseCase.GetCurrentStepVideo(),
            Audio = tutorialUseCase.GetCurrentStepAudio()
        };
    }

    public Texture2D GetCurrentPic()
    {
        if(images.Count==0)
            return null;
        return images[currentImageIndex];
    }

    public Texture2D GetNextPic()
    {
        if(images.Count<=currentImageIndex)
            return null;
        currentImageIndex++;
        return images[currentImageIndex];
    }

    public Texture2D GetPrevPic()
    {
        if(currentImageIndex<=0)
            return null;
        currentImageIndex--;
        return images[currentImageIndex];
    }

    public int GetCurrentStepNumber()
    {
        return stepHandler.getCurrentStep().StepNumber;
    }

    public string GetVideo()
    {
        return tutorialUseCase.GetCurrentStepVideo();
    }

    public bool IsFirstStep()
    {
        return stepHandler.isFirstStep();
    }

    public bool IsLastStep()
    {
        return stepHandler.isLastStep();
    }

    public async Task<AudioClip> GetAudio()
    {
        audioPath=GetAudioPath();
        string url = Path.Combine("file://", audioPath);

        using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            var operation = audioRequest.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (audioRequest.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(audioRequest);
            }
            else
            {
                Debug.LogError("Audio load failed: " + audioRequest.error);
                return null;
            }
        }
    }

    public string GetAudioPath()
    {
        return tutorialUseCase.GetCurrentStepAudio();
    }

    public void AddImageToDetect()
    {
        //Idekerül a kep hozzadas logika
        //detectUseCase.AddImage(byte[]);
    }


    public string GetText()
    {
        return tutorialUseCase.GetTutorialText();
    }

    public bool HasNextImage()
    {
        return currentImageIndex<images.Count-1;
    }

    public bool HasPrevImage()
    {
        return currentImageIndex>0;
    }

    //Screenshot fuggvenyek ahova kell
    /*public void TakeScreenshot()
    {
        string screenshotPath = Application.persistentDataPath + "/screenshot_placeholder.png";
        ScreenCapture.CaptureScreenshot("Assets/screenshot_placeholder.png");
        StartCoroutine(SaveScreenshotWhenReady(screenshotPath));
    }

    private IEnumerator SaveScreenshotWhenReady(string screenshotPath)
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log(screenshotPath);
        while (!File.Exists(screenshotPath))
            yield return null;

        documentationUseCase.TakeScreenshot(screenshotPath, Application.persistentDataPath);
        File.Delete(screenshotPath);
    }*/

    public void InitUseCase(ARTrackedImageManager imageTrackingManager, XRReferenceImageLibrary serializedLibrary)
    {
        detectUseCase = new DetectUseCase(new TargetImageHandler(imageTrackingManager, serializedLibrary));
    }
}
