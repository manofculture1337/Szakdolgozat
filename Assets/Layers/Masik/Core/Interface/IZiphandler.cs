using System.Collections.Generic;

public interface IZiphandler
{
    List<Step> LoadStepsFromZip(string zipPath);
    void saveScreenshotToZip(string zipPath, byte[] imageBytes, string fileName);
}