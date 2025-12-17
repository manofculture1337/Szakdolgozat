using System;
using System.Collections.Generic;

public class Step
{
    private int stepNumber;
    private List<PicData> images= new List<PicData>();
    private string text;
    private string video=null;
    private string audio=null;

    public int StepNumber { get => stepNumber; set => stepNumber = value; }
    public List<PicData> Images { get => images; set => images = value; }
    public string Text { get => text; set => text = value; }
    public string Video { get => video; set => video = value; }
    public string Audio { get => audio; set => audio = value; }
}
