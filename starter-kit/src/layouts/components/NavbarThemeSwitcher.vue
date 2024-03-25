<script setup>
const themes = [
  {
    name: 'light',
    icon: 'tabler-sun-high',
  },
]
</script>

<template>
  <ThemeSwitcher :themes="themes" />
</template>
