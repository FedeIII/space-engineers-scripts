const float FONT_SIZE = 1.1f;
const int PADD = 20;
const int TITLE_PADD = 30;
const int SHIP_PADD = 30;
const int BASE_WIDTH = 512;
const int BASE_HEIGHT = 512;
struct Screen {
    public string name;
    public int index;
}
List<IMyTextSurface> surfaces = new List<IMyTextSurface>();
List<IMyShipConnector> connectors = new List<IMyShipConnector>();

Color greenColor =  new Color(0, 150, 0, 255);
Color yellowColor =  new Color(150, 150, 0, 255);
Color redColor =  new Color(150, 0, 0, 255);

public Program() {
    var screens = new Screen[3];
    screens[0] = new Screen(){ name = "Command LCD Left", index = 0 };
    screens[1] = new Screen(){ name = "Station Connections Logic", index = 0 };
    screens[2] = new Screen(){ name = "FSS - Commander Control Chair", index = 1 };

    foreach(var screen in screens) {
        IMyTextSurfaceProvider block = GridTerminalSystem.GetBlockWithName(screen.name) as IMyTextSurfaceProvider;
        IMyTextSurface surface = block.GetSurface(screen.index);

        surfaces.Add(surface);
        PrepareTextSurfaceForSprites(surface);
    }

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(
        connectors,
        connector => connector.IsSameConstructAs(Me)
    );

    foreach(IMyTextSurface surface in surfaces) {
        DrawConnections(surface);
    }
}

private void DrawConnections(IMyTextSurface screen) {
    var frame = screen.DrawFrame();

    DrawSprites(ref frame, screen);

    frame.Dispose();
}

private void DrawSprites(ref MySpriteDrawFrame frame, IMyTextSurface screen) {
    RectangleF viewport =  GetViewPort(screen);
    float lineGap = viewport.Height / connectors.Count;
    float widthRatio = viewport.Width / BASE_WIDTH;
    float heightRatio = viewport.Width / BASE_HEIGHT;
    float fontSize = FONT_SIZE * widthRatio;
    float padding = PADD * heightRatio;
    float titlePadding = TITLE_PADD * heightRatio;
    float shipPadding = SHIP_PADD * heightRatio;

    var position = new Vector2(padding, padding) + viewport.Position;
    frame.Add(new MySprite() {
        Type = SpriteType.TEXT,
        Data = "Ships docked",
        Position = position,
        RotationOrScale = fontSize,
        Color = greenColor,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    });

    Echo("Ships docked:");

    position += new Vector2(0, titlePadding);

    frame.Add(new MySprite() {
        Type = SpriteType.TEXT,
        Data = "=============",
        Position = position,
        RotationOrScale = fontSize,
        Color = greenColor,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    });

    position += new Vector2(0, 2 * shipPadding);

    foreach(IMyShipConnector connector in connectors) {
        frame.Add(GetLine("- ", connector, position, viewport));
        position += new Vector2(0, lineGap);
    }
}

private RectangleF GetViewPort(IMyTextSurface screen) {
    return  new RectangleF(
        (screen.TextureSize - screen.SurfaceSize) / 2f,
        screen.SurfaceSize
    );
}

private MySprite GetLine(string title, IMyShipConnector connector, Vector2 position, RectangleF viewport) {
    string data = title + GetShip(connector);
    Color color = GetColor(connector);
    float fontSize = FONT_SIZE * viewport.Width / BASE_WIDTH;

    Echo(data);
    return new MySprite() {
        Type = SpriteType.TEXT,
        Data = data,
        Position = position,
        RotationOrScale = fontSize,
        Color = color,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
}

private string GetShip(IMyShipConnector connector) {
    IMyShipConnector otherConnector = connector.OtherConnector;

    if (otherConnector == null) {
        return "no ship (" + connector.CustomName + ")";
    }

    return otherConnector.CubeGrid.CustomName + " (" + connector.CustomName + ")";
}

private Color GetColor(IMyShipConnector connector) {
    if (connector.Status == MyShipConnectorStatus.Unconnected) return redColor;
    if (connector.Status == MyShipConnectorStatus.Connectable) return yellowColor;
    if (connector.Status == MyShipConnectorStatus.Connected) return greenColor;
    return greenColor;
}

private void PrepareTextSurfaceForSprites(IMyTextSurface textSurface) {
    textSurface.ContentType = ContentType.SCRIPT;
    textSurface.Script = "";
}
