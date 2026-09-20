import type { Component } from "vue"
import EmptyForm from "./EmptyForm.vue"
import FaultForm from "./FaultForm.vue"
import ForEachForm from "./ForEachForm.vue"
import ForForm from "./ForForm.vue"
import IfForm from "./IfForm.vue"
import RunJavaScriptForm from "./RunJavaScriptForm.vue"
import SendHttpRequestForm from "./SendHttpRequestForm.vue"
import SwitchForm from "./SwitchForm.vue"
import WhileForm from "./WhileForm.vue"

/**
 * Type → dedicated form component registry. Unregistered activity types fall
 * back to the descriptor-driven generic form.
 */
export const FORM_REGISTRY: Record<string, Component> = {
  Start: EmptyForm,
  End: EmptyForm,
  Break: EmptyForm,
  Flowchart: EmptyForm,
  Fault: FaultForm,
  For: ForForm,
  ForEach: ForEachForm,
  If: IfForm,
  While: WhileForm,
  Switch: SwitchForm,
  RunJavaScript: RunJavaScriptForm,
  SendHttpRequest: SendHttpRequestForm,
}
