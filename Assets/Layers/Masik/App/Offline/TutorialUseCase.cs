using System.Collections.Generic;
public class TutorialUseCase
{
    private IZiphandler ziphandler = new Ziphandler();

    private IStepHandler stepHandler;

    public TutorialUseCase(IStepHandler handler)
    {
        stepHandler = handler;
    }

    public void NextStep()
    {
        if (stepHandler.HasNextStep())
        {
            stepHandler.NextStep();
        }
    }


    public void PrevStep()
    {
        if (stepHandler.HasPrevStep())
        {
            stepHandler.PrevStep();
        }
    }

    public void LoadTutorial(string zipPath)
    {
        List<Step> steps = ziphandler.LoadStepsFromZip(zipPath);
        stepHandler.loadSteps(steps);
    }

    public string GetTutorialText()
    {
        return stepHandler.getCurrentStep().Text;
    }

    public List<PicData> GetCurrentStepImages()
    {
        return stepHandler.getCurrentStep().Images;
    }

    public string GetCurrentStepAudio()
    {
        return stepHandler.getCurrentStep().Audio;
    }

    public string GetCurrentStepVideo()
    {
        return stepHandler.getCurrentStep().Video;
    }

}