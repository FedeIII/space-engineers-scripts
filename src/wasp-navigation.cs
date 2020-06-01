// const double Kp = 3E-2d;
// const double Ki = 3E-4d;
// const double Kd = 2E0d;

const double Kps = 3E-2d;
const double Kis = 0;
const double Kds = 2E0d;

const double Kp = 3E-2d;
const double Ki = 0;
const double Kd = 2E0d;

const int FLY_TIME = 6;

IMyThrust up1;
IMyThrust up2;
IMyThrust down1;
IMyThrust down2;
IMyThrust front1;
IMyThrust front2;
IMyThrust back1;
IMyThrust back2;
IMyThrust left;
IMyThrust right;

IMyTimerBlock timer1;
IMyTimerBlock timer2;

IMyShipConnector connector;
IMyShipConnector landingPad2;

enum State {
    FlyAway,
    FlyIn,
    Dock,
}

enum Action {
    GoBack,
    Dock,
    Fly,
    NoAction
}

MatrixD GetGrid2WorldTransform(IMyCubeGrid grid)
{
    Vector3D origin = grid.GridIntegerToWorld(new Vector3I(0, 0, 0));
    Vector3D plusY = grid.GridIntegerToWorld(new Vector3I(0, 1, 0)) - origin;
    Vector3D plusZ = grid.GridIntegerToWorld(new Vector3I(0, 0, 1))  -origin;
    return MatrixD.CreateScale(grid.GridSize) * MatrixD.CreateWorld(origin, -plusZ, plusY);
}

MatrixD GetBlock2WorldTransform(IMyCubeBlock blk)
{
    Matrix blk2grid;
    blk.Orientation.GetMatrix(out blk2grid);
    return blk2grid *
           MatrixD.CreateTranslation(((Vector3D)new Vector3D(blk.Min + blk.Max)) / 2.0) *
           GetGrid2WorldTransform(blk.CubeGrid);
}

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;

    up1 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Up 1") as IMyThrust;
    up2 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Up 2") as IMyThrust;
    down1 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Down 1") as IMyThrust;
    down2 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Down 2") as IMyThrust;
    front1 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Front 1") as IMyThrust;
    front2 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Front 2") as IMyThrust;
    back1 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Back 1") as IMyThrust;
    back2 = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Back 2") as IMyThrust;
    left = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Left") as IMyThrust;
    right = GridTerminalSystem.GetBlockWithName("Wasp - Thruster Right") as IMyThrust;

    timer1 = GridTerminalSystem.GetBlockWithName("Wasp - Timer 1") as IMyTimerBlock;
    timer2 = GridTerminalSystem.GetBlockWithName("Wasp - Timer 2") as IMyTimerBlock;

    connector = GridTerminalSystem.GetBlockWithName("Wasp - Connector") as IMyShipConnector;
    landingPad2 = GridTerminalSystem.GetBlockWithName("Landing Pad 2") as IMyShipConnector;
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    Action action = Action.NoAction;
    if (argument != "") {
        action = (Action)Enum.Parse(typeof(Action), argument);
    }

    if (Me.CustomData == "") {
        ChangeState(State.FlyAway);
    }

    State state = (State)Enum.Parse(typeof(State), Me.CustomData);
    
    Echo("Action: " + action.ToString());
    Echo("State: " + state.ToString());
    // Echo(string.Format("Kp: {0}", Kp));
    // Echo(string.Format("Ki: {0}", Ki));
    // Echo(string.Format("Kd: {0}", Kd));

    // GoToFSS();

    switch(state) {
        case State.FlyAway:
            FlyAwayState(action);
            break;
        case State.FlyIn:
            FlyInState(action);
            break;
        case State.Dock:
            DockState(action);
            break;
        default:
            break;
    }
}

// STATES

private void FlyAwayState(Action action) {
    if (action == Action.GoBack) {
        ChangeState(State.FlyIn);
    } else {
        if (!timer1.IsCountingDown) {
            timer1.TriggerDelay = FLY_TIME;
            timer1.StartCountdown();
        }

        connector.Enabled = false;
        up1.ThrustOverridePercentage = 0.1f;
        up2.ThrustOverridePercentage = 0.1f;
        back1.ThrustOverridePercentage = 0.1f;
        back2.ThrustOverridePercentage = 0.1f;
    }
}

private void FlyInState(Action action) {
    connector.Enabled = true;

    if (connector.Status == MyShipConnectorStatus.Connectable) {
        ChangeState(State.Dock);
    } else {
        GoToFSS();
    }
}

private void DockState(Action action) {
    if (action == Action.Fly) {
        ChangeState(State.FlyAway);
    } else {
        up1.ThrustOverridePercentage = 0f;
        up2.ThrustOverridePercentage = 0f;
        down1.ThrustOverridePercentage = 0f;
        down2.ThrustOverridePercentage = 0f;
        front1.ThrustOverridePercentage = 0f;
        front2.ThrustOverridePercentage = 0f;
        back1.ThrustOverridePercentage = 0f;
        back2.ThrustOverridePercentage = 0f;
        left.ThrustOverridePercentage = 0f;
        right.ThrustOverridePercentage = 0f;
        connector.Connect();
    }
}

private void ChangeState(State state) {
    Me.CustomData = state.ToString();
}

// NAVIGATION

private void GoToFSS() {
    // var reference = landingPad;// your reference block, like your remote
    // var forwardPos = reference.Position + Base6Directions.GetIntVector(reference.Orientation.TransformDirection(Base6Directions.Direction.Forward));
    // var forward = reference.CubeGrid.GridIntegerToWorld(forwardPos);
    // var forwardVector = Vector3D.Normalize(forward - reference.GetPosition());

    // MatrixD connectorWorld = GetBlock2WorldTransform(connector);
    // MatrixD landingPad2World = GetBlock2WorldTransform(landingPad2);

    // Echo("connector: " + connectorWorld.Translation.ToString());
    // Echo("landingPad2: " + landingPad2World.Translation.ToString());
    // Echo("distance: " + Vector3D.Subtract(connectorWorld.Translation, landingPad2World.Translation).ToString());

    // Vector3D connectorWorldPosition = Vector3D.Transform(connector.GetPosition(), connector.WorldMatrix);

    // Vector3D landingPad2WorldPosition = Vector3D.Transform(landingPad2.GetPosition(), landingPad2.WorldMatrix);

    // Vector3D direction = Vector3D.Subtract(landingPad2WorldPosition, connectorWorldPosition);
    // Echo("distance: " + direction.Length().ToString());

    Vector3D direction = landingPad2.GetPosition() - connector.GetPosition();
    Vector3D bodyDirection = Vector3D.TransformNormal(direction, MatrixD.Transpose(connector.WorldMatrix));

    errorX.Set(bodyDirection.X);
    errorY.Set(bodyDirection.Y);
    errorZ.Set(bodyDirection.Z);
   
    float thrustX = 0f;
    float thrustY = 0f;
    float thrustZ = 0f;

    // Echo("errorP: " + errorY.p.ToString());
    // Echo("errorI: " + errorY.i.ToString());
    // Echo("errorD: " + errorY.d.ToString());
    // Echo("Thrust: " + (Kp * errorY.p).ToString() + " + " + (Ki * errorY.i).ToString() + " + " + (Kd * errorY.d).ToString() + " = " + (Kp * errorY.p + Ki * errorY.i + Kd * errorY.d).ToString());

    // bodyDirection.Normalize();
    // Echo("direction: " + bodyDirection.ToString());
    var distance = bodyDirection.Length();
    Echo("distance: " + distance.ToString());

    // if (distance < 300) {
    //     Echo("Near");
    //     thrustX = GetThrustSlow(errorX);
    //     thrustY = GetThrustSlow(errorY);
    //     thrustZ = GetThrustSlow(errorZ);
    //     if (thrustX < 0.02f && thrustY < 0.02f && thrustZ < 0.02f) {
    //         Echo("Go");
    //         Go(thrustX, thrustY, thrustZ);
    //     } else {
    //         Echo("Break");
    //         Break(thrustX, thrustY, thrustZ);
    //     }
    // } else {
        Echo("Far");
        thrustX = GetThrust(errorX);
        thrustY = GetThrust(errorY);
        thrustZ = GetThrust(errorZ);
        Go(thrustX, thrustY, thrustZ);
    // }

    Echo("Thrust: " + thrustX + ", " + thrustY + ", " + thrustZ);
}

private void Break(float thrustX, float thrustY, float thrustZ) {
    if (thrustX > 0) {
        Echo("Right");
        right.ThrustOverridePercentage = Math.Abs(thrustX);
        left.ThrustOverridePercentage = 0f;
    } else {
        Echo("Left");
        right.ThrustOverridePercentage = 0f;
        left.ThrustOverridePercentage = thrustX;
    }

    if (thrustY > 0) {
        Echo("Front");
        front1.ThrustOverridePercentage = Math.Abs(thrustZ);
        front2.ThrustOverridePercentage = Math.Abs(thrustZ);
        back1.ThrustOverridePercentage = 0f;
        back2.ThrustOverridePercentage = 0f;
    } else {
        Echo("Back");
        front1.ThrustOverridePercentage = 0f;
        front2.ThrustOverridePercentage = 0f;
        back1.ThrustOverridePercentage = thrustZ;
        back2.ThrustOverridePercentage = thrustZ;
    }

    if (thrustZ > 0) {
        Echo("Down");
        up1.ThrustOverridePercentage = 0f;
        up2.ThrustOverridePercentage = 0f;
        down1.ThrustOverridePercentage = Math.Abs(thrustY);
        down2.ThrustOverridePercentage = Math.Abs(thrustY);
    } else {
        Echo("Up");
        up1.ThrustOverridePercentage = thrustY;
        up2.ThrustOverridePercentage = thrustY;
        down1.ThrustOverridePercentage = 0f;
        down2.ThrustOverridePercentage = 0f;
    }
}

private void Go(float thrustX, float thrustY, float thrustZ) {
    if (thrustX > 0) {
        Echo("Left");
        right.ThrustOverridePercentage = 0f;
        left.ThrustOverridePercentage = thrustX;
    } else {
        Echo("Right");
        right.ThrustOverridePercentage = Math.Abs(thrustX);
        left.ThrustOverridePercentage = 0f;
    }

    if (thrustY > 0) {
        Echo("Back");
        front1.ThrustOverridePercentage = 0f;
        front2.ThrustOverridePercentage = 0f;
        back1.ThrustOverridePercentage = thrustZ;
        back2.ThrustOverridePercentage = thrustZ;
    } else {
        Echo("Front");
        front1.ThrustOverridePercentage = Math.Abs(thrustZ);
        front2.ThrustOverridePercentage = Math.Abs(thrustZ);
        back1.ThrustOverridePercentage = 0f;
        back2.ThrustOverridePercentage = 0f;
    }

    if (thrustZ > 0) {
        Echo("Up");
        up1.ThrustOverridePercentage = thrustY;
        up2.ThrustOverridePercentage = thrustY;
        down1.ThrustOverridePercentage = 0f;
        down2.ThrustOverridePercentage = 0f;
    } else {
        Echo("Down");
        up1.ThrustOverridePercentage = 0f;
        up2.ThrustOverridePercentage = 0f;
        down1.ThrustOverridePercentage = Math.Abs(thrustY);
        down2.ThrustOverridePercentage = Math.Abs(thrustY);
    }
}

// PID

class Error {
    public double p;
    public double i;
    public double d;

    public Error() {
        p = 0;
        i = 0;
        d = 0;
    }

    public void Set(double error) {
        i += error;
        d = error - p; 
        p = error;
    }
}

Error errorX = new Error();
Error errorY = new Error();
Error errorZ = new Error();

private float GetThrust(Error error) {
    var thrust = Kp * error.p + Ki * error.i + Kd * error.d;
    if (thrust > 1) thrust = 1;
    if (thrust < -1) thrust = -1;
    return (float)thrust;
}

private float GetThrustSlow(Error error) {
    var thrust = Kps * error.p + Kis * error.i + Kds * error.d;
    if (thrust > 1) thrust = 1;
    if (thrust < -1) thrust = -1;
    return (float)thrust;
}


