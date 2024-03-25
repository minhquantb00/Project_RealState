<template>
  <div
    class="app-image-field"
    :class="$attrs.class"
  >
    <VLabel
      v-if="label"
      :for="elementId"
      class="mb-1 text-body-2 text-high-emphasis"
      :text="label"
    />
    <VInput
      v-bind="{
        ...$attrs,
        class: null,
        id: elementId,
      }"
      :focused="isFocused"
    >
      <VRow>
        <VCol cols="12">
          <VFileInput
            v-bind="{
              ...$attrs,
              class: null,
              id: elementId,
            }"
            :label="undefined"
            variant="underlined"
            accept="image/png, image/jpeg,image/jpe,image/jif,image/jfif,image/jfi, image/bmp,image/dib,image/jpg,image/gif,image/webp,image/tiff,image/tif,image/psd,image/raw,image/arw,image/cr2,image/nrw,image/k25,image/jp2,image/j2k,image/jpf,image/jpm,image/mj2,image/heif,image/heic,image/ind,image/indd,image/indt,image/svg,image/svgz,image/ai,image/eps"
            @change="onFileChange"
          />
        </VCol>
        <VCol cols="12">
          <div
            style="max-width: 20%; padding-left: 50px;"
            class="preview-image"
          >
            <img
              :src="fileUrl || defaultImgUrl"
              style="width: 100%"
            >
          </div>
        </VCol>
      </VRow>
    </VInput>
  </div>
</template>

<script setup>
const props = defineProps({
  defaultImgUrl: {
    type: String,
    required: false,
    default: "",
  },
})

defineOptions({
  name: "AppImageField",
  inheritAttrs: false,
})

const elementId = computed(() => {
  const attrs = useAttrs()
  const _elementIdToken = attrs.id || attrs.label

  return _elementIdToken
    ? `app-ckeditor-${_elementIdToken}-${Math.random()
      .toString(36)
      .slice(2, 7)}`
    : undefined
})

const label = computed(() => {
  const attrs = useAttrs()

  return attrs.label
})

const isFocused = ref(false)

const fileUrl = ref("")

const createImage = file => {
  const reader = new FileReader()

  reader.onload = e => {
    fileUrl.value = e?.target?.result
  }
  reader.readAsDataURL(file)
}

const onFileChange = e => {
  const file = e.currentTarget.files[0]
  if (!file) {
    return
  }
  createImage(file)
}
</script>