using System.Collections.Generic;

public class StepHandler : IStepHandler
{
    private List<Step> steps = new List<Step>();
    private int currentStep = 0;

    public void NextStep()
    {
        currentStep++;
    }

    public bool HasNextStep()
    {
        return currentStep < steps.Count - 1;
    }

    public void PrevStep()
    {
        currentStep--;
    }

    public bool HasPrevStep()
    {
        return currentStep > 0;
    }

    public void loadSteps(List<Step> newSteps)
    {
        steps = newSteps;
        currentStep = 0;
    }

    public Step getCurrentStep()
    {
        return steps[currentStep];
    }

    public bool isLastStep()
    {
        return currentStep == steps.Count - 1;
    }

    public bool isFirstStep()
    {
        return currentStep == 0;
    }
}
