namespace Cerveza_Cristal;

public static class ModValuables
{
    public static readonly ValuableAddition TEST_VALUABLE =
     new ValuableAddition(assetName: "Cone", name: "Test Valuable", logger: ModEntry.Logger, valuableData: new ValuableAddition.Data());

    public static readonly ValuableAddition BOTTLE =
    new ValuableAddition(assetName: "bottle_prefab", name: "Cerveza Cristal Bottle", logger: ModEntry.Logger, valuableData: new ValuableAddition.Data
    (
        value: (1000, 3000),
        mass: 0.25f,
        ValuableVolume.Type.Small,
        durability: 5.0f,
        fragility: 80.0f
    ));

    public static ValuableAddition[] ValuableAdditions { get; private set; } = { TEST_VALUABLE, BOTTLE };
}