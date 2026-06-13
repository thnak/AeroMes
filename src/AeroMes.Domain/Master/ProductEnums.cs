namespace AeroMes.Domain.Master;

public enum ItemType { FG, SEMI, RM, CONS, PKG, SPARE, TOOL }

public enum LifecycleStatus { Development, Active, PhasingOut, Discontinued, Obsolete }

public enum ProcurementType { Buy, Make, Both }

public enum PickingStrategy { Fefo, Fifo, Lifo, Manual }

public enum TrackingMethod
{
    None,   // bulk commodity — no lot/serial tracking
    Lot,    // batch/lot managed (resin, fabric, ingredients)
    Serial, // per-unit tracking (electronics, medical devices)
}

public enum ProductClass
{
    Standard,
    RawMaterial,
    Fabric,
    Resin,
    FinishedGood,
    SemiFinished,
    Consumable,
}
