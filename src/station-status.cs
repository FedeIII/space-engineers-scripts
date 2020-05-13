const int LINES = 4;
const int PADD = 60;
const float FONT_SIZE = 1.1f;
const float PX_PER_CHAR = 20 * FONT_SIZE;
const int TOTAL_TITLE_CHARS = 10;
Color blueColor =  new Color(0, 50, 255, 255);
Color greenColor =  new Color(0, 150, 0, 255);
Color yellowColor =  new Color(150, 150, 0, 255);
Color orangeColor =  new Color(150, 75, 0, 255);
Color redColor =  new Color(150, 0, 0, 255);
IMyTextSurface screen1;
IMyTextSurface screen2;
IMyTextSurface screen3;
IMyAirVent airVent;
List <IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
List <IMyGasTank> tanks = new List<IMyGasTank>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    IMyTextSurfaceProvider block1 = GridTerminalSystem.GetBlockWithName("Command LCD Top") as IMyTextSurfaceProvider;
    screen1 = block1.GetSurface(0);

    IMyTextSurfaceProvider block2 = GridTerminalSystem.GetBlockWithName("FSS - Commander Control Chair") as IMyTextSurfaceProvider;
    screen2 = block2.GetSurface(0);

    IMyTextSurfaceProvider block3 = GridTerminalSystem.GetBlockWithName("Command LCD Left") as IMyTextSurfaceProvider;
    screen3 = block3.GetSurface(0);

    airVent = GridTerminalSystem.GetBlockWithName("FSS - Air Vent") as IMyAirVent;

    PrepareTextSurfaceForSprites(screen1);
    PrepareTextSurfaceForSprites(screen2);
    PrepareTextSurfaceForSprites(screen3);
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    float airRate = airVent.GetOxygenLevel();
    float energyRate = GetEnergyLevel();
    float oxygenRate = GetOxygenLevel();
    float hydrogenRate = GetHydrogenLevel();

    DrawRates(screen1, airRate, energyRate, oxygenRate, hydrogenRate);
    DrawRates(screen2, airRate, energyRate, oxygenRate, hydrogenRate);
    DrawRates(screen3, airRate, energyRate, oxygenRate, hydrogenRate);
}

private float GetEnergyLevel() {
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);

    float max = 0;
    float current = 0;
    foreach (IMyBatteryBlock battery in batteries) {
        max += battery.MaxStoredPower;
        current += battery.CurrentStoredPower;
    }

    return current / max;
}

private float GetOxygenLevel() {
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);

    float max = 0;
    float current = 0;
    foreach (IMyGasTank tank in tanks) {
        if (tank.DisplayNameText.Contains("Oxygen Tank")) {
            max += tank.Capacity;
            current += tank.Capacity * (float)tank.FilledRatio;
        }
    }

    return current / max;
}

private float GetHydrogenLevel() {
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);

    float max = 0;
    float current = 0;
    foreach (IMyGasTank tank in tanks) {
        if (tank.DisplayNameText.Contains("Hydrogen Tank")) {
            max += tank.Capacity;
            current += tank.Capacity * (float)tank.FilledRatio;
        }
    }

    return current / max;
}

private void DrawRates(IMyTextSurface screen, float airRate, float energyRate, float oxygenRate, float hydrogenRate) {
    var frame = screen.DrawFrame();

    Echo(screen.Name);
    Echo(GetMessage("   Air Pressure: ", airRate));
    Echo(GetMessage("   E:  ", energyRate));
    Echo(GetMessage("   O2:  ", oxygenRate));
    Echo(GetMessage("   H2:  ", hydrogenRate));

    DrawSprites(ref frame, screen, airRate, energyRate, oxygenRate, hydrogenRate);

    frame.Dispose();
}

private string GetMessage(string title, float rate) {
    return title + ((int)(rate * 100)).ToString() + "%";
}

private void DrawSprites(ref MySpriteDrawFrame frame, IMyTextSurface screen, float airRate, float energyRate, float oxygenRate, float hydrogenRate) {
    RectangleF viewport =  GetViewPort(screen);
    float lineGap = viewport.Height / LINES;

    var position = new Vector2(20, 20) + viewport.Position;
    frame.Add(GetLine("Air", airRate, position, viewport));
    
    position += new Vector2(0, lineGap);
    frame.Add(GetLine("E", energyRate, position, viewport));

    position += new Vector2(0, lineGap);
    frame.Add(GetLine("O2", oxygenRate, position, viewport));

    position += new Vector2(0, lineGap);
    frame.Add(GetLine("H2", hydrogenRate, position, viewport));
}

private MySprite GetLine(string title, float rate, Vector2 position, RectangleF viewport) {
    string data = GetTitle(title) + GetBars(viewport, rate);

    return new MySprite() {
        Type = SpriteType.TEXT,
        Data = data,
        Position = position,
        RotationOrScale = FONT_SIZE,
        Color = GetColor(rate),
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
}

private string GetBars(RectangleF viewport, float rate) {
    float width = viewport.Width - PADD;

    int totalChars = (int)(width / PX_PER_CHAR);
    int bars = (int)(rate * totalChars);
    int dots = (int)(totalChars - bars);

    string barUnit = "|||";
    string emptyUnit = "··";

    return "[" + string.Concat(Enumerable.Repeat(barUnit, bars)) + string.Concat(Enumerable.Repeat(emptyUnit, dots)) + "]";
}

private string GetTitle(string title) {
    return title + " " + string.Concat(Enumerable.Repeat('-', TOTAL_TITLE_CHARS - title.Length - 1)) + " ";
}

private Color GetColor(float rate) {
    if (rate > 0.85) return greenColor;
    if (rate > 0.66) return yellowColor;
    if (rate > 0.33) return orangeColor;
    return redColor;
}

private void PrepareTextSurfaceForSprites(IMyTextSurface textSurface) {
    textSurface.ContentType = ContentType.SCRIPT;
    textSurface.Script = "";
}

private RectangleF GetViewPort(IMyTextSurface screen) {
    return  new RectangleF(
        (screen.TextureSize - screen.SurfaceSize) / 2f,
        screen.SurfaceSize
    );
}
