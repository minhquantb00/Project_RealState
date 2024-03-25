<script setup>
import moment from 'moment'
import { ref, watch } from 'vue'

const dialog = ref(false)

const productInfo = ref({})

const model = ref()

const showDialog = product => {
  dialog.value = true
  productInfo.value = { ...product }
  if(productInfo.value.productImgDTOs && productInfo.value.productImgDTOs.length > 0) {
    model.value = 0
  }
}

watch(
  () => model.value,
  newVal => {
    if(productInfo.value.productImgDTOs && productInfo.value.productImgDTOs.length > 0 && newVal === undefined) {
      model.value = 0
    }
  },
)

defineExpose({
  showDialog,
})
</script>

<template>
  <VDialog
    v-model="dialog"
    fullscreen
  >
    <VToolbar
      dark
      color="primary"
    >
      <VToolbarTitle>Thông tin bất động sản</VToolbarTitle>
      <VSpacer />
      <VToolbarItems>
        <VBtn
          variant="text"
          @click="dialog = false"
        >
          <VIcon>mdi-close</VIcon>
        </VBtn>
      </VToolbarItems>
    </VToolbar>
    <VCard class="px-10 pb-10">
      <VCardText>
        <VRow>
          <VCol cols="6">
            <VImg
              v-if="model !== undefined"
              height="500"
              :src="productInfo.productImgDTOs[model] ? productInfo.productImgDTOs[model].linkImg : ''"
            />
            <VSlideGroup
              v-model="model"
              class="pa-4"
              selected-class="bg-success"
              show-arrows
            >
              <VSlideGroupItem
                v-for="(image, index) in productInfo.productImgDTOs"
                :key="index"
                v-slot="{ isSelected, toggle, selectedClass }"
              >
                <VCard
                  color="grey-lighten-1"
                  class="ma-4"
                  :class="[selectedClass]"
                  height="100"
                  width="100"
                  @click="toggle"
                >
                  <div class="d-flex fill-height align-center justify-center">
                    <VScaleTransition>
                      <VImg :src="image.linkImg" />
                    </VScaleTransition>
                  </div>
                </VCard>
              </VSlideGroupItem>
            </VSlideGroup>
          </VCol>
          <VCol cols="6">
            <p style="font-size: 24px; font-weight: 500;">
              <VIcon
                icon="mdi-home-map-marker"
                size="small"
                class="mr-2"
              />Địa chỉ • {{ productInfo .address }}
            </p>
            <p>
              <VIcon
                icon="mdi-money"
                size="small"
                class="mr-2 ml-1"
              /> <i>Giá bán • {{ productInfo.giaBan.toLocaleString('it-IT', {style : 'currency', currency : 'VND'}) }}</i>
            </p>
            <div class="mb-4 mt-7 pl-3">
              <h3><i>Thông tin bất động sản</i></h3>
            </div>
            <VDivider class="mb-2" />
            <VRow>
              <VCol cols="4">
                <VIcon
                  icon="mdi-account"
                  size="small"
                  class="mr-2"
                />
                Chủ bất động sản • <span>{{ productInfo.hostName }}</span>
              </VCol>
              <VCol cols="4">
                <VIcon
                  icon="mdi-phone"
                  size="small"
                  class="mr-2"
                />
                Liên hệ • <span>{{ productInfo.hostPhoneNumber }}</span>
              </VCol>
            </VRow>
            <VRow>
              <VCol cols="4">
                <VIcon
                  icon="mdi-home-clock"
                  size="small"
                  class="mr-2"
                />
                Thời gian xây dựng • <span>{{ productInfo.build ? moment(productInfo.build).format('DD/MM/YYYY') : '' }}</span>
              </VCol>
              <VCol cols="4">
                <VIcon
                  icon="mdi-home-silo"
                  size="small"
                  class="mr-2"
                />
                Thời gian bắt đầu bán • <span>{{ productInfo.batDauBan ? moment(productInfo.batDauBan).format('DD/MM/YYYY') : '' }}</span>
              </VCol>
            </VRow>
            <VDivider class="my-7" />
            <div class="my-4 pl-3">
              <h3><i>Thông tin pháp lý</i></h3>
            </div>
            <VDivider class="mb-4" />
            <VRow>
              <VCol cols="12">
                <VIcon
                  icon="mdi-file-document-check"
                  size="small"
                  class="mr-2"
                />
                Giấy chứng nhận đất đai 1 • <span>{{ productInfo.certificateOfLand1 }}</span>
              </VCol>
              <VCol cols="12">
                <VIcon
                  icon="mdi-file-document-check"
                  size="small"
                  class="mr-2"
                />
                Giấy chứng nhận đất đai 2 • <span>{{ productInfo.certificateOfLand2 }}</span>
              </VCol>
            </VRow>
          </VCol>
        </VRow>
      </VCardText>
    </VCard>
  </VDialog>
</template>