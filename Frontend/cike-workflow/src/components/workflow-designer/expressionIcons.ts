import type { Component } from "vue"
import {
  Braces,
  Code,
  Droplet,
  LogIn,
  Type as TypeIcon,
  Variable,
} from "@lucide/vue"

/**
 * Maps backend-provided expression-type icon names (ExpressionDescriptor.icon,
 * lucide kebab-case) to concrete @lucide/vue components. Mirrors the activity
 * icons.ts pattern: the icon *assignment* lives in the backend; the frontend
 * only resolves the bounded set of names it bundles. Backend currently emits
 * type→"type", variable→"variable", log-in (Input), droplet (Liquid),
 * code (JavaScript).
 */
const ICON_BY_NAME: Record<string, Component> = {
  type: TypeIcon,
  variable: Variable,
  "log-in": LogIn,
  droplet: Droplet,
  code: Code,
}

/** Fallback for types without (or with an unbundled) icon name. */
export const FALLBACK_EXPRESSION_ICON: Component = Braces

export function resolveExpressionIcon(name?: string | null): Component {
  return (name ? ICON_BY_NAME[name] : undefined) ?? FALLBACK_EXPRESSION_ICON
}
