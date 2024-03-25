<script setup>
const props = defineProps({
  productInfo: {
    type: Object,
    require: false,
    default: undefined,
  },
})

const emit = defineEmits(['showDetails'])
</script>

<template>
  <VCard
    class="mx-auto my-2"
    height="200px"
  >
    <VRow>
      <VCol cols="3">
        <VCarousel
          v-if="productInfo.productImgDTOs && productInfo.productImgDTOs.length > 0"
          style="height: 100%;"
          show-arrows="hover"
          cycle
        >
          <VCarouselItem
            v-for="(image, index) in productInfo.productImgDTOs"
            :key="index"
            :src="image.linkImg"
            cover
            max-height="200"
          />
        </VCarousel>  
        <VImg
          v-else
          cover
          height="100"
          src="https://cdn.vuetifyjs.com/images/cards/cooking.png"
        /> 
      </VCol>
      <VCol
        cols="9"
        style="padding: 20px;"
      >
        <p style="font-size: 18px; font-weight: 500;">
          <VIcon
            icon="mdi-location"
            size="small"
            class="mr-2"
          />Địa chỉ • {{ productInfo .address }}
        </p>
        <p>
          <VIcon
            icon="mdi-money"
            size="small"
            class="mr-2"
          /> Giá bán • {{ productInfo.giaBan.toLocaleString('it-IT', {style : 'currency', currency : 'VND'}) }}
        </p>
        <p>
          <VIcon
            icon="mdi-phone"
            size="small"
            class="mr-2"
          />
          Liên hệ • <span>{{ productInfo.hostPhoneNumber }}</span>
        </p>

        <VDivider class="mx-4 mb-1" />
        <div style="display: flex; justify-content: end; margin-top: 15px;">
          <VBtn
            variant="text"
            @click="() => {emit('showDetails', productInfo)}"
          >
            Xem chi tiết
          </VBtn>
        </div>
      </VCol>
    </VRow>
  </VCard>
</template>
