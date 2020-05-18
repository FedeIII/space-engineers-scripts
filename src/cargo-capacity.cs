Color textColor =  new Color(0, 50, 255, 255);
Color lowColor =  new Color(0, 150, 0, 255);
Color midColor =  new Color(150, 150, 0, 255);
Color highColor =  new Color(150, 0, 0, 255);

IMyTextSurfaceProvider block1;
IMyTextSurfaceProvider block2;
IMyInteriorLight light;
List <IMyCargoContainer> cargos = new List<IMyCargoContainer>();
List <IMyShipDrill> drills = new List<IMyShipDrill>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;

    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargos, cargo => cargo.IsSameConstructAs(Me));
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drills, drill => drill.IsSameConstructAs(Me));

    light =  GridTerminalSystem.GetBlockWithName("Hulk - Cargo Light") as IMyInteriorLight;

    block1 = GridTerminalSystem.GetBlockWithName("Hulk - Cockpit") as IMyTextSurfaceProvider;
    IMyTextSurface screen1 = block1.GetSurface(0);

    block2 = GridTerminalSystem.GetBlockWithName("Cargo Capacity Logic") as IMyTextSurfaceProvider;
    IMyTextSurface screen2 = block2.GetSurface(0);

    PrepareTextSurfaceForSprites(screen1);
    PrepareTextSurfaceForSprites(screen2);
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    float cargoRate = GetCargoRate();

    Echo("Cargo: " + cargoRate.ToString() + "%");

    DrawCargoRate(block1, cargoRate);
    DrawCargoRate(block2, cargoRate);
    SetLight(cargoRate);
}

private float GetCargoRate() {
    int maxCargo = 0;
    int currentCargo = 0;
    List<MyInventoryItem> items = new List<MyInventoryItem>();

    foreach (IMyCargoContainer cargo in cargos) {
        Echo("Cargo: " + cargo.Name);
        IMyInventory inventory = cargo.GetInventory();
        inventory.GetItems(items);

        foreach(MyInventoryItem item in items) {
            Echo("   - " + item.Type);
        }

        maxCargo += inventory.MaxVolume.ToIntSafe();
        currentCargo += inventory.CurrentVolume.ToIntSafe();
    }

    foreach (IMyCargoContainer drill in drills) {
        Echo("Drill: " + drill.Name);
        IMyInventory inventory = drill.GetInventory();
        inventory.GetItems(items);

        foreach(MyInventoryItem item in items) {
            Echo("   - " + item.Type);
        }

        maxCargo += inventory.MaxVolume.ToIntSafe();
        currentCargo += inventory.CurrentVolume.ToIntSafe();
    }

    return currentCargo * 100 / maxCargo;
}

private void DrawCargoRate(IMyTextSurfaceProvider block, float rate) {
    IMyTextSurface screen = block.GetSurface(0);

    var frame = screen.DrawFrame();
    DrawSprites(ref frame, block, rate);
    frame.Dispose();
}

private void DrawSprites(ref MySpriteDrawFrame frame, IMyTextSurfaceProvider block, float cargoRate) {
    String cargo = (cargoRate.ToString()) + "%";
    IMyTextSurface screen = block.GetSurface(0);

    RectangleF viewport =  new RectangleF(
        (screen.TextureSize - screen.SurfaceSize) / 2f,
        screen.SurfaceSize
    );

    var position = new Vector2(128, 20) + viewport.Position;
    
    var sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "Cargo Capacity",
        Position = position,
        RotationOrScale = 1f,
        Color = textColor,
        Alignment = TextAlignment.CENTER,
        FontId = "White"
    };

    frame.Add(sprite);
 
    position += new Vector2(0, 80);

    sprite = sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = cargo,
        Position = position,
        RotationOrScale = 3f,
        Color =  GetCargoColor(cargoRate),
        Alignment = TextAlignment.CENTER,
        FontId = "White"
    };

    frame.Add(sprite);
}

private void SetLight(float rate) {
    light.Color = GetCargoColor(rate);
}

private Color GetCargoColor(float rate) {
    if (rate > 90) return highColor;
    if (rate > 50) return midColor;
    return lowColor;
}

private void PrepareTextSurfaceForSprites(IMyTextSurface textSurface) {
    textSurface.ContentType = ContentType.SCRIPT;
    textSurface.Script = "";
}
