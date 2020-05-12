public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Save() {}

public void Main(string argument) {
    IMyAirtightHangarDoor doorIn1;
    IMyAirtightHangarDoor doorIn2;
    IMyAirtightHangarDoor doorOut1;
    IMyAirtightHangarDoor doorOut2;

    IMyLightingBlock light1;
    IMyLightingBlock light2;
    IMyLightingBlock lightIn;
    IMyLightingBlock lightOut;

    IMyTimerBlock timer;

    if (argument.Length > 0) {
        string[] args = argument.Split(new char[] { ',' });
        string name = args[0];
        float time = float.Parse(args[1]);

        Echo("Running for...");
        Echo(name);

        doorIn1 = GridTerminalSystem.GetBlockWithName(name + " Door Inside 1") as IMyAirtightHangarDoor;
        doorIn2 = GridTerminalSystem.GetBlockWithName(name + " Door Inside 2") as IMyAirtightHangarDoor;
        doorOut1 = GridTerminalSystem.GetBlockWithName(name + " Door Outside 1") as IMyAirtightHangarDoor;
        doorOut2 = GridTerminalSystem.GetBlockWithName(name + " Door Outside 2") as IMyAirtightHangarDoor;
    
        light1 = GridTerminalSystem.GetBlockWithName(name + " Light 1") as IMyLightingBlock;
        light2 = GridTerminalSystem.GetBlockWithName(name + " Light 2") as IMyLightingBlock;
        lightIn = GridTerminalSystem.GetBlockWithName(name + " Light Inside") as IMyLightingBlock;
        lightOut = GridTerminalSystem.GetBlockWithName(name + " Light Outside") as IMyLightingBlock;

        timer = GridTerminalSystem.GetBlockWithName(name + " Timer Block") as IMyTimerBlock;
        timer.TriggerDelay = time;

        SetDoorsState(doorIn1, doorIn2, doorOut1, doorOut2, light1, light2, timer);
        SetLightState(lightIn, lightOut, doorIn1, doorOut1);
    }
}

Color colorRed = new Color(200, 0, 0, 255);
Color colorGreen = new Color(0, 200, 0, 255);

private void SetDoorsState (
    IMyAirtightHangarDoor doorIn1,
    IMyAirtightHangarDoor doorIn2,
    IMyAirtightHangarDoor doorOut1,
    IMyAirtightHangarDoor doorOut2,
    IMyLightingBlock light1,
    IMyLightingBlock light2,
    IMyTimerBlock timer
) {
    if (doorIn1.Status == DoorStatus.Open && doorOut1.Status == DoorStatus.Closed) {
        doorIn1.CloseDoor();
        if (doorIn2 != null) doorIn2.CloseDoor();
        light1.Color = colorRed;
        if (light2 != null) light2.Color = colorRed;
        timer.StartCountdown();
    } else if (doorIn1.Status == DoorStatus.Closed && doorOut1.Status == DoorStatus.Open) {
        doorOut1.CloseDoor();
        if (doorOut2 != null) doorOut2.CloseDoor();
        light1.Color = colorGreen;
        if (light2 != null) light2.Color = colorGreen;
        timer.StartCountdown();
    } else if (light1.Color == colorGreen) {
        doorIn1.OpenDoor();
        if (doorIn2 != null) doorIn2.OpenDoor();
    } else if (light1.Color == colorRed) {
        doorOut1.OpenDoor();
        if (doorOut2 != null) doorOut2.OpenDoor();
    }
}

private void SetLightState(
    IMyLightingBlock lightIn,
    IMyLightingBlock lightOut,
    IMyAirtightHangarDoor doorIn,
    IMyAirtightHangarDoor doorOut
) {
    if (lightOut != null) {
        if (
            doorIn.Status == DoorStatus.Opening
            || doorIn.Status == DoorStatus.Open
        ) {
            lightOut.Color = colorRed;
        } else {
            lightOut.Color = colorGreen;
        }
    }

    if (lightIn != null) {
        if (
            doorOut.Status == DoorStatus.Opening
            || doorOut.Status == DoorStatus.Open
        ) {
            lightIn.Color = colorRed;
        } else {
            lightIn.Color = colorGreen;
        }
    }
}
