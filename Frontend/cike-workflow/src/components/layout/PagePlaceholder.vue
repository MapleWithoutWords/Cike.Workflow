<script setup lang="ts">
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

defineProps<{
  title: string
  description?: string
}>()
</script>

<template>
  <Card class="mx-auto w-full max-w-2xl">
    <CardHeader>
      <CardTitle>{{ title }}</CardTitle>
      <CardDescription v-if="description">{{ description }}</CardDescription>
    </CardHeader>
    <CardContent>
      <slot>
        <p class="text-sm text-muted-foreground">
          本期未实现。后端接口就绪后，此处将呈现真实数据。
        </p>
      </slot>
    </CardContent>
  </Card>
</template>
