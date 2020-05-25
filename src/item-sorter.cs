const int PERIOD = 0; // seconds
const int MAX_INGOTS = 2000; // kg

IMyInventory main;
IMyInventory reactor;
IMyInventory assemblerIngots;
IMyInventory assemblerComponents;
List<IMyInventory> refineryOres = new List<IMyInventory>();
List<IMyInventory> refineryIngots = new List<IMyInventory>();
List<IMyInventory> inventories = new List<IMyInventory>();
List<IMyInventory> rawMaterials = new List<IMyInventory>();

int loop = 0;
bool flag = false;

List<string> logs = new List<string>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    initMainInventory();
    initReactor();
    initAssembler();
    initRefineries();
    initInventories();
    initRawMaterials();

    loop = 0;
    flag = false;
}

private void initMainInventory() {
    IMyCargoContainer mainCargo = GridTerminalSystem.GetBlockWithName("FSS - Main Cargo Container") as IMyCargoContainer;
    main = mainCargo.GetInventory();
}

private void initReactor() {
    IMyReactor nuclearReactor = GridTerminalSystem.GetBlockWithName("FSS - Nuclear Reactor") as IMyReactor;
    reactor = nuclearReactor.GetInventory();
}

private void initAssembler() {
    IMyAssembler mainAssembler = GridTerminalSystem.GetBlockWithName("FSS - Main Assembler") as IMyAssembler;
    assemblerIngots = mainAssembler.GetInventory(0);
    assemblerComponents = mainAssembler.GetInventory(0);
}

private void initRefineries() {
    List<IMyRefinery> refineries = new List<IMyRefinery>();
    IMyRefinery mainRefinery = GridTerminalSystem.GetBlockWithName("FSS - Main Refinery") as IMyRefinery;

    refineries.Add(mainRefinery);

    foreach(IMyRefinery refinery in refineries) {
        refineryOres.Add(refinery.GetInventory(0));
        refineryIngots.Add(refinery.GetInventory(1));
    }
}

private void initInventories() {
    List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargos, cargo => cargo.IsSameConstructAs(Me) && !cargo.CustomName.Contains("FSS - Raw Materials"));

    foreach(IMyCargoContainer cargo in cargos) {
        inventories.Add(cargo.GetInventory());
    }
}

private void initRawMaterials() {
    List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargos, cargo => cargo.IsSameConstructAs(Me) && cargo.CustomName.Contains("FSS - Raw Materials"));

    foreach(IMyCargoContainer cargo in cargos) {
        rawMaterials.Add(cargo.GetInventory());
    }
}

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    Echo("refineryOres: " + refineryOres.Count);
    Echo("refineryIngots: " + refineryIngots.Count);
    Echo("inventories: " + inventories.Count);
    Echo("rawMaterials: " + rawMaterials.Count);
    Echo("");

    foreach(string log in logs) Echo(log);
    Echo("");

    updateLoop();

    if (loop == PERIOD) {
        moveItems();
    }
}

private void updateLoop() {
    loop += 1;

    if (loop > PERIOD) {
        loop = 0;
    }
}

private void moveItems() {
    foreach(IMyInventory inventory in inventories) {
        Echo(OwnerName(inventory));
        MoveOres(inventory);
        MoveIngots(inventory);
        MoveComponents(inventory);
    }

    foreach(IMyInventory rawInventory in rawMaterials) {
        MoveOres(rawInventory);
        MoveIngots(rawInventory);
    }
}

private void MoveOres(IMyInventory inventory) {
    // List<MyInventoryItem> items = new List<MyInventoryItem>();
    // inventory.GetItems(items, item => item.Type.ToString().Contains("Ore"));

    // if (OwnerName(inventory).Contains("FSS - Raw Materials Cargo")) return;

    // int numItems = items.Count;

    // if(numItems > 0) Echo(" Ores:");

    // for (int i = numItems-1; i >= 0; i--) {
    //     MyInventoryItem item = items[i];
    //     Echo("  - " + ItemName(item));
    // }
}

private void MoveIngots(IMyInventory inventory) {
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    inventory.GetItems(items, item => item.Type.ToString().Contains("Ingot"));

    int numItems = items.Count;

    if(numItems > 0) Echo(" Ingots:");

    for (int iteration = numItems - 1; iteration >= 0; iteration--) {
        int itemIndex = items.Count - iteration - 1;
        MyInventoryItem item = items[itemIndex];
        bool isFull = false;

        Echo("  - " + ItemName(item));
    
        // Echo("U?");
        if (item.Type.ToString().Contains("Uranium")) {
            // Echo("true");
            isFull = MoveItem(item, inventory, reactor);
        } else {
            // Echo("false");
            // Echo("max ingots?");
            MyFixedPoint itemAmount = assemblerIngots.GetItemAmount(item.Type);
            if (itemAmount < MAX_INGOTS) {
                // Echo("false");
                isFull = MoveItem(item, inventory, assemblerIngots);
            } else {
                // Echo("true");
                int inventoryIndex = 0;

                do {
                    IMyInventory targetInventory = rawMaterials[inventoryIndex];
                    isFull = MoveItem(item, inventory, targetInventory);
                    inventoryIndex++;
                } while (isFull && inventoryIndex < rawMaterials.Count);
            }
        }

        if (isFull) break;
    }

    
}

private void MoveComponents(IMyInventory inventory) {
    // List<MyInventoryItem> items = new List<MyInventoryItem>();
    // inventory.GetItems(items, item => !item.Type.ToString().Contains("Ore") && !item.Type.ToString().Contains("Ingot"));

    // int numItems = items.Count;

    // if(numItems > 0) Echo(" Components:");

    // for (int i = numItems-1; i >= 0; i--) {
    //     MyInventoryItem item = items[i];
    //     bool isFull = false;

    //     Echo("  - " + ItemName(item));

    //     isFull = MoveItem(i, inventory, main, item);

    //     if (isFull) break;
    // }
}

private bool MoveItem(MyInventoryItem item, IMyInventory from, IMyInventory to) {
    Echo("Moving");
    //if (flag == true) return false;

    Echo("same inventory?");
    if (OwnerName(from) == OwnerName(to)) {
        Echo("true");
        return false;
    }

    Echo("dest full?");
    if (to.IsFull) {
        Echo("true");
        return true;
    } else {
        logs.Add("Moving " + ItemName(item) + " from:");
        logs.Add("   " + OwnerName(from));
        logs.Add("to:");
        logs.Add("   " + OwnerName(to));
        from.TransferItemTo(to, item);
        flag = true;
        return false;
    }
}

private string ItemName(MyInventoryItem item) {
    string type = item.Type.ToString();
    return type.Substring(type.IndexOf("/") + 1);
}

private string OwnerName(IMyInventory inventory) {
    IMyTerminalBlock owner = inventory.Owner as IMyTerminalBlock;
    return owner.CustomName;
}
