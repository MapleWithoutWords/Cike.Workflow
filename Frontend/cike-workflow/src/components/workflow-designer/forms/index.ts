import type { Component } from "vue"

/**
 * Type → dedicated form component registry. Unregistered activity types fall
 * back to the descriptor-driven generic form.
 */
export const FORM_REGISTRY: Record<string, Component> = {}
