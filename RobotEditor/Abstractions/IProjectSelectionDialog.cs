﻿namespace RobotEditor.Abstractions
{
    public interface IProjectSelectionDialog
    {
        bool InitProjectWithDialog();
        bool OpenNewProgramInstanceOfProjectWithDialog();
    }
}