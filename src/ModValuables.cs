namespace Cerveza_Cristal;

public static class ModValuables
{
    public static readonly ModValuableRegistry.ValuableAddition TEST_VALUABLE =
     new ModValuableRegistry.ValuableAddition(assetName: "Cone", valuableData: new ModValuableRegistry.Data(
        name: "Test Valuable"
    ));

    public static readonly ModValuableRegistry.ValuableAddition BOTTLE =
    new ModValuableRegistry.ValuableAddition(assetName: "CervezaCristal", valuableData: new ModValuableRegistry.Data
    (
        name: "Cerveza Cristal Bottle",
        value: (1000, 3000),
        mass: 0.25f,
        ValuableVolume.Type.Small,
        durability: 5.0f,
        fragility: 80.0f
    ));

    public static ModValuableRegistry.ValuableAddition[] ValuableAdditions { get; private set; } = { TEST_VALUABLE, BOTTLE };
}