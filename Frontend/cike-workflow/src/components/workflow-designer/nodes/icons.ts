import type { Component } from "vue"
import {
  AlertTriangle,
  Boxes,
  FileCode,
  GitFork,
  Globe,
  LogOut,
  Play,
  Repeat,
  RotateCw,
  Split,
  Square,
  Workflow,
} from "@lucide/vue"

/**
 * Maps backend-provided icon names (ActivityDescriptor.Icon, lucide kebab-case)
 * to concrete @lucide/vue components. The icon *assignment* lives in the backend;
 * the frontend only resolves the bounded set of names it bundles.
 */
const ICON_BY_NAME: Record<string, Component> = {
  play: Play,
  square: Square,
  "alert-triangle": AlertTriangle,
  "log-out": LogOut,
  "git-fork": GitFork,
  split: Split,
  "rotate-cw": RotateCw,
  repeat: Repeat,
  workflow: Workflow,
  "file-code": FileCode,
  globe: Globe,
}

/** Fallback for activities without (or with an unbundled) icon name. */
export const FALLBACK_ICON: Component = Boxes

export function resolveActivityIcon(name?: string | null): Component {
  return (name ? ICON_BY_NAME[name] : undefined) ?? FALLBACK_ICON
}
