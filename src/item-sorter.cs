const int PERIOD = 5; // seconds
const int MAX_INGOTS = 2000; // kg
const int MAX_ORES = 2000; // kg

IMyInventory main;
IMyInventory reactor;
IMyInventory assemblerIngots;
IMyInventory assemblerComponents;
IMyInventory tools;
List<IMyInventory> refineryOres = new List<IMyInventory>();
List<IMyInventory> refineryIngots = new List<IMyInventory>();
List<IMyInventory> assemblersComponents = new List<IMyInventory>();
List<IMyInventory> inventories = new List<IMyInventory>();
List<IMyInventory> rawMaterials = new List<IMyInventory>();

int loop = 0;
bool flag = false;

List<string> logs = new List<string>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    initMainInventory();
    initReactor();
    initAssemblers();
    initTools();
    initRefineries();
    initInventories();
    initRawMaterials();

    loop = 0;
    flag = false;
}

// INIT

private void initMainInventory() {
    IMyCargoContainer mainCargo = GridTerminalSystem.GetBlockWithName("FSS - Main Cargo Container") as IMyCargoContainer;
    main = mainCargo.GetInventory();
}

private void initReactor() {
    IMyReactor nuclearReactor = GridTerminalSystem.GetBlockWithName("FSS - Nuclear Reactor") as IMyReactor;
    reactor = nuclearReactor.GetInventory();
}

private void initAssemblers() {
    IMyAssembler mainAssembler = GridTerminalSystem.GetBlockWithName("FSS - Main Assembler") as IMyAssembler;
    assemblerIngots = mainAssembler.GetInventory(0);
    assemblerComponents = mainAssembler.GetInventory(0);

    List<IMyAssembler> assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
    
    foreach(IMyAssembler assembler in assemblers) {
        assemblersComponents.Add(assembler.GetInventory(1));
    }

    assemblersComponents.Prepend(assemblerComponents);
}

private void initTools() {
    IMyCargoContainer toolsCargo = GridTerminalSystem.GetBlockWithName("FSS - Tools Cargo") as IMyCargoContainer;
    tools = toolsCargo.GetInventory();
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

// MAIN

public void Save() {}

public void Main(string argument, UpdateType updateSource) {
    Echo("refineryOres: " + refineryOres.Count);
    Echo("refineryIngots: " + refineryIngots.Count);
    Echo("inventories: " + inventories.Count);
    Echo("rawMaterials: " + rawMaterials.Count);
    Echo("");

    foreach(string log in logs) Echo(log);
    Echo("");

    UpdateLoop();

    if (loop == PERIOD) {
        MoveItems();
    }
}

private void UpdateLoop() {
    loop += 1;

    if (loop > PERIOD) {
        loop = 0;
    }
}

// MOVE

private void MoveItems() {
    foreach(IMyInventory inventory in inventories) {
        Echo(OwnerName(inventory));
        MoveOres(inventory);
        MoveIngots(inventory);
        MoveTools(inventory);
        MoveComponents(inventory);
    }

    foreach(IMyInventory rawInventory in rawMaterials) {
        MoveOres(rawInventory);
        MoveIngots(rawInventory);
    }

    foreach(IMyInventory refinery in refineryIngots) {
        MoveIngots(refinery);
    }

    foreach(IMyInventory assembler in assemblersComponents) {
        MoveComponents(assembler);
    }
}

private void MoveOres(IMyInventory inventory) {
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    inventory.GetItems(items, item => item.Type.ToString().Contains("Ore"));

    int numItems = items.Count;

    if(numItems > 0) Echo(" Ores:");

    for (int iteration = numItems - 1; iteration >= 0; iteration--) {
        int itemIndex = items.Count - iteration - 1;
        MyInventoryItem item = items[itemIndex];
        bool isFull = false;

        Echo("  - " + ItemName(item));
    
        IMyInventory destinationRefinery = null;
        int refineryIndex = 0;
        do {
            destinationRefinery = refineryOres[refineryIndex];
            MyFixedPoint itemAmountInRefinery = destinationRefinery.GetItemAmount(item.Type);
            if (itemAmountInRefinery < MAX_ORES) {
                break;
            }
            refineryIndex++;
        } while (refineryIndex < refineryOres.Count);
    
        MyFixedPoint itemAmountAtDestination = destinationRefinery.GetItemAmount(item.Type);
        if (itemAmountAtDestination < MAX_INGOTS) {
            isFull = MoveItem(item, inventory, destinationRefinery);
        } else {
            int inventoryIndex = 0;

            do {
                IMyInventory targetInventory = rawMaterials[inventoryIndex];
                isFull = MoveItem(item, inventory, targetInventory);
                inventoryIndex++;
            } while (isFull && inventoryIndex < rawMaterials.Count);
        }

        if (isFull) break;
    }
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
    
        if (item.Type.ToString().Contains("Uranium")) {
            isFull = MoveItem(item, inventory, reactor);
        } else {
            MyFixedPoint itemAmount = assemblerIngots.GetItemAmount(item.Type);
            if (itemAmount < MAX_INGOTS) {
                isFull = MoveItem(item, inventory, assemblerIngots);
            } else {
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
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    inventory.GetItems(
        items,
        item => item.Type.ToString().Contains("_Component")
    );
    
    int numItems = items.Count;

    if(numItems > 0) Echo(" Components:");

    for (int iteration = numItems - 1; iteration >= 0; iteration--) {
        int itemIndex = items.Count - iteration - 1;
        MyInventoryItem item = items[itemIndex];
        MoveItem(item, inventory, main);
    }
}

private void MoveTools(IMyInventory inventory) {
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    inventory.GetItems(
        items,
        item => item.Type.ToString().Contains("_PhysicalGunObject")
             || item.Type.ToString().Contains("_ConsumableItem")
             || item.Type.ToString().Contains("ContainerObject/")
    );
    
    int numItems = items.Count;

    if(numItems > 0) Echo(" Tools:");

    for (int iteration = numItems - 1; iteration >= 0; iteration--) {
        int itemIndex = items.Count - iteration - 1;
        MyInventoryItem item = items[itemIndex];
        MoveItem(item, inventory, tools);
    }
}

private bool MoveItem(MyInventoryItem item, IMyInventory from, IMyInventory to) {
    //if (flag == true) return false;

    if (OwnerName(from) == OwnerName(to)) {
        return false;
    }

    if (to.IsFull) {
        logs.Add("Not Moving " + ItemName(item));
        logs.Add("destination full: " + OwnerName(to));
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
