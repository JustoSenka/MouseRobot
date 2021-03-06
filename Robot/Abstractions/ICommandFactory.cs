﻿using RobotRuntime;
using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    public interface ICommandFactory
    {
        IEnumerable<string> CommandNames { get; }

        Command Create(string commandName);
        Command Create(string commandName, Command oldCommand);

        event Action NewUserCommands;

        bool IsNative(Command command);
    }
}