import { resolveActivityClass } from "./registry";

/**
 * Activity palette built from backend activity descriptors. The palette only
 * decides what can be ADDED; rendering always works off the loaded data
 * (unknown types render as generic nodes regardless of the palette).
 */

export interface PaletteDescriptorInput {
  typeName?: string;
  name?: string;
  displayName?: string | null;
  description?: string | null;
  category?: string;
  isBrowsable?: boolean;
  version?: number;
}

export interface PaletteItem {
  typeName: string;
  displayName: string;
  description: string | null;
  /** True when a frontend-mirrored class exists for instantiation. */
  canAdd: boolean;
}

export interface PaletteGroup {
  category: string;
  items: PaletteItem[];
}

export function buildPaletteGroups(descriptors: PaletteDescriptorInput[]): PaletteGroup[] {
  const groups = new Map<string, PaletteItem[]>();
  for (const descriptor of descriptors) {
    if (descriptor.isBrowsable === false) continue;
    const typeName = descriptor.typeName ?? "";
    if (!typeName) continue;
    groups.set(descriptor.category ?? "其他", groups.get(descriptor.category ?? "其他") ?? []);
    groups.get(descriptor.category ?? "其他")!.push({
      typeName,
      displayName: descriptor.displayName ?? descriptor.name ?? typeName,
      description: descriptor.description ?? null,
      canAdd: resolveActivityClass(typeName) !== null,
    });
  }
  const fallbackCategory = "其他";
  return [...groups.entries()]
    .sort(([a], [b]) => {
      // The uncategorized bucket always sorts last for predictable UX.
      if (a === fallbackCategory) return 1;
      if (b === fallbackCategory) return -1;
      return a.localeCompare(b);
    })
    .map(([category, items]) => ({ category, items }));
}
