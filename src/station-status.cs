const int LINES = 5;
const int PADD = 60;
const float FONT_SIZE = 1.1f;
const float PX_PER_CHAR = 20 * FONT_SIZE;
const int TOTAL_TITLE_CHARS = 10;
const float ALARM_RATE = 0.33f;
Color blueColor =  new Color(0, 50, 255, 255);
Color greenColor =  new Color(0, 150, 0, 255);
Color yellowColor =  new Color(150, 150, 0, 255);
Color orangeColor =  new Color(150, 75, 0, 255);
Color redColor =  new Color(150, 0, 0, 255);
struct Screen {
    public string name;
    public int index;
}
List<IMyTextSurface> surfaces = new List<IMyTextSurface>();
IMyAirVent airVent;
IMyReflectorLight alarm;
List <IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
List <IMyGasTank> tanks = new List<IMyGasTank>();
List <IMyCargoContainer> cargos = new List<IMyCargoContainer>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    var screens = new Screen[4];
    screens[0] = new Screen(){ name = "Command LCD Top", index = 0 };
    screens[1] = new Screen(){ name = "FSS - Commander Control Chair", index = 0 };
    screens[2] = new Screen(){ name = "Command LCD Right", index = 0 };
    screens[3] = new Screen(){ name = "Station Status Logic", index = 0 };

    foreach(var screen in screens) {
        IMyTextSurfaceProvider block = GridTerminalSystem.GetBlockWithName(screen.name) as IMyTextSurfaceProvider;
        IMyTextSurface surface = block.GetSurface(screen.index);
        
        surfaces.Add(surface);
        PrepareTextSurfaceForSprites(surface);
    }

    airVent = GridTerminalSystem.GetBlockWithName("FSS - Infirmary Air Vent") as IMyAirVent;
    alarm = GridTerminalSystem.GetBlockWithName("FSS - Alarm Light") as IMyReflectorLight;
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    float airRate = airVent.GetOxygenLevel();
    float energyRate = GetEnergyLevel();
    float oxygenRate = GetOxygenLevel();
    float hydrogenRate = GetHydrogenLevel();
    float cargoRate = GetCargoRate();

    foreach(IMyTextSurface surface in surfaces) {
        DrawRates(surface, airRate, energyRate, oxygenRate, hydrogenRate, cargoRate);
    }
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

private float GetCargoRate() {
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargos, cargo => cargo.IsSameConstructAs(Me));

    float max = 0;
    float current = 0;
    foreach (IMyCargoContainer cargo in cargos) {
        IMyInventory inventory = cargo.GetInventory();

        max += inventory.MaxVolume.ToIntSafe();
        current += inventory.CurrentVolume.ToIntSafe();
    }

    return current / max;
}

private void DrawRates(IMyTextSurface screen, float airRate, float energyRate, float oxygenRate, float hydrogenRate, float cargoRate) {
    var frame = screen.DrawFrame();

    Echo(screen.Name);
    Echo(GetMessage("   Air Pressure: ", airRate));
    Echo(GetMessage("   E:  ", energyRate));
    Echo(GetMessage("   O2:  ", oxygenRate));
    Echo(GetMessage("   H2:  ", hydrogenRate));
    Echo(GetMessage("   Cargo: ", cargoRate));

    alarm.Enabled = AlarmStatus(airRate, energyRate, oxygenRate);

    DrawSprites(ref frame, screen, airRate, energyRate, oxygenRate, hydrogenRate, cargoRate);

    frame.Dispose();
}

private string GetMessage(string title, float rate) {
    return title + ((int)(rate * 100)).ToString() + "%";
}

private void DrawSprites(ref MySpriteDrawFrame frame, IMyTextSurface screen, float airRate, float energyRate, float oxygenRate, float hydrogenRate, float cargoRate) {
    RectangleF viewport =  GetViewPort(screen);
    float lineGap = viewport.Height / LINES;

    var position = new Vector2(20, 20) + viewport.Position;
    frame.Add(GetLine("Air", airRate, position, viewport, false));
    
    position += new Vector2(0, lineGap);
    frame.Add(GetLine("E", energyRate, position, viewport, false));

    position += new Vector2(0, lineGap);
    frame.Add(GetLine("O2", oxygenRate, position, viewport, false));

    position += new Vector2(0, lineGap);
    frame.Add(GetLine("H2", hydrogenRate, position, viewport, false));

    position += new Vector2(0, lineGap);
    frame.Add(GetLine("Cargo", cargoRate, position, viewport, true));
}

private MySprite GetLine(string title, float rate, Vector2 position, RectangleF viewport, bool reverseColors) {
    string data = GetTitle(title) + GetBars(viewport, rate);

    return new MySprite() {
        Type = SpriteType.TEXT,
        Data = data,
        Position = position,
        RotationOrScale = FONT_SIZE,
        Color = GetColor(rate, reverseColors),
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

private Color GetColor(float rate, bool reverseColors) {
    if (reverseColors) {
        if (rate > 0.85) return redColor;
        if (rate > 0.66) return orangeColor;
        if (rate > ALARM_RATE) return yellowColor;
        return greenColor;
    }

    if (rate > 0.85) return greenColor;
    if (rate > 0.66) return yellowColor;
    if (rate > ALARM_RATE) return orangeColor;
    return redColor;
}

private bool AlarmStatus(float airRate, float energyRate, float oxygenRate) {
    return airRate < ALARM_RATE || energyRate < ALARM_RATE || oxygenRate < ALARM_RATE;
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
