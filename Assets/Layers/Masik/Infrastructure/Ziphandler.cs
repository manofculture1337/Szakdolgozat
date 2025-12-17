using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class Ziphandler : IZiphandler
{

    private PicData LoadPic(ZipArchiveEntry pic)
    {
        PicData imageData = new PicData();

        using (var stream = pic.Open())
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                imageData.data = memoryStream.ToArray();
            }

        }

        return imageData;

    }

    private Step LoadStepFromFolder(IGrouping<string, ZipArchiveEntry> stepFolder, string zipPath)
    {
        var step = new Step();

        foreach (var entry in stepFolder)
        {
            if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                step.Images.Add(LoadPic(entry));
            }

            if (entry.FullName.EndsWith("mp3", StringComparison.OrdinalIgnoreCase))
            {
                step.Audio = Path.Combine(zipPath, entry.FullName);
            }

            if (entry.FullName.EndsWith("mp4", StringComparison.OrdinalIgnoreCase))
            {
                step.Video = Path.Combine(zipPath, entry.FullName);
            }

            if (entry.FullName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
            {
                step.Text = LoadText(entry);
            }
        }
        return step;
    }

    private string LoadText(ZipArchiveEntry entry)
    {
        string text;
        using (StreamReader reader = new StreamReader(entry.Open()))
        {
            text = reader.ReadToEnd();
        }
        return text;
    }

    public List<Step> LoadStepsFromZip(string zipPath)
    {
        List<Step> loadedSteps = new List<Step>();

        using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {
                var fileEntries = archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name));

                var stepFolders = fileEntries.GroupBy(e =>
                {
                    var parts = e.FullName.Split('/');
                    return parts.Length > 1 ? parts[0] : "";
                });
                foreach (var stepFolder in stepFolders)
                {
                    var step = LoadStepFromFolder(stepFolder, zipPath);
                    step.StepNumber = loadedSteps.Count + 1;
                    loadedSteps.Add(step);
                }

            }
        }


        return loadedSteps;
    }
    public void saveScreenshotToZip(string zipPath, byte[] imageBytes, string fileName)
    {

        using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {
                ZipArchiveEntry imageEntry = archive.CreateEntry(fileName);
                using (var entryStream = imageEntry.Open())
                {
                    entryStream.Write(imageBytes, 0, imageBytes.Length);
                }
            }
        }
    }
}
