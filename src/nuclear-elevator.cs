const float ELEVATOR_SPEED_SLOW = 1f;
const float ELEVATOR_SPEED_FAST = 3f;

const float ELEVATOR_TOP = 7.9f;
const float ELEVATOR_NEAR_TOP = 6.5f;
const float ELEVATOR_HALF_WAY = ELEVATOR_TOP / 2;

const float ELEVATOR_SPEED_SLOW_UP = ELEVATOR_SPEED_SLOW;
const float ELEVATOR_SPEED_FAST_UP = ELEVATOR_SPEED_FAST;
const float ELEVATOR_SPEED_SLOW_DOWN = -1 * ELEVATOR_SPEED_SLOW;
const float ELEVATOR_SPEED_FAST_DOWN = -1 * ELEVATOR_SPEED_FAST;

IMySensorBlock sensorIn;
IMySensorBlock sensorMid;
IMySensorBlock sensorOut;
IMySensorBlock sensorLoad;

IMyAirtightSlideDoor doorIn;
IMyAirtightSlideDoor doorOut;

IMyExtendedPistonBase elevator1;
IMyExtendedPistonBase elevator2;

IMyAirVent airVent;

IMyProgrammableBlock program;

bool isElevatorLoaded;

enum State {
    Down,
    GoingUpFast,
    GoingDownFast,
    GoingUpSlow,
    GoingDownSlow,
    Up,
}

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;

    sensorIn = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Sensor Inside") as IMySensorBlock;
    sensorMid = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Sensor Middle") as IMySensorBlock;
    sensorOut = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Sensor Outside") as IMySensorBlock;
    sensorLoad = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Sensor Load") as IMySensorBlock;

    doorIn = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Door Inside") as IMyAirtightSlideDoor;
    doorOut = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Door Outside") as IMyAirtightSlideDoor;

    elevator1 = GridTerminalSystem.GetBlockWithName("FSS - Solar Elevator 1") as IMyExtendedPistonBase;
    elevator2 = GridTerminalSystem.GetBlockWithName("FSS - Solar Elevator 2") as IMyExtendedPistonBase;

    airVent = GridTerminalSystem.GetBlockWithName("FSS - Nuclear Elevator Air Vent") as IMyAirVent;

    program = GridTerminalSystem.GetBlockWithName("Nuclear Elevator Logic") as IMyProgrammableBlock;

    isElevatorLoaded = false;
}

public void Save() {}

public void Main(string action, UpdateType updateSource) {
    Echo("Action: " + action);
    State state = (State)Enum.Parse(typeof(State), program.CustomData);

    switch(state) {
        case State.Down:
            DownState(action);
            break;
        case State.GoingUpFast:
            GoingUpFastState(action);
            break;
        case State.GoingDownFast:
            GoingDownFastState(action);
            break;
        case State.GoingUpSlow:
            GoingUpSlowState(action);
            break;
        case State.GoingDownSlow:
            GoingDownSlowState(action);
            break;
        case State.Up:
            UpState(action);
            break;
        default:
            break;
    }

    Echo("State: " + program.CustomData);
    Echo("Elevator 1: " + elevator1.CurrentPosition.ToString());
    Echo("Elevator 2: " + elevator2.CurrentPosition.ToString());
    Echo("Elevator loaded: " + isElevatorLoaded.ToString());
}

// STATES

private void DownState(string action) {
    ElevatorStop();

    if (action == "up") {
        program.CustomData = State.GoingUpFast.ToString();
    }

    if (!IsPlayerNear(sensorLoad)) {
        isElevatorLoaded = false;
    }

    if (IsPlayerNear(sensorLoad) && !isElevatorLoaded) {
        isElevatorLoaded = true;
        program.CustomData = State.GoingUpSlow.ToString();
    }
}

private void GoingUpFastState(string action) {
    ElevatorUpFast();
    CheckUpDoor();
    ExtractAir();

    if (IsElevatorUp()) {
        program.CustomData = State.Up.ToString();
    }
}

private void GoingDownFastState(string action) {
    ElevatorDownFast();
    CheckUpDoor();
    InsertAir();
    
    if (IsElevatorDown()) {
        program.CustomData = State.Down.ToString();
    }
}

private void GoingUpSlowState(string action) {
    ElevatorUpSlow();
    CheckUpDoor();
    ExtractAir();
    
    if (IsElevatorUp()) {
        program.CustomData = State.Up.ToString();
    }
}

private void GoingDownSlowState(string action) {
    ElevatorDownSlow();
    CheckUpDoor();
    InsertAir();
    
    if (IsElevatorDown()) {
        program.CustomData = State.Down.ToString();
    }
}

private void UpState(string action) {
    ElevatorStop();
    
    if (IsPlayerNear(sensorMid) || action == "down") {
        program.CustomData = State.GoingDownFast.ToString();
    }

    if (!IsPlayerNear(sensorLoad)) {
        isElevatorLoaded = false;
    }

    if (IsPlayerNear(sensorLoad) && !isElevatorLoaded) {
        isElevatorLoaded = true;
        program.CustomData = State.GoingDownSlow.ToString();
    }
}

// BLOCKS

private bool IsElevatorDown() {
    return elevator1.CurrentPosition == 0;
}

private bool IsElevatorUp() {
    return elevator1.CurrentPosition >= ELEVATOR_TOP;
}

private void ElevatorDownFast() {
    elevator1.Velocity = ELEVATOR_SPEED_FAST_DOWN;
    elevator2.Velocity = ELEVATOR_SPEED_FAST_DOWN;
}

private void ElevatorUpFast() {
    elevator1.Velocity = ELEVATOR_SPEED_FAST_UP;
    elevator2.Velocity = ELEVATOR_SPEED_FAST_UP;
}

private void ElevatorDownSlow() {
    elevator1.Velocity = ELEVATOR_SPEED_SLOW_DOWN;
    elevator2.Velocity = ELEVATOR_SPEED_SLOW_DOWN;
}

private void ElevatorUpSlow() {
    elevator1.Velocity = ELEVATOR_SPEED_SLOW_UP;
    elevator2.Velocity = ELEVATOR_SPEED_SLOW_UP;
}

private void ElevatorStop() {
    elevator1.Velocity = 0;
    elevator2.Velocity = 0;
}

private void CheckUpDoor() {
    if (elevator1.CurrentPosition >= ELEVATOR_NEAR_TOP) {
        doorOut.OpenDoor();
    } else {
        doorOut.CloseDoor();
    }
}

private void ExtractAir() {
    airVent.Depressurize = true;
}

private void InsertAir() {
    if (doorOut.Status == DoorStatus.Open || doorOut.Status == DoorStatus.Opening || doorOut.Status == DoorStatus.Closing) {
        airVent.Depressurize = true;
    } else {
        airVent.Depressurize = false;
    }
}

private bool IsPlayerNear(IMySensorBlock sensor) {
    List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
    sensor.DetectedEntities(entities);

    return entities.Count > 0;
}
