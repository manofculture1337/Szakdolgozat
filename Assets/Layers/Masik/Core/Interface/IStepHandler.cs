using System.Collections.Generic;

public interface IStepHandler
{
    Step getCurrentStep();
    bool HasNextStep();
    bool HasPrevStep();
    bool isFirstStep();
    bool isLastStep();
    void loadSteps(List<Step> newSteps);
    void NextStep();
    void PrevStep();
}