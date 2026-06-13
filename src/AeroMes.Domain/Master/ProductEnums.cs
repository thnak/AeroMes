namespace AeroMes.Domain.Master;

public enum ItemType { FG, SEMI, RM, CONS, PKG, SPARE, TOOL }

public enum LifecycleStatus { Development, Active, PhasingOut, Discontinued, Obsolete }

public enum ProcurementType { Buy, Make, Both }

public enum PickingStrategy { Fefo, Fifo, Lifo, Manual }
