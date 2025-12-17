public interface ITargetImageHandler
{
    void AddImage(byte[] pictureData);
    MarkerData GetMarkerData();
    void StartDetection();
    void StopDetection();
}