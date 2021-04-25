﻿using UnityEngine;

namespace SAL
{
    public interface TouchDetector : UISDetector
    {
        void TouchStarted(UserInputService.InputObject inputObject, bool wasProcessed);

        void TouchEnded(UserInputService.InputObject inputObject, bool wasProcessed);
    }
}