import { computed } from "vue"
import { useColorMode, useToggle } from "@vueuse/core"

const mode = useColorMode({
  attribute: "class",
  modes: { light: "light", dark: "dark" },
  initialValue: "light",
})

export function useTheme() {
  const isDark = computed(() => mode.value === "dark")
  const toggleTheme = useToggle(
    computed({
      get: () => isDark.value,
      set: (val: boolean) => {
        mode.value = val ? "dark" : "light"
      },
    }),
  )

  return { isDark, toggleTheme }
}
