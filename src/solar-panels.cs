IMyProgrammableBlock program;
List<IMyMotorAdvancedStator> doorRotors = new List<IMyMotorAdvancedStator>();
List<IMyMotorStator> solarMainRotors = new List<IMyMotorStator>();

List<IMyMotorStator> solarRotors = new List<IMyMotorStator>();
List<IMyExtendedPistonBase> masts = new List<IMyExtendedPistonBase>();
List<IMyExtendedPistonBase> catwalk = new List<IMyExtendedPistonBase>();
List<IMyReflectorLight> lights = new List<IMyReflectorLight>();
IMyTimerBlock openingTimer;
IMyTimerBlock turningTimer;
IMyTimerBlock extendingTimer;
IMyTimerBlock deployingTimer;

public Program() {
    GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(doorRotors, rotor => rotor.CustomName.Contains("FSS - Solar Door Rotor"));
    GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(solarMainRotors, rotor => rotor.CustomName.Contains("FSS - Solar Main Rotor"));
    GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(solarRotors, rotor => rotor.CustomName.Contains("FSS - Solar Rotor"));
    GridTerminalSystem.GetBlocksOfType<IMyExtendedPistonBase>(masts, piston => piston.CustomName.Contains("FSS - Solar Piston"));
    GridTerminalSystem.GetBlocksOfType<IMyExtendedPistonBase>(catwalk, piston => piston.CustomName.Contains("FSS - Solar Catwalk Piston"));
    GridTerminalSystem.GetBlocksOfType<IMyReflectorLight>(lights, light => light.CustomName.Contains("FSS - Solar Panel Light"));
    program = GridTerminalSystem.GetBlockWithName("Solar Panels Logic") as IMyProgrammableBlock;
    openingTimer = GridTerminalSystem.GetBlockWithName("FSS - Solar Panels Timer Open") as IMyTimerBlock;
    turningTimer = GridTerminalSystem.GetBlockWithName("FSS - Solar Panels Timer Turn") as IMyTimerBlock;
    extendingTimer = GridTerminalSystem.GetBlockWithName("FSS - Solar Panels Timer Extend") as IMyTimerBlock;
    deployingTimer = GridTerminalSystem.GetBlockWithName("FSS - Solar Panels Timer Deploy") as IMyTimerBlock;
}

public void Save() {}

enum State {
    Closed,
    Opening,
    Closing,
    TurningOut,
    TurningIn,
    Extending,
    Retracting,
    Deploying,
    Collecting,
    Deployed
}

public void Main(string action, UpdateType updateSource) {
    Echo(action);
    State state = (State)Enum.Parse(typeof(State), program.CustomData);

    switch(state) {
        case State.Closed:
            ClosedState(action);
            break;
        case State.Opening:
            OpeningState(action);
            break;
        case State.Closing:
            ClosingState(action);
            break;
        case State.TurningOut:
            TurningOutState(action);
            break;
        case State.TurningIn:
            TurningInState(action);
            break;
        case State.Extending:
            ExtendingState(action);
            break;
        case State.Retracting:
            RetractingState(action);
            break;
        case State.Deploying:
            DeployingState(action);
            break;
        case State.Collecting:
            CollectingState(action);
            break;
        case State.Deployed:
            DeployedState(action);
            break;
        default:
            break;
    }

    Echo(program.CustomData);
}

// STATES

private void ClosedState(string action) {
    OpenDoors();
    ToggleLights();
}

private void OpeningState(string action) {
    if (action != null) {
        TurnOut();
    } else {
        CloseDoors();
    }
}

private void ClosingState(string action) {
    if (action != null) {
        program.CustomData = State.Closed.ToString();
        ToggleLights();
    } else {
        OpenDoors();
    }
}

private void TurningOutState(string action) {
    if (action != null) {
        ExtendMasts();
    } else {
        TurnIn();
    }
}

private void TurningInState(string action) {
    if (action != null) {
        CloseDoors();
    } else {
        TurnOut();
    }
}

private void ExtendingState(string action) {
    if (action != null) {
        DeployPanels();
    } else {
        RetractMasts();
    }
}

private void RetractingState(string action) {
    if (action != null) {
        TurnIn();
    } else {
        ExtendMasts();
    }
}

private void DeployingState(string action) {
    if (action != null) {
        program.CustomData = State.Deployed.ToString();
        ToggleLights();
    } else {
       CollectPanels();
    }
}

private void CollectingState(string action) {
    if (action != null) {
        RetractMasts();
    } else {
        DeployPanels();
    }
}

private void DeployedState(string action) {
    CollectPanels();
    ToggleLights();
}

// OPEN

private void OpenDoors() {
    ToggleDoors();
    program.CustomData = State.Opening.ToString();
    StopTimers();
    Echo(openingTimer.TriggerDelay.ToString());
    openingTimer.StartCountdown();
}

private void TurnOut() {
    ToggleRotation();
    program.CustomData = State.TurningOut.ToString();
    StopTimers();
    turningTimer.TriggerDelay = 2;
    Echo(turningTimer.TriggerDelay.ToString());
    turningTimer.StartCountdown();
}

private void ExtendMasts() {
    QuickMasts();
    ToggleMasts();
    program.CustomData = State.Extending.ToString();
    StopTimers();
    extendingTimer.TriggerDelay = 9;
    Echo(extendingTimer.TriggerDelay.ToString());
    extendingTimer.StartCountdown();
}


private void DeployPanels() {
    TogglePanels();
    program.CustomData = State.Deploying.ToString();
    StopTimers();
    deployingTimer.TriggerDelay = 14;
    Echo(deployingTimer.TriggerDelay.ToString());
    deployingTimer.StartCountdown();
}

// CLOSE

private void CollectPanels() {
    TogglePanels();
    program.CustomData = State.Collecting.ToString();
    StopTimers();
    deployingTimer.TriggerDelay = 7;
    Echo(deployingTimer.TriggerDelay.ToString());
    deployingTimer.StartCountdown();
}

private void RetractMasts() {
    SlowMasts();
    ToggleMasts();
    program.CustomData = State.Retracting.ToString();
    StopTimers();
    extendingTimer.TriggerDelay = 0;
    Echo(extendingTimer.TriggerDelay.ToString());
    extendingTimer.StartCountdown();
}

private void TurnIn() {
    ToggleRotation();
    program.CustomData = State.TurningIn.ToString();
    StopTimers();
    turningTimer.TriggerDelay = 20;
    Echo(turningTimer.TriggerDelay.ToString());
    turningTimer.StartCountdown();
}

private void CloseDoors() {
    ToggleDoors();
    program.CustomData = State.Closing.ToString();
    StopTimers();
    Echo(openingTimer.TriggerDelay.ToString());
    openingTimer.StartCountdown();
}

// TOGGLE

private void ToggleLights() {
    foreach(IMyReflectorLight light in lights) {
        light.Enabled = !light.Enabled;
    }
}

private void ToogleCatwalk() {
    foreach(IMyExtendedPistonBase piston in catwalk) {
        piston.Reverse();
    }
}

private void ToggleDoors() {
    foreach(IMyMotorAdvancedStator rotor in doorRotors) {
        rotor.TargetVelocityRPM = (-1) * rotor.TargetVelocityRPM;
    }

    ToogleCatwalk();
}

private void ToggleRotation() {
    foreach(IMyMotorStator rotor in solarMainRotors) {
        rotor.TargetVelocityRPM = (-1) * rotor.TargetVelocityRPM;
    }
}

private void ToggleMasts() {
    foreach(IMyExtendedPistonBase piston in masts) {
        piston.Reverse();
    }
}

private void QuickMasts() {
    foreach(IMyExtendedPistonBase piston in masts) {
        piston.Velocity = piston.Velocity > 0 ? 1 : -1;
    }
}

private void SlowMasts() {
    foreach(IMyExtendedPistonBase piston in masts) {
        piston.Velocity = piston.Velocity > 0 ? 0.6f : -0.6f;
    }
}
private void TogglePanels() {
    foreach(IMyMotorStator rotor in solarRotors) {
        rotor.TargetVelocityRPM = (-1) * rotor.TargetVelocityRPM;
    }

    ToogleCatwalk();
}

private void StopTimers() {
    openingTimer.StopCountdown();
    turningTimer.StopCountdown();
    extendingTimer.StopCountdown();
}
