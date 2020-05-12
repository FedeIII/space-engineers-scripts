string [] names = {
    "Main Entrance",
    "Hangar",
    "Command",
    "Power",
    "Engines"
};

// {name} Door Inside
// {name} Door Outside
// {name} Sensor Inside
// {name} Sensor Outside
// {name} Light (optional)
// {name} Light Inside
// {name} Light Outside


public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Save() {}

public void Main(string argument) {
    foreach (string name in names) {
        manageDoors(name);
    }
}

private void manageDoors (string name) {
    IMyAirtightSlideDoor doorIn;
    IMyAirtightSlideDoor doorOut;

    IMySensorBlock sensorIn;
    IMySensorBlock sensorOut;

    IMyLightingBlock light;
    IMyLightingBlock lightIn;
    IMyLightingBlock lightOut;

    Echo(name);

    doorIn = GridTerminalSystem.GetBlockWithName(name + " Door Inside") as IMyAirtightSlideDoor;
    doorOut = GridTerminalSystem.GetBlockWithName(name + " Door Outside") as IMyAirtightSlideDoor;

    sensorIn = GridTerminalSystem.GetBlockWithName(name + " Sensor Inside") as IMySensorBlock;
    sensorOut = GridTerminalSystem.GetBlockWithName(name + " Sensor Outside") as IMySensorBlock;
    
    light = GridTerminalSystem.GetBlockWithName(name + " Light") as IMyLightingBlock;
    lightIn = GridTerminalSystem.GetBlockWithName(name + " Light Inside") as IMyLightingBlock;
    lightOut = GridTerminalSystem.GetBlockWithName(name + " Light Outside") as IMyLightingBlock;

    Echo("   door In");
    SetDoorState(doorIn, sensorIn, doorOut);
    Echo("   door Out");
    SetDoorState(doorOut, sensorOut, doorIn);
    Echo("   lights");
    SetLightState(light, lightIn, lightOut, doorIn, doorOut);
}

private void SetDoorState (
    IMyAirtightSlideDoor door,
    IMySensorBlock sensor,
    IMyAirtightSlideDoor otherDoor
) {
    if (door != null) Echo("      door: o");
        else Echo("      door: x");
    if (sensor != null) Echo("      sensor: o");
        else Echo("      sensor: x");
    if (door == null || sensor == null) return;

    List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
    sensor.DetectedEntities(entities);

    if (entities.Count > 0) {
        otherDoor.CloseDoor();

        if (otherDoor.Status == DoorStatus.Opening || otherDoor.Status == DoorStatus.Open) {
            door.CloseDoor();
        }     
    } else {
        door.CloseDoor();
    }
}

Color colorRed = new Color(200, 0, 0, 255);
Color colorGreen = new Color(0, 200, 0, 255);

private void SetLightState(
    IMyLightingBlock light,
    IMyLightingBlock lightIn,
    IMyLightingBlock lightOut,
    IMyAirtightSlideDoor doorIn,
    IMyAirtightSlideDoor doorOut
) {
    if (light != null) Echo("      light: o");
    if (lightIn != null) Echo("      lightIn: o");
    if (lightOut != null) Echo("      lightOut: o");
    if (doorIn == null || doorOut == null) return;

    if (light != null) {
        if (
            doorIn.Status == DoorStatus.Opening
            || doorIn.Status == DoorStatus.Open
            || doorOut.Status == DoorStatus.Opening
            || doorOut.Status == DoorStatus.Open
        ) {
            light.Color = colorRed;
        }  else {
            light.Color = colorGreen;
        }
    }

    if (lightIn != null) {
        if (
            doorIn.Status == DoorStatus.Opening
            || doorIn.Status == DoorStatus.Open
        ) {
            lightOut.Color = colorRed;
        } else {
            lightOut.Color = colorGreen;
        }
    }

    if (lightOut != null) {
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
