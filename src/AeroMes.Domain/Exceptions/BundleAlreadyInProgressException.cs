namespace AeroMes.Domain.Exceptions;

public class BundleAlreadyInProgressException(string bundleBarcode)
    : Exception($"Bundle '{bundleBarcode}' already has an open movement. Close the current operation first.");
