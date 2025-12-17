
public class DetectUseCase
{
    private ITargetImageHandler targetImageHandler;

    public DetectUseCase(ITargetImageHandler targetImageHandler)
    {
        this.targetImageHandler = targetImageHandler;
    }

    public void StartDetection()
    { 
        targetImageHandler.StartDetection();
    }

    public void StopDetection()
    {
        targetImageHandler.StopDetection();
    }

    public void AddImage(byte[] pictureData)
    {
        targetImageHandler.AddImage(pictureData);
    }
}
