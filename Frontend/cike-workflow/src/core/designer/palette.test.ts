import { describe, expect, it } from "vitest";
import { buildPaletteGroups } from "./palette";

describe("palette", () => {
  it("BuildGroups_SortsByCategory_FiltersHidden", () => {
    const groups = buildPaletteGroups([
      { typeName: "Cike.If", category: "Branching", displayName: "Decision" },
      { typeName: "Cike.Hidden", category: "Branching", isBrowsable: false },
      { typeName: "Cike.Start", category: "Basic" },
      { typeName: "Cike.Mystery" },
    ]);
    expect(groups.map((g) => g.category)).toEqual(["Basic", "Branching", "其他"]);
    const branching = groups.find((g) => g.category === "Branching")!;
    expect(branching.items).toHaveLength(1);
    const other = groups.find((g) => g.category === "其他")!;
    expect(other.items[0].canAdd).toBe(false);
    expect(branching.items[0].canAdd).toBe(true);
  });
});
