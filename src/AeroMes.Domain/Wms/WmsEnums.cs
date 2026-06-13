namespace AeroMes.Domain.Wms;

public enum ZoneType { Receiving, Storage, Staging, Shipping, Scrap }
public enum TemperatureZone { Ambient, Cold, Frozen }
public enum BinType { Bulk, Pick, Overflow, Quarantine, QcHold }
public enum PoStatus { Draft, Confirmed, PartiallyReceived, FullyReceived, Closed, Cancelled }
public enum GrnStatus { Draft, Confirmed, Quarantine, Rejected }
public enum QcStatus { Pending, Passed, Quarantine, Rejected }
public enum MovementType { Receive, Issue, Transfer, Adjust, Return }
