# Bannerlord API - Correct Usage Guide

This document provides the **CORRECT** Bannerlord API usage for common tasks, based on actual working code.

## Overview of Issues Fixed

The following compilation errors were fixed:

1. ❌ `LogEntry.GameActionLogText` - **Does not exist**
2. ❌ `PlayerTradeGoldLogEntry` - **Does not exist in public API**
3. ❌ `PlayerTradeGainLogEntry` - **Does not exist in public API**
4. ❌ `ItemObject.EquipmentElement` - **ItemObject is not an ItemRosterElement**

## Required Using Directives

```csharp
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
```

## 1. Working with Log Entries

### ❌ INCORRECT (Does not compile):
```csharp
var log = Campaign.Current.LogEntryHistory.GameActionLogs.First();
string text = log.GameActionLogText?.ToString(); // ERROR: GameActionLogText does not exist
```

### ✅ CORRECT:
```csharp
// Note: LogEntry does not expose text content directly in the public API
// The following properties/methods are NOT available:
// - GameActionLogText
// - GetEncyclopediaText()
// - GetText()

// LogEntry type checking works:
LogEntry entry = Campaign.Current.LogEntryHistory.GameActionLogs.FirstOrDefault();
if (entry != null)
{
    // You can check the type name
    string typeName = entry.GetType().Name;
    bool isTradeRelated = typeName.ToLower().Contains("trade");
}

// However, you cannot access the actual log text through the public API
```

### Known LogEntry Types (that ARE accessible):
- `LogEntry` - Base class
- Type checking can be done via reflection or `GetType().Name`
- Specific trade-related log entry classes are **not exposed** in the public API

## 2. Working with Settlement Item Rosters

### ❌ INCORRECT (Does not compile):
```csharp
var roster = town.Settlement.ItemRoster;
var itemElement = roster.GetItemAtIndex(0);
var item = itemElement.EquipmentElement.Item; // ERROR: ItemObject doesn't have EquipmentElement
```

### ✅ CORRECT:
```csharp
var roster = town.Settlement.ItemRoster;
var itemRosterElement = roster.GetElementCopyAtIndex(0);

// ItemRosterElement contains EquipmentElement, which then contains ItemObject
var item = itemRosterElement.EquipmentElement.Item;
```

## 3. Getting Item Prices from a Town

### ✅ CORRECT (Full working example):
```csharp
public static void GetItemPriceExample(Town town)
{
    var roster = town.Settlement.ItemRoster;
    
    if (roster.Count == 0)
        return;
    
    // Get a random item from the roster
    int itemIndex = MBRandom.RandomInt(roster.Count);
    var itemRosterElement = roster.GetElementCopyAtIndex(itemIndex);
    
    // Access the ItemObject through EquipmentElement
    var item = itemRosterElement.EquipmentElement.Item;
    
    if (item == null) 
        return;
    
    // Get current price in the town
    int currentPrice = town.GetItemPrice(item);
    
    // Get base value of the item
    int basePrice = item.Value;
    
    // Calculate price ratio
    float priceRatio = currentPrice / (float)basePrice;
    
    // Determine if price is good or bad
    if (priceRatio > 1.4f)
        Console.WriteLine($"{item.Name} is expensive in {town.Name}");
    else if (priceRatio < 0.6f)
        Console.WriteLine($"{item.Name} is cheap in {town.Name}");
}
```

## 4. Working with Towns and Settlements

### ✅ CORRECT (Accessing all towns):
```csharp
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;

// Get all towns
var allTowns = Town.AllTowns?.Where(t => t?.Settlement != null).ToList();

foreach (var town in allTowns)
{
    // Access town properties
    string townName = town.Name.ToString();
    Settlement settlement = town.Settlement;
    ItemRoster itemRoster = settlement.ItemRoster;
    
    // Check if town has items
    if (itemRoster != null && itemRoster.Count > 0)
    {
        // Process items...
    }
}
```

## 5. Item Roster Operations

### ✅ CORRECT (Common operations):
```csharp
ItemRoster roster = settlement.ItemRoster;

// Get count of items
int itemCount = roster.Count;

// Get item at specific index
ItemRosterElement element = roster.GetElementCopyAtIndex(index);

// Access the actual item
ItemObject item = element.EquipmentElement.Item;

// Get item quantity
int quantity = element.Amount;
```

## 6. Random Number Generation

### ✅ CORRECT (Using Bannerlord's RNG):
```csharp
using TaleWorlds.Library;

// Random integer between 0 and max (exclusive)
int randomIndex = MBRandom.RandomInt(maxValue);

// Random float between 0.0 and 1.0
float randomChance = MBRandom.RandomFloat;

// Example usage
if (MBRandom.RandomFloat < 0.5f)
{
    // 50% chance
}
```

## Summary of Key API Classes

| Class | Namespace | Purpose |
|-------|-----------|---------|
| `Campaign` | `TaleWorlds.CampaignSystem` | Main campaign access |
| `Town` | `TaleWorlds.CampaignSystem.Settlements` | Town operations |
| `Settlement` | `TaleWorlds.CampaignSystem.Settlements` | Settlement data |
| `ItemRoster` | `TaleWorlds.Core` | Item collections |
| `ItemRosterElement` | `TaleWorlds.Core` | Individual roster item |
| `ItemObject` | `TaleWorlds.Core` | Item data |
| `EquipmentElement` | `TaleWorlds.Core` | Equipment wrapper |
| `LogEntry` | `TaleWorlds.CampaignSystem.LogEntries` | Base log class |
| `MBRandom` | `TaleWorlds.Library` | Random number generation |

## Important Notes

1. **LogEntry text is not accessible**: The Bannerlord API does not expose log entry text through properties like `GameActionLogText` or methods like `GetEncyclopediaText()`.

2. **No specific trade log types**: Classes like `PlayerTradeGoldLogEntry` and `PlayerTradeGainLogEntry` are not exposed in the public API.

3. **ItemRosterElement vs ItemObject**: Always remember that when working with rosters, you get `ItemRosterElement` objects, which contain `EquipmentElement`, which then contains the actual `ItemObject`.

4. **Null safety**: Always check for null values when working with Bannerlord objects, especially when accessing collections and properties.


## References

- Official API Documentation: https://apidoc.bannerlord.com/
- Modding Documentation: https://docs.bannerlordmodding.com/

## Code Example Location

See `/Dialogues/TradeRumorHelper.cs` for a complete working example of:
- Accessing town item rosters
- Getting item prices
- Working with settlements
- Safe null handling
