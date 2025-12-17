using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using VContainer;

public class TutorialView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stepNumberText;
    [SerializeField] private Button prevStepButton;
    [SerializeField] private Button nextStepButton;

    [SerializeField] private Button loadButton;
    
    [SerializeField] private Button TextButton;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [SerializeField] private Button imageButton;
    [SerializeField] private Button nextImageButton;
    [SerializeField] private Button prevImageButton;
    [SerializeField] private RawImage tutorialImage;

    [SerializeField] private Button videoButton;
    [SerializeField] private Button playVideoButton;
    [SerializeField] private Button pauseVideoButton;
    [SerializeField] private Button restartVideoButton;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;

    [SerializeField] private Button audioButton;
    [SerializeField] private Button playAudioButton;
    [SerializeField] private Button pauseAudioButton;
    [SerializeField] private Button restartAudioButton;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private XRReferenceImageLibrary serializedLibrary;
    [SerializeField] private ARTrackedImageManager imageTrackingManager;

    private TutorialPresenter presenter;

    [Inject]
    public void Construct(TutorialPresenter injectedPresenter)
    {
        Debug.Log("TutorialView Constructed");
        presenter = injectedPresenter;
    }

    void Start()
    {
        prevStepButton.onClick.AddListener(() =>
        {
            
            presenter.PrevStep();
            tutorialText.text = presenter.GetText();
            CheckContent();
            ActivateText();
            stepNumberText.text = "Step " + presenter.GetCurrentStepNumber().ToString();
            tutorialText.text = presenter.GetText();
        });
        nextStepButton.onClick.AddListener(() =>
        {
            presenter.NextStep();
            tutorialText.text = presenter.GetText();
            CheckContent();
            ActivateText();
            stepNumberText.text = "Step " + presenter.GetCurrentStepNumber().ToString();
            tutorialText.text = presenter.GetText();
        });
        loadButton.onClick.AddListener(() =>
        {
            Step first=presenter.LoadTutorial(Application.persistentDataPath + "/Saves/kavefozo.zip");
            stepNumberText.text = "Step " + presenter.GetCurrentStepNumber().ToString();
            tutorialText.text = first.Text;
            CheckContent();
        });

        TextButton.onClick.AddListener(() =>
        {
            ActivateText();
            tutorialText.text = presenter.GetText();
        });


        imageButton.onClick.AddListener(() =>
        {
            Texture2D tex = presenter.GetCurrentPic();
            
            ActivateImage();
            if (tex != null)
            {
                tutorialImage.texture = tex;
            }
            tutorialImage.texture = tex;
            if(!presenter.HasNextImage())
            {
                nextImageButton.interactable=false;
            }else
            {
                nextImageButton.interactable=true;
            }
            if (!presenter.HasPrevImage())
            {
                prevImageButton.interactable = false;
            }else
            {
                prevImageButton.interactable = true;
            }

        });
        nextImageButton.onClick.AddListener(() =>
        {
            Texture2D tex = presenter.GetNextPic();
            if (tex != null)
            {
                tutorialImage.texture = tex;
            }
            tutorialImage.texture = tex;
            if(!presenter.HasNextImage())
            {
                nextImageButton.interactable=false;
            }
            if(presenter.HasPrevImage())
            {
                prevImageButton.interactable=true;
            }
        });
        prevImageButton.onClick.AddListener(() =>
        {
            Texture2D tex = presenter.GetPrevPic();
            if (tex != null)
            {
                tutorialImage.texture = tex;
            }
            tutorialImage.texture = tex;
            if(!presenter.HasPrevImage())
            {
                prevImageButton.interactable=false;
            }
            if(presenter.HasNextImage())
            {
                nextImageButton.interactable=true;
            }
        });

        
        videoButton.onClick.AddListener(() => 
        { 
            ActivateVideo();
            videoPlayer.url = presenter.GetVideo();
        });
        playVideoButton.onClick.AddListener(() => { videoPlayer.Play(); });
        pauseVideoButton.onClick.AddListener(() => { videoPlayer.Pause(); });
        restartVideoButton.onClick.AddListener(() => { videoPlayer.time = 0; });


        audioButton.onClick.AddListener(async () => 
        {  
            ActivateAudio();
            audioSource.clip= await presenter.GetAudio(); 
        });
        playAudioButton.onClick.AddListener(() => { audioSource.Play(); });
        pauseAudioButton.onClick.AddListener(() => { audioSource.Pause(); });
        restartAudioButton.onClick.AddListener(() => { audioSource.time = 0; });

        TextButton.onClick.AddListener(() =>
        {
            ActivateText();
            tutorialText.text = presenter.GetText();
        });

        presenter.InitUseCase(imageTrackingManager,serializedLibrary);

        UnactivateAudio();
        UnactivateVideo();
        UnactivateImage();
    }

    public void UnactivateAudio()
    {
        audioSource.Stop();
        playAudioButton.gameObject.SetActive(false);
        pauseAudioButton.gameObject.SetActive(false);
        restartAudioButton.gameObject.SetActive(false);
    }

    public void ActivateAudio()
    {
        DeactivateAll();
        playAudioButton.gameObject.SetActive(true);
        pauseAudioButton.gameObject.SetActive(true);
        restartAudioButton.gameObject.SetActive(true);
    }

    public void UnactivateVideo()
    {
        videoPlayer.Stop();
        playVideoButton.gameObject.SetActive(false);
        pauseVideoButton.gameObject.SetActive(false);
        restartVideoButton.gameObject.SetActive(false);
        videoDisplay.gameObject.SetActive(false);
    }

    public void ActivateVideo()
    {
        DeactivateAll();
        playVideoButton.gameObject.SetActive(true);
        pauseVideoButton.gameObject.SetActive(true);
        restartVideoButton.gameObject.SetActive(true);
        videoDisplay.gameObject.SetActive(true);
    }

    public void UnactivateImage()
    {
        nextImageButton.gameObject.SetActive(false);
        prevImageButton.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);
    }

    public void ActivateImage()
    {
        DeactivateAll();
        nextImageButton.gameObject.SetActive(true);
        prevImageButton.gameObject.SetActive(true);
        tutorialImage.gameObject.SetActive(true);
    }

    public void UnactivateText()
    {
        tutorialText.gameObject.SetActive(false);
    }

    public void ActivateText()
    {
        DeactivateAll();
        tutorialText.gameObject.SetActive(true);
    }

    public void DeactivateAll()
    {   
        UnactivateAudio();
        UnactivateVideo();
        UnactivateImage();
        UnactivateText();
    }

    private void CheckContent()
    {
        if (presenter.GetAudioPath()!=null)
        {

            audioButton.interactable = true;
        }
        else
        {
            audioButton.interactable = false;
        }

        if (presenter.GetVideo() != null)
        {
            videoButton.interactable = true;
        }
        else
        {
            videoButton.interactable = false;
        }

        if (presenter.GetCurrentPic() != null)
        {
            imageButton.interactable = true;
        }
        else
        {
            imageButton.interactable = false;
        }
        
        if( presenter.IsFirstStep())
        {
            prevStepButton.interactable = false;
        }
        else
        {
            prevStepButton.interactable = true;
        }

        if( presenter.IsLastStep())
        {
            nextStepButton.interactable = false;
        }
        else
        {
            nextStepButton.interactable = true;
        }
    }
}
